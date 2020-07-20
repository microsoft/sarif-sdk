# Rules and guidelines for producing effective SARIF

## Introduction

This document is for creators of static analysis tools who want to produce SARIF output that's useful to SARIF consumers, both human and automated.

Teams can use SARIF log files in many ways. They can view the results in an IDE extension such as the [SARIF extension for VS Code](https://marketplace.visualstudio.com/items?itemName=MS-SarifVSCode.sarif-viewer) or the [SARIF Viewer VSIX for Visual Studio](https://marketplace.visualstudio.com/items?itemName=WDGIS.MicrosoftSarifViewer), or in a [web-based viewer](https://microsoft.github.io/sarif-web-component/). They can import it into a static analysis results database, or use it to drive automatic bug fiing. Most important, developers use the information in a SARIF log file to understand and fix the problems it reports.

Because of this variety of usage scenarios, a SARIF log file that is useful in one scenario might not be useful in another. Ideally, static analysis tools will provide options to let their users specify the output that meets their needs.

The [SARIF specification](https://docs.oasis-open.org/sarif/sarif/v2.1.0/sarif-v2.1.0.html) defines dozens of objects with hundreds of properties. It can be hard to decide which ones are important (aside from the few that the spec says are mandatory). What information is most helpful to developers? What information should you include if you want to file useful bugs from the SARIF log?

On top of all that, the spec is written in format language that's hard to read. If you want to learn SARIF, take a look at the [SARIF Tutorials](https://github.com/microsoft/sarif-tutorials).

The purpose of this document is to cut through the confusion and provide clear guidance on what information your tool should include in a SARIF file, and how to make that information as helpful and usable as possible.

## Principles for producing effective SARIF

This document contains dozens of individual rules and guidelines for producing effective SARIF, but they all derive from a handful of bedrock principles.

### Readability/Understandability/Actionability

The most important elements of a SARIF log file are the "result messages". A result messages must be readable, understandable, and actionable. It must describe exactly what went wrong, why it's a problem, and how to fix it -- and it must do all of that in one compact, well-written plain-text paragraph. (You can supply Markdown messages as well, but the plain-text message is required because not every SARIF consumer can interpret Markdown.)

### Fitness for purpose

The SARIF format has many optional properties. Some of them depend on what kind of analysis tool you are writing. For example, a Web analyzer will probably emit `run.webRequests` and `.webResponses`; a crash dump analyzer might emit `run.addresses`.

Other optional properties are more or less useful depending how end users or downstream systems plan to use the logs. For example:
- If you plan to use SARIF log files as the input to an automated bug filing system, you'll want to populate `result.partialFingerprints` to make it easier to determine which results are new in each run.
- If you plan to view the results in an environment where you don't have access to the source code (for example, in a Web-based SARIF viewer), you'll want to populate `physicalLocation.contextRegion` so that the viewer can display a few lines of code around the actual location of the result. You might even want to populate `run.artifacts[].contents`, which contains the entire contents of the artifact that was analyzed.
- If you plan to use SARIF files as an input to a compliance system, you might want to populate `run.tool.driver.rules` with the complete set of rules that were run, even if most of them didn't produce any results. Similarly, you might want to populate `run.artifacts` with the complete list of files that were analyzed, even if most of them didn't contain any results.

### Compactness

SARIF files from large projects can be huge: multiple gigabytes in size, with over a million results. Even though a great deal of work has been and is being done to compress SARIF files and make them faster to access, it's still important not to unnecessarily increase log file size.

Some optional SARIF properties can take up alot of space, most notably `artifact.contents`.

In some cases, SARIF can represent the same information in multiple places in the log file. For example, a `result` object can (and usually does) specify the result's location with a URI, but that same URI appears in the `run.artifacts` array. Deciding which duplicative information to include is a trade-off between file size on the one hand and what we might call "local readability" on the other.

In short, both "fitness for purpose" and "compactness" are important values, they are in tension with each other, and so it's important for analysis tools to provide flexibility in which properties they emit.

Having said that, the SARIF MultiTool can "enrich" SARIF files with additional properties after the fact (especially if the MultiTool has access to the source code). So one possible strategy is for a tool to emit a minimal SARIF file, and rely on consumers to enrich it as necessary to address specific usage scenarios.

### Serviceability

SARIF files are often used in scenarios where it's important to know which tool, and which _version_ of the tool, produced the results. Therefore it's important for your tool to populate `run.tool.driver` with enough information to identify your tool and its version.

### What's next

The remainder of this document will present a set of specific rules and guidelines, all of them aimed at producing SARIF that conforms to these principles.

## Structural requirements

Many of SARIF's structural requirements are expressed in a [JSON schema](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/schemas/sarif-schema-2.1.0.json), but the schema can't express all the structural requirements. In addition to providing helpful, useful information, it's important for tools to produce SARIF that meet all the structural requirements, even the ones that the schema can't express.

## The SARIF validation tool

The SARIF validation tool (the "validator") helps ensure that a SARIF file conforms to SARIF's structural requirements as well as to the guidelines for producing effective SARIF output.

### What the validator does

Here's how the validator process a SARIF log file:

1. If the file is not valid JSON, report the syntax error and stop.
2. If the file does not conform to the SARIF schema, report the schema violations and stop.
3. Run a set of analysis rules. Report any rule violations.

The analysis rules in Step 3 fall into two categories:

1. Rules that detect structural problems that the schema can't express.
2. Rules that enforce guidelines for producing effective SARIF.

The validator is incomplete: it does not enforce every structural condition in the spec, nor every guideline for producing effective SARIF. We hope to continue to add analysis rules in both those areas.

### Installing and using the validator

To install the validator, run the command
```
dotnet tool install Sarif.Multitool --global
```
Now you can validate a SARIF file by running the command
```
sarif validate MyFile.sarif --output MyFile-validation.sarif
```
The SARIF Multitool can do many things besides validate a SARIF file (that's why it's called the "MultiTool"). To see what it can do, just run the command
```
sarif
```

## How this document is organized

This document expresses each structural requirement and guideline as a validator analysis rule. At the time of this writing, not all of those rules actually exist. Those that do not are labled "(NYI)".

First come the rules that detect serious violations of the SARIF spec (rules which the validator would report as `"error"`). They have numbers in the range 1000-1999, for example, `SARIF1001.RuleIdentifiersMustBeValid`.

Then come the rules that detect either less serious violations of the SARIF spec (rules which the validator would report as `"warning"` or `"note"`), or guidance based on integrating SARIF into a wide variety of static analysis tools. They have numbers in the range 2000-2999, for example, `SARIF2005.ProvideToolProperties`.

Each rule has a description that describes its purpose, followed by one or more messages that can appear in a SARIF result object that reports a violation of this rule. Each message includes one or more replacement sequences (`{0}`, `{1}`, _etc._). The first one (`{0}`) is always a JSON path expression that describes the location of the result. For example, `/runs/0/results/0/locations/0/physicalLocation` specifies the `physicalLocation` property of the first location of the first result in the first run in the log file.

## Rules that describe serious violations

Rules that describe violations of **SHALL**/**SHALL NOT** requirements of the [SARIF specification](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html) have numbers between 1000 and 1999, and always have level `"error"`.

---

### Rule `SARIF1001.RuleIdentifiersMustBeValid`

#### Description

The two identity-related properties of a SARIF rule must be consistent. The required 'id' property must be a "stable, opaque identifier" (the SARIF specification ([3.49.3](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317839)) explains the reasons for this). The optional 'name' property ([3.49.7](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317843)) is an identifer that is understandable to an end user. Therefore if both 'id' and 'name' are present, they must be different. If both 'name' and 'id' are opaque identifiers, omit the 'name' property. If both 'name' and 'id' are human-readable identifiers, then consider assigning an opaque identifier to each rule, but in the meantime, omit the 'name' property.

#### Messages

##### `Default`: error

{0}: The rule '{1}' has a 'name' property that is identical to its 'id' property. The required 'id' property must be a "stable, opaque identifier" (the SARIF specification ([3.49.3](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317839)) explains the reasons for this). The optional 'name' property ([3.49.7](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317843)) is an identifer that is understandable to an end user. Therefore if both 'id' and 'name' are present, they must be different. If they are identical, the tool must omit the 'name' property.

---

### Rule `SARIF1002.UrisMustBeValid`

#### Description

Specify a valid URI reference for every URI-valued property.

URIs must conform to [RFC 3986](https://tools.ietf.org/html/rfc3986). In addition, 'file' URIs must not include '..' segments. If symbolic links are present, '..' might have different meanings on the machine that produced the log file and the machine where an end user or a tool consumes it.

#### Messages

##### `UrisMustConformToRfc3986`: error

{0}: The string '{1}' is not a valid URI reference. URIs must conform to [RFC 3986](https://tools.ietf.org/html/rfc3986).

##### `FileUrisMustConformToRfc8089`: error

##### `FileUrisMustNotIncludeDotDotSegments`: error

{0}: The 'file' URI '{1}' contains a '..' segment. This is dangerous because if symbolic links are present, '..' might have different meanings on the machine that produced the log file and the machine where an end user or a tool consumes it.

### Rule `SARIF1004.ExpressUriBaseIdsCorrectly`

#### Description

When using the 'uriBaseId' property, obey the requirements in the SARIF specification [3.4.4](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317431) that enable it to fulfill its purpose of resolving relative references to absolute locations. In particular:

If an 'artifactLocation' object has a 'uriBaseId' property, its 'uri' property must be a relative reference, because if 'uri' is an absolute URI then 'uriBaseId' serves no purpose.

Every URI reference in 'originalUriBaseIds' must resolve to an absolute URI in the manner described in the SARIF specification [3.14.14](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317498).

#### Messages

##### `UriBaseIdRequiresRelativeUri`: error

{0}: This 'artifactLocation' object has a 'uriBaseId' property '{1}', but its 'uri' property '{2}' is an absolute URI. Since the purpose of 'uriBaseId' is to resolve a relative reference to an absolute URI, it is not allowed when the 'uri' property is already an absolute URI.

##### `TopLevelUriBaseIdMustBeAbsolute`: error

{0}: The '{1}' element of 'originalUriBaseIds' has no 'uriBaseId' property, but its 'uri' property '{2}' is not an absolute URI. According to the SARIF specification, every such "top-level" entry in 'originalUriBaseIds' must specify an absolute URI, because the purpose of 'originalUriBaseIds' is to enable the resolution of relative references to absolute URIs.

##### `UriBaseIdValueMustEndWithSlash`: error

{0}: The '{1}' element of 'originalUriBaseIds' has a 'uri' property '{2}' that does not end with a slash. The trailing slash is required to minimize the likelihood of an error when concatenating URI segments together.

##### `UriBaseIdValueMustNotContainDotDotSegment`: error

{0}: The '{1}' element of 'originalUriBaseIds' has a 'uri' property '{2}' that contains a '..' segment. This is dangerous because if symbolic links are present, '..' might have different meanings on the machine that produced the log file and the machine where an end user or a tool consumes it.

##### `UriBaseIdValueMustNotContainQueryOrFragment`: error

{0}: The '{1}' element of 'originalUriBaseIds' has a 'uri' property '{2}' that contains a query or a fragment. This is not valid because the purpose of the 'uriBaseId' property is to help resolve a relative reference to an absolute URI by concatenating the relative reference to the absolute base URI. This won't work if the base URI contains a query or a fragment.

---

### Rule `SARIF1005.UriMustBeAbsolute`

#### Description

Certain URIs are required to be absolute. For the most part, these are URIs that refer to http addresses, such as work items or rule help topics.

#### Messages

##### `Default`: error

{0}: The value of this property is required to be an absolute URI, but '{1}' is a relative URI reference.

---

### Rule `SARIF1006.InvocationPropertiesMustBeConsistent`

#### Description

The properties of an 'invocation' object must be consistent.

If the 'invocation' object specifies both 'startTimeUtc' and 'endTimeUtc', then 'endTimeUtc' must not precede 'startTimeUtc'. To allow for the possibility that the duration of the run is less than the resolution of the string representation of the time, the start time and the end time may be equal.

#### Messages

##### `EndTimeMustNotPrecedeStartTime`: error

{0}: The 'endTimeUtc' value '{1}' precedes the 'startTimeUtc' value '{2}'. The properties of an 'invocation' object must be internally consistent.

---

### Rule `SARIF1007.RegionPropertiesMustBeConsistent`

#### Description

The properties of a 'region' object must be consistent.

SARIF can specify a 'region' (a contiguous portion of a file) in a variety of ways: with line and column numbers, with a character offset and count, or with a byte offset and count. The specification states certain constraints on these properties, both within each property group (for example, the start line cannot be greater than end line) and between the groups (for example, if more than one group is present, they must independently specify the same portion of the file). See the SARIF specification ([3.30](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317685)).

#### Messages

##### `EndLineMustNotPrecedeStartLine`: error

{0}: In this 'region' object, the 'endLine' property '{1}' is less than the 'startLine' property '{2}'. The properties of a 'region' object must be internally consistent.

##### `EndColumnMustNotPrecedeStartColumn`: error

{0}: In this 'region' object, the 'endColumn' property '{1}' is less than the 'startColumn' property '{2}'. The properties of a 'region' object must be internally consistent.

##### `RegionStartPropertyMustBePresent`: error

{0}: This 'region' object does not specify 'startLine', 'charOffset', or 'byteOffset'. As a result, it is impossible to determine whether this 'region' object describes a line/column text region, a character offset/length text region, or a binary region.

---

### Rule `SARIF1008.PhysicalLocationPropertiesMustBeConsistent`

#### Description

Ensure consistency among the properties of a 'physicalLocation' object.

A SARIF 'physicalLocation' object has two related properties 'region' and 'contextRegion'. If 'contextRegion' is present, then 'region' must also be present, and 'contextRegion' must be a "proper superset" of 'region'. That is, 'contextRegion' must completely contain 'region', and it must be larger than 'region'. To understand why this is so we must understand the roles of the 'region' and 'contextRegion' properties.

'region' allows both users and tools to distinguish similar results within the same artifact. If a SARIF viewer has access to the artifact, it can display it, and highlight the location identified by the analysis tool. If the region has a 'snippet' property, then even if the viewer doesn't have access to the artifact (which might be the case for a web-based viewer), it can still display the faulty code.

'contextRegion' provides users with a broader view of the result location. Typically, it consists of a range starting a few lines before 'region' and ending a few lines after. Again, if a SARIF viewer has access to the artifact, it can display it, and highlight the context region (perhaps in a lighter shade than the region itself). This isn't terribly useful since the user can already see the whole file, with the 'region' already highlighted. But if 'contextRegion' has a 'snippet' property, then even a viewer without access to the artifact can display a few lines of code surrounding the actual result, which is helpful to users.

If the validator reports that 'contextRegion' is not a proper superset of 'region', then it's possible that the tool reversed 'region' and 'contextRegion'. If 'region' and 'contextRegion' are identical, the tool should simply omit 'contextRegion'.

#### Messages

##### `ContextRegionRequiresRegion`: error

{0}: This 'physicalLocation' object contains a 'contextRegion' property, but it does not contain a 'region' property. This is invalid because the purpose of 'contextRegion' is to provide a viewing context around the 'region' which is the location of the result. If a tool associates only one region with a result, it must populate 'region', not 'contextRegion'.

##### `ContextRegionMustBeProperSupersetOfRegion`: error

{0}: This 'physicalLocation' object contains both a 'region' and a 'contextRegion' property, but 'contextRegion' is not a proper superset of 'region'. This is invalid because the purpose of 'contextRegion' is to provide a viewing context around the 'region' which is the location of the result. It's possible that the tool reversed 'region' and 'contextRegion'. If 'region' and 'contextRegion' are identical, the tool must omit 'contextRegion'.

---

### Rule `SARIF1009.IndexPropertiesMustBeConsistentWithArrays`

#### Description

If an object contains a property that is used as an array index (an "index-valued property"), then that array must be present and must contain at least "index + 1" elements.

#### Messages

##### `TargetArrayMustExist`: error

{0}: This '{1}' object contains a property '{2}' with value {3}, but '{4}' does not exist. An index-valued property always refers to an array, so the array must be present.

##### `TargetArrayMustBeLongEnough`: error

{0}: This '{1}' object contains a property '{2}' with value {3}, but '{4}' has fewer than {5} elements. An index-valued properties must be valid for the array that it refers to.

---

### Rule `SARIF1010.RuleIdMustBeConsistent`

#### Description

Every result must contain at least one of the properties 'ruleId' and 'rule.id'. If both are present, they must be equal. See the SARIF specification ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)).

This validation rule is required because this constraint cannot be expressed in a JSON schema.

#### Messages

##### `ResultMustSpecifyRuleId`: error

{0}: This result contains neither of the properties 'ruleId' or 'rule.id'. The SARIF specification ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)) requires at least one of these properties to be present.

##### `ResultRuleIdMustBeConsistent`: error

{0}: This result contains both the 'ruleId' property '{1}' and the 'rule.id' property '{2}', but they are not equal. The SARIF specification ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)) requires that if both of these properties are present, they must be equal.

---

### Rule `SARIF1011.ReferenceFinalSchema`

#### Description

The '$schema' property must refer to the final version of the SARIF 2.1.0 schema. This enables IDEs to provide Intellisense for SARIF log files.

The SARIF standard was developed over several years, and many intermediate versions of the schema were produced. Now that the standard is final, only the OASIS standard version of the schema is valid.

#### Messages

##### `Default`: error

{0}: The '$schema' property value '{1}' does not refer to the final version of the SARIF 2.1.0 schema. If you are using an earlier version of the SARIF format, consider upgrading your analysis tool to produce the final version. If this file does in fact conform to the final version of the schema, upgrade the tool to populate the '$schema' property with a URL that refers to the final version of the schema.

---

### Rule `SARIF1012.MessageArgumentsMustBeConsistentWithRule`

#### Description

The properties of a result's 'message' property must be consistent with the properties of the rule that the result refers to.

When a result's 'message' object uses the 'id' and 'arguments' properties (which, by the way, is recommended: see SARIF2002.ProvideMessageArguments), it must ensure that the rule actually defines a message string with that id, and that 'arguments' array has enough elements to provide values for every replacement sequence in the message specified by 'id'. For example, if the highest numbered replacement sequence in the specified message string is '{{3}}', then the 'arguments' array must contain at least 4 elements.

#### Messages

##### `SupplyEnoughMessageArguments`: error

{0}: The message with id '{1}' in rule '{2}' requires '{3}' arguments, but the 'arguments' array in this message object has only '{4}' element(s). When a tool creates a result message that use the 'id' and 'arguments' properties, it must ensure that the 'arguments' array has enough elements to provide values for every replacement sequence in the message specified by 'id'. For example, if the highest numbered replacement sequence in the specified message string is '{{3}}', then the 'arguments' array must contain 4 elements.

##### `MessageIdMustExist`: error

{0}: This message object refers to the message with id '{1}' in rule '{2}', but that rule does not define a message with that id. When a tool creates a result message that uses the 'id' property, it must ensure that the specified rule actually has a message with that id.

---

## Rules that describe less serious violations and guidelines for effective SARIF

Rules that describe violations of **SHOULD**/**SHOULD NOT** requirements of the [SARIF specification](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html) have numbers between 2000 and 2999, and always have level `"warning"`.

Rules that describe violations of SARIF recommendations or best practices also have numbers in this range. Some of those recommendations are expressed in the spec as **MAY** requirements; others are based on experience using the format. These rules have level `"warning"` or `"note"`, depending on the tool's opinion of the seriousness of the violation.

This section also includes guidelines which, although not found in the spec text, are based on experience in integrating SARIF into a wide variety of static analysis tools.

---

### Rule `SARIF2001.TerminateMessagesWithPeriod`

#### Description

Express plain text result messages as complete sentences and end each sentence with a period. This guidance does not apply to Markdown messages, which might include formatting that makes the punctuation unnecessary.

This is part of a set of authoring practices that make your rule messages more readable, understandable, and actionable. See also `SARIF2014.ProvideDynamicMessageContent` and `SARIF2015.EnquoteDynamicMessageContent`.

#### Messages

##### `Default`: warning

{0}: In rule '{1}', the message with id '{2}' does not end in a period. Express plain text rule messages as complete sentences. This guidance does not apply to Markdown messages, which might include formatting that makes the punctuation unnecessary.

---

### Rule `SARIF2002.ProvideMessageArguments`

#### Description

In result messages, use the 'message.id' and 'message.arguments' properties rather than 'message.text'. This has several advantages. If 'text' is lengthy, using 'id' and 'arguments' makes the SARIF file smaller. If the rule metadata is stored externally to the SARIF log file, the message text can be improved (for example, by adding more text, clarifying the phrasing, or fixing typos), and the result messages will pick up the improvements the next time it is displayed. Finally, SARIF supports localizing messages into different languages, which is possible if the SARIF file contains 'message.id' and 'message.arguments', but not if it contains 'message.text' directly.

#### Messages

##### `Default`: warning

{0}: The 'message' property of this result contains a 'text' property. Consider replacing it with 'id' and 'arguments' properties. This potentially reduces the log file size, allows the message text to be improved without modifying the log file, and enables localization.

---

### Rule `SARIF2003.ProvideVersionControlProvenance`

#### Description

Provide 'versionControlProvenance' to record which version of the code was analyzed, and to enable paths to be expressed relative to the root of the repository.

#### Messages

##### `Default`: note

This run does not provide 'versionControlProvenance'. As a result, it is not possible to determine which version of code was analyzed, nor to map relative paths to their locations within the repository.

---

### Rule `SARIF2004.OptimizeFileSize`

#### Description

Emit arrays only if they provide additional information.

In several parts of a SARIF log file, a subset of information about an object appears in one place, and the full information describing all such objects appears in an array elsewhere in the log file. For example, each 'result' object has a 'ruleId' property that identifies the rule that was violated. Elsewhere in the log file, the array 'run.tool.driver.rules' contains additional information about the rules. But if the elements of the 'rules' array contained no information about the rules beyond their ids, then there might be no reason to include the 'rules' array at all, and the log file could be made smaller simply by omitting it. In some scenarios (for example, when assessing compliance with policy), the 'rules' array might be used to record the full set of rules that were evaluated. In such a scenario, the 'rules' array should be retained even if it contains only id information.

Similarly, most 'result' objects contain at least one 'artifactLocation' object. Elsewhere in the log file, the array 'run.artifacts' contains additional information about the artifacts that were analyzed. But if the elements of the 'artifacts' array contained not information about the artifacts beyond their locations, then there might be no reason to include the 'artifacts' array at all, and again the log file could be made smaller by omitting it. In some scenarios (for example, when assessing compliance with policy), the 'artifacts' array might be used to record the full set of artifacts that were analyzed. In such a scenario, the 'artifacts' array should be retained even if it contains only location information.

#### Messages

##### `AvoidDuplicativeAnalysisTarget`: warning

The 'analysisTarget' property '{1}' at '{0}' can be removed because it is the same as the result location. This unnecessarily increases log file size. The 'analysisTarget' property is used to distinguish cases when a tool detects a result in a file (such as an included header) that is different than the file that was scanned (such as a .cpp file that included the header). 

#### `AvoidDuplicativeResultRuleInformation`: warning

'{0}' uses the 'rule' property to specify the violated rule, so it is not necessary also to specify 'ruleId' or 'ruleIndex'. This unnecessarily increases log file size. Remove the 'ruleId' and 'ruleIndex' properties.

##### `EliminateLocationOnlyArtifacts`: warning

The 'artifacts' array at '{0}' contains no information beyond the locations of the artifacts. Removing this array might reduce the log file size without losing information. In some scenarios (for example, when assessing compliance with policy), the 'artifacts' array might be used to record the full set of artifacts that were analyzed. In such a scenario, the 'artifacts' array should be retained even if it contains only location information.

##### `EliminateIdOnlyRules`: warning

The 'rules' array at '{0}' contains no information beyond the ids of the rules. Removing this array might reduce the log file size without losing information. In some scenarios (for example, when assessing compliance with policy), the 'rules' array might be used to record the full set of rules that were evaluated. In such a scenario, the 'rules' array should be retained even if it contains only id information.

#### `PreferRuleId`: warning

The result at '{0}' uses the 'rule' property to specify the violated rule, but this is not necessary because the rule is defined by 'tool.driver'. Use the 'ruleId' and 'ruleIndex' instead, because they are shorter and just as clear.

---

### Rule `SARIF2005.ProvideToolProperties`

#### Description

Provide information that makes it easy to identify the name and version of your tool.

The tool's 'name' property should be no more than three words long. This makes it easy to remember and allows it to fit into a narrow column when displaying a list of results. If you need to provide more information about your tool, use the 'fullName' property.

The tool should provide either or both of the 'version' and 'semanticVersion' properties. This enables the log file consumer to determine whether the file was produced by an up to date version, and to avoid accidentally comparing log files produced by different tool versions.

If 'version' is used, facilitate comparison between versions by specifying a version number that starts with an integer, optionally followed by any desired characters.

#### Messages

##### `ProvideConciseToolName`: note

{0}: The tool name '{1}' contains {2} words, which is more than the recommended maximum of {3} words. A short tool name is easy to remember and fits into a narrow column when displaying a list of results. If you need to provide more information about your tool, use the 'fullName' property.

##### `ProvideToolVersion`: warning

{0}: The tool '{1}' provides neither a 'version' property nor a 'semanticVersion' property. Providing a version enables the log file consumer to determine whether the file was produced by an up to date version, and to avoid accidentally comparing log files produced by different tool versions.

##### `UseNumericToolVersions`: warning

{0}: The tool '{1}' contains the 'version' property '{2}', which is not numeric. To facilitate comparison between versions, specify a 'version' that starts with an integer, optionally followed by any desired characters.

---

### Rule `SARIF2006.UrisShouldBeReachable`

#### Description

URIs that refer to locations such as rule help pages and result-related work items should be reachable via a GET request.

#### Messages

##### `Default`: warning

{0}: The URI '{1}' was not reachable via a GET request.

---

### Rule `SARIF2007.ExpressPathsRelativeToRepoRoot`

#### Description

Provide information that makes it possible to determine the repo-relative locations of files that contain analysis results.

Each element of the 'versionControlProvenance' array is a 'versionControlDetails' object that describes a repository containing files that were analyzed. 'versionControlDetails.mappedTo' defines the file system location to which the root of that repository is mapped. If 'mappedTo.uriBaseId' is present, and if result locations are expressed relative to that 'uriBaseId', then the repo-relative location of each result can be determined.

#### Messages

##### `ProvideUriBaseIdForMappedTo`: warning

{0}: The 'versionControlDetails' object that describes the repository '{1}' does not provide 'mappedTo.uriBaseId'. As a result, it will not be possible to determine the repo-relative location of files containing analysis results for this repository.

##### `ExpressResultLocationsRelativeToMappedTo`: warning

{0}: This result location does not provide any of the 'uriBaseId' values that specify repository locations: '{1}'. As a result, it will not be possible to determine the location of the file containing this result relative to the root of the repository that contains it.

---

### Rule `SARIF2008.ProvideSchema`

#### Description

A SARIF log file should contain, on the root object, a '$schema' property that refers to the final, OASIS standard version of the SARIF 2.1.0 schema. This enables IDEs to provide Intellisense for SARIF log files.

#### Messages

##### `Default`: warning

{0}: The SARIF log file does not contain a '$schema' property. Add a '$schema' property that refers to the final, OASIS standard version of the SARIF 2.1.0 schema. This enables IDEs to provide Intellisense for SARIF log files.

---

### Rule `SARIF2009.ConsiderConventionalIdentifierValues`

#### Description

Adopt uniform naming conventions for rule ids.

Many tools follow a conventional format for the 'reportingDescriptor.id' property: a short string identifying the tool concatenated with a numeric rule number,
for example, 'CS2001' for a diagnostic from the Roslyn C# compiler. For uniformity of experience across tools, we recommend this format.

#### Messages

##### `UseConventionalRuleIds`: note

{0}: The 'id' property of the rule '{1}' does not follow the recommended format: a short string identifying the tool concatenated with a numeric rule number, for example, 'CS2001'. Using a conventional format for the rule id provides a more uniform experience across tools.

---

### Rule `SARIF2010.ProvideCodeSnippets`

#### Description

Provide code snippets to enable users to see the code that triggered each result, even if they are not enlisted in the code.

#### Messages

##### `Default`: note

{0}: The 'region' object in this result location does not provide a 'snippet' property. Providing a code snippet enables users to see the source code that triggered the result, even if they are not enlisted in the code.

---

### Rule `SARIF2011.ProvideContextRegion`

#### Description

Provide context regions to enable users to see a portion of the code that surrounds each result, even if they are not enlisted in the code.

##### `Default`: note

{0}: This result location does not provide a 'contextRegion' property. Providing a context region enables users to see a portion of the code that surrounds the result, even if they are not enlisted in the code.

---

### Rule `SARIF2012.ProvideHelpUris`

#### Description

For each rule, provide a URI where users can find detailed information about the rule. This information should include a detailed description of the invalid pattern, an explanation of why the pattern is poor practice (particularly in contexts such as security or accessibility where driving considerations might not be readily apparent), guidance for resolving the problem (including describing circumstances in which ignoring the problem altogether might be appropriate), examples of invalid and valid patterns, and special considerations (such as noting when a violation should never be ignored or suppressed, noting when a violation could cause downstream tool noise, and noting when a rule can be configured in some way to refine or alter the analysis).

##### `Default`: note

{0}: The rule '{1}' does not provide a help URI. Providing a URI where users can find detailed information about the rule helps users to understand the result and how they can best address it.

---

### Rule `SARIF2013.ProvideEmbeddedFileContent`

Provide embedded file content so that users can examine results in their full context without having to enlist in the source repository. Embedding file content in a SARIF log file can dramatically increase its size, so consider the usage scenario when you decide whether to provide it.

#### Description

##### `Default`: note

{0}: This run does not provide embedded file content. Providing embedded file content enables users to examine results in their full context without having to enlist in the source repository. Embedding file content in a SARIF log file can dramatically increase its size, so consider the usage scenario when you decide whether to provide it.

---

### Rule `SARIF2014.ProvideDynamicMessageContent`

#### Description

Include "dynamic content" (information that varies among results from the same rule) to makes your messages more specific, and to avoid the "wall of bugs" phenomenon, where hundreds of occurrences of the same message appear unapproachable.

This is part of a set of authoring practices that make your rule messages more readable, understandable, and actionable. See also 'SARIF2001.TerminateMessagesWithPeriod' and 'SARIF2015.EnquoteDynamicMessageContent'.

#### Messages

##### `Default`: note

{0}: In rule '{1}', the message with id '{2}' does not include any dynamic content. Dynamic content makes your messages more specific and avoids the "wall of bugs" phenomenon, where hundreds of occurrences of the same message appear unapproachable.

---

### Rule `SARIF2015.EnquoteDynamicMessageContent`

#### Description

Place dynamic content in single quotes to set it off from the static text and to make it easier to spot. It's especially helpful when the dynamic content is a string that might contain spaces, and most especially when the string might be empty (and so would be invisible if it weren't for the quotes). We recommend single quotes for a less cluttered appearance, even though US English usage would require double quotes.

This is part of a set of authoring practices that make your rule messages more readable, understandable, and actionable. See also 'SARIF2001.TerminateMessagesWithPeriod' and 'SARIF2014.ProvideDynamicMessageContent'.

#### Messages

##### `Default`: note

{0}: In rule '{1}', the message with id '{2}' includes dynamic content that is not enclosed in single quotes. Enquoting dynamic content makes it easier to spot, and single quotes give a less cluttered appearance.

---
### Rule `SARIF2016.FileUrisShouldBeRelative`

#### Description

When an artifact location refers to a file on the local file system, specify a relative reference for the uri property and provide a uriBaseId property, rather than specifying an absolute URI.

There are several advantages to this approach:

Portability: A log file that contains relative references together with uriBaseI properties can be interpreted on a machine where the files are located at a different absolute location.

Determinism: A log file that uses uriBaseId properties has a better chance of being “deterministic”; that is, of being identical from run to run if none of its inputs have changed, even if those runs occur on machines where the files are located at different absolute locations.

Security: The use of uriBaseId properties avoids the persistence of absolute path names in the log file. Absolute path names can reveal information that might be sensitive.

Semantics: Assuming the reader of the log file (an end user or another tool) has the necessary context, they can understand the meaning of the location specified by the uri property, for example, “this is a source file”.

#### Messages

##### `Default`: note

{0}: The file location '{1}' is specified with absolute URI. Prefer a relative reference together with a uriBaseId property.