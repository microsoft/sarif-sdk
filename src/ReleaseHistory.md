# Sarif Driver and SDK Release History

## **v2.1.0** [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.0) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/2.1.0)
* Add support for converting TSLint logs to SARIF
* Add support for converting Pylint logs to SARIF

## **v1.5.19-beta** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.19-beta) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.19-beta)
* Moved SarifLogger and its dependencies from driver to SDK package
* Include this file and JSON schema in packages

## **v1.5.20-beta** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.20-beta) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.20-beta)
* Rename Microsoft.CodeAnalysis.Sarif.Sdk namespace to Microsoft.CodeAnalysis.Sarif
* Rename Microsoft.CodeAnalysis.Sarif.Driver namespace to Microsoft.CodeAnalysis.Driver
* Eliminate some tool version details. Add SarifLogger version as tool property

## **v1.5.21-beta** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.21-beta) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.21-beta)
* Persist mime-type for files in SarifLogger
* Remove stack persistence for configuration notification exceptions
* Reclassify `could not parse target` as a configuration notification
* Fix diffing visitor to diff using value type semantics rather than by reference equality

## **v1.5.22-beta** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.22-beta) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.22-beta)
* Add suppressionStates enum (with a single current value, indicating `suppressedInSource`)
* Add `id` and `correlationId` as arguments to ResultLogJsonWriter.Initialize. Log `id` is populated with a generated guid by default.
* Add `sarifLoggerVersion` that identifies the SDK logger version used to produce a log file.
* Provide serialization of arbitrary JSON content to `properties` members.
* Move `tags` into properties (but provide top-level Tags member for setting/retrieving this data)
* Add annotatedCodeLocation.kind enum (with values such as `branch`, `declaration`, et al.)
* Update all converters to Sarif beta.5
* Add optional `id` to each result, to allow correlation with external data, annotations, work items, etc.
* Add flag to configure file hash computation to FileData.Create helper
* Add `uriBaseId` conceptual base URI to all format URI properties (to allow all URIs to be relative)
* Add `analysisTargetUri` to run object, for cases where a single target is associated with a run
* Add `threadId` to notification, annotatedCodeLocation and stackFrame.
* Rework files and logicalLocations dictionary to store discrete items (with parent keys), not arrays
* Add logicalLocationKey and fullyQualifiedLogicalLocationName to annotatedCodeLocation
* Add `id` and `essential` properties to annotatedCodeLocation
* Rename `toolFingerprint` to `toolFingerprintContribution`
* Add baselineId. Rename `correlationId` to `automationId`
* Add `physicalLocation` property to notification

## **v1.5.23-beta** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.23-beta) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.23-beta)
* Rename `codeSnippet` to `snippet`
* Remove requirement to specify `description` on code fixes
* Add `architecture` back to `run` object

## **v1.5.24-beta** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.24-beta) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.24-beta)
* Permit annotatedCodeLocation.id to be a numeric value (in addition to a string)

## **v1.5.25** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.25) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.25)
* NOTE: NON-BETA RELEASE
* Add a converter for Static Driver Verifier trace files
* Add SuppressedExternally to SuppressionStates enum

## **v1.5.26** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.26) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.26)  
* API BREAKING change on SarifLogger to explicitly specify hash computation for all files  
* SarifLogger now automatically persists file data for all URIs through format  
* Add run.stableId, a consistent run-over-run log identifier  
* Add annotatedCodeLocation.callee and annotatedCodeLocation.calleeKey for annotation call sites  
* Add invocation.responseFiles to capture response file contents
* Drop .NET framework dependency to 4.5 (from 4.5.1)

## **v1.5.27** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.27) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.27)
* Ship checked in CommandLine.dll in order to allow this `beta` NuGet component to ship in Driver non-beta release

## **v1.5.28** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.28) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.28)
* API BREAKING change: rename PropertyBagDictionary to PropertiesDictionary
* Add `functionReturn` to annotatedCodeLocation.kind
* Remove `source`, `sink` and `sanitizer` from annotatedCodeLocation.kind
* Add `taint` enum to annotatedCodeLocation with values `source`, `sink` and `sanitizer`
* Add `parameters` and `variables` members to annotatedCodeLocation
* Rename annotatedCodeLocation.callee member to `target`
* Rename annotatedCodeLocation.calleeKey member to `targetKey`

