Notes on producing test examples.

This output is a Semmle run against a debug build of SARIF-SDK 

0.  Git clone into a test SARIF-SDK enlistment: `git clone https://github.com/microsoft/sarif-sdk sarif-sdk-test`
1. cd sarif-sdk-test. git checkout fc172277895ed80cb3c03d0ce66738cff89aad75
2. Produce a Semmle project: `odasa bootstrap`
3. cd into the project + revision directory. Create a snapshot: `odasa buildSnapshot --overwrite`
4. Run Microsoft rules suite: `analyzeSnapshot --suite E:\repros\semmle\odasa\microsoft-queries\suites\csharp\all --output-file Microsoft.Semmle.sarif --format sarifv2`
2. Run all Semmle default rules, e.g.`for %i in (E:\repros\semmle\odasa\queries\suites\csharp\*.) do odasa analyzeSnapshot --format sarifv2 --suite %i --output-file Semmle.%~ni.sarif`
3. Run multitool to rewrite each Semmle SARIF file: `for %i in (*.sarif) do e:\src\sarif-sdk\bld\bin\AnyCPU_Release\Sarif.Multitool\net461\Sarif.Multitool.exe rewrite %i --force --pretty-print -u "%SRCROOT%=https://raw.githubusercontent.com/Microsoft/sarif-sdk/fc172277895ed80cb3c03d0ce66738cff89aad75/" --insert "Hashes;TextFiles;RegionSnippets;ContextRegionSnippets" --inline`
4. Run multitool to merge all files together: `Sarif.Multitool.exe merge *.sarif --pretty-print --output-file SarifSdk.sarif`
