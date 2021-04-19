@echo off
cls

set ProjectName=FlutnetUI

:: Define paths
set ScriptDir=%~dp0
set SolutionPath=%ScriptDir%\..\%ProjectName%.sln
set ProjectPath=%ScriptDir%\..\src\%ProjectName%\%ProjectName%.csproj

:: Restore Nuget Packages for Flutnet Console projects
dotnet restore "%SolutionPath%"

:: Clean and build Flutnet Console
dotnet clean "%ProjectPath%" -c Debug --nologo 
dotnet clean "%ProjectPath%" -c Release --nologo

dotnet build "%ProjectPath%" -c Debug --no-restore --nologo
dotnet build "%ProjectPath%" -c Release --no-restore --nologo