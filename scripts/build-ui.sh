#!/usr/bin/env bash

# Define paths
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
SOLUTION_PATH=$SCRIPT_DIR/../FlutnetUI.sln
PROJECT_PATH=$SCRIPT_DIR/../src/FlutnetUI/FlutnetUI.csproj

# Clean and build Flutnet Console
dotnet clean "$PROJECT_PATH" -c Debug --nologo 
dotnet clean "$PROJECT_PATH" -c Release --nologo

dotnet build "$PROJECT_PATH" -c Debug --no-restore --nologo
dotnet build "$PROJECT_PATH" -c Release --no-restore --nologo