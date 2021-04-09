#!/usr/bin/env bash

# Define paths
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
SOLUTION_PATH=$SCRIPT_DIR/../Flutnet.Cli.sln
PROJECT_PATH=$SCRIPT_DIR/../src/Flutnet.Cli/Flutnet.Cli.csproj

# Clean and build Flutnet CLI
dotnet clean "$PROJECT_PATH" -c Debug -f netcoreapp3.1 --nologo 
dotnet clean "$PROJECT_PATH" -c Release -f netcoreapp3.1 --nologo

dotnet build "$PROJECT_PATH" -c Debug -f netcoreapp3.1 --no-restore --nologo
dotnet build "$PROJECT_PATH" -c Release -f netcoreapp3.1 --no-restore --nologo