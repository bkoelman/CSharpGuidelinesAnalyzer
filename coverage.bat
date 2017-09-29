@echo off
rd /s /q coverage 2>nul
md coverage

set configuration=Debug
set opencover="%USERPROFILE%\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe"
set reportgenerator="%USERPROFILE%\.nuget\packages\ReportGenerator\3.0.0\tools\ReportGenerator.exe"
set testrunner="%USERPROFILE%\.nuget\packages\xunit.runner.console\2.2.0\tools\xunit.console.x86.exe"
set target=".\src\CSharpGuidelinesAnalyzer\CSharpGuidelinesAnalyzer.Test\bin\%configuration%\net46\CSharpGuidelinesAnalyzer.Test.dll -noshadow"
set filter="+[CSharpGuidelinesAnalyzer*]*  -[CSharpGuidelinesAnalyzer.Test*]*"
set coveragefile=".\coverage\CodeCoverage.xml"

%opencover% -register:user -target:%testrunner% -targetargs:%target% -filter:%filter% -hideskipped:All -output:%coveragefile%
%reportgenerator% -targetdir:.\coverage -reports:%coveragefile%
