@echo off
cls

set ProjectName=Flutnet.Cli

:: Define paths
set ScriptDir=%~dp0
set SolutionPath=%ScriptDir%\..\%ProjectName%.sln
set ProjectPath=%ScriptDir%\..\src\%ProjectName%\%ProjectName%.csproj

:: Clean and build Flutnet CLI
dotnet clean "%ProjectPath%" -c Debug --nologo 
dotnet clean "%ProjectPath%" -c Release --nologo

dotnet build "%ProjectPath%" -c Debug --no-restore --nologo
dotnet build "%ProjectPath%" -c Release --no-restore --nologo