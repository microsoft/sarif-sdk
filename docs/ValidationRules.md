# Rules

## Rule `GH1001.ProvideRequiredLocationProperties`

### Description

Each result location must provide the property 'physicalLocation.artifactLocation.uri'. GitHub Advanced Security code scanning will not display a result whose location does not provide the URI of the artifact that contains the result.

### Messages

#### `NoLocationsArray`: Error

{0}: The 'locations' property is absent. GitHub Advanced Security code scanning will not display a result unless it provides a location that specifies the URI of the artifact that contains the result.

#### `EmptyLocationsArray`: Error

{0}: The 'locations' array is empty. GitHub Advanced Security code scanning will not display a result unless it provides a location that specifies the URI of the artifact that contains the result.

#### `MissingLocationProperty`: Error

{0}: '{1}' is absent. GitHub Advanced Security code scanning will not display a result location that does not provide the URI of the artifact that contains the result.

---

## Rule `GH1002.InlineThreadFlowLocations`

### Description

Results that include codeFlows must specify each threadFlowLocation directly within the codeFlow, rather than relying on threadFlowLocation.index to refer to an element of the run.threadFlowLocations array. GitHub Advanced Security code scanning will not display a result that uses such threadFlowLocations.

### Messages

#### `Default`: Error

{0}: This 'threadFlowLocation' uses its 'index' property to refer to information in the 'run.threadFlowLocations' array. GitHub Advanced Security code scanning will not display a result that includes such a 'threadFlowLocation'.

---

## Rule `GH1003.ProvideRequiredRegionProperties`

### Description

Every result must provide a 'region' that specifies its location with line and optional column information. GitHub Advanced Security code scanning can display the correct location only for results that provide this information. At minimum, 'region.startLine' is required. 'region' can also provide 'startColumn', 'endLine', and 'endColumn', although all of those have reasonable defaults.

### Messages

#### `MissingRegion`: Error

{0}: The 'region' property is absent. GitHub Advanced Security code scanning can display the correct location only for results that provide a 'region' object with line and optional column information. At minimum, 'region.startLine' is required. 'region' can also provide 'startColumn', 'endLine', and 'endColumn', although all of those have reasonable defaults.

#### `MissingRegionProperty`: Error

{0}: The 'startLine' property is absent. GitHub Advanced Security code scanning can display the correct location only for results that provide a 'region' object with line and optional column information. At minimum, 'region.startLine' is required. 'region' can also provide 'startColumn', 'endLine', and 'endColumn', although all of those have reasonable defaults.

---

## Rule `GH1004.ReviewArraysThatExceedConfigurableDefaults`

### Description

GitHub Advanced Security code scanning limits the amount of information it displays. There are limits on the number of runs per log file, rules per run, results per run, locations per result, code flows per result, and steps per code flow. You can provide a configuration file at the root of your repository to specify higher limits.

### Messages

#### `Default`: Error

{0}: This array contains {1} element(s), which exceeds the default limit of {2} imposed by GitHub Advanced Security code scanning. GitHub will only display information up to that limit. You can provide a configuration file at the root of your repository to specify a higher limit.

---

## Rule `GH1005.LocationsMustBeRelativeUrisOrFilePaths`

### Description

GitHub Advanced Security code scanning only displays results whose locations are specified by file paths, either as relative URIs or as absolute URIs that use the 'file' scheme.

### Messages

#### `Default`: Error

{0}: '{1}' is not a file path. GitHub Advanced Security code scanning only displays results whose locations are specified by file paths, either as relative URIs or as absolute URIs that use the 'file' scheme.

---

## Rule `GH1006.ProvideCheckoutPath`

### Description

GitHub Advanced Security code scanning will reject a SARIF file that expresses result locations as absolute 'file' scheme URIs unless GitHub can determine the URI of the repository root (which GitHub refers to as the "checkout path"). There are three ways to address this issue.

1. Recommended: Express all result locations as relative URI references with respect to the checkout path.

