Notes on producing test examples.

This output is an FxCop run against a debug build of SARIF-SDK 

0. Enlist in the SARIF-SDK. You will update the test files in this version.
1. Git clone into a second version of the tree, e.g.:
   git clone https://github.com/microsoft/sarif-sdk sarif-sdk-test
2. cd sarif-sdk-test. git checkout git checkout fc172277895ed80cb3c03d0ce66738cff89aad75
3. BuildAndTest.cmd --Configuration Debug -NoTest (debug to emit in-source suppressions)
4. Open SarifSdk.FxCop in the non-test enlistment.
5. Disable the Phoenix engine (Tools/Settings/Analysis Engines)
6. Analyze. You will need to 'Skip' an indirect assembly reference
7. Enable Phoenix engine. Analyze again (this generates 'new' messages)
8. Save project. 
9. File/Save Report As... to SarifSdk.xml