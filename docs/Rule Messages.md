
# Rules that describe serious violations

Rules that describe violations of **SHALL**/**SHALL NOT** requirements of the [SARIF specification](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html) have numbers between 1000 and 1999, and always have level `"error"`.

---

## Rule `SARIF1001.RuleIdentifiersMustBeValid`

### Description

SARIF rules have two identifiers. The required 'id' property must be a "stable, opaque identifier" (the SARIF specification ([§3.49.3](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317839)) explains the reasons for this). The optional 'name' property ([§3.49.7](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317843)) is an identifer that is understandable to an end user. Therefore if both 'id' and 'name' are present, they must be different. If both 'name' and 'id' are opaque identifiers, omit the 'name' property. If both 'name' and 'id' are human-readable identifiers, then consider assigning an opaque identifier to each rule, but in the meantime, omit the 'name' property.

### Messages

#### `Default`: error

{0}: The rule '{1}' has a 'name' property that is identical to its 'id' property. The required 'id' property must be a "stable, opaque identifier" (the SARIF specification ([§3.49.3](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317839)) explains the reasons for this). The optional 'name' property ([§3.49.7](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317843)) is an identifer that is understandable to an end user. Therefore if both 'id' and 'name' are present, they must be different.

---

## Rule `SARIF1002.UrisMustBeValid`

### Description

### Messages

#### `UrisMustConformToRfc3986`: error

#### `FileUrisMustConformToRfc8089`: error

#### `FileUrisMustNotIncludeDotDotSegments`: error
---

## Rule `SARIF1003.UrisShouldUseConventionalForm`

#### `FileUrisWithoutHostNameShouldUseTripleSlashForm`: warning

---

## Rule `SARIF1004.ExpressUriBaseIdsCorrectly`

### Description

### Messages

#### `UriBaseIdRequiresRelativeUri`: error

#### `TopLevelUriBaseIdMustBeAbsolute`: error

#### `UriBaseIdValueMustEndWithSlash`: error

#### `UriBaseIdValueMustNotContainDotDotSegment`: error

#### `UriBaseIdValueMustNotContainQueryOrFragment`: error

---

## Rule `SARIF1005.UriMustBeAbsolute`

### Description

#### Messages

#### `Default`: error

---

## Rule `SARIF1006.InvocationPropertiesMustBeConsistent`

### Description

### Messages

#### `EndTimeMustNotPrecedeStartTime`: error

---

## Rule `SARIF1007.RegionPropertiesMustBeConsistent`

### Description

SARIF can specify a 'region' (a contiguous portion of a file) in a variety of ways: with line and column numbers, with a character offset and count, or with a byte offset and count. The specification states certain constraints on these properties, both within each property groups (for example, the start line cannot be greater than end line) and between the groups (for example, if more than one group is present, they must independently specify the exact same portion of the file). See the SARIF specification ([§3.30](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317685)).

### Messages

#### `EndLineMustNotPrecedeStartLine`: error

#### `EndColumnMustNotPrecedeStartColumn`: error

#### `RegionStartPropertyMustBePresent`: error

---

## Rule `SARIF1008.PhysicalLocationPropertiesMustBeConsistent`

### Description

A SARIF 'physicalLocation' object has two related properties 'region' and 'contextRegion'. If 'contextRegion' is present, then 'region' must also be present, and 'contextRegion' must be a "proper superset" of 'region'. That is, 'contextRegion' must completely contain 'region', and it must be larger than 'region'. To understand why this is so we must understand the roles of the 'region' and 'contextRegion' properties.

'region' allows both users and tools to distinguish similar results within the same artifact. If a SARIF viewer has access to the artifact, it can display it, and highlight the location identified by the analysis tool. If the region has a 'snippet' property, then even if the viewer doesn't have access to the artifact (which might be the case for a web-based viewer), it can still display the faulty code.

'contextRegion' provides users with a broader view of the result location. Typically, it consists of a range starting a few lines before 'region' and ending a few lines after. Again, if a SARIF viewer has access to the artifact, it can display it, and highlight the context region (perhaps in a lighter shade than the region itself). This isn't terribly useful since the user can already see the whole file, with the 'region' already highlighted. But if 'contextRegion' has a 'snippet' property, then even a viewer without access to the artifact can display a few lines of code surrounding the actual result, which is helpful to users.

If the SARIF validator reports that 'contextRegion' is present but 'region' is absent, then it's possible that the tool should have populated 'region' rather than 'contextRegion', or that it simply neglected to populate 'region'. If the validator reports that 'contextRegion' is not a proper superset of 'region', then it's possible that the tool reversed 'region' and 'contextRegion'. If 'region' and 'contextRegion' are identical, the tool should simply omit 'contextRegion'.

### Messages

#### `ContextRegionRequiresRegion`: error

{0}: This 'physicalLocation' object contains a 'contextRegion' property, but it does not contain a 'region' property. This is invalid because the purpose of 'contextRegion' is to provide a viewing context around the 'region' which is the location of the result. If the tool incorrectly populated 'contextRegion' instead of 'region', then fix it so that it populates only the 'region'. If the tool simply neglected to populate 'region', then fix it so that it does.

#### `ContextRegionMustBeProperSupersetOfRegion`: error

{0}: This 'physicalLocation' object contains both a 'region' and a 'contextRegion' property, but 'contextRegion' is not a proper superset of 'region'. This is invalid because the purpose of 'contextRegion' is to provide a viewing context around the 'region' which is the location of the result. If the tool simply reversed 'region', then fix it puts the correct values in the correct properties. If 'region' and 'contextRegion' are identical, the 'contextRegion' is unnecessary, and (by the spec) the tool must not populate it.

---

## Rule `SARIF1009.IndexPropertiesMustBeConsistentWithArrays`

### Description

### Messages

#### `TargetArrayMustExist`: error

#### `TargetArrayMustBeLongEnough`: error

---

## Rule `SARIF1010.RuleIdMustBeConsistent`

### Description

Every result must contain at least one of the properties 'ruleId' and 'rule.id'. If both are present, they must be equal. See the SARIF specification ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)).

This validation rule is required because this constraint cannot be expressed in a JSON schema.

### Messages

#### `ResultMustSpecifyRuleId`: error

{0}: This result contains neither of the properties 'ruleId' or 'rule.id'. The SARIF specification ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)) requires at least one of these properties to be present.

#### `ResultRuleIdMustBeConsistent`: error

{0}: This result contains both the 'ruleId' property '{1}' and the 'rule.id' property '{2}', but they are not equal. The SARIF specification ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)) requires that if both of these properties are present, they must be equal.

---

## Rule `SARIF1011.ReferenceFinalSchema`

### Description

### Messages

#### `Default`: error

---

## Rule `SARIF1012.MessagePropertiesMustBeConsistent`

### Description

### Messages

#### `SupplyCorrectNumberOfArguments`: error

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

---

## Rule `SARIF2002.UseMessageArguments`

### Description

### Messages

#### `Default`: warning

---

## Rule `SARIF2003.ProduceEnrichedSarif`

### Description

### Messages

#### `ProvideVersionControlProvenance`: note

#### `ProvideCodeSnippets`: note

#### `ProvideContextRegion`: note

#### `ProvideRuleHelpUris`: note

#### `EmbedFileContent`: note

---

## Rule `SARIF2004.OptimizeFileSize`

### Description

### Messages

#### `EliminateLocationOnlyArtifacts`: warning

#### `DoNotIncludeExtraIndexedObjectProperties`: warning

---

## Rule `SARIF2005.ProvideHelpfulToolInformation`

### Description

### Messages

#### `ProvideConciseToolName`: note

#### `ProvideToolVersion`: warning

#### `UseNumericToolVersions`: warning

---

## Rule `SARIF2006.UrisShouldBeReachable`

### Description

### Messages

#### `Default`: warning

---

## Rule `SARIF2007.ExpressPathsRelativeToRepoRoot`

### Description

### Messages

#### `Default`: warning

---

## Rule `SARIF2008.ProvideSchema`

### Description

### Messages

#### `Default`: warning

---

## Rule `SARIF2009.UseConventionalSymbolicNames`

### Description

SARIF uses symbolic names in various contexts. This rule proposes uniform naming conventions for the various types of symbolic names.

Many tools follow a conventional format for the 'reportingDescriptor.id' property: a short string identifying the tool concatenated with a numeric rule number,
for example, 'CS2001' for a diagnostic from the Roslyn C# compiler. For uniformity of experience across tools, we recommend this format.

### Messages

#### `UseConventionalRuleIds`: note

{0}: The 'name' property ' of the rule '{1}' does not follow the recommended format: a short string identifying the tool concatenated with a numeric rule number, for example, `CS2001`. Using a conventional format for the rule id provides a more uniform experience across tools.

#### `UseConventionalUriBaseIdNames`: note

---