1. Place the checkout path in 'invocations[].workingDirectory'. The SARIF specification defines that property to be the working directory of the process that executed the analysis tool, so if the tool was not invoked from the repository root directory, it isn't strictly legal to place the checkout path there.

2. Place the checkout path in a configuration file at the root of the repository. This requires the analysis tool always to be invoked from that same directory.

### Messages

#### `Default`: Error

{0}: This result location is expressed as an absolute 'file' URI. GitHub Advanced Security code scanning will reject this file because it cannot determine the location of the repository root (which it refers to as the "checkout path"). Either express result locations as relative URI references with respect to the checkout path, place the checkout path in 'invocations[].workingDirectory', or place the checkout path in a configuration file at the root of the repository.

---

## Rule `SARIF1001.RuleIdentifiersMustBeValid`

### Description

The two identity-related properties of a SARIF rule must be consistent. The required 'id' property must be a "stable, opaque identifier" (the SARIF specification ([3.49.3](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317839)) explains the reasons for this). The optional 'name' property ([3.49.7](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317843)) is an identifer that is understandable to an end user. Therefore if both 'id' and 'name' are present, they must be different. If both 'name' and 'id' are opaque identifiers, omit the 'name' property. If both 'name' and 'id' are human-readable identifiers, then consider assigning an opaque identifier to each rule, but in the meantime, omit the 'name' property.

### Messages

#### `Default`: Warning

{0}: The rule '{1}' has a 'name' property that is identical to its 'id' property. The required 'id' property must be a "stable, opaque identifier" (the SARIF specification ([3.49.3](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317839)) explains the reasons for this). The optional 'name' property ([3.49.7](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317843)) is an identifer that is understandable to an end user. Therefore if both 'id' and 'name' are present, they must be different. If they are identical, the tool must omit the 'name' property.

---

## Rule `SARIF1002.UrisMustBeValid`

### Description

Specify a valid URI reference for every URI-valued property.

