#!/bin/sh
set -eu

project_root=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
unity=${UNITY_PATH:-/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity}
target=${1:-linux}
output=${2:-}
mode=${3:-server}

if [ ! -x "$unity" ]; then
  echo "Unity executable not found: $unity" >&2
  exit 1
fi

set -- \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "$project_root" \
  -executeMethod ClaudeOfTanks.Editor.DedicatedServerBuild.BuildFromCommandLine \
  "--cot-build-target=$target"

if [ -n "$output" ]; then
  set -- "$@" "--cot-build-output=$output"
fi
if [ "$mode" = "player" ]; then
  set -- "$@" --cot-build-player
elif [ "$mode" != "server" ]; then
  echo "Build mode must be 'server' or 'player'." >&2
  exit 1
fi

if [ "$(uname -s)" = "Darwin" ] && [ -n "${TRAE_SANDBOX_SBOX_ID:-}" ]; then
  echo "Refusing to launch Unity from the TRAE sandbox: injected processes crash while forking Bee." >&2
  echo "Run this build from a normal terminal after installing the target's Unity build support." >&2
  exit 2
fi

exec "$unity" "$@"