## **v1.5.29** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.29) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.29)

* Add `--quiet` option to suppress console output.

## **v1.5.30** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.30) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.30)

* Add static helper method `AnalyzeCommandBase.LogToolNotification`.

## **v1.5.31** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.31) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.31)

* API BREAKING CHANGE: RuleUtilities.BuildResult no longer automatically prepends the target file path to the list of FormattedRuleMessage.Arguments array in the Result object being built.

## **v1.5.32** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.32) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.32)

* Add `annotations` member to annotatedCodeLocation object.
* Rename annotatedCodeLocation `variables` member to `state`
* Rename annotatedCodeLocation `parameters` member to `values`

## **v1.5.33** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.33) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.33)

* Resolve crash generating `not applicable` messages

## **v1.5.34** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.34) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.34)

* Update schema for `annotations` object required properties

## **v1.5.35** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.35) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.35)

* Add `configuration` member to rule objects

## **v1.5.36** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.36) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.36)

* Provide better reporting for non-fatal messages.

## **v1.5.37** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.37) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.37)

* Further refinements to output on analysis completion.

## **v1.5.38** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.38) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.38)

* Preliminary Semmle converter

## **v1.5.39** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.39) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.39)

* Loosen requirement to explicitly provide --config argument for default configuration
* Convert Semmle embedded links to related locations
* Add File/Open of Semmle CSV to VS add-ing
* Eliminate redundant output of notifications
* Update FileSpecifier to resolve patternts such as File* properly

## **v1.5.40** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.40) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.40)

* Add JSON settings persistence 
* Populate context objects from configuration file argument

## **v1.5.41** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.41) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.41)
 * Control invocation property logging

## **v1.5.42** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.42) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.42)
 * Add EntryPointUtilities class that provides response file injection assistance
 * Rich return code support

## **v1.5.43** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.43) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.43)
 * Expose EntryPointUtilities helpers as public

## **v1.5.44** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.44) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.44)
 * Update default AnalyzeCommandBase behavior to utilize rich return code, if specified.

## **v1.5.45** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.45) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.45)
 * Track RuntimeConditions.OneOrMoreWarnings|ErrorsFired in RuleUtilities.BuildResult

## **v1.5.46** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.46) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.46)
 * Resolved crash deserializing empty property bags

## **v1.5.47** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.47) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.47)
 * Enable converter plugins
 * Adjust RuntimeConditions enum so that `command line parse` error is 0x1. 

