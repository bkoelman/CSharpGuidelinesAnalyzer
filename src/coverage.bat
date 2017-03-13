@echo off
rd /s /q coverage 2>nul
md coverage
.\packages\OpenCover.4.6.519\tools\OpenCover.Console.exe -register:user -target:".\packages\xunit.runner.console.2.2.0\tools\xunit.console.x86.exe" -targetargs:".\CSharpGuidelinesAnalyzer\CSharpGuidelinesAnalyzer.Test\bin\Debug\CSharpGuidelinesAnalyzer.Test.dll -noshadow" -filter:"+[CSharpGuidelinesAnalyzer]*  -[CSharpGuidelinesAnalyzer]CSharpGuidelinesAnalyzer.Properties.*" -excludebyattribute:*.ExcludeFromCodeCoverage* -hideskipped:All -output:.\coverage\CSharpGuidelinesAnalyzerCoverage.xml
.\packages\ReportGenerator.2.5.6\tools\ReportGenerator.exe -targetdir:.\coverage -reports:.\coverage\CSharpGuidelinesAnalyzerCoverage.xml
