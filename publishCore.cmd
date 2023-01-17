echo off
del Sabatex.Core\bin\Release\*.nupkg
call %HOMEPATH%\.scripts\SetSecret.cmd
dotnet pack --configuration Release Sabatex.Core\Sabatex.Core.csproj
if %ERRORLEVEL% neq 0 goto error
echo Build OK
dotnet nuget push Sabatex.Core\bin\Release\*.nupkg -k %NUGET_AUTH_TOKEN% -s https://api.nuget.org/v3/index.json --skip-duplicate
if %ERRORLEVEL% neq 0 goto error
echo Publish OK
goto end
:error
echo Error publish
:end