URIs must conform to [RFC 3986](https://tools.ietf.org/html/rfc3986). In addition, 'file' URIs must not include '..' segments. If symbolic links are present, '..' might have different meanings on the machine that produced the log file and the machine where an end user or a tool consumes it.

### Messages

#### `UrisMustConformToRfc3986`: Error

{0}: The string '{1}' is not a valid URI reference. URIs must conform to [RFC 3986](https://tools.ietf.org/html/rfc3986).

#### `FileUrisMustNotIncludeDotDotSegments`: Error

{0}: The 'file' URI '{1}' contains a '..' segment. This is dangerous because if symbolic links are present, '..' might have different meanings on the machine that produced the log file and the machine where an end user or a tool consumes it.

---

## Rule `SARIF1004.ExpressUriBaseIdsCorrectly`

### Description

When using the 'uriBaseId' property, obey the requirements in the SARIF specification [3.4.4](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317431) that enable it to fulfill its purpose of resolving relative references to absolute locations. In particular:

If an 'artifactLocation' object has a 'uriBaseId' property, its 'uri' property must be a relative reference, because if 'uri' is an absolute URI then 'uriBaseId' serves no purpose.

Every URI reference in 'originalUriBaseIds' must resolve to an absolute URI in the manner described in the SARIF specification [3.14.14](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317498).

### Messages

#### `UriBaseIdRequiresRelativeUri`: Error

{0}: This 'artifactLocation' object has a 'uriBaseId' property '{1}', but its 'uri' property '{2}' is an absolute URI. Since the purpose of 'uriBaseId' is to resolve a relative reference to an absolute URI, it is not allowed when the 'uri' property is already an absolute URI.

#### `TopLevelUriBaseIdMustBeAbsolute`: Error

{0}: The '{1}' element of 'originalUriBaseIds' has no 'uriBaseId' property, but its 'uri' property '{2}' is not an absolute URI. According to the SARIF specification, every such "top-level" entry in 'originalUriBaseIds' must specify an absolute URI, because the purpose of 'originalUriBaseIds' is to enable the resolution of relative references to absolute URIs.

#### `UriBaseIdValueMustEndWithSlash`: Error

{0}: The '{1}' element of 'originalUriBaseIds' has a 'uri' property '{2}' that does not end with a slash. The trailing slash is required to minimize the likelihood of an error when concatenating URI segments together.

#### `UriBaseIdValueMustNotContainDotDotSegment`: Error

{0}: The '{1}' element of 'originalUriBaseIds' has a 'uri' property '{2}' that contains a '..' segment. This is dangerous because if symbolic links are present, '..' might have different meanings on the machine that produced the log file and the machine where an end user or a tool consumes it.

#### `UriBaseIdValueMustNotContainQueryOrFragment`: Error

{0}: The '{1}' element of 'originalUriBaseIds' has a 'uri' property '{2}' that contains a query or a fragment. This is not valid because the purpose of the 'uriBaseId' property is to help resolve a relative reference to an absolute URI by concatenating the relative reference to the absolute base URI. This won't work if the base URI contains a query or a fragment.

---

## Rule `SARIF1005.UriMustBeAbsolute`

### Description

Certain URIs are required to be absolute. For the most part, these are URIs that refer to http addresses, such as work items or rule help topics.

### Messages

#### `Default`: Error

{0}: The value of this property is required to be an absolute URI, but '{1}' is a relative URI reference.

---

## Rule `SARIF1006.InvocationPropertiesMustBeConsistent`

### Description

The properties of an 'invocation' object must be consistent.

If the 'invocation' object specifies both 'startTimeUtc' and 'endTimeUtc', then 'endTimeUtc' must not precede 'startTimeUtc'. To allow for the possibility that the duration of the run is less than the resolution of the string representation of the time, the start time and the end time may be equal.

### Messages

#### `EndTimeMustNotPrecedeStartTime`: Error

{0}: The 'endTimeUtc' value '{1}' precedes the 'startTimeUtc' value '{2}'. The properties of an 'invocation' object must be internally consistent.

---

## Rule `SARIF1007.RegionPropertiesMustBeConsistent`

### Description

The properties of a 'region' object must be consistent.

SARIF can specify a 'region' (a contiguous portion of a file) in a variety of ways: with line and column numbers, with a character offset and count, or with a byte offset and count. The specification states certain constraints on these properties, both within each property group (for example, the start line cannot be greater than end line) and between the groups (for example, if more than one group is present, they must independently specify the same portion of the file). See the SARIF specification ([3.30](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317685)).

### Messages

#### `EndLineMustNotPrecedeStartLine`: Error

{0}: In this 'region' object, the 'endLine' property '{1}' is less than the 'startLine' property '{2}'. The properties of a 'region' object must be internally consistent.

#### `EndColumnMustNotPrecedeStartColumn`: Error

{0}: In this 'region' object, the 'endColumn' property '{1}' is less than the 'startColumn' property '{2}'. The properties of a 'region' object must be internally consistent.

#### `RegionStartPropertyMustBePresent`: Error

{0}: This 'region' object does not specify 'startLine', 'charOffset', or 'byteOffset'. As a result, it is impossible to determine whether this 'region' object describes a line/column text region, a character offset/length text region, or a binary region.

---

## Rule `SARIF1008.PhysicalLocationPropertiesMustBeConsistent`

### Description

Ensure consistency among the properties of a 'physicalLocation' object.

A SARIF 'physicalLocation' object has two related properties 'region' and 'contextRegion'. If 'contextRegion' is present, then 'region' must also be present, and 'contextRegion' must be a "proper superset" of 'region'. That is, 'contextRegion' must completely contain 'region', and it must be larger than 'region'. To understand why this is so we must understand the roles of the 'region' and 'contextRegion' properties.

'region' allows both users and tools to distinguish similar results within the same artifact. If a SARIF viewer has access to the artifact, it can display it, and highlight the location identified by the analysis tool. If the region has a 'snippet' property, then even if the viewer doesn't have access to the artifact (which might be the case for a web-based viewer), it can still display the faulty code.

'contextRegion' provides users with a broader view of the result location. Typically, it consists of a range starting a few lines before 'region' and ending a few lines after. Again, if a SARIF viewer has access to the artifact, it can display it, and highlight the context region (perhaps in a lighter shade than the region itself). This isn't terribly useful since the user can already see the whole file, with the 'region' already highlighted. But if 'contextRegion' has a 'snippet' property, then even a viewer without access to the artifact can display a few lines of code surrounding the actual result, which is helpful to users.

If the validator reports that 'contextRegion' is not a proper superset of 'region', then it's possible that the tool reversed 'region' and 'contextRegion'. If 'region' and 'contextRegion' are identical, the tool should simply omit 'contextRegion'.

### Messages

#### `ContextRegionRequiresRegion`: Error

{0}: This 'physicalLocation' object contains a 'contextRegion' property, but it does not contain a 'region' property. This is invalid because the purpose of 'contextRegion' is to provide a viewing context around the 'region' which is the location of the result. If a tool associates only one region with a result, it must populate 'region', not 'contextRegion'.

#### `ContextRegionMustBeProperSupersetOfRegion`: Error

{0}: This 'physicalLocation' object contains both a 'region' and a 'contextRegion' property, but 'contextRegion' is not a proper superset of 'region'. This is invalid because the purpose of 'contextRegion' is to provide a viewing context around the 'region' which is the location of the result. It's possible that the tool reversed 'region' and 'contextRegion'. If 'region' and 'contextRegion' are identical, the tool must omit 'contextRegion'.

---

## Rule `SARIF1009.IndexPropertiesMustBeConsistentWithArrays`

### Description

If an object contains a property that is used as an array index (an "index-valued property"), then that array must be present and must contain at least "index + 1" elements.

### Messages

#### `TargetArrayMustExist`: Error

{0}: This '{1}' object contains a property '{2}' with value {3}, but '{4}' does not exist. An index-valued property always refers to an array, so the array must be present.

#### `TargetArrayMustBeLongEnough`: Error

{0}: This '{1}' object contains a property '{2}' with value {3}, but '{4}' has fewer than {5} elements. An index-valued properties must be valid for the array that it refers to.

---

## Rule `SARIF1010.RuleIdMustBeConsistent`

### Description

Every result must contain at least one of the properties 'ruleId' and 'rule.id'. If both are present, they must be equal. See the SARIF specification ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)).

