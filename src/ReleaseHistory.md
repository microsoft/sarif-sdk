# Sarif Driver and SDK Release History

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

