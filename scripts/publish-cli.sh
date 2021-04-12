#!/usr/bin/env bash

PROJECT_NAME=Flutnet.Cli

# Define paths
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
ARTIFACTS_DIR=$SCRIPT_DIR/../artifacts
SOLUTION_PATH=$SCRIPT_DIR/../$PROJECT_NAME.sln
PROJECT_PATH=$SCRIPT_DIR/../src/$PROJECT_NAME/$PROJECT_NAME.csproj
PUBLISH_DIR_OSX=$ARTIFACTS_DIR/osx-x64/$PROJECT_NAME
PUBLISH_DIR_WIN=$ARTIFACTS_DIR/win-x64/$PROJECT_NAME

# Clean the output of the previous builds 
# (ignore any error, the clean will still complete successfully)
dotnet clean "$PROJECT_PATH" -c Release -f netcoreapp3.1 -r osx-x64 -v quiet --nologo
dotnet clean "$PROJECT_PATH" -c Release -f netcoreapp3.1 -r win-x64 -v quiet --nologo

# Clean the output of the previous publish operations
rm -rf "$PUBLISH_DIR_OSX"
rm -rf "$PUBLISH_DIR_WIN"

# Publish exeutable for macOS
dotnet publish "$PROJECT_PATH" -c Release -f netcoreapp3.1 -o "$PUBLISH_DIR_OSX" -r osx-x64 --no-self-contained -v minimal --nologo

# Publish exeutable for Windows
dotnet publish "$PROJECT_PATH" -c Release -f netcoreapp3.1 -o "$PUBLISH_DIR_WIN" -r win-x64 --no-self-contained -v minimal --nologo