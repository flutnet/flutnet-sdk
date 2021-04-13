@echo off
cls

:: Define paths
set ScriptDir=%~dp0

dotnet restore "%ScriptDir%\..\Flutnet.Cli.sln"
dotnet restore "%ScriptDir%\..\FlutnetUI.sln" --ignore-failed-sources