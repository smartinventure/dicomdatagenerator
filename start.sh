#!/usr/bin/env bash
# Builds and starts the DICOM Data Generator (Web API + web UI in one app).
# Usage:  ./start.sh             (Release build, runs on http://localhost:5300)
#         ./start.sh --no-browser
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$ROOT/src/DicomDataGenerator"
URL="http://localhost:5300"
OPEN_BROWSER=1

for arg in "$@"; do
  case "$arg" in
    --no-browser) OPEN_BROWSER=0 ;;
    *) echo "Unknown option: $arg" >&2; exit 2 ;;
  esac
done

if ! command -v dotnet >/dev/null 2>&1; then
  echo "The .NET SDK (dotnet) was not found on PATH. Install .NET 8 SDK: https://dotnet.microsoft.com/download" >&2
  exit 1
fi

echo "==> Building (Release)..."
dotnet build "$PROJECT" -c Release --nologo

open_browser() {
  sleep 3
  if command -v xdg-open >/dev/null 2>&1; then xdg-open "$URL" >/dev/null 2>&1
  elif command -v open >/dev/null 2>&1; then open "$URL" >/dev/null 2>&1
  elif command -v start >/dev/null 2>&1; then start "$URL" >/dev/null 2>&1
  fi
}

echo "==> Starting on $URL (Ctrl+C to stop)..."
if [ "$OPEN_BROWSER" -eq 1 ]; then
  open_browser &
fi

# Run without launchSettings so the URL is deterministic.
exec dotnet run --project "$PROJECT" -c Release --no-build --no-launch-profile --urls "$URL"
