#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

"$SCRIPT_DIR/build.sh" 1.3
"$SCRIPT_DIR/build.sh" 1.4
"$SCRIPT_DIR/build.sh" 1.5
"$SCRIPT_DIR/build.sh" 1.6
