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
* Reclassify 'could not parse target' as a configuration notification
* Fix diffing visitor to diff using value type semantics rather than by reference equality

## **v1.5.22-beta** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.22-beta) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.22-beta)
* Add suppressionStates enum (with a single current value, indicating 'suppressedInSource')
* Add 'id' and 'correlationId' as arguments to ResultLogJsonWriter.Initialize. Log 'id' is populated with a generated guid by default.
* Add 'sarifLoggerVersion' that identifies the SDK logger version used to produce a log file.
* Provide serialization of arbitrary JSON content to 'properties' members.
* Move 'tags' into properties (but provide top-level Tags member for setting/retrieving this data)
* Add annotatedCodeLocation.kind enum (with values such as 'branch', 'declaration', et al.)
* Update all converters to Sarif beta.5
* Add optional 'id' to each result, to allow correlation with external data, annotations, work items, etc.
* Add flag to configure file hash computation to FileData.Create helper
* Add 'uriBaseId' conceptual base URI to all format URI properties (to allow all URIs to be relative)
* Add 'analysisTargetUri' to run object, for cases where a single target is associated with a run
* Add 'threadId' to notification, annotatedCodeLocation and stackFrame.
* Rework files and logicalLocations dictionary to store discrete items (with parent keys), not arrays
* Add logicalLocationKey and fullyQualifiedLogicalLocationName to annotatedCodeLocation
* Add 'id' and 'essential' properties to annotatedCodeLocation
* Rename 'toolFingerprint' to 'toolFingerprintContribution'
* Add baselineId. Rename 'correlationId' to 'automationId'
* Add 'physicalLocation' property to notification

## **v1.5.23-beta** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.23-beta) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.23-beta)
* Rename 'codeSnippet' to 'snippet'
* Remove requirement to specify 'description' on code fixes
* Add 'architecture' back to 'run' object

## **v1.5.24-beta** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.24-beta) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.24-beta)
* Permit annotatedCodeLocation.id to be a numeric value (in addition to a string)

## **v1.5.25** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.25) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.25)
* NOTE: NON-BETA RELEASE
* Add a converter for Static Driver Verifier trace files
* Add SuppressedExternally to SuppressionStates enum

## **v1.5.26** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.26) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.26)  
* Breaking change on SarifLogger to explicitly specify hash computation for all files  
* SarifLogger now automatically persists file data for all URIs through format  
* Add run.stableId, a consistent run-over-run log identifier  
* Add annotatedCodeLocation.callee and annotatedCodeLocation.calleeKey for annotation call sites  
* Add invocation.responseFiles to capture response file contents
* Drop .NET framework dependency to 4.5 (from 4.5.1)

## **v1.5.27** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.27) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.27)
* Ship checked in CommandLine.dll in order to allow this 'beta' NuGet component to ship in Driver non-beta release

## **v1.5.28** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.28) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.28)
* Breaking change: rename PropertyBagDictionary to PropertiesDictionary
* Add 'functionReturn' to annotatedCodeLocation.kind
* Remove 'source', 'sink' and 'sanitizer' from annotatedCodeLocation.kind
* Add 'taint' enum to annotatedCodeLocation with values 'source', 'sink' and 'sanitizer'
* Add 'parameters' and 'variables' members to annotatedCodeLocation
* Rename annotatedCodeLocation.callee member to 'target'
* Rename annotatedCodeLocation.calleeKey member to 'targetKey'

## **v1.5.29** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.29) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.29)

* Add `--quiet` option to suppress console output.

## **v1.5.30** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.30) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.30)

* Add static helper method `AnalyzeCommandBase.LogToolNotification`.

## **v1.5.31** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.31) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.31)

* BREAKING CHANGE: RuleUtilities.BuildResult no longer automatically prepends the target file path to the list of FormattedRuleMessage.Arguments array in the Result object being built.

## **v1.5.32** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.32) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.32)

* Add 'annotations' member to annotatedCodeLocation object.
* Rename annotatedCodeLocation 'variables' member to 'state'
* Rename annotatedCodeLocation 'parameters' member to 'values'

## **v1.5.33** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.33) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.33)

* Resolve crash generating 'not applicable' messages

## **v1.5.34** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.34) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.34)

* Update schema for 'annotations' object required properties

## **v1.5.35** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.35) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.35)

* Add 'configuration' member to rule objects

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
 * Adjust RuntimeConditions enum so that 'command line parse' error is 0x1. 

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

## **v2.0.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0))
* Convert object model to conform to SARIF v2 CSD.1 draft specification
* Distinguish textual vs. binary file persistence in rewrite option (and allow for both in multitool rewrite verb)
*   NOTE: the change above introduces a command-line breaking change. --persist-file-contents is now renamed to --insert
* Add Regions as possible qualifier to --insert option
