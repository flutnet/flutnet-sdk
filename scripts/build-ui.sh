#!/usr/bin/env bash

PROJECT_NAME=FlutnetUI

# Define paths
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
SOLUTION_PATH=$SCRIPT_DIR/../$PROJECT_NAME.sln
PROJECT_PATH=$SCRIPT_DIR/../src/$PROJECT_NAME/$PROJECT_NAME.csproj

# Restore Nuget Packages for Flutnet Console projects
dotnet restore "$SOLUTION_PATH"

# Clean and build Flutnet Console
dotnet clean "$PROJECT_PATH" -c Debug --nologo 
dotnet clean "$PROJECT_PATH" -c Release --nologo

dotnet build "$PROJECT_PATH" -c Debug --no-restore --nologo
dotnet build "$PROJECT_PATH" -c Release --no-restore --nologo