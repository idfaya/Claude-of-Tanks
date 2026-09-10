#!/bin/sh
set -eu

project_root=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
project="$project_root/server/dotnet/ClaudeOfTanks.Server/ClaudeOfTanks.Server.csproj"

exec "$project_root/tools/dotnet.sh" run \
  --project "$project" \
  --configuration Release \
  -- "$@"
