# Authoring Principles for SARIF Validation Rules

This document explains the principles that should guide the definition, implementation, and maintainance of SARIF validation rules.

## Rule granularity

We say that a rule has "coarse granularity" if it detects a large number of loosely related conditions,
and "fine granularity" if it detects a small number of closely related conditions.
There are a few principles you can use to decide on the appropriate granularity for a new rule.

### Think of the validator as a SARIF "help system" and its rules as "help topics".

A SARIF developer might want to learn about topics such as authoring high quality result messages or creating `region` objects that conform to the constraints of the spec.
Consider representing each help topic as a validation rule.

For example, you might create rules named `AuthorHighQualityResultMessages` or `ProvideConsistentRegionProperties`.

### Represent help sub-topics with distinct result messages.

Some help topics are large enough to benefit from being divided into sub-topics.
Consider providing a different result message for each sub-topic.

For example, authoring high quality result messages is a large topic.
Although not all characteristics of a high quality message are easily detectable be a tool, some of them are.
You might create result messages whose names describe the conditions the rule can detect:

- `TerminateResultMessagesWithPeriod`
- `IncludeDynamicDataInResultMessages` (this means placeholders such as "`{1}`")

**NOTE**: Although the validator does not do this today, the result message names might in future be treated as sub-rule names.
This would allow SARIF consumers the flexibility to suppress the results on a per-sub-rule basis (see the next section).

### Consider the effect of disabling a rule, or suppressing a subset of its results.

Not all teams will find all validation rules equally valuable, and they might want to disable certain rules.
If a rule is too coarse, disabling it might remove results the team considers valuable along with the unwanted results.
For that reason, be careful about how much you put into a single rule.

Another option to eliminate results from consideration is to suppress them.
Define your result messages in such a way that it would be sensible for a team to suppress all
results that produce that message.

**NOTE**: In future, if the validator represents the results from each result message as distinct sub-rules,
it will be possible for a team to suppress results on a per-sub-rule basis.

**NOTE**: At the time of this writing, the validator does not provide a way to disable rules,
but teams can get the same effect by post-processing the SARIF file to remove the results from unwanted rules,
or by suppressing all results from unwanted rules.

### Examples

Given these overarching principles, you might group validation logic into analyzers in many ways, for example:

- Combine all requirements on a given property into one analyzer.

  Example: You might assign the requirements "`run.$schema` should be present." and "`run.$schema` should refer to the final version of the SARIF specification." to a single analyzer, `ReferToFinalSchema`.

- Combine all requirements on a given property _type_ (_e.g._, "uri", or "result message string") into one analyzer.

  Example: You might assign the requirements "Result message strings should contain dynamic data." and "Result message strings should end with a period." to a single analyzer, `AuthorHighQualityResultMessages`.

- Combine all consistency constraints among the properties of a given object into one analyzer.

  Example: You might assign the requirements "`region.startLine` should not be greater than `region.endLine`." and "`region.startColumn` should not be greater than `region.endColumn`." to a single analyzer, `ProvideConsistentRegionProperties`.

- Combine highly granular quality constraints on the properties of a single object into one analyzer.

  Example: You might assign the requirements "`toolComponent.version` should be present." and "`toolComponent.name` should be no more than three words long." to a single analyzer, `ProvideStandardToolMetadata`.

- Combine similar formatting recommendations into one analyzer.

  Example: You might assign the requirements "`rule.id` should follow conventional format (_e.g._ 'CA1001')." and "Use conventional names for `uriBaseIds` (_e.g_, 'SRCROOT')." to a single analyzer, `UseConventionalSymbolicNames`.

## Requirements and recommendations

The SARIF validator tool first checks that the file is syntactically valid JSON,
and then checks that it conforms to the SARIF schema.
Only if those checks pass does the validator run the validation rules.

The SARIF spec imposes some constraints that a JSON schema cannot represent. (For example, it requires that exactly one of `result.ruleId` and `result.rule.id` be present, and that if both are present, they have the same value.)

In defining a rule's "level" (SARIF's term for what is more commonly called "severity"),
use the `"error"` level only for rules that detect such constraints.
For rules that embody recommendations (for example, a rule that enforce result message quality), use the `"warning"` or `"note"` level.

## Message quality

- In the [SARIF Tutorials](https://github.com/microsoft/sarif-tutorials), the appendix [Authoring rule metadata and result messages](https://github.com/microsoft/sarif-tutorials/blob/master/docs/Authoring-rule-metadata-and-result-messages.md) provides detailed guidance for authoring informative, actionable result messages. See the section "Message strings".

- In the rule's `fullDescription` and in the result messages, include links to the relevant sections of the [SARIF Specification](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html), and to the [SARIF Tutorials](https://github.com/microsoft/sarif-tutorials), if the tutorials cover the topic.

## Rule names

Rule names should be concise, informative, and uniform in tone.

- The friendly rule name MUST be in PascalCase.

- The friendly rule name SHOULD be use the imperative [grammatical mood](https://en.wikipedia.org/wiki/Grammatical_mood), telling the user what to do (for example, `UseAbsoluteUris`).

- If the imperative mood produces an awkward name, try the indicative mood, describing the erroneous condition (for example, `EndTimeIsBeforeStartTime`). In this case, `DoNotSpecifyEndTimeBeforeStartTime` seems awkward.
You might prefer a name like `EndTimeMustNotPrecedeStartTime`.

## Result message names

Result message names should be concise, informative, and uniform in tone.
Just as for rule names, result message names MUST be in PascalCase and SHOULD use the imperative mood (for example, `TerminateResultMessagesWithPeriod`).
