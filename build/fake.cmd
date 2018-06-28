@echo off
PUSHD %~dp0

SET TOOL_PATH=.fake
IF NOT EXIST "%TOOL_PATH%\fake.exe" (
  dotnet tool install fake-cli --tool-path ./%TOOL_PATH% --version 5.*
)

"%TOOL_PATH%/fake.exe" %*
SET ret=%ERRORLEVEL%

POPD
EXIT /B %ret%