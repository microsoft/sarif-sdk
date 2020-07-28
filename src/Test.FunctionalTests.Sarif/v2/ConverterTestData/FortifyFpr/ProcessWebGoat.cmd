set %DRIVE%=%DRIVE%

if exist *webgoat*.sarif (
del /q *webgoat*.sarif
)

set MT=%DRIVE%\src\sarif\bld\bin\AnyCPU_Debug\Sarif.Multitool\netcoreapp3.1\Sarif.Multitool.exe

%MT% convert -t FortifyFpr %DRIVE%\src\sarif\src\Test.FunctionalTests.Sarif\v2\ConverterTestData\FortifyFpr\WebGoat.fpr -o WebGoat.fpr.sarif -f -p --normalize-for-github-dsp

%MT% page WebGoat.fpr.sarif -i 0 -c 5 -o WebGoat.fpr.page.1.sarif -f 
%MT% page WebGoat.fpr.sarif -i 5 -c 500 -o WebGoat.fpr.page.2.sarif -f 

%MT% validate %DRIVE%\src\sarif\src\Test.FunctionalTests.Sarif\v2\ConverterTestData\FortifyFpr\WebGoat.fpr.page.1.sarif
%MT% validate %DRIVE%\src\sarif\src\Test.FunctionalTests.Sarif\v2\ConverterTestData\FortifyFpr\WebGoat.fpr.page.2.sarif

copy WebGoat.fpr.page.1.sarif %DRIVE%\repros\WebGoat.sarif

if "%1" EQU "DSP" (
pushd %DRIVE%\src\dsp-testing
call %DRIVE%\src\sarif\src\Test.FunctionalTests.Sarif\v2\ConverterTestData\FortifyFpr\DoIt.cmd
popd
)