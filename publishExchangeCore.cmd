echo off
call %HOMEPATH%\.scripts\SetSecret.cmd
dotnet pack --configuration Release sabatex.WebApiDocumentsExchange.Core\sabatex.WebApiDocumentsExchange.Core.csproj
if %ERRORLEVEL% neq 0 goto error
echo Build OK
dotnet nuget push sabatex.WebApiDocumentsExchange.Core\bin\Release\*.nupkg -k %NUGET_AUTH_TOKEN% -s https://api.nuget.org/v3/index.json --skip-duplicate
if %ERRORLEVEL% neq 0 goto error
echo Publish OK
goto end
:error
echo Error publish
:end