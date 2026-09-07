#!/bin/sh
set -eu

project_root=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
cli=${ULOOP_CLI_PATH:-"$HOME/.local/bin/uloop"}
runner="$project_root/.unity-tools/uloop-project-runner"

if [ ! -x "$cli" ]; then
  echo "uloop CLI not found at $cli" >&2
  exit 1
fi

if [ -x "$runner" ]; then
  export ULOOP_PROJECT_RUNNER_PATH="$runner"
fi

exec "$cli" "$@" --project-path "$project_root"
