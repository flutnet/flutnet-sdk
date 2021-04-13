@echo off
cls

set ProjectName=FlutnetUI

:: Define paths
set ScriptDir=%~dp0
set ArtifactsDir=%ScriptDir%\..\artifacts
set SolutionPath=%ScriptDir%\..\%ProjectName%.sln
set ProjectPath=%ScriptDir%\..\src\%ProjectName%\%ProjectName%.csproj
set PublishDirOsx=%ArtifactsDir%\osx-x64\%ProjectName%
set PublishDirWin=%ArtifactsDir%\win-x64\%ProjectName%

:: Clean the output of the previous builds 
:: (ignore any error, the clean will still complete successfully)
dotnet clean "%ProjectPath%" -c Release -f netcoreapp3.1 -r osx-x64 -v quiet --nologo
dotnet clean "%ProjectPath%" -c Release -f netcoreapp3.1 -r win-x64 -v quiet --nologo

:: Clean the output of the previous publish operations
if exist %PublishDirOsx% rmdir /s /q %PublishDirOsx%
if exist %PublishDirWin% rmdir /s /q %PublishDirWin%

:: Publish exeutable for macOS
dotnet publish "%ProjectPath%" -c Release -f netcoreapp3.1 -o "%PublishDirOsx%" -r osx-x64 --no-self-contained -v minimal --nologo

:: Publish exeutable for Windows
dotnet publish "%ProjectPath%" -c Release -f netcoreapp3.1 -o "%PublishDirWin%" -r win-x64 --no-self-contained -v minimal --nologo