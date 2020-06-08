
# Rules that describe spec violations

Rules that describe violations of **SHALL**/**SHALL NOT** requirements of the [SARIF specification](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html) have numbers between 1000 and 1999, and always have level `"error"`.

---

## Rule `SARIF1001.RuleIdentifiersMustBeValid`

### Description

SARIF rules have two identifiers. The required 'id' property must be a "stable, opaque identifier" (the SARIF specification ([§3.49.3](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317839)) explains the reasons for this). The optional 'name' property ([§3.49.7](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317843)) is an identifer that is understandable to an end user. Therefore if both 'id' and 'name' are present, they must be different.

### Messages

#### `DistinguishRuleIdFromRuleName`: error

{0}: The rule '{1}' has a 'name' property that is identical to its 'id' property. The required 'id' property must be a "stable, opaque identifier" (the SARIF specification ([§3.49.3](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317839)) explains the reasons for this). The optional 'name' property ([§3.49.7](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317843)) is an identifer that is understandable to an end user. Therefore if both 'id' and 'name' are present, they must be different.

---

## Rule `SARIF1002.UrisMustBeValid`

### Description

### Messages

#### `UrisMustConformToRfc3986`: error

#### `FileUrisMustConformToRfc8089`: error

#### `FileUrisMustNotIncludeDotDotSegments`: error

#### `FileUrisWithoutHostNameShouldUseTripleSlashForm`: warning

---

## Rule `SARIF1003.UseUriBaseIdsCorrectly`

### Description

### Messages

#### `UriBaseIdRequiresRelativeUri`: error

#### `TopLevelUriBaseIdMustBeAbsolute`: error

#### `UriBaseIdValueMustEndWithSlash`: error

#### `UriBaseIdValueMustNotContainDotDotSegment`: error

#### `UriBaseIdValueMustNotContainQueryOrFragment`: error

---

## Rule `SARIF1004.CertainUrisMustBeAbsolute`

### Description

#### Messages

#### `UriMustBeAbsolute`: error

---

## Rule `SARIF1005.InvocationPropertiesMustBeConsistent`

### Description

### Messages

#### `EndTimeMustNotPrecedeStartTime`: error

---

## Rule `SARIF1006.RegionPropertiesMustBeConsistent`

### Description

SARIF can specify a 'region' (a contiguous portion of a file) in a variety of ways: with line and column numbers, with a character offset and count, or with a byte offset and count. The specification states certain constraints on these properties, both within each property groups (for example, the start line cannot be greater than end line) and between the groups (for example, if more than one group is present, they must independently specify the exact same portion of the file). See the SARIF specification ([§3.30](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317685)).

### Messages

#### `EndLineMustNotPrecedeStartLine`: error

#### `EndColumnMustNotPrecedeStartColumn`: error

#### `RegionStartPropertyMustBePresent`: error

---

## Rule `SARIF1007.ContextRegionMustEncloseRegion`

### Description

### Messages

#### `ContextRegionRequiresRegion`: error

#### `ContextRegionMustBeProperSupersetOfRegion`: error

---

## Rule `SARIF1008.IndexPropertiesMustBeConsistentWithArrays`

### Description

### Messages

#### `TargetArrayMustExist`: error

#### `TargetArrayMustBeLongEnough`: error

---

## Rule `SARIF1009.RuleIdMustBePresentAndConsistent`

### Description

Every result must contain at least one of the properties 'ruleId' and 'rule.id'. If both are present, they must be equal. See the SARIF specification ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)).

This validation rule is required because this constraint cannot be expressed in a JSON schema.

### Messages

#### `ResultMustSpecifyRuleId`

{0}: This result contains neither of the properties 'ruleId' or 'rule.id'. The SARIF specification ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)) requires at least one of these properties to be present.

#### `ResultRuleIdMustBeConsistent`

{0}: This result contains both the 'ruleId' property '{1}' and the 'rule.id' property '{2}', but they are not equal. The SARIF specification ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)) requires that if both of these properties are present, they must be equal.

---

# Rules that describe less serious violations

Rules that describe violations of **SHOULD**/**SHOULD NOT** requirements of the [SARIF specification](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html) have numbers between 2000 and 2999, and always have level `"warning"`.

Rules that describe violations of SARIF recommendations or best practices also have numbers in this range. Some of those recommendations are expressed in the spec as **MAY** requirements; others are based on experience using the format. These rules have level `"warning"` or `"note"`, depending on the tool's opinion of the seriousness of the violation.

---

## Rule `SARIF2001.AuthorHighQualityMessages`

### Description

### Messages

#### `IncludeDynamicContent`: warning

#### `EnquoteDynamicContent`: warning

#### `TerminateWithPeriod`: warning

#### `UseMessageArguments`: warning

---

## Rule `SARIF2002.UseConventionalSymbolicNames`

### Description

SARIF uses symbolic names in various contexts. This rule proposes uniform naming conventions for the various types of symbolic names.

Many tools follow a conventional format for the 'reportingDescriptor.id' property: a short string identifying the tool concatenated with a numeric rule number,
for example, 'CS2001' for a diagnostic from the Roslyn C# compiler. For uniformity of experience across tools, we recommend this format.

### Messages

#### `UseConventionalRuleIds`: note

{0}: The 'name' property ' of the rule '{1}' does not follow the recommended format: a short string identifying the tool concatenated with a numeric rule number, for example, `CS2001`. Using a conventional format for the rule id provides a more uniform experience across tools.

#### `UseConventionalUriBaseIdNames`: note
---

## Rule `SARIF2003.FacilitateAutomaticBugFiling`

### Description

### Messages

#### `ProvideVersionControlProvenance`: note

---

## Rule `SARIF2004.FacilitateProblemResolution`

### Description

### Messages

#### `ProvideCodeSnippets`: note

#### `ProvideContextRegion`: note

#### `EmbedFileContent`: note

#### `ProvideRuleHelpUris`: note

---

## Rule `SARIF2005.ReduceFileSize`

### Description

### Messages

#### `EliminateLocationOnlyArtifacts`: warning

---

## Rule `SARIF2006.ProvideHelpfulToolInformation`

### Description

### Messages

#### `ProvideConciseToolName`: note

#### `ProvideToolVersion`: note

#### `UseNumericToolVersions`: note

---

## Rule `SARIF2007.ProvideUsableUris`

### Description

### Messages

#### `UrisShouldBeReachable`

---

## Rule `SARIF2008.ExpressPathsRelativeToRepoRoot`

### Description

### Messages

#### `??`