### Messages

#### `ResultRuleIdMustBeConsistent`: Error

{0}: This result contains both the 'ruleId' property '{1}' and the 'rule.id' property '{2}', but they are not equal. The SARIF specification ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)) requires that if both of these properties are present, they must be equal.

#### `ResultMustSpecifyRuleId`: Error

{0}: This result contains neither of the properties 'ruleId' or 'rule.id'. The SARIF specification ([§3.27.5](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317643)) requires at least one of these properties to be present.

---

## Rule `SARIF1011.ReferenceFinalSchema`

### Description

The '$schema' property must refer to the final version of the SARIF 2.1.0 schema. This enables IDEs to provide Intellisense for SARIF log files.

The SARIF standard was developed over several years, and many intermediate versions of the schema were produced. Now that the standard is final, only the OASIS standard version of the schema is valid.

### Messages

#### `Default`: Error

{0}: The '$schema' property value '{1}' does not refer to the final version of the SARIF 2.1.0 schema. If you are using an earlier version of the SARIF format, consider upgrading your analysis tool to produce the final version. If this file does in fact conform to the final version of the schema, upgrade the tool to populate the '$schema' property with a URL that refers to the final version of the schema.

---

## Rule `SARIF1012.MessageArgumentsMustBeConsistentWithRule`

### Description

The properties of a result's 'message' property must be consistent with the properties of the rule that the result refers to.

When a result's 'message' object uses the 'id' and 'arguments' properties (which, by the way, is recommended: see SARIF2002.ProvideMessageArguments), it must ensure that the rule actually defines a message string with that id, and that 'arguments' array has enough elements to provide values for every replacement sequence in the message specified by 'id'. For example, if the highest numbered replacement sequence in the specified message string is '{{3}}', then the 'arguments' array must contain at least 4 elements.

