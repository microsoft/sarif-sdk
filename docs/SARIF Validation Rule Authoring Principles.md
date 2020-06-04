# Authoring Principles for SARIF Validation Rules

Keep these principles in mind when defining, implementing, and maintaining a SARIF validation rule.

## Author high quality result messages

- In the [SARIF Tutorials](https://github.com/microsoft/sarif-tutorials), the appendix [Authoring rule metadata and result messages](https://github.com/microsoft/sarif-tutorials/blob/master/docs/Authoring-rule-metadata-and-result-messages.md) provides detailed guidance for authoring informative, actionable result messages. See the section "Message strings".

## Group requirements into analyzers at an appropriate and uniform level of granularity.

- Combine all requirements on a given property into one analyzer.

  Example: The requirements "`run.$schema` should be present" and "`run.$schema` should refer to the final version of the SARIF specification" belong in a single analyzer, `ReferToFinalSchema`.

- Combine all requirements on a given property _type_ (_e.g._, "uri", or "result message string") into one analyzer.

  Example: "The requirements "Result message strings should contain dynamic data" and "Result message strings should end with a period" belong in a single analyzer, `AuthorHighQualityResultMessages`.

- Combine all consistency constraints among the properties of a given object into one analyzer.

  Example: The requirements "`region.startLine` should not be greater than `region.endLine`" and "`region.startColumn` should not be greater than `region.endColumn`" belong in a single analyzer, `ProvideConsistentRegionProperties`.

- Combine highly granular quality constraints on the properties of a single object into one analyzer.

  Example: The requirements "`toolComponent.version` should be present" and "`toolComponent.name` should be no more than three words long" belong in a single analyzer, `ProvideStandardToolMetadata`.

- Combine similar formatting recommendations into one analyzer.

  Example: The requirements "`rule.id` should follow conventional format (_e.g._ 'CA1001')" and "

## Specify concise, informative, uniform friendly rule names

- The friendly rule name MUST be in PascalCase.

- The friendly rule name SHOULD be use the imperative [grammatical mood](https://en.wikipedia.org/wiki/Grammatical_mood), telling the user what to do (for example, `UseAbsoluteUris`).

- If the imperative mood produces an awkward name, try the indicative mood, describing the erroneous condition (for example, `EndTimeIsBeforeStartTime`). In this case, `DoNotSpecifyEndTimeBeforeStartTime` seems awkward.
like this: `EndTimeMustNotBeBeforeStartTime`.

## Name resource strings consistently

- The rule description resource string MUST be named `<ruleId>_<ruleName>`, _e.g._, `SARIF1020_ReferToFinalSchema`.

- Result message string resource strings MUST named `<ruleId>_<messageStringId>`, e.g., `SARIF1020_SchemaReferenceMissing`. These names must be concise and informative.

- Every result message string resource MUST have use a "real" friendly name, even if there is only one in the analyzer. Do NOT use a generic name like "default" for a singleton message string.

- Result message string resource names MUST be PascalCase (except for the underscore between the ruleId and the friendly name).