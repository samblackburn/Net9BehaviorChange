#!/bin/bash
set -euo pipefail

echo "Output with net8:"
dotnet run --framework net8.0

echo "Output with net9:"
dotnet run --framework net9.0
