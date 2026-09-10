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

if [ "${1:-}" = "launch" ] && [ -n "${TRAE_SANDBOX_SBOX_ID:-}" ]; then
  echo "Refusing to launch Unity from the TRAE sandbox: injected processes crash while forking Bee." >&2
  echo "Open this project from Unity Hub, then rerun the uloop command:" >&2
  echo "  $project_root" >&2
  exit 2
fi

exec "$cli" "$@" --project-path "$project_root"