### Messages

#### `MessageIdMustExist`: Error

{0}: This message object refers to the message with id '{1}' in rule '{2}', but that rule does not define a message with that id. When a tool creates a result message that uses the 'id' property, it must ensure that the specified rule actually has a message with that id.

#### `SupplyEnoughMessageArguments`: Error

{0}: The message with id '{1}' in rule '{2}' requires '{3}' arguments, but the 'arguments' array in this message object has only '{4}' element(s). When a tool creates a result message that use the 'id' and 'arguments' properties, it must ensure that the 'arguments' array has enough elements to provide values for every replacement sequence in the message specified by 'id'. For example, if the highest numbered replacement sequence in the specified message string is '{{3}}', then the 'arguments' array must contain 4 elements.

---

## Rule `SARIF2001.TerminateMessagesWithPeriod`

### Description

Express plain text result messages as complete sentences and end each sentence with a period. This guidance does not apply to Markdown messages, which might include formatting that makes the punctuation unnecessary.

This is part of a set of authoring practices that make your rule messages more readable, understandable, and actionable. See also `SARIF2014.ProvideDynamicMessageContent` and `SARIF2015.EnquoteDynamicMessageContent`.

### Messages

#### `Default`: Warning

{0}: In rule '{1}', the message with id '{2}' does not end in a period. Express plain text rule messages as complete sentences. This guidance does not apply to Markdown messages, which might include formatting that makes the punctuation unnecessary.

---

## Rule `SARIF2002.ProvideMessageArguments`

### Description

In result messages, use the 'message.id' and 'message.arguments' properties rather than 'message.text'. This has several advantages. If 'text' is lengthy, using 'id' and 'arguments' makes the SARIF file smaller. If the rule metadata is stored externally to the SARIF log file, the message text can be improved (for example, by adding more text, clarifying the phrasing, or fixing typos), and the result messages will pick up the improvements the next time it is displayed. Finally, SARIF supports localizing messages into different languages, which is possible if the SARIF file contains 'message.id' and 'message.arguments', but not if it contains 'message.text' directly.

### Messages

#### `Default`: Note

{0}: The 'message' property of this result contains a 'text' property. Consider replacing it with 'id' and 'arguments' properties. This potentially reduces the log file size, allows the message text to be improved without modifying the log file, and enables localization.

---

## Rule `SARIF2003.ProvideVersionControlProvenance`

### Description

Provide 'versionControlProvenance' to record which version of the code was analyzed, and to enable paths to be expressed relative to the root of the repository.

### Messages

#### `Default`: Note

{0}: This run does not provide 'versionControlProvenance'. As a result, it is not possible to determine which version of code was analyzed, nor to map relative paths to their locations within the repository.

---

## Rule `SARIF2004.OptimizeFileSize`

### Description

Emit arrays only if they provide additional information.

In several parts of a SARIF log file, a subset of information about an object appears in one place, and the full information describing all such objects appears in an array elsewhere in the log file. For example, each 'result' object has a 'ruleId' property that identifies the rule that was violated. Elsewhere in the log file, the array 'run.tool.driver.rules' contains additional information about the rules. But if the elements of the 'rules' array contained no information about the rules beyond their ids, then there might be no reason to include the 'rules' array at all, and the log file could be made smaller simply by omitting it. In some scenarios (for example, when assessing compliance with policy), the 'rules' array might be used to record the full set of rules that were evaluated. In such a scenario, the 'rules' array should be retained even if it contains only id information.

Similarly, most 'result' objects contain at least one 'artifactLocation' object. Elsewhere in the log file, the array 'run.artifacts' contains additional information about the artifacts that were analyzed. But if the elements of the 'artifacts' array contained not information about the artifacts beyond their locations, then there might be no reason to include the 'artifacts' array at all, and again the log file could be made smaller by omitting it. In some scenarios (for example, when assessing compliance with policy), the 'artifacts' array might be used to record the full set of artifacts that were analyzed. In such a scenario, the 'artifacts' array should be retained even if it contains only location information.

