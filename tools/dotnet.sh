#!/bin/sh
set -eu

project_root=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)

if [ -n "${DOTNET_PATH:-}" ]; then
  dotnet=$DOTNET_PATH
elif command -v dotnet >/dev/null 2>&1; then
  dotnet=$(command -v dotnet)
elif [ -x "$project_root/.qa-device/dotnet/dotnet" ]; then
  dotnet="$project_root/.qa-device/dotnet/dotnet"
else
  echo ".NET 8 SDK was not found." >&2
  echo "Install it from https://dotnet.microsoft.com/download/dotnet/8.0" >&2
  echo "or set DOTNET_PATH to the dotnet executable." >&2
  exit 1
fi

export DOTNET_CLI_TELEMETRY_OPTOUT=1
exec "$dotnet" "$@"
