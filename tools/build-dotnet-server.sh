#!/bin/sh
set -eu

project_root=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
project="$project_root/server/dotnet/ClaudeOfTanks.Server/ClaudeOfTanks.Server.csproj"
runtime=${1:-linux-x64}
output=${2:-"$project_root/Build/Server/DotNet/$runtime"}

case "$runtime" in
  linux-x64|linux-arm64|osx-x64|osx-arm64|win-x64) ;;
  *)
    echo "Unsupported .NET server runtime: $runtime" >&2
    exit 1
    ;;
esac

exec "$project_root/tools/dotnet.sh" publish "$project" \
  --configuration Release \
  --runtime "$runtime" \
  --self-contained true \
  --output "$output" \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=false \
  -p:DebugType=None \
  --nologo