In addition to the avoiding unnecessary arrays, there are other ways to optimize the size of SARIF log files.

Prefer the result object properties 'ruleId' and 'ruleIndex' to the nested object-valued property 'result.rule', unless the rule comes from a tool component other than the driver (in which case only 'result.rule' can accurately point to the metadata for the rule). The 'ruleId' and 'ruleIndex' properties are shorter and just as clear.

Do not specify the result object's 'analysisTarget' property unless it differs from the result location. The canonical scenario for using 'result.analysisTarget' is a C/C++ language analyzer that is instructed to analyze example.c, and detects a result in the included file example.h. In this case, 'analysisTarget' is example.c, and the result location is in example.h.

### Messages

#### `AvoidDuplicativeAnalysisTarget`: Warning

The 'analysisTarget' property '{1}' at '{0}' can be removed because it is the same as the result location. This unnecessarily increases log file size. The 'analysisTarget' property is used to distinguish cases when a tool detects a result in a file (such as an included header) that is different than the file that was scanned (such as a .cpp file that included the header).

#### `AvoidDuplicativeResultRuleInformation`: Warning

'{0}' uses the 'rule' property to specify the violated rule, so it is not necessary also to specify 'ruleId' or 'ruleIndex'. This unnecessarily increases log file size. Remove the 'ruleId' and 'ruleIndex' properties.

#### `EliminateLocationOnlyArtifacts`: Warning

The 'artifacts' array at '{0}' contains no information beyond the locations of the artifacts. Removing this array might reduce the log file size without losing information. In some scenarios (for example, when assessing compliance with policy), the 'artifacts' array might be used to record the full set of artifacts that were analyzed. In such a scenario, the 'artifacts' array should be retained even if it contains only location information.

#### `EliminateIdOnlyRules`: Warning

The 'rules' array at '{0}' contains no information beyond the ids of the rules. Removing this array might reduce the log file size without losing information. In some scenarios (for example, when assessing compliance with policy), the 'rules' array might be used to record the full set of rules that were evaluated. In such a scenario, the 'rules' array should be retained even if it contains only id information.

#### `PreferRuleId`: Warning

The result at '{0}' uses the 'rule' property to specify the violated rule, but this is not necessary because the rule is defined by 'tool.driver'. Use the 'ruleId' and 'ruleIndex' instead, because they are shorter and just as clear.

---

## Rule `SARIF2005.ProvideToolProperties`

### Description

Provide information that makes it easy to identify the name and version of your tool.

The tool's 'name' property should be no more than three words long. This makes it easy to remember and allows it to fit into a narrow column when displaying a list of results. If you need to provide more information about your tool, use the 'fullName' property.

The tool should provide either or both of the 'version' and 'semanticVersion' properties. This enables the log file consumer to determine whether the file was produced by an up to date version, and to avoid accidentally comparing log files produced by different tool versions.

If 'version' is used, facilitate comparison between versions by specifying a version number that starts with an integer, optionally followed by any desired characters.

### Messages

#### `ProvideToolVersion`: Warning

{0}: The tool '{1}' does not provide any of the version-related properties {2}. Providing version information enables the log file consumer to determine whether the file was produced by an up to date version, and to avoid accidentally comparing log files produced by different tool versions.

#### `ProvideConciseToolName`: Warning

{0}: The tool name '{1}' contains {2} words, which is more than the recommended maximum of {3} words. A short tool name is easy to remember and fits into a narrow column when displaying a list of results. If you need to provide more information about your tool, use the 'fullName' property.

#### `UseNumericToolVersions`: Warning

{0}: The tool '{1}' contains the 'version' property '{2}', which is not numeric. To facilitate comparison between versions, specify a 'version' that starts with an integer, optionally followed by any desired characters.

#### `ProvideToolnformationUri`: Warning

{0}: The tool '{1}' does not provide 'informationUri'. This property helps the developer responsible for addessing a result by providing a way to learn more about the tool.

---

