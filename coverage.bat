@echo off
rd /s /q coverage 2>nul
md coverage

set configuration=Debug
set opencover="%USERPROFILE%\.nuget\packages\OpenCover\4.7.922\tools\OpenCover.Console.exe"
set reportgenerator="%USERPROFILE%\.nuget\packages\ReportGenerator\4.0.13.1\tools\net47\ReportGenerator.exe"
set testrunner="%USERPROFILE%\.nuget\packages\xunit.runner.console\2.4.1\tools\net46\xunit.console.x86.exe"
set target=".\src\CSharpGuidelinesAnalyzer\CSharpGuidelinesAnalyzer.Test\bin\%configuration%\net46\CSharpGuidelinesAnalyzer.Test.dll -noshadow"
set filter="+[CSharpGuidelinesAnalyzer*]*  -[CSharpGuidelinesAnalyzer.Test*]*"
set coveragefile=".\coverage\CodeCoverage.xml"

%opencover% -register:user -target:%testrunner% -targetargs:%target% -filter:%filter% -hideskipped:All -output:%coveragefile%
%reportgenerator% -targetdir:.\coverage -reports:%coveragefile%
