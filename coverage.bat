@echo off
rd /s /q coverage 2>nul
del /s /q coverage.cobertura.xml 2>nul

dotnet test src --configuration Release --no-build --collect:"XPlat Code Coverage"
dotnet reportgenerator -reports:**\coverage.cobertura.xml -targetdir:coverage
