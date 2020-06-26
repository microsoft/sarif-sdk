# Rules and guidelines for producing effective SARIF

## Introduction

This document is for creators of static analysis tools who want to produce SARIF output that's useful to SARIF consumers, both human and automated.

Teams can use SARIF log files in many ways. They can view the results in an IDE extension such as the [SARIF extension for VS Code](https://marketplace.visualstudio.com/items?itemName=MS-SarifVSCode.sarif-viewer) or the [SARIF Viewer VSIX for Visual Studio](https://marketplace.visualstudio.com/items?itemName=WDGIS.MicrosoftSarifViewer), or in a [web-based viewer](https://microsoft.github.io/sarif-web-component/). They can import it into a static analysis results database, or use it to drive automatic bug fiing. Most important, developers use the information in a SARIF log file to understand and fix the problems it reports.

Because of this variety of usage scenarios, a SARIF log file that is useful  one scenario might not be useful in another. Ideally, static analysis tools will provide options to let their users specify the output that meets their needs.

The [SARIF specification](https://docs.oasis-open.org/sarif/sarif/v2.1.0/sarif-v2.1.0.html) defines dozens of objects with hundreds of properties. It can be hard to decide which ones are important (aside from the few that the spec says are mandatory). What information is most helpful to developers? What information should you include if you want to file useful bugs from the SARIF log?

On top of all that, the spec is written in format language that's hard to read. If you want to learn SARIF, take a look at the [SARIF Tutorials](https://github.com/microsoft/sarif-tutorials).

The purpose of this document is to cut through the confusion and provide clear guidance on what information your tool should include in a SARIF file, and how to make that information as helpful and usable as possible.

### Structural requirements

Many of SARIF's structural requirements are expressed in a [JSON schema](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/schemas/sarif-schema-2.1.0.json), but the schema can't express all the structural requirements. In addition to providing helpful, useful information, it's important for tools to produce SARIF that meet all the structural requirements, even the ones that the schema can't express.

### The SARIF validation tool

The SARIF validation tool (the "validator") helps ensure that a SARIF file conforms to SARIF's structural requirements as well as to the guidelines for producing high-quality SARIF output.

#### What the validator does

Here's how the validator process a SARIF log file:

1. If the file is not valid JSON, report the syntax error and stop.
2. If the file does not conform to the SARIF schema, report the schema violations and stop.
3. Run a set of analysis rules. Report any rule violations.

The analysis rules in Step 3 fall into two categories:

1. Rules that detect structural problems that the schema can't express.
2. Rules that enforce guidelines for producing high quality SARIF.

The validator is incomplete: it does not enforce every structural condition in the spec, nor every guideline for producing high quality SARIF. We hope to continue to add analysis rules in both those areas.

#### Installing and using the validator

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

### How this document is organized

This document expresses each structural requirement and guideline as a validator analysis rule. At the time of this writing, not all of those rules actually exist. Those that do not are labled "(NYI)".

First come the rules that detect serious violations of the SARIF spec (rules which the validator would report as `"error"`). They have numbers in the range 1000-1999, for example, `SARIF1001.RuleIdentifiersMustBeValid`.

Then come the rules that detect either less serious violations of the SARIF spec (rules which the validator would report as `"warning"` or `"note"`). They have numbers in the range 2000-2999, for example, `SARIF2001.AuthorHighQualityMessages`.

Each rule has a description that describes its purpose, followed by one or more messages that can appear in a SARIF result object that reports a violation of this rule. Each message includes one or more replacement sequences (`{0}`, `{1}`, _etc._). The first one (`{0}`) is always a JSON path expression that describes the location of the result. For example, `/runs/0/results/0/locations/0/physicalLocation` specifies the `physicalLocation` property of the first location of the first result in the first run in the log file.

## Rules that describe serious violations

Rules that describe violations of **SHALL**/**SHALL NOT** requirements of the [SARIF specification](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html) have numbers between 1000 and 1999, and always have level `"error"`.

---

### Rule `SARIF1001.RuleIdentifiersMustBeValid`

#### Description

The two identity-related properties of a SARIF rule must be consistent. The required 'id' property must be a "stable, opaque identifier" (the SARIF specification ([§3.49.3](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317839)) explains the reasons for this). The optional 'name' property ([§3.49.7](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317843)) is an identifer that is understandable to an end user. Therefore if both 'id' and 'name' are present, they must be different. If both 'name' and 'id' are opaque identifiers, omit the 'name' property. If both 'name' and 'id' are human-readable identifiers, then consider assigning an opaque identifier to each rule, but in the meantime, omit the 'name' property.

#### Messages

##### `Default`: error

{0}: The rule '{1}' has a 'name' property that is identical to its 'id' property. The required 'id' property must be a "stable, opaque identifier" (the SARIF specification ([§3.49.3](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317839)) explains the reasons for this). The optional 'name' property ([§3.49.7](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317843)) is an identifer that is understandable to an end user. Therefore if both 'id' and 'name' are present, they must be different. If they are identical, the tool must omit the 'name' property.

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

---

### Rule `SARIF1003.UrisShouldUseConventionalForm`

##### `FileUrisWithoutHostNameShouldUseTripleSlashForm`: warning

---

### Rule `SARIF1004.ExpressUriBaseIdsCorrectly`

#### Description

#### Messages

##### `UriBaseIdRequiresRelativeUri`: error

##### `TopLevelUriBaseIdMustBeAbsolute`: error

##### `UriBaseIdValueMustEndWithSlash`: error

##### `UriBaseIdValueMustNotContainDotDotSegment`: error

##### `UriBaseIdValueMustNotContainQueryOrFragment`: error

---

### Rule `SARIF1005.UriMustBeAbsolute`

#### Description

#### Messages

##### `Default`: error

---

### Rule `SARIF1006.InvocationPropertiesMustBeConsistent`

#### Description

#### Messages

##### `EndTimeMustNotPrecedeStartTime`: error

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

If the validator reports that 'contextRegion' is not a proper superset of 'region', then it's possible that the tool reversed 'region' and 'contextRegion'. If 'region' and 'contextRegion' are identical, the tool should simply omit 'contextRegion'

#### Messages

##### `ContextRegionRequiresRegion`: error

{0}: This 'physicalLocation' object contains a 'contextRegion' property, but it does not contain a 'region' property. This is invalid because the purpose of 'contextRegion' is to provide a viewing context around the 'region' which is the location of the result. If a tool associates only one region with a result, it must populate 'region', not 'contextRegion'.

##### `ContextRegionMustBeProperSupersetOfRegion`: error

{0}: This 'physicalLocation' object contains both a 'region' and a 'contextRegion' property, but 'contextRegion' is not a proper superset of 'region'. This is invalid because the purpose of 'contextRegion' is to provide a viewing context around the 'region' which is the location of the result. It's possible that the tool reversed 'region' and 'contextRegion'. If 'region' and 'contextRegion' are identical, the tool must omit 'contextRegion'.

---

### Rule `SARIF1009.IndexPropertiesMustBeConsistentWithArrays`

#### Description

#### Messages

##### `TargetArrayMustExist`: error

##### `TargetArrayMustBeLongEnough`: error

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

#### Messages

##### `Default`: error

---

### Rule `SARIF1012.MessagePropertiesMustBeConsistent`

#### Description

Ensure consistency among the properties of a 'message' object.

When a tool creates a 'message' object that uses the 'id' and 'arguments' properties, it must ensure that the 'arguments' array has enough elements to provide values for every replacement sequence in the message specified by 'id'. For example, if the highest numbered replacement sequence in the specified message string is '{3}', then the 'arguments' array must contain 4 elements.

#### Messages

##### `SupplyCorrectNumberOfArguments`: error

{0}: The message with id '{1}' in rule '{2}' requires {3} arguments, but the 'arguments' array in this message object has only {4} elements. When a tool creates 'message' objects that use the 'id' and 'arguments' properties, it must ensure that the 'arguments' array has enough elements to provide values for every replacement sequence in the message specified by 'id'. For example, if the highest numbered replacement sequence in the specified message string is '{{3}}', then the 'arguments' array must contain 4 elements.

---

## Rules that describe less serious violations

Rules that describe violations of **SHOULD**/**SHOULD NOT** requirements of the [SARIF specification](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html) have numbers between 2000 and 2999, and always have level `"warning"`.

Rules that describe violations of SARIF recommendations or best practices also have numbers in this range. Some of those recommendations are expressed in the spec as **MAY** requirements; others are based on experience using the format. These rules have level `"warning"` or `"note"`, depending on the tool's opinion of the seriousness of the violation.

---

### Rule `SARIF2001.AuthorHighQualityMessages`

#### Description

#### Messages

##### `IncludeDynamicContent`: warning

##### `EnquoteDynamicContent`: warning

##### `TerminateWithPeriod`: warning

---

### Rule `SARIF2002.UseMessageArguments`

#### Description

In result messages, use the 'message.id' and 'message.arguments' properties rather than 'message.text'. This has several advantages. If 'text' is lengthy, using 'id' and 'arguments' makes the SARIF file smaller. If the rule metadata is stored externally to the SARIF log file, the message text can be improved (for example, by adding more text, clarifying the phrasing, or fixing typos), and the result messages will pick up the improvements the next time it is displayed. Finally, SARIF supports localizing messages into different languages, which is possible if the SARIF file contains 'message.id' and 'message.arguments', but not if it contains 'message.text' directly.

#### Messages

##### `Default`: warning

{0}: The 'message' property of this result contains a 'text' property. Consider replacing it with 'id' and 'arguments' properties. This potentially reduces the log file size, allows the message text to be improved without modifying the log file, and enables localization.

---

### Rule `SARIF2003.ProduceEnrichedSarif`

#### Description

#### Messages

##### `ProvideVersionControlProvenance`: note

##### `ProvideCodeSnippets`: note

##### `ProvideContextRegion`: note

##### `ProvideHelpUris`: note

##### `EmbedFileContent`: note

---

### Rule `SARIF2004.OptimizeFileSize`

#### Description

#### Messages

##### `EliminateLocationOnlyArtifacts`: warning

##### `DoNotIncludeExtraIndexedObjectProperties`: warning

---

### Rule `SARIF2005.ProvideToolProperties`

#### Description

Provide information that makes it easy to identify the name and version of your tool.

The tool's 'name' property should be no more than three words long. This makes it easy to remember and allows it to fit into a narrow column when displaying a list of results. If you need to provide more information about your tool, use the 'fullName' property.

The tool should provide either or both of the 'version' and 'semanticVersion' properties. This enables the log file consumer to determine whether the file was produced by an up to date version, and to avoid accidentally comparing log files produced by different tool versions.

If 'version' is used, facilitate comparison between versions by specifying it either with an integer, or with at least two dot-separated integer components, optionally followed by any desired characters.

#### Messages

##### `ProvideConciseToolName`: note

{0}: The tool name '{1}' contains {2} words, which is more than the recommended maximum of {3} words. A short tool name is easy to remember and fits into a narrow column when displaying a list of results. If you need to provide more information about your tool, use the 'fullName' property.

##### `ProvideToolVersion`: warning

{0}: The tool '{1}' provides neither a 'version' property nor a 'semanticVersion' property. Providing a version enables the log file consumer to determine whether the file was produced by an up to date version, and to avoid accidentally comparing log files produced by different tool versions.

##### `UseNumericToolVersions`: warning

{0}: The tool '{1}' contains the 'version' property '{2}', which is not numeric. To facilitate comparison between versions, specify a 'version' that starts with at least two dot-separated integer components, optionally followed by any desired characters.

---

### Rule `SARIF2006.UrisShouldBeReachable`

#### Description

#### Messages

##### `Default`: warning

---

### Rule `SARIF2007.ExpressPathsRelativeToRepoRoot`

#### Description

#### Messages

##### `Default`: warning

---

### Rule `SARIF2008.ProvideSchema`

#### Description

#### Messages

##### `Default`: warning

---

### Rule `SARIF2009.ConsiderConventionalIdentifierValues`

#### Description

Adopt uniform naming conventions for the symbolic names that SARIF uses various contexts.

Many tools follow a conventional format for the 'reportingDescriptor.id' property: a short string identifying the tool concatenated with a numeric rule number,
for example, 'CS2001' for a diagnostic from the Roslyn C# compiler. For uniformity of experience across tools, we recommend this format.

Many tool use similar names for 'uriBaseId' symbols. We suggest 'REPOROOT' for the root of a repository, 'SRCROOT' for the root of the directory containing all source code, 'TESTROOT' for the root of the directory containing all test code (if your repository is organized in that way), and 'BINROOT' for the root of the directory containing build output (if your project places all build output in a common directory).

#### Messages

##### `UseConventionalRuleIds`: note

{0}: The 'id' property of the rule '{1}' does not follow the recommended format: a short string identifying the tool concatenated with a numeric rule number, for example, `CS2001`. Using a conventional format for the rule id provides a more uniform experience across tools.

##### `UseConventionalUriBaseIdNames`: note

{0}: The 'originalUriBaseIds' symbol '{1}' is not one of the conventional symbols. We suggest 'REPOROOT' for the root of a repository, 'SRCROOT' for the root of the directory containing all source code, 'TESTROOT' for the root of the directory containing all test code (if your repository is organized in that way), and 'BINROOT' for the root of the directory containing build output (if your project places all build output in a common directory).

---
