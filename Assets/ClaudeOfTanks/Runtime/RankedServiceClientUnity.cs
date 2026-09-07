using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class RankedServiceClient
    {
        private async Task<T> SendUnityAsync<T>(
            HttpMethod method,
            string path,
            object body,
            string bearer,
            CancellationToken cancellationToken)
            where T : class
        {
            string jsonBody = body == null
                ? null
                : JsonUtility.ToJson(body);
            using (UnityWebRequest request = new UnityWebRequest(
                new Uri(_baseUri, path),
                method.Method))
            {
                request.downloadHandler =
                    new DownloadHandlerBuffer();
                if (jsonBody != null)
                {
                    request.uploadHandler = new UploadHandlerRaw(
                        Encoding.UTF8.GetBytes(jsonBody));
                    request.SetRequestHeader(
                        "Content-Type",
                        "application/json");
                }
                if (!string.IsNullOrEmpty(bearer))
                {
                    request.SetRequestHeader(
                        "Authorization",
                        "Bearer " + bearer);
                }
                UnityWebRequest completed =
                    await Await(request, cancellationToken);
                string json = completed.downloadHandler?.text;
                if (completed.responseCode < 200 ||
                    completed.responseCode >= 300)
                {
                    ErrorResponse error =
                        Parse<ErrorResponse>(json);
                    throw new RankedServiceException(
                        (int)completed.responseCode,
                        error?.error ?? "ranked_service_error",
                        error?.message ?? completed.error);
                }
                T value = Parse<T>(json);
                if (value == null)
                    throw new FormatException(
                        "Ranked service returned an invalid response.");
                return value;
            }
        }

        private static Task<UnityWebRequest> Await(
            UnityWebRequest request,
            CancellationToken cancellationToken)
        {
            TaskCompletionSource<UnityWebRequest> completion =
                new TaskCompletionSource<UnityWebRequest>();
            UnityWebRequestAsyncOperation operation =
                request.SendWebRequest();
            CancellationTokenRegistration cancellation =
                cancellationToken.Register(() =>
                {
                    request.Abort();
                    completion.TrySetCanceled(cancellationToken);
                });
            operation.completed += _ =>
            {
                cancellation.Dispose();
                completion.TrySetResult(request);
            };
            if (operation.isDone)
            {
                cancellation.Dispose();
                completion.TrySetResult(request);
            }
            return completion.Task;
        }
    }
}
