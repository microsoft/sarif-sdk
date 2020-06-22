# Rules and Guidelines for producing high-quality SARIF output

## Introduction

This document is for creators of static analysis tools who want to produce the best possible SARIF output.

Teams can use SARIF log files in many ways. They can view the results in an IDE extension such as the [SARIF extension for VS Code](https://marketplace.visualstudio.com/items?itemName=MS-SarifVSCode.sarif-viewer) or the [SARIF Viewer VSIX for Visual Studio](https://marketplace.visualstudio.com/items?itemName=WDGIS.MicrosoftSarifViewer), or in a [web-based viewer](https://microsoft.github.io/sarif-web-component/). They can import it into a static analysis results database, or use it to drive automatic bug fiing. Most important, developers use the information in a SARIF log file to understand and fix the problems it reports.

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

Each rule has a description that describes its purpose, followed by one or more messages that can appear in a SARIF result object that reports a violation of this rule.

## Rules that describe serious violations

Rules that describe violations of **SHALL**/**SHALL NOT** requirements of the [SARIF specification](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html) have numbers between 1000 and 1999, and always have level `"error"`.

---

### Rule `SARIF1001.RuleIdentifiersMustBeValid`

#### Description

SARIF rules have two identifiers. The required 'id' property must be a "stable, opaque identifier" (the SARIF specification ([§3.49.3](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317839)) explains the reasons for this). The optional 'name' property ([§3.49.7](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317843)) is an identifer that is understandable to an end user. Therefore if both 'id' and 'name' are present, they must be different.

#### Messages

##### `DistinguishRuleIdFromRuleName`: error

{0}: The rule '{1}' has a 'name' property that is identical to its 'id' property. The required 'id' property must be a "stable, opaque identifier" (the SARIF specification ([§3.49.3](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317839)) explains the reasons for this). The optional 'name' property ([§3.49.7](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317843)) is an identifer that is understandable to an end user. Therefore if both 'id' and 'name' are present, they must be different.

---

### Rule `SARIF1002.UrisMustBeValid`

#### Description

#### Messages

##### `UrisMustConformToRfc3986`: error

##### `FileUrisMustConformToRfc8089`: error

##### `FileUrisMustNotIncludeDotDotSegments`: error

##### `FileUrisWithoutHostNameShouldUseTripleSlashForm`: warning (TODO: This can't be here. Different level.)

---

### Rule `SARIF1003.UseUriBaseIdsCorrectly`

#### Description

#### Messages

##### `UriBaseIdRequiresRelativeUri`: error

##### `TopLevelUriBaseIdMustBeAbsolute`: error

##### `UriBaseIdValueMustEndWithSlash`: error

##### `UriBaseIdValueMustNotContainDotDotSegment`: error

##### `UriBaseIdValueMustNotContainQueryOrFragment`: error

---

### Rule `SARIF1004.CertainUrisMustBeAbsolute`

#### Description

#### Messages

##### `UriMustBeAbsolute`: error

---

### Rule `SARIF1005.InvocationPropertiesMustBeConsistent`

#### Description

#### Messages

##### `EndTimeMustNotPrecedeStartTime`: error

---

### Rule `SARIF1006.RegionPropertiesMustBeConsistent`

#### Description

SARIF can specify a 'region' (a contiguous portion of a file) in a variety of ways: with line and column numbers, with a character offset and count, or with a byte offset and count. The specification states certain constraints on these properties, both within each property groups (for example, the start line cannot be greater than end line) and between the groups (for example, if more than one group is present, they must independently specify the exact same portion of the file). See the SARIF specification ([§3.30](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317685)).

#### Messages

##### `EndLineMustNotPrecedeStartLine`: error

##### `EndColumnMustNotPrecedeStartColumn`: error

##### `RegionStartPropertyMustBePresent`: error (LJG additional proposal)

---

### Rule `SARIF1007.ContextRegionMustEncloseRegion`

#### Description

#### Messages

##### `ContextRegionRequiresRegion`: error

##### `ContextRegionMustBeProperSupersetOfRegion`: error (LJG additional proposal)

---

### Rule `SARIF1008.IndexPropertiesMustBeConsistentWithArrays`

#### Description

#### Messages

##### `TargetArrayMustExist`: error

##### `TargetArrayMustBeLongEnough`: error (LJG proposal to split message, doesn't feel strongly.)

---

### Rule `SARIF1009.ResultMustContainConsistentRuleId`

#### Description

Every result must contain at least one of the properties 'ruleId' and 'rule.id'. If both are present, they must be equal. See the SARIF specification ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)).

This validation rule is required because this constraint cannot be expressed in a JSON schema.

#### Messages

##### `ResultMustSpecifyRuleId`

{0}: This result contains neither of the properties 'ruleId' or 'rule.id'. The SARIF specification ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)) requires at least one of these properties to be present.

##### `ResultRuleIdMustBeConsistent`

{0}: This result contains both the 'ruleId' property '{1}' and the 'rule.id' property '{2}', but they are not equal. The SARIF specification ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)) requires that if both of these properties are present, they must be equal.

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

##### `UseMessageArguments`: warning (TODO: This doesn't belong here. Separate concern; this is about _usage_.)

---

### Rule `SARIF2002.UseConventionalSymbolicNames`

#### Description

SARIF uses symbolic names in various contexts. This rule proposes uniform naming conventions for the various types of symbolic names.

Many tools follow a conventional format for the 'reportingDescriptor.id' property: a short string identifying the tool concatenated with a numeric rule number,
for example, 'CS2001' for a diagnostic from the Roslyn C# compiler. For uniformity of experience across tools, we recommend this format.

#### Messages

##### `UseConventionalRuleIds`: note

{0}: The 'name' property ' of the rule '{1}' does not follow the recommended format: a short string identifying the tool concatenated with a numeric rule number, for example, `CS2001`. Using a conventional format for the rule id provides a more uniform experience across tools.

##### `UseConventionalUriBaseIdNames`: note

---

### Rule `SARIF2003.FacilitateAutomaticBugFiling`

#### Description

#### Messages

##### `ProvideVersionControlProvenance`: note

---

### Rule `SARIF2004.FacilitateBugFixing`

#### Description

#### Messages

##### `ProvideCodeSnippets`: note

##### `ProvideContextRegion`: note

##### `EmbedFileContent`: note

##### `ProvideRuleHelpUris`: note

---

### Rule `SARIF2005.ReduceFileSize`

#### Description

#### Messages

##### `EliminateLocationOnlyArtifacts`: warning

##### `DoNotIncludeExtraIndexedObjectProperties`: warning (TODO: Needs a better name.)

---

### Rule `SARIF2006.ProvideHelpfulToolInformation`

#### Description

#### Messages

##### `ProvideConciseToolName`: note

##### `ProvideToolVersion`: note

##### `UseNumericToolVersions`: note

---

### Rule `SARIF2007.ProvideUsableUris` (TODO: Not a great name.)

#### Description

#### Messages

##### `UrisShouldBeReachable`

---

### Rule `SARIF2008.ExpressPathsRelativeToRepoRoot`

#### Description

#### Messages

##### `??`

---

## Rule `SARIF2009.ReferToFinalSchema`

#### Description

#### Messages

##### `SchemaPropertyShouldBePresent`: warning

##### `SchemaPropertyShouldReferToFinalSchema`: warning (**NOTE** Why isn't this an error? It can't possibly be ok.)