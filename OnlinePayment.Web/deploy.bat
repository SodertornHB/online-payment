@echo off
setlocal

:: Check if the required arguments are provided
if "%1"=="" (
    echo Usage: deploy.bat app_name resource_group
    exit /b 1
)
if "%2"=="" (
    echo Usage: deploy.bat app_name resource_group
    exit /b 1
)

:: Set the parameters
set "APP_NAME=%1"
set "RESOURCE_GROUP=%2"

az webapp deploy --resource-group %RESOURCE_GROUP% --name %APP_NAME% --src-path .\bin\Release\net8.0\publish\publish.zip --type zip --clean

:: Build the project
dotnet build . -c Release

:: Publish the project
dotnet publish . -c Release -o ./publish

:: Remove existing zip file if it exists
if exist .\bin\Release\net8.0\publish\publish.zip (
    del .\bin\Release\net8.0\publish\publish.zip
)

:: Zip the published files
powershell -Command "Compress-Archive -Path ./publish/* -DestinationPath .\bin\Release\net8.0\publish\publish.zip"

:: Deploy using Azure CLI
az webapp deploy --resource-group %RESOURCE_GROUP% --name %APP_NAME% --src-path .\bin\Release\net8.0\publish\publish.zip --type zip

az webapp restart --resource-group %RESOURCE_GROUP% --name %APP_NAME%

