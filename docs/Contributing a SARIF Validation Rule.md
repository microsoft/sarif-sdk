# Contributing a SARIF Validation Rule

1. File a (or locate an existing) sarif-sdk issue that serves as the specification for the rule.
2. Create a shell rule.
3. Prepare test assets for the rule.
4. Author standard functional tests.
5. Implement the analysis. 
6. Review the impact of the new analysis on the test assets and update "expected outputs" if necessary.
----
## File or locate a rule request.

1. Create a "Validation rule request" issue in the `sarif-sdk` repository by clicking **New issue** and selecting the **Validation rule request** template (or just click [here](https://github.com/microsoft/sarif-sdk/issues/new?assignees=&labels=rule-request&template=validation-rule-request.md&title=%5BRULE+REQUEST%5D+Concise+description+of+new+analysis)).

    **CONCERN** As written, that URL requires the user to have "add labels" permission.

- The issue template contains additional guidance for authoring a complete rule specification.

- See the [CET shadow stack compatibility](https://github.com/microsoft/sarif-sdk/issues/277) rule specification for an example.

    **TODO** Update to a real example ASAP.

2. Work with a repository maintainer to iron out details in implementation, _etc._

3. Rules marked with the `approved` label are ready to implement.

## Create shell rule for implementation.

1. Create a constant for the rule id in [RuleId.cs](https://github.com/microsoft/sarif-sdk/blob/master/src/Sarif.Multitool/Rules/RuleId.cs). The name of the constant must match the rule friendly name, _e.g._, `ReferToFinalSchema`, and its value must be the rule identifier, _e.g._, `"SARIF1020"`.

2. Open [RuleResources.resx](https://github.com/microsoft/sarif-sdk/blob/master/src/Sarif.Multitool/Rules/RuleResources.resx) and create new user-facing strings for the rule description and for the result. See  The string values should be retrieved from the approved rule specific/issue. You can start rules development with placeholder values, obviously.

3. Add the rule description from the specification issue to a string named as RULEID_FRIENDLYNAME_Decription, e.g., `BA2025_EnableControlEnforcementTechnologyShadowStack_Description`. Again, you can use a placeholder if this text is still under refinement.

4. Open the shell rule starter code in [BAXXXX.RuleFriendlyName.cs]() and save a copy to either the [ELFRules] or [PERules] directory, depending on the target platform for the rule. Rename the file on save to conform to the actual assigned rule id and friendly rule name, e.g., `BA2025.EnableControlEnforcementTechnologyShadowStack`.

5. Find/Replace all occurrences of `BAXXXX` in this file with the rule id.

6. Find/Replace all occurrences of `RULEFRIENDLYNAME` in this file with the rule name. 



----
This section originally appeared in the "principles" document. Integrate it into this section.

- The rule description resource string MUST be named `<ruleId>_<ruleName>`, _e.g._, `SARIF1020_ReferToFinalSchema`.

- Result message string resource strings MUST named `<ruleId>_<messageStringId>`, e.g., `SARIF1020_SchemaReferenceMissing`. These names must be concise and informative.

- Every result message string resource MUST have use a "real" friendly name, even if there is only one in the analyzer. Do NOT use a generic name like "default" for a singleton message string.

- Result message string resource names MUST be PascalCase (except for the underscore between the ruleId and the friendly name).
---
## Prepare test assets.

- Every rule should be tested against one SARIF file that explicitly violates the rule (covering every check that the rule performs), and one that conforms to the rule.
- Both passing and failing SARIF files should conform to all other . I.e., the goal is for test binaries to be entirely clean in the `pass` case and to only fire results for the new rule in the `rule` case.
- Create a directory in the rules functional test directory that matches the rule id and friendly name, separated with a dot character, e.g. [BA2025.EnableControlEnforcementTechnologyShadowStack](https://github.com/microsoft/binskim/tree/master/src/Test.FunctionalTests.BinSkim.Rules/FunctionalTestsData/BA2025.EnableControlEnforcementTechnologyShadowStack).
- Create directories named [Pass](https://github.com/microsoft/binskim/tree/master/src/Test.FunctionalTests.BinSkim.Rules/FunctionalTestsData/BA2025.EnableControlEnforcementTechnologyShadowStack/Pass) and [Fail](https://github.com/microsoft/binskim/tree/master/src/Test.FunctionalTests.BinSkim.Rules/FunctionalTestsData/BA2025.EnableControlEnforcementTechnologyShadowStack/Fail) in this directory and copy relevant secure and vulnerable test binaries to their respective location.
By convention, test binary names indicate their language, bittedness/processor, toolchain, and kind, with each attribute separated by an underscore. `Native_x64_VS2019_Console.exe`, for example, indicates a C++ Intel 64-bit console application compiled by the Microsoft Visual Studio 2019 toolchain.
- In some cases, it may be useful to create a specific binary to test proper return of the BinSkim `notApplicable` result (which indicates that the binary itself is not a relevant candidate for analysis). For many rules, the standard BinSkim "zoo" of test binaries can be used to verify proper enforcement of applicability. 

## Author functional tests that use the new test assets


## Implement the analysis

There's not much more to say about this.

## Review the impact of the new analysis on the test assets and update "expected outputs" if necessary

Since the test assets for the existing rules were authored before you created the new rule, it's possible that some of them will violate the new rule. You need to correct any new violations in existing test assets so that tests for the existing rules continue to pass