## Rule `SARIF2006.UrisShouldBeReachable`

### Description

URIs that refer to locations such as rule help pages and result-related work items should be reachable via an HTTP GET request.

### Messages

#### `Default`: Note

{0}: The URI '{1}' was not reachable via an HTTP GET request.

---

## Rule `SARIF2007.ExpressPathsRelativeToRepoRoot`

### Description

Provide information that makes it possible to determine the repo-relative locations of files that contain analysis results.

Each element of the 'versionControlProvenance' array is a 'versionControlDetails' object that describes a repository containing files that were analyzed. 'versionControlDetails.mappedTo' defines the file system location to which the root of that repository is mapped. If 'mappedTo.uriBaseId' is present, and if result locations are expressed relative to that 'uriBaseId', then the repo-relative location of each result can be determined.

### Messages

#### `ExpressResultLocationsRelativeToMappedTo`: Warning

{0}: This result location does not provide any of the 'uriBaseId' values that specify repository locations: '{1}'. As a result, it will not be possible to determine the location of the file containing this result relative to the root of the repository that contains it.

#### `ProvideUriBaseIdForMappedTo`: Warning

{0}: The 'versionControlDetails' object that describes the repository '{1}' does not provide 'mappedTo.uriBaseId'. As a result, it will not be possible to determine the repo-relative location of files containing analysis results for this repository.

---

## Rule `SARIF2008.ProvideSchema`

### Description

A SARIF log file should contain, on the root object, a '$schema' property that refers to the final, OASIS standard version of the SARIF 2.1.0 schema. This enables IDEs to provide Intellisense for SARIF log files.

### Messages

#### `Default`: Warning

{0}: The SARIF log file does not contain a '$schema' property. Add a '$schema' property that refers to the final, OASIS standard version of the SARIF 2.1.0 schema. This enables IDEs to provide Intellisense for SARIF log files.

---

## Rule `SARIF2009.ConsiderConventionalIdentifierValues`

### Description

Adopt uniform naming conventions for rule ids.

Many tools follow a conventional format for the 'reportingDescriptor.id' property: a short string identifying the tool concatenated with a numeric rule number, for example, 'CS2001' for a diagnostic from the Roslyn C# compiler. For uniformity of experience across tools, we recommend this format.

### Messages

#### `UseConventionalRuleIds`: Note

{0}: The 'id' property of the rule '{1}' does not follow the recommended format: a short string identifying the tool concatenated with a numeric rule number, for example, 'CS2001'. Using a conventional format for the rule id provides a more uniform experience across tools.

---

## Rule `SARIF2010.ProvideCodeSnippets`

### Description

Provide code snippets to enable users to see the code that triggered each result, even if they are not enlisted in the code.

### Messages

#### `Default`: Note

{0}: The 'region' object in this result location does not provide a 'snippet' property. Providing a code snippet enables users to see the code that triggered the result, even if they are not enlisted in the code.

---

## Rule `SARIF2011.ProvideContextRegion`

### Description

Provide context regions to enable users to see a portion of the code that surrounds each result, even if they are not enlisted in the code.

### Messages

#### `Default`: Note

{0}: This result location does not provide a 'contextRegion' property. Providing a context region enables users to see a portion of the code that surrounds the result, even if they are not enlisted in the code.

---

## Rule `SARIF2012.ProvideRuleProperties`

### Description

Rule metadata should provide information that makes it easy to understand and fix the problem.

Provide the 'name' property, which contains a "friendly name" that helps users see at a glance the purpose of the rule. For uniformity of experience across all tools that produce SARIF, the friendly name should be a single Pascal identifier, for example, 'ProvideRuleFriendlyName'.

Provide the 'helpUri' property, which contains a URI where users can find detailed information about the rule. This information should include a detailed description of the invalid pattern, an explanation of why the pattern is poor practice (particularly in contexts such as security or accessibility where driving considerations might not be readily apparent), guidance for resolving the problem (including describing circumstances in which ignoring the problem altogether might be appropriate), examples of invalid and valid patterns, and special considerations (such as noting when a violation should never be ignored or suppressed, noting when a violation could cause downstream tool noise, and noting when a rule can be configured in some way to refine or alter the analysis).

