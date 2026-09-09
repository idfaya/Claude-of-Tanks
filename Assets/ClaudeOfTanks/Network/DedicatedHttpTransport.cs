using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClaudeOfTanks.Network
{
    public sealed class DedicatedHttpRequest
    {
        public string Method;
        public string Target;
        public string Origin;
        public string Authorization;
        public string RemoteAddress;
        public byte[] Body;
    }

    public sealed class DedicatedHttpResponse
    {
        public int Status;
        public string Reason;
        public string ContentType;
        public byte[] Body;
        public Dictionary<string, string> Headers;
    }

    public interface IDedicatedHttpHandler
    {
        DedicatedHttpResponse Handle(DedicatedHttpRequest request);
        void Pump(long nowMs);
    }

    public sealed class DedicatedTransportRequest
    {
        public string Method;
        public string Target;
        public string Path;
        public string WebSocketKey;
        public string Origin;
        public string Authorization;
        public byte[] Body;
        public bool IsWebSocket;

        public DedicatedHttpRequest ToHttpRequest(string remoteAddress)
        {
            return new DedicatedHttpRequest
            {
                Method = Method,
                Target = Target,
                Origin = Origin,
                Authorization = Authorization,
                RemoteAddress = remoteAddress,
                Body = Body
            };
        }
    }

    public static class DedicatedHttpTransport
    {
        public const int MaximumHeaderBytes = 8192;
        public const int MaximumBodyBytes = 16 * 1024;

        public static async Task<DedicatedTransportRequest> ReadAsync(
            Stream stream,
            CancellationToken cancellationToken)
        {
            byte[] bytes = new byte[MaximumHeaderBytes + MaximumBodyBytes];
            int length = 0;
            while (length < MaximumHeaderBytes)
            {
                int read = await stream.ReadAsync(
                    bytes,
                    length,
                    MaximumHeaderBytes - length,
                    cancellationToken).ConfigureAwait(false);
                if (read <= 0) throw new IOException("HTTP request ended early.");
                length += read;
                int headerEnd = HeaderEnd(bytes, length);
                if (headerEnd < 0) continue;
                string text = Encoding.ASCII.GetString(bytes, 0, headerEnd);
                string[] lines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);
                string[] request = lines[0].Split(' ');
                if (request.Length != 3 || request[2] != "HTTP/1.1")
                    throw new FormatException("HTTP request line is invalid.");
                Dictionary<string, string> headers =
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 1; i < lines.Length; i++)
                {
                    if (lines[i].Length == 0) continue;
                    int colon = lines[i].IndexOf(':');
                    if (colon <= 0) throw new FormatException("HTTP header is invalid.");
                    string name = lines[i].Substring(0, colon).Trim();
                    string value = lines[i].Substring(colon + 1).Trim();
                    if (headers.ContainsKey(name)) headers[name] += "," + value;
                    else headers.Add(name, value);
                }

                string key;
                string upgrade;
                string connection;
                string version;
                headers.TryGetValue("Sec-WebSocket-Key", out key);
                headers.TryGetValue("Upgrade", out upgrade);
                headers.TryGetValue("Connection", out connection);
                headers.TryGetValue("Sec-WebSocket-Version", out version);
                bool isWebSocket =
                    string.Equals(upgrade, "websocket", StringComparison.OrdinalIgnoreCase);
                if (isWebSocket &&
                    (string.IsNullOrEmpty(connection) ||
                     connection.IndexOf("upgrade", StringComparison.OrdinalIgnoreCase) < 0 ||
                     version != "13" ||
                     !ValidWebSocketKey(key)))
                {
                    throw new FormatException("WebSocket upgrade headers are invalid.");
                }

                string origin;
                string authorization;
                string contentLengthValue;
                headers.TryGetValue("Origin", out origin);
                headers.TryGetValue("Authorization", out authorization);
                headers.TryGetValue("Content-Length", out contentLengthValue);
                int contentLength = 0;
                if (!string.IsNullOrEmpty(contentLengthValue) &&
                    (!int.TryParse(contentLengthValue, out contentLength) ||
                     contentLength < 0 ||
                     contentLength > MaximumBodyBytes))
                {
                    throw new FormatException("HTTP content length is invalid.");
                }
                if (headers.ContainsKey("Transfer-Encoding"))
                    throw new FormatException("Chunked HTTP requests are unsupported.");
                int totalLength = headerEnd + contentLength;
                while (length < totalLength)
                {
                    int bodyRead = await stream.ReadAsync(
                        bytes,
                        length,
                        totalLength - length,
                        cancellationToken).ConfigureAwait(false);
                    if (bodyRead <= 0) throw new IOException("HTTP request body ended early.");
                    length += bodyRead;
                }
                byte[] body = new byte[contentLength];
                if (contentLength > 0)
                    Buffer.BlockCopy(bytes, headerEnd, body, 0, contentLength);
                string target = request[1];
                int query = target.IndexOf('?');
                return new DedicatedTransportRequest
                {
                    Method = request[0],
                    Target = target,
                    Path = query >= 0 ? target.Substring(0, query) : target,
                    WebSocketKey = key,
                    Origin = origin,
                    Authorization = authorization,
                    Body = body,
                    IsWebSocket = isWebSocket
                };
            }
            throw new FormatException("HTTP headers exceed the size limit.");
        }

        public static async Task WriteResponseAsync(
            Stream stream,
            DedicatedHttpResponse response,
            CancellationToken cancellationToken)
        {
            byte[] body = response.Body ?? Array.Empty<byte>();
            StringBuilder headers = new StringBuilder();
            headers.Append("HTTP/1.1 ")
                .Append(response.Status)
                .Append(' ')
                .Append(string.IsNullOrEmpty(response.Reason) ? "OK" : response.Reason)
                .Append("\r\nConnection: close\r\n")
                .Append("Cache-Control: no-store\r\n")
                .Append("Content-Length: ")
                .Append(body.Length)
                .Append("\r\n");
            if (!string.IsNullOrEmpty(response.ContentType))
                headers.Append("Content-Type: ").Append(response.ContentType).Append("\r\n");
            if (response.Headers != null)
                foreach (KeyValuePair<string, string> header in response.Headers)
                    headers.Append(header.Key).Append(": ").Append(header.Value).Append("\r\n");
            headers.Append("\r\n");
            byte[] head = Encoding.ASCII.GetBytes(headers.ToString());
            await stream.WriteAsync(head, 0, head.Length, cancellationToken)
                .ConfigureAwait(false);
            if (body.Length > 0)
                await stream.WriteAsync(body, 0, body.Length, cancellationToken)
                    .ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        private static int HeaderEnd(byte[] bytes, int length)
        {
            for (int i = 3; i < length; i++)
                if (bytes[i - 3] == 13 && bytes[i - 2] == 10 &&
                    bytes[i - 1] == 13 && bytes[i] == 10)
                    return i + 1;
            return -1;
        }

        private static bool ValidWebSocketKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            try { return Convert.FromBase64String(key).Length == 16; }
            catch (FormatException) { return false; }
        }
    }
}
