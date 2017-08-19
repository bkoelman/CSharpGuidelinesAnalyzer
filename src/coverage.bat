@echo off
rd /s /q coverage 2>nul
md coverage

set opencover="%USERPROFILE%\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe"
set reportgenerator="%USERPROFILE%\.nuget\packages\ReportGenerator\2.5.10\tools\ReportGenerator.exe"
set testrunner="%USERPROFILE%\.nuget\packages\xunit.runner.console\2.2.0\tools\xunit.console.x86.exe"
set target=".\CSharpGuidelinesAnalyzer\CSharpGuidelinesAnalyzer.Test\bin\Debug\net46\CSharpGuidelinesAnalyzer.Test.dll -noshadow"
set coveragefile=".\coverage\CSharpGuidelinesAnalyzerCoverage.xml"

%opencover% -register:user -target:%testrunner% -targetargs:%target% -excludebyattribute:*.ExcludeFromCodeCoverage* -hideskipped:All -output:%coveragefile%
%reportgenerator% -targetdir:.\coverage -reports:%coveragefile%

