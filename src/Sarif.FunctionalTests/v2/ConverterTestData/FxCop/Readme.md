Notes on producing test examples.

This output is an FxCop run against a debug build of SARIF-SDK 

1. Git checkout 
2. BuildAndTest.cmd --Configuration Debug (debug to emit in-source suppressions)
3. Open SarifSdk.FxCop. Disable the Phoenix engine (Tools/Settings/Analysis Engines)
4. Analyze. You will need to 'Skip' an indirect assembly reference
5. Enable Phoenix engine. Analyze again (this generates 'new' messages)
6. Save project. 
7. File/Save Report As... to SarifSdk.xml