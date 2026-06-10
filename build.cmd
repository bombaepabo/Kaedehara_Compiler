@echo off
dotnet build --ignore-failed-sources
dotnet test .\Kaedehara.Tests\Kaedehara.Tests.csproj --no-restore --no-build