#!/usr/bin/env bash

# Define paths
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

dotnet restore "$SCRIPT_DIR/../Flutnet.Cli.sln"
dotnet restore "$SCRIPT_DIR/../FlutnetUI.sln" --ignore-failed-sources