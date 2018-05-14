set TOOL=d:\src\binskim\bld\bin\BinSkim.Driver\x64_Release\BinSkim.exe
set OUTPUT=.\
set TEST_ROOT=d:\src\binskim\src\BinSkim.Rules.FunctionalTests\FunctionalTestsData

%TOOL% analyze %TEST_ROOT%\*.dll %TEST_ROOT%\*.exe --hashes --recurse --config default -o %OUTPUT%\BinSkim.AllRules.sarif

xcopy /y d:\src\binskim\src\BinSkim.Driver.FunctionalTests\BaselineTestsData\Expected\*.sarif .
