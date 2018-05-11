@echo off
PUSHD %~dp0
dotnet restore fake-bootstrap.csproj
dotnet fake %*
SET ret=%ERRORLEVEL%
POPD
EXIT /B %ret%