### Messages

#### `ProvideFriendlyName`: Note

{0}: The rule '{1}' does not provide a "friendly name" in its 'name' property. The friendly name should be a single Pascal identifier, for example, 'ProvideRuleFriendlyName', that helps users see at a glance the purpose of the analysis rule.

#### `FriendlyNameNotAPascalIdentifier`: Note

{0}: '{1}' is not a Pascal identifier. For uniformity of experience across all tools that produce SARIF, the friendly name should be a single Pascal identifier, for example, 'ProvideRuleFriendlyName'.

#### `ProvideHelpUri`: Note

{0}: The rule '{1}' does not provide a help URI. Providing a URI where users can find detailed information about the rule helps users to understand the result and how they can best address it.

---

## Rule `SARIF2013.ProvideEmbeddedFileContent`

### Description

Provide embedded file content so that users can examine results in their full context without having to enlist in the source repository. Embedding file content in a SARIF log file can dramatically increase its size, so consider the usage scenario when you decide whether to provide it.

### Messages

#### `Default`: Note

{0}: This run does not provide embedded file content. Providing embedded file content enables users to examine results in their full context without having to enlist in the source repository. Embedding file content in a SARIF log file can dramatically increase its size, so consider the usage scenario when you decide whether to provide it.

---

## Rule `SARIF2014.ProvideDynamicMessageContent`

### Description

Include "dynamic content" (information that varies among results from the same rule) to makes your messages more specific, and to avoid the "wall of bugs" phenomenon, where hundreds of occurrences of the same message appear unapproachable.

This is part of a set of authoring practices that make your rule messages more readable, understandable, and actionable. See also 'SARIF2001.TerminateMessagesWithPeriod' and 'SARIF2015.EnquoteDynamicMessageContent'.

### Messages

#### `Default`: Note

{0}: In rule '{1}', the message with id '{2}' does not include any dynamic content. Dynamic content makes your messages more specific and avoids the "wall of bugs" phenomenon, where hundreds of occurrences of the same message appear unapproachable.

---

## Rule `SARIF2015.EnquoteDynamicMessageContent`

### Description

Place dynamic content in single quotes to set it off from the static text and to make it easier to spot. It's especially helpful when the dynamic content is a string that might contain spaces, and most especially when the string might be empty (and so would be invisible if it weren't for the quotes). We recommend single quotes for a less cluttered appearance, even though US English usage would require double quotes.

This is part of a set of authoring practices that make your rule messages more readable, understandable, and actionable. See also 'SARIF2001.TerminateMessagesWithPeriod' and 'SARIF2014.ProvideDynamicMessageContent'.

### Messages

#### `Default`: Note

{0}: In rule '{1}', the message with id '{2}' includes dynamic content that is not enclosed in single quotes. Enquoting dynamic content makes it easier to spot, and single quotes give a less cluttered appearance.

---

## Rule `SARIF2016.FileUrisShouldBeRelative`

### Description

When an artifact location refers to a file on the local file system, specify a relative reference for the uri property and provide a uriBaseId property, rather than specifying an absolute URI.

There are several advantages to this approach:

Portability: A log file that contains relative references together with uriBaseI properties can be interpreted on a machine where the files are located at a different absolute location.

Determinism: A log file that uses uriBaseId properties has a better chance of being 'deterministic'; that is, of being identical from run to run if none of its inputs have changed, even if those runs occur on machines where the files are located at different absolute locations.

Security: The use of uriBaseId properties avoids the persistence of absolute path names in the log file. Absolute path names can reveal information that might be sensitive.

Semantics: Assuming the reader of the log file (an end user or another tool) has the necessary context, they can understand the meaning of the location specified by the uri property, for example, 'this is a source file'.

### Messages

#### `Default`: Note

{0}: The file location '{1}' is specified with absolute URI. Prefer a relative reference together with a uriBaseId property.

---