## **v1.6.0** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.6.0) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.6.0)
 * Enable persistence of base64-encoded file contents via SarifLogger.
 * Rename AnalyzeOptions.ComputeTargetsHash to ComputeFileHashes
 * Fix bug in Semmle conversion (crash on embedded file:// scheme links)

## **v1.7.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/1.7.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/1.7.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/1.7.0) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/1.7.0)
 * Security and accessibility clean-up
 * TSLint converter fixes
 * Provide .NET core version
 * VSIX improvements (including auto-expansion of file contents persisted to SARIF logs)

## **v1.7.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/1.7.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/1.7.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/1.7.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/1.7.1)
 * Update nuget package build

## **v1.7.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/1.7.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/1.7.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/1.7.2) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/1.7.2)
 * Update Multitool nuget package build
 * Enable "pretty print" .sarif formatting via --pretty argument
 * Code sign 3rd party dependency assemblies (CommandLineParser, CsvHelper, Newtonsoft.Json)
 * Remove -beta flag from Driver and Multitool packages
 
 ## **v1.7.3** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/1.7.3) | [Driver](https://www.nuget.org/packages/Sarif.Driver/1.7.3) | [Converters](https://www.nuget.org/packages/Sarif.Converters/1.7.3) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/1.7.3)
 * Make SupportedPlatform a first class concept for skimmers
 * Rename --pretty argument to --pretty-print

 ## **v1.7.4** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/1.7.4) | [Driver](https://www.nuget.org/packages/Sarif.Driver/1.7.4) | [Converters](https://www.nuget.org/packages/Sarif.Converters/1.7.4) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/1.7.4)
* Platform Specific Tooling Text Fix
* Skimmers can now be disabled via the configuration file
* The Driver will now pull configuration from a default location to allow for easier re-packaging of tools with custom configurations

## **v1.7.5** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/1.7.5) | [Driver](https://www.nuget.org/packages/Sarif.Driver/1.7.5) | [Converters](https://www.nuget.org/packages/Sarif.Converters/1.7.5) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/1.7.5)
* Disabling skimmers text fix
* Fix a serialization bug with strings in a PropertyBag (not correctly escaped after a reserializing the data structure).
* Multitool improvements--added "rebaseUri" and "absoluteUri" tasks, which will either make the URIs in a SARIF log relative to some base URI, or take base URIs stored in SARIF and make the URIs absolute again.
* Added a "processing pipeline" model to the SARIF SDK in order to allow easy chaining of operations on SARIF logs (like making all URIs relative/absolute).

## **v2.0.0-csd.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.1)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.1))
* Convert object model to conform to SARIF v2 CSD.1 draft specification
* Distinguish textual vs. binary file persistence in rewrite option (and allow for both in multitool rewrite verb)
*   NOTE: the change above introduces a command-line breaking change. --persist-file-contents is now renamed to --insert
* Add ComprehensiveRegionProperties, RegionSnippets and ContextCodeSnippets as possible qualifier to --insert option
* Provide SARIF v1.0 object model and v1 <-> v2 transformation API

## **v2.0.0-csd.1.0.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.1.0.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.1.0.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.1.0.1)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.1.0.1))
* API BREAKING CHANGE: Fix weakly typed CreateNotification calls and make API more strongly typed
* API BREAKING CHANGE: Rename OptionallyEmittedData.ContextCodeSnippets to ContextRegionSnippets
* API BREAKING CHANGE: Eliminate result.ruleMessageId (in favor of result.message.messageId)

## **v2.0.0-csd.1.0.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.1.0.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.1.0.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.1.0.2)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.1.0.2))
* BUGFIX: In result matching algorithm, an empty or null previous log no longer causes a NullReferenceException.
* BUGFIX: In result matching algorithm, duplicate data is no longer incorrectly detected across files. Also: changed a "NotImplementedException" to the correct "InvalidOperationException".

## **v2.0.0-csd.2.beta.2018-10-10** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2018-10-10) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2018-10-10) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2018-10-10)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2018-10-10))
* FEATURE:Add --sarif-version command to driver (to transform SARIF output to v1 format)
* BUGFIX: Drop erroneous persistence of redaction tokens as files objects.
* API NON-BREAKING: Add `result.occurrenceCount` (denotes # of occurrences of an identical results within an analysisRun)
* API NON-BREAKING: Add `run.externalFiles` object to schema. Sync generally to OASIS TC schema.
* API BREAKING: `originalUriBaseIds` is now a dictionary of file locations, not strings.
* API BREAKING: Suffix `invocation.startTime`, `invocation.endTime`, `file.lastModifiedTime` and `notification.time` with Utc (`startTimeUtc`, `endTimeUtc`, etc.).
* API BREAKING: `threadflowLocation.timestamp` renamed to `executionTimeUtc`.
* API BREAKING: `versionControlDetails.timestamp` renamed to `asOfTimeUtc`.
* API BREAKING: `versionControlDetails.uri` renamed to `repositoryUri`.
* API BREAKING: `versionControlDetails.tag` renamed to `revisionTag`
* API BREAKING: `exception.message` type converted from string to message object.
* API BREAKING: `file.hashes` is now a string/string dictionary, not an array of `hash` objects (the type for which is deleted)
* API BREAKING: `run.instanceGuid`, `run.correlationGuid`, `run.logicalId`, `run.description` combined into new `runAutomationDetails` object instance defined at `run.id`.
* API BREAKING: `run.automationLogicalId` subsumed by `run.aggregateIds`, an array of `runAutomationDetails` objects.
* API BREAKING: Remove `threadFlowLocation.step`
* API BREAKING: `invocation.workingDirectory` is now a FileLocation object (and not a URI expressed as a string)

## **v2.0.0-csd.2.beta.2018-10-10.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2018-10-10.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2018-10-10.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2018-10-10.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2018-10-10.1)
* BUGFIX: Persist region information associated with analysis target

## **v2.0.0-csd.2.beta.2018-10-10.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2018-10-10.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2018-10-10.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2018-10-10.2) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2018-10-10.2)
* BUGFIX: Don`t emit v2 analysisTarget if there is no v1 resultFile.
* BUILD: Bring NuGet publishing scripts into conformance with new Microsoft requirements.

## **v2.0.0-csd.2.beta.2019-01-09** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019-01-09) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019-01-09) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019-01-09) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019-01-09)
* BUGFIX: Result matching improvements in properties persistence.
* FEATURE: Fortify FPR converter improvements.
* API NON-BREAKING: Remove uniqueness requirement from `result.locations`.
* API NON-BREAKING: Add `run.newlineSequences` to schema. https://github.com/oasis-tcs/sarif-spec/issues/169
* API NON-BREAKING: Add `rule.deprecatedIds` to schema. https://github.com/oasis-tcs/sarif-spec/issues/293
* API NON-BREAKING: Add `versionControlDetails.mappedTo`. https://github.com/oasis-tcs/sarif-spec/issues/248
* API NON-BREAKING: Add result.rank`. Add `ruleConfiguration.defaultRank`.
* API NON-BREAKING: Add `file.sourceLocation` and `region.sourceLanguage` to guide in snippet colorization. `run.defaultSourceLanguage` provides a default value. https://github.com/oasis-tcs/sarif-spec/issues/286
* API NON-BREAKING: default values for `result.rank` and `ruleConfiguration.defaultRank` is now -1.0 (from 0.0). https://github.com/oasis-tcs/sarif-spec/issues/303
* API BREAKING: Remove `run.architecture` https://github.com/oasis-tcs/sarif-spec/issues/262
* API BREAKING: `result.message` is now a required property https://github.com/oasis-tcs/sarif-spec/issues/283
* API BREAKING: Rename `tool.fileVersion` to `tool.dottedQuadFileVersion` https://github.com/oasis-tcs/sarif-spec/issues/274
* API BREAKING: Remove `open` from valid rule default configuration levels. https://github.com/oasis-tcs/sarif-spec/issues/288. The transformer remaps this value to `note`.
* API BREAKING: `run.columnKind` default value is now `unicodeCodePoints`. https://github.com/Microsoft/sarif-sdk/pull/1160. The transformer will inject `utf16CodeUnits`, however, when this property is absent, as this value is a more appropriate default for the Windows platform.
* API BREAKING: Make `run.logicalLocations` an array, not a dictionary. Add result.logicalLocationIndex to point to associated logical location.
* API BREAKING: `run.externalFiles` renamed to `run.externalPropertyFiles`, which is not a bundle of external property file objects. NOTE: no transformation will be provided for legacy versions of the external property files API.
* API BREAKING: rework `result.provenance` object, including moving result.conversionProvenance to `result.provenance.conversionSources`. NOTE: no transformation currently exists for this update.
* API BREAKING: Make `run.files` an array, not a dictionary. Add fileLocation.fileIndex to point to a file object associated with the location within `run.files`.
* API BREAKING: Make `resources.rules` an array, not a dictionary. Add result.ruleIndex to point to a rule object associated with the result within `resources.rules`.
* API BREAKING: `run.logicalLocations` now requires unique array elements. https://github.com/oasis-tcs/sarif-spec/issues/304

## **v2.0.0-csd.2.beta.2019.01-24** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.01-24) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.01-24) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.01-24)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.01-24))
* BUGFIX: SDK compatibility update for sample apps.
* BUGFIX: Add Sarif.Multitool.exe.config file to multitool package to resolve "Could not load file or assembly `Newtonsoft.Json, Version=9.0.0.0`" exception on using validate command.
* API BREAKING: rename baselineState `existing` value to `unchanged`. Add new baselineState value `updated`. https://github.com/oasis-tcs/sarif-spec/issues/312
* API BREAKING: unify result and notification failure levels (`note`, `warning`, `error`). Break out result evaluation state into `result.kind` property with values `pass`, `fail`, `open`, `review`, `notApplicable`. https://github.com/oasis-tcs/sarif-spec/issues/317
* API BREAKING: remove IRule entirely, in favor of utilizing ReportingDescriptor base class.
* API BREAKING: define `toolComponent` object to persist tool data. The `tool.driver` component documents the standard driver metadata. `tool.extensions` is an array of `toolComponent` instances that describe extensions to the core analyzer. This change also deletes `tool.sarifLoggerVersion` (from the newly created `toolComponent` object) due to its lack of utility. Adds `result.extensionIndex` to allow results to be associated with a plug-in. `toolComponent` also added as a new file role. https://github.com/oasis-tcs/sarif-spec/issues/179
* API BREAKING: Remove `run.resources` object. Rename `rule` object to `reportingDescriptor`. Move rule and notification reportingDescriptor objects to `tool.notificationDescriptors` and `tool.ruleDescriptors`. `resources.messageStrings` now located at `toolComponent.globalMessageStrings`. `rule.configuration` property now named `reportingDescriptor.defaultConfiguration`. `reportingConfiguration.defaultLevel` and `reportingConfiguration.defaultRank` simplified to `reportingConfiguration.level` and `reportingConfiguration.rank`. Actual runtime reportingConfiguration persisted to new array of reportingConfiguration objects at `invocation.reportingConfiguration`. https://github.com/oasis-tcs/sarif-spec/issues/311
* API BREAKING: `run.richTextMessageMimeType` renamed to `run.markdownMessageMimeType`. `message.richText` renamed to `message.markdown`. `message.richMessageId` deleted. Create `multiformatMessageString` object, that holds plain text and markdown message format strings. `reportingDescriptor.messageStrings` is now a dictionary of these objects, keyed by message id. `reporting.Descriptor.richMessageStrings` dictionary is deleted. https://github.com/oasis-tcs/sarif-spec/issues/319
* API BREAKING: `threadflowLocation.kind` is now `threadflowLocation.kinds`, an array of strings that categorize the thread flow location. https://github.com/oasis-tcs/sarif-spec/issues/202
* API BREAKING: `file` renamed to `artifact`. `fileLocation` renamed to `artifactLocation`. `run.files` renamed to `run.artifacts`. https://github.com/oasis-tcs/sarif-spec/issues/309

## **v2.0.0-csd.2.beta.2019.01-24.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.01-24.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.01-24.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.01-24.1)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.01-24.1))
* BUGFIX: `region.charOffset` default value should be -1 (invalid value) rather than 0. Fixes an issue where `region.charLength` is > 0 but `region.charOffset` is absent (because its value of 0 was incorrectly elided due to being the default value). 

## **v2.0.0-csd.2.beta.2019.02-20** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.02-20) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.02-20) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.02-20)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.02-20))
* COMMAND-LINE BREAKING: Rename `--sarif-version` to `--sarif-output-version`. Remove duplicative tranform `--target-version` command-line argument.
* COMMAND-LINE NON-BREAKING: add `--inline` option to multitool `rebaseuri` verb, to write output directly into input files.
* API NON-BREAKING: Add additional properties to `toolComponent`. https://github.com/oasis-tcs/sarif-spec/issues/336
* API NON-BREAKING: Provide a caching mechanism for duplicated code flow data. https://github.com/oasis-tcs/sarif-spec/issues/320
* API NON-BREAKING: Add `inlineExternalPropertyFiles` at the log level. https://github.com/oasis-tcs/sarif-spec/issues/321
* API NON-BREAKING: Update logical location kinds to accommodate XML and JSON paths. https://github.com/oasis-tcs/sarif-spec/issues/291
* API NON-BREAKING: Define result taxonomies. https://github.com/oasis-tcs/sarif-spec/issues/314
* API BREAKING: Remove `invocation.attachments`, now replaced by `run.tool.extensions`. https://github.com/oasis-tcs/sarif-spec/issues/327
* API NON-BREAKING: Introduce new localization mechanism. https://github.com/oasis-tcs/sarif-spec/issues/338
* API BREAKING: Remove `tool.language` and localization support. https://github.com/oasis-tcs/sarif-spec/issues/325
* API NON-BREAKING: Add additional properties to toolComponent. https://github.com/oasis-tcs/sarif-spec/issues/336
* API BREAKING: Rename `invocation.toolNotifications` and `invocation.configurationNotifications` to `toolExecutionNotifications` and `toolConfigurationNotifications`. https://github.com/oasis-tcs/sarif-spec/issues/330
* API BREAKING: Add address property to a location object (and other nodes). https://github.com/oasis-tcs/sarif-spec/issues/302
* API BREAKING: External property file related renames. https://github.com/oasis-tcs/sarif-spec/issues/335

## **v2.0.0-csd.2.beta.2019.04-03.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.04-03.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.04-03.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.04-03.0)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.04-03.0))
* API NON-BREAKING: Introduce new localization mechanism (post ballot changes). https://github.com/oasis-tcs/sarif-spec/issues/338
* API BREAKING: Add `address` property to a `location` object (post ballot changes). https://github.com/oasis-tcs/sarif-spec/issues/302
* API NON-BREAKING: Define result `taxonomies`. https://github.com/oasis-tcs/sarif-spec/issues/314
* API NON-BREAKING: Define a `reportingDescriptorReference` object. https://github.com/oasis-tcs/sarif-spec/issues/324
* API BREAKING: Change `run.graphs` and `result.graphs` from objects to arrays. https://github.com/oasis-tcs/sarif-spec/issues/326
* API BREAKING: External property file related renames (post ballot changes). https://github.com/oasis-tcs/sarif-spec/issues/335
* API NON-BREAKING: Allow toolComponents to be externalized. https://github.com/oasis-tcs/sarif-spec/issues/337
* API BREAKING: Rename all `instanceGuid` properties to `guid`. https://github.com/oasis-tcs/sarif-spec/issues/341
* API NON-BREAKING: Add `reportingDescriptor.deprecatedNames` and `deprecatedGuids` to match `deprecatedIds` property. https://github.com/oasis-tcs/sarif-spec/issues/346
* API NON-BREAKING: Add `referencedOnCommandLine` as a role. https://github.com/oasis-tcs/sarif-spec/issues/347
* API NON-BREAKING: Rename `reportingConfigurationOverride` to `configurationOverride`. https://github.com/oasis-tcs/sarif-spec/issues/350

## **v2.0.0-csd.2.beta.2019.04-03.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.04-03.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.04-03.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.04-03.1)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.04-03.1))
* API BREAKING: Rename `message.messageId` property to `message.id`. https://github.com/oasis-tcs/sarif-spec/issues/352

## **v2.0.0-csd.2.beta.2019.04-03.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.04-03.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.04-03.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.04-03.2)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.04-03.2))
* API NON-BREAKING: Add `module` to `address.kind`. https://github.com/oasis-tcs/sarif-spec/issues/353
* API BREAKING: `address.baseAddress` & `address.offset` to int. https://github.com/oasis-tcs/sarif-spec/issues/353
* API BREAKING: Update how reporting descriptors describe their taxonomic relationships. https://github.com/oasis-tcs/sarif-spec/issues/356
* API NON-BREAKING: Add `initialState` and `immutableState` properties to thread flow object. Add `immutableState` to `graphTraversal` object. https://github.com/oasis-tcs/sarif-spec/issues/168

## **v2.0.0-csd.2.beta.2019.04-03.3** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.04-03.3) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.04-03.3) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.04-03.3)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.04-03.3))
* API BREAKING: Rename `reportingDescriptor.descriptor` to `reportingDescriptor.target`. https://github.com/oasis-tcs/sarif-spec/issues/356
* API NON-BREAKING: Remove `canPrecedeOrFollow` from relationship kind list. https://github.com/oasis-tcs/sarif-spec/issues/356

## **v2.1.0--beta.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.0--beta.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.0--beta.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.0--beta.0)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.0--beta.0))
* API BREAKING: All SARIF state dictionaries now contains multiformat strings as values. https://github.com/oasis-tcs/sarif-spec/issues/361
