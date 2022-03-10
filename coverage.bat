@echo off
rd /s /q coverage 2>nul
md coverage

set configuration=Debug
set opencover="%USERPROFILE%\.nuget\packages\OpenCover\4.7.1221\tools\OpenCover.Console.exe"
set reportgenerator="%USERPROFILE%\.nuget\packages\ReportGenerator\5.0.4\tools\net47\ReportGenerator.exe"
set testrunner="%USERPROFILE%\.nuget\packages\xunit.runner.console\2.4.1\tools\net472\xunit.console.x86.exe"
set target=".\src\CSharpGuidelinesAnalyzer\CSharpGuidelinesAnalyzer.Test\bin\%configuration%\net48\CSharpGuidelinesAnalyzer.Test.dll -noshadow"
set filter="+[CSharpGuidelinesAnalyzer*]*  -[CSharpGuidelinesAnalyzer.Test*]*"
set coveragefile=".\coverage\CodeCoverage.xml"

%opencover% -register:user -target:%testrunner% -targetargs:%target% -filter:%filter% -hideskipped:All -output:%coveragefile%
%reportgenerator% -targetdir:.\coverage -reports:%coveragefile%
