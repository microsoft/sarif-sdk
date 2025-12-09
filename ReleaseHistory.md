# SARIF Package Release History (SDK, Driver, Converters, and Multitool)

## **v4.6.1 UNRELEASED**
* NEW: Add health check query parameter support for `--post-uri` validation. The driver now appends `?healthcheck=true` to POST URIs during validation and accepts HTTP 202 (Accepted), or 422 (Unprocessable Entity) as valid responses. This provides better support for endpoints that implement health check functionality while maintaining backwards compatibility with servers that return 422 for empty payloads.

## **v4.6.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/v4.6.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/v4.6.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/v4.6.0)  | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/v4.6.0) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/v4.6.0)
* BRK: Remove defunct and unsupported `kusto` command in `Sarif.Multitool`.
* BRK: Remove support for .NET Core 3.1 and .NET 6.0 in preference of a [supported version of .NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core), `net8.0`.
* BRK: Remove `HashData.MD5`, `HashUtilities.ComputeMD5Hash` due to the inherent insecurity of this algorithm.
* BRK: 'HashUtilities.ComputeHash' no longer generates MD5 hashes (only SHA1 and SHA256).
* DEP: Remove dependency on `Microsoft.Azure.Kusto.Data`.
* DEP: Update `Azure.Identity` reference from 1.10.2 to 1.13.1 in `WorkItems` and `Sarif.Multitool.Library` to resolve [CVE-2024-29992](https://github.com/advisories/GHSA-wvxc-855f-jvrv) and other CVEs.
* DEP: Update `Azure.Core` from 1.35.0 to 1.41.1 to satisfy minimum requirement of `Azure.Identity` 1.12.1 (that has no known vulnerabilities).
* DEP: Update `System.Text.Encodings.Web` from 5.0.1 to 6.0.0 (required by transitive closure of dependency requirements from other updates).
* DEP: Update all `Newtonsoft.Json` references to 13.0.3 to resolve [CVE-2024-21907](https://nvd.nist.gov/vuln/detail/CVE-2024-21907).
* DEP: Update `Microsoft.Data.SqlClient` from 2.1.7 to 5.2.2 so its dependencies `Microsoft.IdentityModel.JsonWebTokens` and `System.IdentityModel.Tokens.Jwt` upgrade to non-vulnerable version 6.35.0 (https://github.com/dotnet/aspnetcore/security/advisories/GHSA-59j7-ghrg-fj52).
* BUG: Resolve process hangs when a file path is provided with a wildcard, but without a `-r` (recurse) flag during the multi-threaded analysis file enumeration phase.
* BUG: Fix error `ERR997.NoValidAnalysisTargets` when scanning symbolic link files.
* BUG: Fix error `ERR997.NoValidAnalysisTargets` when passing wildcard patterns (e.g., *.txt) to `OrderedFileSpecifier`. A recent change limited our wildcard support strictly to use of * only.
* BUG: Fix `ERR999.UnhandledEngineException: System.IO.FileNotFoundException: Could not find file` when a file name or directory path contains URL-encoded characters.
* BUG: Fix error `ERR997.NoValidAnalysisTargets` when ambiguous file/directory references are provided to `OrderedFileSpecifier`. Previously, the code required an explicit directory separator to be added to the end of a directory path. Now, the code inspects the file system and assumes that a reference to an existing directory was intended by the user (even without a trailing separator).
* BUG: Fixed error `ERR997.NoValidAnalysisTargets | TargetParseError` when processing OPC files by correctly handling programmatic usage and skipping redundant file access when a stream is provided via `EnumeratedArtifact`.
* BUG: Eliminate unhandled `UriFormatException: Invalid URI: The format of the URI could not be determined.` when creating a `ZipArchiveArtifact` with a relative URI.
* BUG: Refactored `MultithreadedCommandBase` to check for empty or oversized artifacts before attempting to load OPC artifacts. This avoids unnecessary processing and improves performance by skipping invalid inputs early.
* NEW: Allow null archive uri in `MultithreadedZipArchiveArtifactProvider` (which indicates that enumerated artifact paths should not include the base archive).
* NEW: Update `LogTargetParseError(IAnalysisContext, Region, string, Exception)` to include optional exception argument to denote code location where parse error occurred.
* NEW: `MultithreadedAnalyzeCommandBase.EnumerateArtifact` now supports scanning into compressed (OPC) files. Initial support file extensions are: `.apk`, `.appx`, `.appxbundle`, `.docx`, `.epub`, `.jar`, `.msix`, `.msixbundle`, `.odp`, `.ods`, `.odt`, `.onepkg`, `.oxps`, `.pkg`, `.pptx`, `.unitypackage`, `.vsix`, `.vsdx`, `.xps`, `.xlsx`, `.zip`.

## **v4.5.4** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/v4.5.4) | [Driver](https://www.nuget.org/packages/Sarif.Driver/v4.5.4) | [Converters](https://www.nuget.org/packages/Sarif.Converters/v4.5.4)  | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/v4.5.4) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/v4.5.4)
* BUG: Fix incorrect base class in rule ADO2012.

## **v4.5.3** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/v4.5.3) | [Driver](https://www.nuget.org/packages/Sarif.Driver/v4.5.3) | [Converters](https://www.nuget.org/packages/Sarif.Converters/v4.5.3)  | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/v4.5.3) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/v4.5.3)
* BUG: Restructure shared `MessageResourceNames` collections to ensure return of correct error messages.

## **v4.5.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/v4.5.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/v4.5.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/v4.5.2)  | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/v4.5.2) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/v4.5.2)
* BUG: Update `Skimmer` stack in `Multitool.Library` to support shared `MessageResourceNames` collections between base rules and their derivatives.
* BUG: Fix message strings to always assume {1} is reserved for the rule's service name.
* BUG: Clean up unused resource strings in Multitool.Library.Rules.RuleResources.resx.

## **v4.5.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/v4.5.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/v4.5.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/v4.5.1)  | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/v4.5.1) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/v4.5.1)
* DEP: Add explicit package references to `Sarif` and `Sarif.Driver` to resolve version conflict build error.
  `System.Diagnostics.Debug` 4.3.0,
  `System.IO.FileSystem.Primitives` 4.3.0,
  `System.Text.Encoding.Extensions` 4.3.0.
* NEW: Expose `MultithreadedAnalyzeCommandBase.BuildDisabledSkimmersSet`, a utility function which extracts a disabled skimmer set from a `TContext`.

## **v4.5.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/v4.5.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/v4.5.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/v4.5.0)  | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/v4.5.0) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/v4.5.0)
* DEP: Downgrade `System.Text.Encoding.CodePages` from 8.0.0 to 4.3.0 in `Sarif`.
* DEP: Remove explicit versioning for `System.Memory` and `System.Runtime.CompilerServices.Unsafe`.
* DEP: Remove spurious references to `System.Collections.Immutable`.
* DEP: Update `Microsoft.Data.SqlClient` reference from 2.1.2 to 2.1.7 in `WorkItems` and `Sarif.Multitool.Library` to resolve [CVE-2024-0056](https://github.com/advisories/GHSA-98g6-xh36-x2p7).
* DEP: Update `System.Data.SqlClient` reference from 4.8.5 to 4.8.6 in `WorkItems` to resolve [CVE-2024-0056](https://github.com/advisories/GHSA-98g6-xh36-x2p7).
* BUG: Improve `FileEncoding.IsTextualData` method for detecting binary files.
* BUG: Update `Stack.Create` method to populate missing `PhysicalLocation` instances when stack frames reference relative file paths.
* BUG: Fix `UnsupportedOperationException` in `ZipArchiveArtifact`.
* BUG: Fix `MultithreadedAnalyzeCommandBase` to return rich return code with the `--rich-return-code` option.
* NEW: Add `IsBinary` property to `IEnumeratedArtifact` and implement the property in `ZipArchiveArtifact`.
* NEW: Switch to content-based `IsBinary` categorization for `ZipArchiveArtifact`s.
* PRF: Change default `max-file-size-in-kb` parameter to 10 megabytes.
* PRF: Add support for efficiently peeking into non-seekable streams for binary/text categorization.
* NEW: Add a new `--timeout-in-seconds` parameter to `AnalyzeOptionsBase`, which will override the `TimeoutInMilliseconds` property in `AnalyzeContextBase`.
* NEW: `--post-uri` will skip sending the SARIF log to the configured endpoint if the file contains no results or fatal execution errors.
* NEW: Add the following rules:  
  `ADO1011.ReferenceFinalSchema`,  
  `ADO1013.ProvideRequiredSarifLogProperties`,  
  `ADO1014.ProvideRequiredRunProperties`,  
  `ADO1015.ProvideRequiredResultProperties`,  
  `ADO1016.ProvideRequiredLocationProperties`,  
  `ADO1017.ProvideRequiredPhysicalLocationProperties`,  
  `ADO1018.ProvideRequiredToolProperties`,  
  `ADO2012.ProvideRequiredReportingDescriptorProperties`,  
  `GH1011.ReferenceFinalSchema`,  
  `GH1013.ProvideRequiredSarifLogProperties`,  
  `GH1014.ProvideRequiredRunProperties`,  
  `GH1015.ProvideRequiredResultProperties`,  
  `GH1016.ProvideRequiredLocationProperties`,  
  `GH1017.ProvideRequiredPhysicalLocationProperties`,  
  `GH1018.ProvideRequiredToolProperties`,  
  `GH2012.ProvideRequiredReportingDescriptorProperties`.
* NEW: Add a new `--rule-kind` parameter to `AnalyzeOptionsBase`, which specifies rule kinds to run (`Sarif`, `Ghas`, `Ado`). Example: `--rule-kind Ado;Sarif`.

## **v4.4.1** UNRELEASED
* DEP: Update reference to `System.Collections.Immutable` 5.0.0 for `Sarif` and `Sarif.Converters`.
* BUG: Emit `WRN997.OneOrMoreFilesSkippedDueToExceedingSizeLimit` when no valid analysis targets are detected (due to exceeding size limits).
* BUG: Emit `FailureLevel.Note` messages with label `info` (rather than `fail`) in `ConsoleLogger`.

## **v4.4.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/v4.4.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/v4.4.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/v4.4.0)  | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/v4.4.0) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/v4.4.0)
* DEP: Add reference to `System.Text.Encoding.CodePages` 8.0.0 (to support Windows 1252 code pages in binary vs. text classification).
* DEP: Update `Newtonsoft.Json` reference from 8.0.3 to 9.0.1 to provide `net462` compatibility.
* DEP: Update target framework from `net461` to `net462` in `Sarif` `Sarif.Converters` projects (to allow for use of `System.Text.Encoding.CodePages`).
* DEP: Explicitly add `Azure.Identity` 1.10.2 in `Sarif.Multitool.Library` and `WorkItems` to avoid the vulnerable 1.3.0 package via `Microsoft.Azure.Kusto.Data` 10.0.3 per compliance requirements.
* DEP: Explicitly add `Microsoft.Data.SqlClient` 2.1.2 in `Sarif.Multitool.Library` and `WorkItems` to avoid the vulnerable 2.1.1 package via `Microsoft.Azure.Kusto.Data` 10.0.3 per compliance requirements.
* DEP: Explicitly add `System.Data.SqlClient` 4.8.5 in `WorkItems` to avoid the vulnerable 4.2.2 package via `Microsoft.TeamFoundationServer.Client` 16.170.0 per compliance requirements.
* BRK: `EnumeratedArtifact` now sniffs artifacts to distinguish between textual and binary data. The `Contents` property will be null for binary files (use `Bytes` instead).
* BRK: `MultithreadedZipArchiveArtifactProvider` now distinguishes binary vs. textual data using a hard-coded binary files extensions list. This data will be made configurable in a future change. Current extensions include `.bmp`, `.cer`, `.der`, `.dll`, `.exe`, `.gif`, `.gz`, `.iso`, `.jpe`, `.jpeg`, `.lock`, `.p12`, `.pack`, `.pfx`, `.pkcs12`, `.png`, `.psd`, `.rar`, `.tar`, `.tif`, `.tiff`, `.xcf`, `.zip`.
* NEW: `EnumeratedArtifact` now automatically detects and populates a `Bytes` property for binary files such as executables and certificates.
* NEW: `FileEncoding.IsTextualData` utility can effectively distinguish between binary and textual data.

## **v4.3.7** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/v4.3.7) | [Driver](https://www.nuget.org/packages/Sarif.Driver/v4.3.7) | [Converters](https://www.nuget.org/packages/Sarif.Converters/v4.3.7)  | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/v4.3.7) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/v4.3.7)
* DEP: Updated NewtonSoft.JSON to 8.0.3 in Sarif.Converters for .NET targets later than `netstandard2.0`.
* BUG: Logging improved when work item client is called with invalid work item values.
* NEW: Add `Path.Combine`, `Path.GetDirectoryName` and `Path.GetFileNameWithoutExtension` to `IFileSystem`.

## **v4.3.6** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/v4.3.6) | [Driver](https://www.nuget.org/packages/Sarif.Driver/v4.3.6) | [Converters](https://www.nuget.org/packages/Sarif.Converters/v4.3.6)  | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/v4.3.6) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/v4.3.)
* BUG: Resolve `InvalidOperationException` processing `RuleNotCalled` events.
* BUG: Emit optional data arguments for `RuleNotCalled` events in auto-formatted messages. 
* PRF: Switch file system traversal to pre-order with producer-consumer to accelerate time to scan first artifact.

## **v4.3.5** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/v4.3.5) | [Driver](https://www.nuget.org/packages/Sarif.Driver/v4.3.5) | [Converters](https://www.nuget.org/packages/Sarif.Converters/v4.3.5)  | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/v4.3.5) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/v4.3.)
* BRK: Remove processing of `/u2028` (Unicode line separator) and `/u2029` (Unicode paragraph separator) from `NewLineIndex`.
* BUG: Resolve `KeyNotFoundException: The given key was not present` exception when scanning content that contains Unicode line and paragraph separators (`/u2028` and `/u2029`) when enabling `OptionallyEmittedData.RollingHashPartialFingerprints`. 
* BUG: Fix `Unhandled Exception: System.IO.FileNotFoundException: Could not load file or assembly 'Sarif.Multitool.Library, Version=...` when using net462 version of the Multitool. [#2722](https://github.com/microsoft/sarif-sdk/issues/2722)

## **v4.3.4** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/v4.3.4) | [Driver](https://www.nuget.org/packages/Sarif.Driver/v4.3.4) | [Converters](https://www.nuget.org/packages/Sarif.Converters/v4.3.4)  | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/v4.3.4) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/v4.3.4)
* BUG: Disable certain console outputs (such as reporting of threads count) when `AnalyzeContextBase.Quiet` is set.

## **v4.3.3** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/v4.3.3) | [Driver](https://www.nuget.org/packages/Sarif.Driver/v4.3.3) | [Converters](https://www.nuget.org/packages/Sarif.Converters/v4.3.3)  | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/v4.3.3) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/v4.3.3)
* BUG: Update `dump-events` command to be resilient in cases where the thread id changes between artifact enumeration start/stop event pairs.
* BUG: Resolve trace parsing `InvalidOperationException` by updating `dump-events` command to process `PartitionInfoExtension` session event as we do `PartitionInfoExtensionV2`.

## **v4.3.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/v4.3.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/v4.3.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/v4.3.2)  | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/v4.3.2) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/v4.3.2)
* BUG: Correct multitool query OR logic [#2709](https://github.com/microsoft/sarif-sdk/issues/2709)

## **v4.3.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/v4.3.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/v4.3.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/v4.3.1)  | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/v4.3.1) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/v4.3.2)
* BUG: Improve `HdfConverter` to ensure uri data is populated and to provide location and region data property from `SourceLocation`. [#2704](https://github.com/microsoft/sarif-sdk/pull/2704)
* BUG: Correct `run.language` regex in JSON schema. [#2708]https://github.com/microsoft/sarif-sdk/pull/2708
* BUG: Improve `HdfConverter` to set `precision` and `tags` as recommended by GitHub. [#2712](https://github.com/microsoft/sarif-sdk/pull/2712)

## **v4.3.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/v4.3.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/v4.3.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/v4.3.0) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/v4.3.0) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/v4.3.0)
* BUG: Resolve `NullReferenceException` retrieving `MultithreadedZipArchiveArtifactProvider.SizeInBytes` after content have been faulted in.
* BUG: Improve HDF->SARIF conversion to properly map various properties (e.g., `kind`, `level`, `rank`) and generally prepare the converted SARIF for ingestion to [GitHub Advanced Security](https://docs.github.com/en/code-security/code-scanning/integrating-with-code-scanning/sarif-support-for-code-scanning).

## **v4.2.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/4.2.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/4.2.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/4.2.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/4.2.1) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/4.2.1)
* BUG: Resolve `NotSupportedException` thrown (on .NET 4.8 and earlier) on accessing `DeflateStream.Length` from `MultithreadedZipArchiveArtifactProvider.SizeInBytes` property. 

## **v4.2.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/4.2.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/4.2.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/4.2.0) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/4.2.0) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/4.2.0)
* BRK: Change `ArtifactProvicer.SizeInBytes` property type from `ulong` to `long`. [#2675](https://github.com/microsoft/sarif-sdk/pull/2675)
* BRK: Update `SarifLog.Post(Uri, StreamWriter, HttpClient)` return value to `HttpResponseMessage` (to make returned correlation id and error messages available). [#2672](https://github.com/microsoft/sarif-sdk/pull/2672)
* BRK: `RuntimeConditions` now of type `long` to permit more flag values. Many literal values have changed for individual members. [#2660](https://github.com/microsoft/sarif-sdk/pull/2660)
* BRK: `RuntimeConditions.OneOrMoreFilesSkippedDueToSize` renamed to `OneOrMoreFilesSkippedDueToExceedingSizeLimits`. [#2660](https://github.com/microsoft/sarif-sdk/pull/2660)
* BRK: `Notes.LogFileSkippedDueToSize` renamed to `LogFileExceedingSizeLimitSkipped`. [#2660](https://github.com/microsoft/sarif-sdk/pull/2660)
* BRK: Command-line argument `automationGuid` renamed to `automation-guid`. [#2647](https://github.com/microsoft/sarif-sdk/pull/2647)
* BRK: Command-line argument `automationId` renamed to `automation-id`. [#2647](https://github.com/microsoft/sarif-sdk/pull/2647)
* BRK: Update `AnalyzeOptionsBase` `Quiet`, `Recurse`, `LogEnvironment`, and `RichReturnCode` properties to bool? type. [#2644](https://github.com/microsoft/sarif-sdk/pull/2644)
* BRK: Rename `Errors.LogExceptionCreatingLogFile` to `Errors.LogExceptionCreatingOutputFile` to reflect its general purpose. [#2643](https://github.com/microsoft/sarif-sdk/pull/2643)
* BRK: Add `IAnalysisContext.FileRegionsCache` property. Used for data sharing across analysis phases. [#2642](https://github.com/microsoft/sarif-sdk/pull/2642)
* BRK: Remove `FileRegionsCache.Instance` singleton object. Analysis should always prefer context file region context instead. [#2642](https://github.com/microsoft/sarif-sdk/pull/2642)
* BRK: `fileRegionsCache` parameter is now required for the `InsertOptionalDataVisitor`. [#2642](https://github.com/microsoft/sarif-sdk/pull/2642)
* BRK: Add `IAnalysisLogger.TargetAnalysisComplete` method. [#2637](https://github.com/microsoft/sarif-sdk/pull/2637)
* BRK: Remove unused `quiet` parameter from `SarifLogger`. [#2639]https://github.com/microsoft/sarif-sdk/pull/2639
* BRK: Remove `ComputeHashData` and `AnalysisTargetToHashDataMap` properties from `SarifLogger` (in preference of new `fileRegionsCache` parameter. [#2639](https://github.com/microsoft/sarif-sdk/pull/2639)
* BRK: Eliminate proactive hashing of artifacts in `SarifLogger` constructor when `OptionallyEmittedData.Hashes` is specified. [#2639](https://github.com/microsoft/sarif-sdk/pull/2639)
* BUG: Provider better size return values for in-memory `EnumeratedArtifact` instances. [#2674](https://github.com/microsoft/sarif-sdk/pull/2674)
* BUG: Fixed `ERR999.UnhandledEngineException: System.InvalidOperationException: This operation is not supported for a relative URI` when running in Linux with files skipped due to zero byte size. [#2664](https://github.com/microsoft/sarif-sdk/pull/2664)
* BUG: Properly report skipping empty files (rather than reporting file was skipped due to exceeding size limits). [#2660](https://github.com/microsoft/sarif-sdk/pull/2660)
* BUG: Update user messages and code comments that refer to `--force` (replaced by `--log ForceOverwrite`). [#2656](https://github.com/microsoft/sarif-sdk/pull/2656)
* BUG: Handle return code 422 `UnprocessableEntity` when validating that log file POST endpoint is available. [#2656](https://github.com/microsoft/sarif-sdk/pull/2656)
* BUG: Eliminate erroneous `Posted log file successfully` message when context `PostUri` is non-null but empty. [#2655](https://github.com/microsoft/sarif-sdk/pull/2655)
* BUG: Resolves `IOException` raised by calling `FileSystem.ReadAllText` on file locked for write (but not read). [#2655](https://github.com/microsoft/sarif-sdk/pull/2655)
* BUG: Correct `toolComponent.language` regex in JSON schema. [#2653]https://github.com/microsoft/sarif-sdk/pull/2653
* BUG: Generate `IAnalysisLogger.AnalyzingTarget` callbacks from `MulthreadedAnalyzeCommandBase`. [#2637](https://github.com/microsoft/sarif-sdk/pull/2637)
* BUG: Persist `fileRegionsCache` parameter in `SarifLogger` to support retrieving hash data. [#2639](https://github.com/microsoft/sarif-sdk/pull/2639)
* BUG: Allow override of `FailureLevels` and `ResultKinds` in context objects. [#2639](https://github.com/microsoft/sarif-sdk/pull/2639)
* NEW: Add general `Notes.LogFileSkipped` notification mechanism for any skipped files. [#2675](https://github.com/microsoft/sarif-sdk/pull/2675)
* NEW: Add 50K files to analysis channel (rather than previous value of 25k). Smooths performance analyzing many small artifacts. [#2674](https://github.com/microsoft/sarif-sdk/pull/2674)
* NEW: Provide new ETW telemetry for runtime behavior, provider `SarifDriver`, guid `c84480b4-a77f-421f-8a11-48210c1724d4`. https://github.com/microsoft/sarif-sdk/pull/2668
* NEW: Provide convenience enumerator at the `SarifLog` level that iterates over all results in all runs in the log. [#2660](https://github.com/microsoft/sarif-sdk/pull/2660)
* NEW: Provide `Notes.LogEmptyFileSkipped` helper for reporting zero-byte files skipped at scan time. [#2660](https://github.com/microsoft/sarif-sdk/pull/2660)
* NEW: Add `MemoryStreamSarifLogger` (for in-memory SARIF generation). [#2655](https://github.com/microsoft/sarif-sdk/pull/2655)
* NEW: Add `AnalyzeContext.VersionControlProvenance` property. [#2646](https://github.com/microsoft/sarif-sdk/pull/2646)
* NEW: Add `DefaultTraces.ResultsSummary` property that drives naive results summary in console logger. [#2643](https://github.com/microsoft/sarif-sdk/pull/2643)
* NEW: Prove `AnalyzeContextBase.Inline` helper. [#2643](https://github.com/microsoft/sarif-sdk/pull/2643)
* NEW: `SarifLogger.FileRegionsCache` property added (to support sharing this instance with context and other classes). [#2642](https://github.com/microsoft/sarif-sdk/pull/2642)
* NEW: `MultithreadedAnalyzeCommandBase.Tool` is now public to support in-memory analysis (and logging) of targets. [#2639](https://github.com/microsoft/sarif-sdk/pull/2639)
* NEW: Add `DefaultTraces.TargetsScanned` which is used by `ConsoleLogger` to emit target start and stop analysis messages. [#2637](https://github.com/microsoft/sarif-sdk/pull/2637)
* NEW: Update `FileRegionsCache` to retrieve cached newline indices and hash data via `GetNewLineIndex` and `GetHashData` methods. [#2639](https://github.com/microsoft/sarif-sdk/pull/2639)

## **v4.1.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/4.1.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/4.1.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/4.1.0) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/4.1.0) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/4.1.0)
* BRK: `MultithreadedAnalyzeCommandBase` IDispose implementation now manages logging dispose. Be sure to call `base.Dispose()` in any derived type implementations. [#2614](https://github.com/microsoft/sarif-sdk/pull/2614)
* BRK: Eliminate `MulthreadedAnalyzeCommandBase.EngineException` and `IAnalysisContext.RuntimeException` properties in favor of `IAnalysisContext.RuntimeExceptions`. [#2627](https://github.com/microsoft/sarif-sdk/pull/2627)
* BRK: Rename `LogFilePersistenceOptions` to `FilePersistenceOptions` (due to its general applicability in other file persistence contexts other than output logs).[#2625](https://github.com/microsoft/sarif-sdk/pull/2625)
* BRK: Many breaking changes in `IAnalysisContext` and `AnalyzeContextBase`. [#2625](https://github.com/microsoft/sarif-sdk/pull/2625)
* BUG: In `HDFConverter` if `code_desc` is empty, use `desc` as the SARIF `message`. [#2632](https://github.com/microsoft/sarif-sdk/pull/2632)
* BUG: Store `HDFConverter` `desc` in SARIF's `FullDescription`, not `ShortDescription`. [#2634](https://github.com/microsoft/sarif-sdk/pull/2634)
* BUG: Eliminate creation of extremely large context region snippets (now always restricted to 512 chars). https://github.com/microsoft/sarif-sdk/pull/2629
* BUG: Eliminate per-context allocations contributing to unnecessary memory use. [#2625](https://github.com/microsoft/sarif-sdk/pull/2625)
* NEW: Rewrite  `MultithreadedAnalyzeCommandBase` pipeline to allow for timeout, cancellation, and better API-driven use. [#2625](https://github.com/microsoft/sarif-sdk/pull/2625)
* NEW: Move large amounts of scan data to the context object, to streamline pipeline and allow for XML-driven configuration. [#2625](https://github.com/microsoft/sarif-sdk/pull/2625)
* NEW: Switch file processing to an `ArtifactProvider` model where enumerated artifacts consist of URI and optional content. [#2625](https://github.com/microsoft/sarif-sdk/pull/2625)
* NEW: Add new `FailureLevelSet` and `ResultKindSet` types that are compatible with XML-based configuration. [#2625](https://github.com/microsoft/sarif-sdk/pull/2625)
* NEW: Add `PeakWorkingSet` to `--trace` command to report maximum working set value during analysis. [#2619](https://github.com/microsoft/sarif-sdk/pull/2619)
* NEW: Add `ArtifactProvider` for simple artifact enumeration. Add single-threaded and thread-safe classes for enumerating zip archives. [#2630](hhttps://github.com/microsoft/sarif-sdk/pull/2630)

## **v4.0.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/4.0.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/4.0.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/4.0.0) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/4.0.0) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/4.0.0)
* BRK: `SarifLogger` no longer allows providing a `Tool` instance. Use the `run` parameter instead (and populate it with any custom `Tool` object). [#2614](https://github.com/microsoft/sarif-sdk/pull/2614)
* BRK: `SarifLogger` updates version details differently. [#2611](https://github.com/microsoft/sarif-sdk/pull/2611)
* BRK: Add `ToolComponent` argument to `IAnalysisLogger.Log(ReportingDescriptor, Result)` method. [#2611](https://github.com/microsoft/sarif-sdk/pull/2611)
* BRK: Rename `--normalize-for-github` argument to `--normalize-for-ghas` for `convert` command and mark `--normalize-for-github` as obsolete. [#2581](https://github.com/microsoft/sarif-sdk/pull/2581)
* BRK: Update `IAnalysisContext.LogToolNotification` method to add `ReportingDescriptor` parameter. This is required in order to populated `AssociatedRule` data in `Notification` instances. The new method has an option value of null for the `associatedRule` parameter to maximize build compatibility. [#2604](https://github.com/microsoft/sarif-sdk/pull/2604)
* BRK: Correct casing of `LogMissingreportingConfiguration` helper to `LogMissingReportingConfiguration`. [#2599](https://github.com/microsoft/sarif-sdk/pull/2599)
* BRK: Change type of `MaxFileSizeInKilobytes` from int to long in `IAnalysisContext` and other classes.  [#2599](https://github.com/microsoft/sarif-sdk/pull/2599)
* BRK: For `Guid` properties defined in SARIF spec, updated Json schema to use `uuid`, and updated C# object model to use `Guid?` instead of `string`. [#2555](https://github.com/microsoft/sarif-sdk/pull/2555)
* BRK: Mark `AnalyzeCommandBase` as obsolete. This type will be removed in the next significant update.  [#2599](https://github.com/microsoft/sarif-sdk/pull/2599)
* BRK: `LogUnhandledEngineException` no longer has a return value (and updates the `RuntimeErrors` context property directly as other helpers do).  [#2599](https://github.com/microsoft/sarif-sdk/pull/2599)
* BUG: Populate missing context region data for small, single-line scan targets. [#2616](https://github.com/microsoft/sarif-sdk/pull/2616)
* BUG: Increase parallelism in `MultithreadedAnalyzeCommandBase` by correcting task creation. []#2618](https://github.com/microsoft/sarif-sdk/pull/2618)
* BUG: Resolve hangs due to unhandled exceptions during multithreaded analysis file enumeration phase.  [#2599](https://github.com/microsoft/sarif-sdk/pull/2599)
* BUG: Resolve hangs due to unhandled exceptions during multithreaded analysis file hashing phase.  [#2600](https://github.com/microsoft/sarif-sdk/pull/2600)
* BUG: Another attempt to resolve 'InvalidOperationException' with message `Collection was modified; enumeration operation may not execute` in `MultithreadedAnalyzeCommandBase`, raised when analyzing with the `--hashes` switch. [#2459](https://github.com/microsoft/sarif-sdk/pull/2549). There was a previous attempt to fix this in [#2447](https://github.com/microsoft/sarif-sdk/pull/2447).
* BUG: Resolve issue where `match-results-forward` command fails to generate VersionControlDetails data. [#2487](https://github.com/microsoft/sarif-sdk/pull/2487)
* BUG: Remove duplicated rule definitions when executing `match-results-forward` commands for results with sub-rule ids. [#2486](https://github.com/microsoft/sarif-sdk/pull/2486)
* BUG: Update `merge` command to properly produce runs by tool and version when passed the `--merge-runs` argument. [#2488](https://github.com/microsoft/sarif-sdk/pull/2488)
* BUG: Eliminate `IOException` and `DirectoryNotFoundException` exceptions thrown by `merge` command when splitting by rule (due to invalid file characters in rule ids). [#2513](https://github.com/microsoft/sarif-sdk/pull/2513)
* BUG: Fix classes inside NotYetAutoGenerated folder missing `virtual` keyword for public methods and properties, by regenerate and manually sync the changes. [#2537](https://github.com/microsoft/sarif-sdk/pull/2537)
* BUG: MSBuild Converter now accepts case insensitive keywords and supports PackageValidator msbuild log output. [#2579](https://github.com/microsoft/sarif-sdk/pull/2579)
* BUG: Eliminate `NullReferenceException` when file hashing fails (due to file locked or other errors reading the file). [#2596](https://github.com/microsoft/sarif-sdk/pull/2596)
* NEW: Provide `PluginDriver` property (`AdditionalOptionsProvider`) that allows additional options to be exported (typically for command-line arguments).  [#2599](https://github.com/microsoft/sarif-sdk/pull/2599)
* NEW: Provide `LogFileSkippedDueToSize` that fires a warning notification if any file is skipped due to exceeding size threshold. [#2599](https://github.com/microsoft/sarif-sdk/pull/2599)
* NEW: Provide overridable `ShouldEnqueue` predicate method to filter files from driver processing.  [#2599](https://github.com/microsoft/sarif-sdk/pull/2599)
* NEW: Provide overridable `ShouldComputeHashes` predicate method to prevent files from hashing.  [#2601](https://github.com/microsoft/sarif-sdk/pull/2601)
* NEW: Allow external set of `MaxFileSizeInKilobytes`, which will allow SDK users to change the value. (Default value is 1024) [#2578](https://github.com/microsoft/sarif-sdk/pull/2578)
* NEW: Add a Github validation rule `GH1007`, which requires flattened result message so GHAS code scanning can ingest the log. [#2580](https://github.com/microsoft/sarif-sdk/issues/2580)
* NEW: Provide mechanism to populate `SarifLogger` with a `FileRegionsCache` instance.
* NEW: Allow initialization of file regions cache in `InsertOptionalDataVisitor` (previously initialized exclusively from `FileRegionsCache.Instance`).
* NEW: Provide 'RuleScanTime` trace and emitted timing data. Provide `ScanExecution` trace with no utilization.
* NEW: Populate associated rule data in `LogToolNotification` as called from `SarifLogger`. [#2604](https://github.com/microsoft/sarif-sdk/pull/2604)
* NEW: Add `--normalize-for-ghas` argument to the `rewrite` command to ensure rewritten SARIF is compatible with GitHub Advanced Security (GHAS) ingestion requirements. [#2581](https://github.com/microsoft/sarif-sdk/pull/2581)
* NEW: Allow per-line rolling (partial) hash computation for a file. [#2605](https://github.com/microsoft/sarif-sdk/pull/2605)
* NEW: `SarifLogger` now supports extensions rules data when logging (by providing a `ToolComponent` instance to the result logging method). [#2661](https://github.com/microsoft/sarif-sdk/pull/2611)
* NEW: `SarifLogger` provides a `ComputeHashData` callback to provide hash data for in-memory scan targets. [#2614](https://github.com/microsoft/sarif-sdk/pull/2614)
* NEW: Provide	`HashUtilities.ComputeHashes(Stream)` and `ComputeHashesForText(string) helpers. [#2614](https://github.com/microsoft/sarif-sdk/pull/2614)

## **v3.1.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/3.1.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/3.1.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/3.1.0) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/3.1.0) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/3.1.0)
* BUG: Loosen `System.Collections.Immutable` minimum version requirement to 1.5.0. [#2504](https://github.com/microsoft/sarif-sdk/pull/2533)

## **v3.0.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/3.0.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/3.0.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/3.0.0) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/3.0.0) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/3.0.0)
* BUG: Loosen Newtonsoft.JSON minimum version requirement to 6.0.8 (for .NET framework) or 9.0.1 (for all other compilations) for Sarif.Sdk. Sarif.Converts requires 8.0.1, minimally, for .NET framework compilations.
* BUG: Broaden set of supported .NET frameworks for compatibility reasons. Sarif.Sdk, Sarif.Driver and Sarif.WorkItems requires net461.

## **v2.4.16** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.16) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.16) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.16) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.16) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.16)
* BRK: SARIF now requires Newtonsoft.JSON 13.0.1. Updating [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/13.0.1) to v13.0.1, [Microsoft.Json.Schema](https://www.nuget.org/packages/Microsoft.Json.Schema) to v1.1.5, [Microsoft.Json.Pointer](https://www.nuget.org/packages/Microsoft.Json.Pointer) to v1.1.5, [Microsoft.Azure.Kusto.Data](https://www.nuget.org/packages/Microsoft.Azure.Kusto.Data) to v10.0.3, [Microsoft.NET.Test.Sdk](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk/17.4.0-preview-20220707-01) to v17.4.0-preview-20220707-01, [Microsoft.Extensions.Logging.ApplicationInsights](https://www.nuget.org/packages/Microsoft.Extensions.Logging.ApplicationInsights/2.20.0) to v.2.20.0, [Microsoft.TeamFoundationServer.Client](https://www.nuget.org/packages/Microsoft.TeamFoundationServer.Client/16.170.0) to v.16.170.0, [Microsoft.Coyote](https://www.nuget.org/packages/Microsoft.Coyote) to v.1.5.8 and [Microsoft.Coyote.Test](https://www.nuget.org/packages/Microsoft.Coyote.Test) to v.1.5.8 in response to [Advisory: Improper Handling of Exceptional Conditions in Newtonsoft.Json](https://github.com/advisories/GHSA-5crp-9r3c-p9vr). [#2504](https://github.com/microsoft/sarif-sdk/pull/2504)
* BUG: Fix false positive for `SARIF1002.UrisMustBeValid` for file URIs that omit the `authority`. [#2501](https://github.com/microsoft/sarif-sdk/pull/2501)
* NEW: Add `max-file-size-in-kb` argument that allows filtering scan targets by file size. [#2494](https://github.com/microsoft/sarif-sdk/pull/2494)

## **v2.4.15** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.15) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.15) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.15) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.15) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.15)
* BUG: Fix `ArgumentNullException` when `PropertiesDictionary` is instantiated with a null comparer. [#2482](https://github.com/microsoft/sarif-sdk/pull/2482)
* BUG: Fix `UnhandledEngineException` when target path does not exist for multithreaded application by validating directories as is done for singlethreaded analysis. [#2461](https://github.com/microsoft/sarif-sdk/pull/2461)

## **v2.4.14** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.14) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.14) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.14) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.14) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.14)
* BRK: `Id` property of `Location` changed from `int`(32bit) to `BigInteger`(unlimited) to fix `Newtonsoft.Json.JsonReaderException: JSON integer XXXXX is too large or small for an Int32.` [#2463](https://github.com/microsoft/sarif-sdk/pull/2463)
* BUG: Eliminate dispose of stream and `StreamWriter` arguments passed to `SarifLog.Save` helpers. This would result in `ObjectDisposedException` being raised on attempt to access streams after save.

## **v2.4.13** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.13) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.13) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.13) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.13) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.13)
* BRK: `AnalyzeCommandBase` previously persisted all scan target artifacts to SARIF logs rather than only persisting artifacts referenced by an analysis result, when an option to persist hashes, text file or binary information was set. `MultithreadedAnalyzeCommandBase` previously persisted all scan targets artifacts to SARIF logs in cases when hash insertion was eenabled rather than only persisting artifacts referenced by an analysis result. [#2433](https://github.com/microsoft/sarif-sdk/pull/2433)
* BRK: Fix `InvalidOperationException` when using PropertiesDictionary in a multithreaded application, and remove `[Serializable]` from it. Now use of BinaryFormatter on it will result in `SerializationException`: Type `PropertiesDictionary` is not marked as serializable. [#2415](https://github.com/microsoft/sarif-sdk/pull/2415)
* BRK: `SarifLogger` now emits an artifacts table entry if `artifactLocation` is not null for tool configuration and tool execution notifications. [#2437](https://github.com/microsoft/sarif-sdk/pull/2437)
* BUG: Adjust Json Serialization property order for ReportingDescriptor and skip emit empty AutomationDetails node. [#2420](https://github.com/microsoft/sarif-sdk/pull/2420)
* BUG: Fix `ArgumentException` when `--recurse` is enabled and two file target specifiers generates the same file path. [#2438](https://github.com/microsoft/sarif-sdk/pull/2438)
* BUG: Fix 'InvalidOperationException' with message `Collection was modified; enumeration operation may not execute` in `MultithreadedAnalyzeCommandBase`, which is raised when analyzing with the `--hashes` switch. [#2447](https://github.com/microsoft/sarif-sdk/pull/2447)
* BUG: Fix `Merge` command produces empty SARIF file in Linux when providing file name only without path. [#2408](https://github.com/microsoft/sarif-sdk/pull/2408)
* BUG: Fix `NullReferenceException` when filing work item with a SARIF file which has no filable results. [#2412](https://github.com/microsoft/sarif-sdk/pull/2412)
* BUG: Fix missing `endLine` and `endColumn` properties and remove vulnerable packages for ESLint SARIF formatter. [#2458](https://github.com/microsoft/sarif-sdk/pull/2458)
* NEW: Add `--sort-results` argument to the `rewrite` command to get sorted SARIF results. [#2422](https://github.com/microsoft/sarif-sdk/pull/2422)

## **v2.4.12** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.12) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.12) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.12) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.12) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.12)
* BUG: Fix number of results when filing work item. [#2391](https://github.com/microsoft/sarif-sdk/pull/2391)
* BUG: Fix `TryIsSuppressed` logic. [#2395](https://github.com/microsoft/sarif-sdk/pull/2395)
* NEW: Add `suppress` command to multitool. [#2394](https://github.com/microsoft/sarif-sdk/pull/2394)
* NEW: `MultithreadCommandBase` will use cache when hashing is enabled. [#2388](https://github.com/microsoft/sarif-sdk/pull/2388)
* NEW: Flow suppressions when baselining. [#2390](https://github.com/microsoft/sarif-sdk/pull/2390)

## **v2.4.11** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.11) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.11) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.11) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.11) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.11)
* BUG: Fix partitioning visitor log duplication. [#2369](https://github.com/microsoft/sarif-sdk/pull/2369)
* NEW: Add `baseline` argument in `AnalyzeCommandBase` classes. [#2371](https://github.com/microsoft/sarif-sdk/pull/2371)
* NEW: Clang-Tidy converter will also accept console output log. [#2373](https://github.com/microsoft/sarif-sdk/pull/2373)

## **v2.4.10** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.10) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.10) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.10) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.10) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.10)
* NEW: Add Clang-Tidy converter. [#2367](https://github.com/microsoft/sarif-sdk/pull/2367)

## **v2.4.9** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.9) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.9) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.9) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.9) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.9)

* NEW: Report inner exception details if available. [#2357](https://github.com/microsoft/sarif-sdk/pull/2357)
* NEW: Add support for git blame. [#2358](https://github.com/microsoft/sarif-sdk/pull/2358)

## **v2.4.8** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.8) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.8) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.8) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.8) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.8)

* BUG: Fix `file-work-item` baselining. [#2344](https://github.com/microsoft/sarif-sdk/pull/2344)
* BUG: Fix `FileRegionsCache` context region construction. [#2348](https://github.com/microsoft/sarif-sdk/pull/2348)

## **v2.4.7** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.7) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.7) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.7) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.7) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.7)

* BUG: Fix `SubId` handling in `CachingLogger`. [#2334](https://github.com/microsoft/sarif-sdk/pull/2334)
* NEW: Add Hdf converter. [#2340](https://github.com/microsoft/sarif-sdk/pull/2340)
* BUG: Fix max result ingestion from `GitHubIngestionVisitor`. [#2341](https://github.com/microsoft/sarif-sdk/pull/2341)

## **v2.4.6** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.6) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.6) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.6) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.6) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.6)

* NEW: Add CWE relationship in FlawFinder converter. [#2332](https://github.com/microsoft/sarif-sdk/pull/2332)
* NEW: Add `ResultLevelKind` which will handle `FailureLevel` and `ResultKind`. [#2331](https://github.com/microsoft/sarif-sdk/pull/2331)
* BUG: Fix `GitHelper` logic. [#2327](https://github.com/microsoft/sarif-sdk/pull/2327)

## **v2.4.5** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.5) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.5) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.5) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.5) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.5)

* BUG: Fix `FileRegionsCache` logic. [#2309](https://github.com/microsoft/sarif-sdk/pull/2309)

## **v2.4.4** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.4) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.4) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.4) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.4) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.4)

* BUG: Fix performance issue in `CachingLogger`. [#2301](https://github.com/microsoft/sarif-sdk/pull/2301)
* BUG: Fix context dispose while analyzing. [#2303](https://github.com/microsoft/sarif-sdk/pull/2303)
* BUG: Fix export json configuration. [#2305](https://github.com/microsoft/sarif-sdk/pull/2305)
* BUG: Fix thread issues while using `Cache`. [#2306](https://github.com/microsoft/sarif-sdk/pull/2306)

## **v2.4.3** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.3) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.3) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.3) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.3) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.3)

* BUG: Fix issue when executing sarif.multitool. [#2298](https://github.com/microsoft/sarif-sdk/pull/2298)

## **v2.4.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.2) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.2) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.2)

* NEW: `ConstructMultilineContextSnippet` will retrieve a few character after/before to prevent entire file when the file is one line only. [#2288](https://github.com/microsoft/sarif-sdk/pull/2288)
* NEW: `baseliner` will consider `locations`. [2290](https://github.com/microsoft/sarif-sdk/pull/2290)
* BUG: Fix AzureDevOps title maxLength. [#2292](https://github.com/microsoft/sarif-sdk/pull/2292)
* NEW: Add `PerFingerprint` and `PerPropertyBagProperty` splitting for `file-work-items` command. [#2293](https://github.com/microsoft/sarif-sdk/pull/2293)
* NEW: Add `kusto` command in Sarif.Multitool. [#2296](https://github.com/microsoft/sarif-sdk/pull/2296)

## **v2.4.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.1) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.1)

* BRK: Move `transform` functionality into `rewrite` and delete redundant `transform` command. [#2252](https://github.com/microsoft/sarif-sdk/pull/2252)
* NEW: kind, level, insert, and remove options can now be added to from environment variables. [#2273](https://github.com/microsoft/sarif-sdk/pull/2273)
* NEW: `Merge` command will de-duplicate results. [#2280](https://github.com/microsoft/sarif-sdk/pull/2280)
* NEW: `Merge` command will merge artifacts. [#2285](https://github.com/microsoft/sarif-sdk/pull/2285)

## **v2.4.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.4.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.4.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.4.0) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.4.0) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.4.0)

* BRK: Entirely remove `verbose` whose fuctionality has been replaced by `--level` and `--kind`. [#2241](https://github.com/microsoft/sarif-sdk/pull/2241)
* BRK: Rename `LoggingOptions` to `LogFilePersistenceOptions`. [#2241](https://github.com/microsoft/sarif-sdk/pull/2241)
* NEW: `--quiet` will now suppress all console messages except for errors. [#2241](https://github.com/microsoft/sarif-sdk/pull/2241)
* BUG: Fix NullReference in SARIF1012 rule validation [#2254]. (<https://github.com/microsoft/sarif-sdk/pull/2254>)
* BRK: Rename `--plug-in` to `--plugin`. [#2264](https://github.com/microsoft/sarif-sdk/pull/2264)
* NEW: Pass `--plugin` to load more binaries to analyze or export data. [#2264](https://github.com/microsoft/sarif-sdk/pull/2264)

## **v2.3.18** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.18) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.18) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.18) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.18) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.3.18)

* NEW: Relax GH1005. [#2248](https://github.com/microsoft/sarif-sdk/pull/2248)

## **v2.3.17** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.17) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.17) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.17) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.17) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.3.17)

* BRK: Move `CommandBase` class from `Multitool.Library` assembly to `Driver`. [#2238](https://github.com/microsoft/sarif-sdk/pull/2238)
* NEW: Argument `VersionControlDetails` for `OptionallyEmittedData` in a analysis command will fill `VersionControlProvenance`. [#2237](https://github.com/microsoft/sarif-sdk/pull/2237)

## **v2.3.16** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.16) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.16) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.16) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.16) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.3.16)

* BRK: Rename flag `VersionControlInformation` to `VersionControlDetails` from `OptionallyEmittedData`. [#2222](https://github.com/microsoft/sarif-sdk/pull/2222)
* BUG: Fix filtering when using the command `analyze` with custom configuration. [#2230](https://github.com/microsoft/sarif-sdk/pull/2230)
* NEW: If argument `computeFileHashes`, it will be converted to `OptionallyEmittedData.Hashes`. [#2231](https://github.com/microsoft/sarif-sdk/pull/2231)
* NEW: Ensure all command options argument properties are settable (useful for API-driven invocation). [#2234](https://github.com/microsoft/sarif-sdk/pull/2234)
* NEW: TargetUri from context can be relative. [#2235](https://github.com/microsoft/sarif-sdk/pull/2235)

## **v2.3.14** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.14) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.14) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.14) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.14) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.3.14)

* BUG: Fix concurrency issue in when using `Cache`. [#2215](https://github.com/microsoft/sarif-sdk/pull/2215)
* NEW: `ConsoleLogger` will print exception if that exists. [#2217](https://github.com/microsoft/sarif-sdk/pull/2217)
* BUG: Fix `WebRequest` parameters parse that resulted in regex hang [#2219](https://github.com/microsoft/sarif-sdk/pull/2219)

## **v2.3.11** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.11) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.11) | [Converters]

* DEPENDENCY BRK: SARIF now requires Newtonsoft.JSON 12.0.3.
* Add `PerRun` splitting strategy for log file refactoring.

## **v2.3.10** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.10) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.10) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.10) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.10) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.3.10)

* BRK: Rename package `WorkItems` to `Microsoft.WorkItems`. [#2180](https://github.com/microsoft/sarif-sdk/pull/2180)
* BUG: Fix `export-validation-config` exception. [#2181](https://github.com/microsoft/sarif-sdk/pull/2181)

## **v2.3.9** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.9) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.9) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.9) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.9) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.3.9)

* NEW: Multitool SARIF rewrite accepts `remove` parameter. [#2160](https://github.com/microsoft/sarif-sdk/pull/2160)
* BRK: Remove command `export-validation-docs` and extend `export-validation-rules` command to export markdown file. [#2156](https://github.com/microsoft/sarif-sdk/pull/2156)
* DEPENDENCY BRK: SARIF now requires Newtonsoft.JSON 11.0.2 (rather than 10.0.3). [#2172](https://github.com/microsoft/sarif-sdk/pull/2172)
* BRK: Remove unused `run` argument from FileRegionsCache constructors. [#2173](https://github.com/microsoft/sarif-sdk/pull/2173)
* BRK: Rename various methods in `IFileSystem` and `FileSystem` classes (to consistently prefix all method names with their containing .NET static type, e.g. `Directory`. [#2173](https://github.com/microsoft/sarif-sdk/pull/2173)

## **v2.3.8** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.8) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.8) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.8) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.8) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.3.8)

* NEW: PACKAGE BRK: Upgrade from .NET Framework 4.5 to .NET Framework 4.5.2. [#2135](https://github.com/microsoft/sarif-sdk/pull/2135)
* NEW: Multitool SARIF merge accepts `threads` parameter. [#2026](https://github.com/microsoft/sarif-sdk/pull/2026)
* NEW: Enable GitHub SourceLink to all project [#2148](https://github.com/microsoft/sarif-sdk/pull/2148)

## **v2.3.7** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.7) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.7) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.7) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.7) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.3.7)

* DEPENDENCY BRK: SARIF now requires Newtonsoft.JSON 11.0.2 (rather than 10.0.3)
* DEPENDENCY: SARIF TypeScript package now requires minimist 1.2.3 or later (rather than >=1.2.0)
* BUG: Fix index out of range exception when baselining [#2102](https://github.com/microsoft/sarif-sdk/pull/2102)
* NEW: Add a setter to `GitHelper.GitExePath`. [#2110](https://github.com/microsoft/sarif-sdk/pull/2110)
* NEW: `GitHelper` will search in %PATH% variable for `git.exe` instead of its default install location. [#2107](https://github.com/microsoft/sarif-sdk/pull/2107)
* NEW: Add helper in `SarifLog` and `Run` to `ApplyPolicies`. [#2109](https://github.com/microsoft/sarif-sdk/pull/2109)
* NEW: Add a converter for FlawFinder's CSV output format. [#2092](https://github.com/microsoft/sarif-sdk/issues/2092)
* NEW: Multitool SARIF output is now pretty-printed by default. To remove white space, specify `--minify`. [#2098](https://github.com/microsoft/sarif-sdk/issues/2098)
* NEW: The Multitool `query` command can now evaluate properties in the result and rule property bags, for example `sarif query "properties.confidence:f > 0.95 AND rule.properties.category == 'security'"`
* NEW: The validation rule `SARIF1004.ExpressUriBaseIdsCorrectly` now verifies that if an `artifactLocation.uri` is a relative reference, it does not begin with a slash. [#2090](https://github.com/microsoft/sarif-sdk/issues/2090)
* BUG: GitHub policy should not turn off any note level rules. [#2089](https://github.com/microsoft/sarif-sdk/issues/2089)
* NEW: Add `apply-policy` command to Multitool. [#2118](https://github.com/microsoft/sarif-sdk/pull/2118)

## **v2.3.6** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.6) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.6) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.6) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.6) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.3.6)

* BUG: Restore multitool client app package build.
* BUG: Fix ESLint additional formatter corner cases that result in invalid SARIF.
* NEW: COMMAND-LINE BRK: The analysis rules that validate a SARIF file's compatibility with GitHub Advanced Security code scanning now have rule ids that begin with `GH` rather than `SARIF`.

## **v2.3.5** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.5) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.5) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.5) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.5) | [Multitool Library](https://www.nuget.org/packages/Sarif.Multitool.Library/2.3.5)

* NEW: COMMAND-LINE BRK: Validation rule `SARIF2005.ProvideToolProperties` now requires `informationUri`, it allows `dottedQuadFileVersion` to satisfy the requirement that version information be present, and it is configurable.
* NEW: Extract the public APIs from Sarif.Multitool into a new dependency package Sarif.Multitool.Library. Sarif.Multitool remains as a dotnet tool package.
* NEW: Validation rule `SARIF2012` now checks for the presence of a friendly name in PascalCase in the `name` property, and is renamed from `ProvideHelpUris` to `ProvideRuleProperties`.
* NEW: The Multitool `rewrite` command now accepts `VersionControlInformation` as an argument to the `--insert` option. This argument populates `run.versionControlProvenance`, and it re-expresses all absolute URIs as relative references with respect to the nearest enclosing repository root, if any.

## **v2.3.4** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.4) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.4) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.4) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.4)

* COMMAND-LINE BRK: Change `merge` command output directory argument name to `output-directory`.
* NEW: Add analysis rules appropriate for SARIF files that are to be uploaded to GitHub Advanced Security code scanning.
* BUG: Various Fortify FPR converter improvements (such as improve variable expansion in result messages).
* BUG: The validator no longer reports `SARIF2010.ProvideCodeSnippets` if embedded file content for the specified artifact is present. [#2003](https://github.com/microsoft/sarif-sdk/issues/2003)

## **v2.3.3** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.3) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.3) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.3) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.3)

* NEW: Improve `SarifSdkSample` application: use `uriBaseIds`.
* NEW: Add additional checks to SARIF analysis rule `SARIF2004.OptimizeFileSize`.
* NEW: Introduce new SARIF analysis rule `SARIF2016.FileUrisShouldBeRelative`.
* BUG: If you created a URI from an absolute file path (for example, `C:\test\file.c`), then it would be serialized with that exact string, which is not a valid URI. This is now fixed. [#2001](https://github.com/microsoft/sarif-sdk/issues/2001)

## **v2.3.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.2) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.2)

* NEW: The `Sarif.Multitool` command line verbs are now exposed programmatically. For example, the `validate` verb is exposed through the classes `ValidateCommand` and `ValidateOptions`.

## **v2.3.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.1)

* NEW: Revised and improved validation rules in `Sarif.Multitool`.
* NEW: Properties serialization performance improved (~20% faster load when Results use Properties).
* NEW: Allow result messages to be truncated for display. [#1915](https://github.com/microsoft/sarif-sdk/issues/1915)
* BUG: Rebase URI command now honors `--insert` and `--remove` arguments for injecting or eliding optional data (such as region snippets).
* BUG: Ensure all DateTimes on object model are using DateTimeConverter consistently.
* BUG: Fix DateTime roundtripping in properties collections to follow normal DateTime output format.

## **v2.3.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.0) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.0)

* BUG: `ResultLogJsonWriter` now creates an empty `results` array if there are no results, rather than leaving `results` as `null`. [#1821](https://github.com/microsoft/sarif-sdk/issues/1821)
* BUG: In validation rules, `shortDescription` is now calculated by `GetFirstSentence` method, fixing a bug in sentence breaking. [#1887](https://github.com/microsoft/sarif-sdk/issues/1887)
* BUG: `WorkItemFiler` now logs correctly the details for `LogMetricsForProcessedModel` method [#1896](https://github.com/microsoft/sarif-sdk/issues/1896)
* NEW: Add validation rule `SARIF1019`, which requires every result to have at least one of `result.ruleId` and `result.rule.id`. If both are present, they must be equal. [#1880](https://github.com/microsoft/sarif-sdk/issues/1880)
* NEW: Add validation rule `SARIF1020`, which requires that the $schema property should be present, and must refer to the final version of the SARIF 2.1.0 schema. [#1890](https://github.com/microsoft/sarif-sdk/issues/1890)
* NEW: Expose `Run.MergeResultsFrom(Run)` to merge Results from multiple Runs using code from result matching algorithm.
* BRK: Rename `RemapIndicesVisitor` to `RunMergingVisitor` and redesign to control how much merging occurs internally.

## **v2.2.5** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.2.5) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.2.5) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.2.5) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.2.5)

* BUG: Fix SDK doubling Uris with certain escaped characters (ex: '-' and '_') on every Load/Save cycle (cause: <https://github.com/dotnet/runtime/issues/36288>)

## **v2.2.4** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.2.4) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.2.4) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.2.4) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.2.4)

* BUG: Validation rule SARIF1018 was not checking for a trailing slash on `uri` properties in `originalUriBaseIds` if `uriBaseId` was present.
* BUG: Build Sarif.Multitool NPM package non-trimmed to avoid more assembly load problems.
* NEW: DeferredList will cache last item returned and won't throw if same instance written. (SarifRewritingVisitor + Deferred OM usable)

## **v2.2.3** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.2.3) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.2.3) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.2.3) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.2.3)

* NEW: Introduce `SarifConstants.SarifFileExtension` with value `".sarif"`.
* NEW: In validation rule SARIF1018, require `uri` values in `originalUriBaseIds` to end with a slash, per the SARIF spec.
* BUG: Result.GetRule will look up by RuleId if RuleIndex not present.
* BUG: Baselining will properly persist Run.Tool.Driver.Rules if Results reference by RuleId.
* BUG: DeferredOM will properly load files with a BOM. (LineMappingStreamReader fix)
* BUG: Remove CsvHelper dependency to avoid assembly load problem in Sarif.Multitool NPM package.

## **v2.2.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.2.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.2.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.2.2) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.2.2)

* BUG: `dotnet tool install` command for Multitool now produces a working installation rather than reporting missing `Sarif.Converters` binary.
* BUG: Result.GetRule will look up by RuleId if RuleIndex not present.
* BUG: Baselining will properly persist Run.Tool.Driver.Rules if Results reference by RuleId.
* BUG: DeferredOM will properly load files with a BOM. (LineMappingStreamReader fix)

## **v2.2.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.2.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.2.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.2.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.2.1)

* NEW: Multitool `remove` option now supports `Guids` value to remove `Result.Guid`.
* NEW: Significant Baselining algorithm improvements: dynamic `partialFingerprint` trust, location-specific unique what property matching, 'nearby' matching, correct omitted `Region` property handling, correct `ReportingDescriptor.DeprecatedIds` handling.
* DEPENDENCY BRK: SARIF now requires Newtonsoft.JSON 10.0.3 (rather than 9.0.x).

## **v2.2.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.2.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.2.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.2.0) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.2.0)

* PACKAGE BRK: Update tool directory to netstandard2.1, to reflect use of that version of .NET Core.
* NEW: Multitool `rewrite` command performance when populating regions and snippets is greatly improved.
* NEW: Multitool `insert` option now supports `Guids` value to populate `Result.Guid`.
* API + SCHEMA BRK: Fix typo in schema: suppression.state should be suppression.status according to the spec. [#1785](https://github.com/microsoft/sarif-sdk/issues/1785)
* BUG: Multitool `rewrite` no longer throws when it encounters an invalid value (such as -1) for a region property.
* BUG: ESLint SARIF formatter no longer produces invalid SARIF when given an ESLint message with no rule id. It is treated as a `toolConfigurationNotification`. [#1791](https://github.com/microsoft/sarif-sdk/issues/1791)
* BUG: Resolve crash on converting PREfast log files with non-null but empty help URLs.

## **v2.1.25** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.25) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.25) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.25) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.25)

* NEW: The baseliner (available through the Multitool's `match-results-forward` command) now populates `result.provenance.firstDetectionTimeUtc` so you can now track the age of each issue. [#1737](https://github.com/microsoft/sarif-sdk/issues/1737)

## **v2.1.24** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.24) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.24) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.24) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.24)

* NEW: Introduce API to partition log files by arbitrary criteria (method `SarifPartitioner.Partition` and class `PartitioningVisitor`).
* BUG: `Tool.CreateFromAssembly` now properly handles file versions that contain extra characters after the "dotted quad" string. [#1728](https://github.com/microsoft/sarif-sdk/issues/1728)

## **v2.1.23** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.23) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.23) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.23) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.23)

* API BRK: Remove 'Errors.LogExceptionLoadingPdb' helper (as not relevant to core SDK).
* NEW: Allow emitting non-failure tool notifications as debug/informational messages.
* NEW: `SarifLogger` now populates `tool.driver`'s `organization` and `product` properties instead of adding `"Company"` and `"ProductName"` to `tool.driver'`s property bag. [#1716](https://github.com/microsoft/sarif-sdk/issues/1716)
* NEW: Add `closeWriterOnDispose` argument (with a default of 'true') that indicates whether SarifLogger writers are closed during its Dispose() method. Providing a value of `false` to this argument allows SarifLogger to work against a stream that can subsequently be reused (for example, to deserialize the logged content back to a `SarifLog` instance).
* NEW: Update PREfast converter to render optional suppression data.
* BUG: Update PREfast converter to handle paths with no trailing slash.
* BUG: Baselining now matches the first and last Result per URI as an additional pass.

## **v2.1.22** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.22) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.22) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.22) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.22)

* BUG: Fix bug in validation rule `EndTimeMustNotBeBeforeStartTime`, which threw if `invocation.startTimeUtc` was present but `endTimeUtc` was absent.

## **v2.1.21** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.21) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.21) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.21) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.21)

* NEW: Provide an API `SarifPartitioner.Filter` that selects results according to a predicate, and filters `run.artifacts` to only those artifacts used by the included results.

## **v2.1.20** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.20) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.20) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.20) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.20)

* NEW: Added Stream-based SarifLog.Load and Save overloads
* NEW: Enhanced property bag serialization unit testing. [#1673](https://github.com/microsoft/sarif-sdk/issues/1673)
* BUG: Fix packaging warning NU5048 during build. [#1687](https://github.com/microsoft/sarif-sdk/issues/1687)
* BUG: SarifLogger.Optimized could not be set from the command line. [#1695](https://github.com/microsoft/sarif-sdk/issues/1695)
* BUG: Result Matching now omits previously Absent results.
* BUG: Result Matching properly compares results from the same RuleID when multiple Rules match the same source line.
* BUG: Result Matching works when a result moves and has the line number in the message.
* BUG: Result Matching always assigns Result.CorrelationGuid and Result.Guid.
* BUG: Null hardening in Result Matching
* BUG: Console logger now outputs file location, if available, when writing notifications.

## **v2.1.19** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.19) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.19) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.19) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.19)

* Sort driver skimmers by rule id + name during analysis, in order to improve deterministic ordering of log file data.
* API BRK: Convert various public SARIF Driver framework API to prefer abstract ISet<string> type over HashSet<string>.
* API BRK: Remove helper method `SarifUtilities.DeserializeObject` introduced in 2.1.15 to fix. [#1577](https://github.com/microsoft/sarif-sdk/issues/1577)
Now that an underlying bug in `PropertyBagConverter` has been fixed, there is no need to work around it with this helper method. `JsonConvert.DeserializeObject` works fine.
* NEW: Expanding Sarif SDK query mode to support Result.Uri, string StartsWith/EndsWith/Contains.
* NEW: Adding Result.Run and a populating method, so that methods which need the Run context for a given Result have an integrated way to retrieve it.

## **v2.1.17** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.17) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.17) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.17) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.17)

* API NON-BRK: emit all core object model members as 'virtual'.
* NEW: Introduce SarifConsolidator to shrink large log files. [#1675](https://github.com/microsoft/sarif-sdk/issues/1675)
* BUG: Analysis rule SARIF1017 incorrectly rejected index-valued properties that referred to taxonomies. [#1678](https://github.com/microsoft/sarif-sdk/issues/1678)
* BUG: `match-results-forward-command` dropped log contents and mishandled `rules` array. [#1684](https://github.com/microsoft/sarif-sdk/issues/1684)

## **v2.1.16** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.16) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.16) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.16) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.16)

* BUGFIX, BRK: In the Multitool `page` command, the default for `--force` was `true` and it could not be changed. [#1630](https://github.com/microsoft/sarif-sdk/issues/1630)
* BUG: The Multitool `match-results-forward` command failed if results included logical locations. [#1656](https://github.com/microsoft/sarif-sdk/issues/1656)
* BUG: `SarifLogger(ReportingDescriptor rule, Result result)` failed if it tried to log a result whose `ruleId` was a sub-rule; for example, `rule.Id == "TEST0001"` but `result.ruleId == "TEST0001/1"`. [#1668](https://github.com/microsoft/sarif-sdk/issues/1668)
* NEW: Implement results and notifications caching when `--hashes` is specified on the SARIF driver command line.

## **v2.1.15** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.15) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.15) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.15) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.15)

* BUG: Validation rule `SARIF1015` incorrectly required `originalUriBaseIds` to be contain URIs. [#1485](https://github.com/microsoft/sarif-sdk/issues/1485)
* BUG: Persist Fortify rule metadata properties. [#1490](https://github.com/microsoft/sarif-sdk/issues/1490)
* BUG: Multitool transform mishandled dottedQuadFileVersion. [#1532](https://github.com/microsoft/sarif-sdk/issues/1532)
* BUG: Restore missing FxCop converter unit test. [#1575](https://github.com/microsoft/sarif-sdk/issues/1575)
* BUG: Multitool transform mishandled date/time values in property bags. [#1577](https://github.com/microsoft/sarif-sdk/issues/1577)
* BUG: Multitool transform could not upgrade SARIF files from the sarif-2.1.0-rtm.1 schema. [#1584](https://github.com/microsoft/sarif-sdk/issues/1584)
* BUG: Multitool merge command produced invalid SARIF if there were 0 input files. [#1592](https://github.com/microsoft/sarif-sdk/issues/1592)
* BUG: FortifyFpr converter produced invalid SARIF. [#1593](https://github.com/microsoft/sarif-sdk/issues/1593)
* BUG: FxCop converter produced empty `result.message` objects. [#1594](https://github.com/microsoft/sarif-sdk/issues/1594)
* BUG: Some Multitool commands required --force even if --inline was specified. [#1642](https://github.com/microsoft/sarif-sdk/issues/1642)
* NEW: Add validation rule to ensure correctness of `originalUriBaseIds` entries. [#1485](https://github.com/microsoft/sarif-sdk/issues/1485)
* NEW: Improve presentation of option validation messages from the Multitool `page` command. [#1629](https://github.com/microsoft/sarif-sdk/issues/1629)

## **v2.1.14** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.14) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.14) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.14) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.14)

* BUG: FxCop converter produced logicalLocation.index but did not produce the run.logicalLocations array. [#1571](https://github.com/microsoft/sarif-sdk/issues/1571)
* BUG: Include Sarif.WorkItemFiling.dll in the Sarif.Multitool NuGet package. [#1636](https://github.com/microsoft/sarif-sdk/issues/1636)
* NEW: Add validation rule to ensure that all array-index-valued properties are consistent with their respective arrays.

## **v2.1.13** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.13) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.13) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.13) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.13)

* BUG: Respect the --force option in Sarif.Multitool rather than overwriting the output file. [#1340](https://github.com/microsoft/sarif-sdk/issues/1340)
* BUG: Accept URI-valued properties whose value is the empty string. [#1632](https://github.com/microsoft/sarif-sdk/issues/1632)

## **v2.1.12** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.12) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.12) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.12) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.12)

* BUG: Improve handling of `null` values in property bags. [#1581](https://github.com/microsoft/sarif-sdk/issues/1581)

## **v2.1.11** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.11) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.11) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.11) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.11)

* BUG: Result matching should prefer the suppression info from the current run. [#1600](https://github.com/microsoft/sarif-sdk/issues/1600)

## **v2.1.10** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.10) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.10) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.10) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.10)

* BUG: Resolve a performance issue in web request parsing code. <https://github.com/microsoft/sarif-sdk/issues/1608>

## **v2.1.9** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.9) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.9) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.9) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.9)

* NEW: add --remove switch to eliminate certain properties (currently timestamps only) from log file output.
* BUG: remove verbose 'Analyzing file..' reporting for drivers.

## **v2.1.8** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.8) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.8) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.8) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.8)

* BUG: Add missing `"additionalProperties": false` constraints to schema; add missing object descriptions and improve other object descriptions in schema; update schema version to -rtm.4.

## **v2.1.7** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.7) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.7) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.7) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.7)

* BUG: Multitool rewrite InsertOptionalData operations fail if a result object references `run.artifacts` using the `index` property.
* BUG: The `SarifCurrentToVersionOneVisitor` was not translating v2 `result.partialFingerprints` to v1 `result.toolFingerprintContribution`. [#1556](https://github.com/microsoft/sarif-sdk/issues/1556)
* BUG: The `SarifCurrentToVersionOneVisitor` was dropping `run.id` and emitting an empty `run.stableId`. [#1557](https://github.com/microsoft/sarif-sdk/issues/1557)

## **v2.1.6** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.6) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.6) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.6) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.6)

* BUG: Fortify FPR converter does not populate originalUriBaseIds if the source is a drive letter (e.g. C:)
* BUG: Multitool rebaseUri command throws null reference exception if results reference run.artifacts using the index property.
* BUG: Pre-release transformer does not upgrade schema uri if input version is higher than rtm.1.

## **v2.1.5** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.5) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.5) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.5) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.5)

* Change schemas back to draft-04 to reenable Intellisense in the Visual Studio JSON editor.

## **v2.1.4** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.4) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.4) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.4) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.4)

* BUG: Fix bugs related to parsing the query portion of a URI, and to the parsing of header strings.
* API NON-BRK: Introduce `WebRequest.TryParse` and `WebResponse.TryParse` to accompany existing `Parse` methods.

## **v2.1.3** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.3) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.3) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.3) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.3)

* Change schema uri to secure (https) instance.
* BUG: Fix tranformer bug where schema id would not be updated if no other transformation occurred.
* BUG: `ThreadFlowLocation.Kind` value is getting lost during pre-release transformation. [#1502](https://github.com/microsoft/sarif-sdk/issues/1502)
* BUG: `Location.LogicalLocation` convenience setter mishandles null. [#1514](https://github.com/microsoft/sarif-sdk/issues/1514)
* BUG: Upgrade schemas to latest version (remove `draft-04` from `$schema` property and change `id` to `$id`). This is necessary because the schemas use the `uri-reference` format, which was not defined in draft-04. [#1521](https://github.com/microsoft/sarif-sdk/issues/1521)
* API BRK: The `Init` methods in the Autogenerated SARIF object model classes are now `protected virtual`. This enables derived classes to add additional properties without having to copy the entire code of the `Init` method.
* BUG: Transformation from SARIF 1.0 to 2.x throws `ArgumentOutOfRangeException`, if `result.locations` is an empty array. [#1526](https://github.com/microsoft/sarif-sdk/issues/1526)
* BUG: Add `Result.Level` (and remove `Result.Rank`) for Fortify Converter based on MicroFocus feedback.
* BUG: Invocation constructor should set `executionSuccessful` to true by default.
* BUG: Contrast security converter now populates `ThreadFlowLocation.Location`. [#1530](https://github.com/microsoft/sarif-sdk/issues/1530)
* BUG: Contrast Security converter no longer emits incomplete `Artifact` objects. [#1529](https://github.com/microsoft/sarif-sdk/issues/1529)
* BUG: Fix crashing bugs and logic flaws in `ArtifactLocation.TryReconstructAbsoluteUri`.
* NEW: Provide a SARIF converter for Visual Studio log files.
* NEW: Extend the `PrereleaseCompatibilityTransformer` to handle SARIF v1 files.
* API NON-BRK: Introduce `WebRequest.Parse` and `WebResponse.Parse` to parse web traffic strings into SARIF `WebRequest` and `WebResponse` objects.
* API NON-BRK: Introduce `PropertyBagHolder.{Try}GetSerializedPropertyInfo`, a safe way of retrieving a property whose type is unknown.

## **v2.1.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.2) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.2)

* API BRK: Change location.logicalLocation to logicalLocations array. [oasis-tcs/sarif-spec#414](https://github.com/oasis-tcs/sarif-spec/issues/414)

## **v2.1.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.1)

* BUG: Multitool crashes on launch: Can't find CommandLine.dll. [#1487](https://github.com/microsoft/sarif-sdk/issues/1487)

## **v2.1.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.0) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.0)

* API NON-BRK: `PhysicalLocation.id` property is getting lost during 2.1.0 pre-release transformation. [#1479](https://github.com/microsoft/sarif-sdk/issues/1479)
* Add support for converting TSLint logs to SARIF
* Add support for converting Pylint logs to SARIF

## **v2.1.0-rtm.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.0-rtm.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.0-rtm.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.0-rtm.0)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.0-rtm.0)

* API BRK: OneOf `graphTraversal.runGraphIndex` and `graphTraversal.resultGraphIndex` is required.
* API NON-BRK: Add address.kind well-known values "instruction" and "data". [oasis-tcs/sarif-spec#397](https://github.com/oasis-tcs/sarif-spec/issues/397)
* API BRK: Rename `invocation.toolExecutionSuccessful` to `invocation.executionSuccessful`. [oasis-tcs/sarif-spec#399](https://github.com/oasis-tcs/sarif-spec/issues/399)
* API BRK: Add regex patterns for guid and language in schema.
* API NON-BRK: Add `run.specialLocations` in schema. [oasis-tcs/sarif-spec#396](https://github.com/oasis-tcs/sarif-spec/issues/396)
* API BRK: Improve `address` object design. [oasis-tcs/sarif-spec#401](https://github.com/oasis-tcs/sarif-spec/issues/401)

## **v2.1.0-beta.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.0-beta.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.0-beta.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.0-beta.2)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.0-beta.2)

* API NON-BRK: Change `request.target` type to string. [oasis-tcs/sarif-spec#362](https://github.com/oasis-tcs/sarif-spec/issues/362)
* API BRK: anyOf `physicalLocation.artifactLocation` and `physicalLocation.address` is required. [oasis-tcs/sarif-spec#353](https://github.com/oasis-tcs/sarif-spec/issues/353)
* API BRK: Rename `run.defaultFileEncoding` to `run.defaultEncoding`.
* API NON-BRK: Add `threadFlowLocation.taxa`. [oasis-tcs/sarif-spec#381](https://github.com/oasis-tcs/sarif-spec/issues/381)
* API BRK: anyOf `message.id` and `message.text` is required.
* API NON-BRK: Add `request.noResponseReceived` and `request.failureReason`. [oasis-tcs/sarif-spec#378](https://github.com/oasis-tcs/sarif-spec/issues/378)
* API BRK: anyOf `externalPropertyFileReference.guid` and `externalPropertyFileReference.location` is required.
* API BRK: `artifact.length` should have `default: -1, minimum: -1` values.
* API BRK: Rename `fix.changes` to `fix.artifactChanges`.
* API BRK: Each redaction token in an originalUriBaseId represents a unique location. [oasis-tcs/sarif-spec#377](https://github.com/oasis-tcs/sarif-spec/issues/377)
* API BRK: Rename file related enums in `artifact.roles`.
* API BRK: anyOf `artifactLocation.uri` and `artifactLocation.index` is required.
* API BRK: `multiformatMessageString.text` is required.
* API BRK: `inlineExternalProperties` array must have unique items.
* API BRK: `run.externalPropertyFileReferences`, update unique flag and minItems on every item according to spec.
* API BRK: `run.markdownMessageMimeType` should be removed from schema.
* API BRK: `externalPropertyFileReference.itemCount` should have a minimum value of 1.
* API NON-BRK: Add `toolComponent.informationUri` property.
* API NON-BRK: `toolComponent.isComprehensive` default value should be false.
* API BRK: `artifact.offset` minimum value allowed should be 0.
* API NON-BRK: Add `directory` enum value in `artifact.roles`.
* API BRK: `result.suppressions` array items should be unique and default to null.
* API NON-BRK: Add `suppression.guid` in schema.
* API BRK: `graph.id` should be removed from schema.
* API BRK: `edgeTraversal.stepOverEdgeCount` minimum should be 0.
* API BRK: `threadFlowLocation.nestingLevel` minimum should be 0.
* API BRK: `threadFlowLocation.importance` should default to `important`.
* API BRK: `request.index` should have default: -1, minimum: -1.
* API BRK: `response.index` should have default: -1, minimum: -1.
* API NON-BRK: `externalProperties.version` is not a required property if it is not root element.
* API NON-BRK: Add artifact roles for configuration files. [oasis-tcs/sarif-spec#372](https://github.com/oasis-tcs/sarif-spec/issues/372)
* API NON-BRK: Add suppression.justification. [oasis-tcs/sarif-spec#373](https://github.com/oasis-tcs/sarif-spec/issues/373)
* API NON-BRK: Associate descriptor metadata with thread flow locations. [oasis-tcs/sarif-spec#381](https://github.com/oasis-tcs/sarif-spec/issues/381)
* API BRK: Move `location.physicalLocation.id` to `location.id`. [oasis-tcs/sarif-spec#375](https://github.com/oasis-tcs/sarif-spec/issues/375)
* API BRK: `result.stacks` array should have unique items.
* API BRK: `result.relatedLocations` array should have unique items.
* API BRK: Separate `suppression` `status` from `kind`. [oasis-tcs/sarif-spec#371](https://github.com/oasis-tcs/sarif-spec/issues/371)
* API BRK: `reportingDescriptorReference` requires anyOf (`index`, `guid`, `id`).
* API BRK: Rename `request` object and related properties to `webRequest`.
* API BRK: Rename `response` object and related properties to `webResponse`.
* API NON-BRK: Add `locationRelationship` object. [oasis-tcs/sarif-spec#375](https://github.com/oasis-tcs/sarif-spec/issues/375)
* API BRK: `externalPropertyFileReference.itemCount` can be 0 and defaults to minimum: -1, default: -1.
* API BRK: `threadFlowLocation.executionOrder` can be 0 and defaults to -1, so minimum: -1, default: -1
* API BRK: Rename artifact role `traceFile` to `tracedFile`.
* API NON-BRK: Add artifact role `debugOutputFile`.
* API NON-BRK: Add `value` to `threadFlowLocation.kinds`.
* API NON-BRK: Add a new value to `result.kind`: `informational`.
* API NON-BRK: add `address.kind`values `function` and `page`.
* API NON-BRK: `run.columnKind` has no default value.
* API NON-BRK: In the `reportingDescriptorRelationship` object, add a property `description` of type `message`, optional.
* API NON-BRK: In the `locationRelationship` object, add a property `description` of type `message`, optional.
* API BRK: `region.byteOffset` should have default: -1, minimum: -1.
* API BRK: Change `notification.physicalLocation` of type `physicalLocation` to `notification.locations` of type `locations`.

## **v2.1.0-beta.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.0-beta.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.0-beta.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.0-beta.1)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.0-beta.1))

* API BRK: Change `request.uri` to `request.target`. [oasis-tcs/sarif-spec#362](https://github.com/oasis-tcs/sarif-spec/issues/362)

## **v2.1.0-beta.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.0-beta.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.0-beta.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.0-beta.0)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.0-beta.0))

* API BRK: All SARIF state dictionaries now contains multiformat strings as values. [oasis-tcs/sarif-spec#361](https://github.com/oasis-tcs/sarif-spec/issues/361)
* API NON-BRK: Define `request` and `response` objects. [oasis-tcs/sarif-spec#362](https://github.com/oasis-tcs/sarif-spec/issues/362)

## **v2.0.0-csd.2.beta.2019.04-03.3** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.04-03.3) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.04-03.3) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.04-03.3)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.04-03.3))

* API BRK: Rename `reportingDescriptor.descriptor` to `reportingDescriptor.target`. [oasis-tcs/sarif-spec#356](https://github.com/oasis-tcs/sarif-spec/issues/356)
* API NON-BRK: Remove `canPrecedeOrFollow` from relationship kind list. [oasis-tcs/sarif-spec#356](https://github.com/oasis-tcs/sarif-spec/issues/356)

## **v2.0.0-csd.2.beta.2019.04-03.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.04-03.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.04-03.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.04-03.2)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.04-03.2))

* API NON-BRK: Add `module` to `address.kind`. [oasis-tcs/sarif-spec#353](https://github.com/oasis-tcs/sarif-spec/issues/353)
* API BRK: `address.baseAddress` & `address.offset` to int. [oasis-tcs/sarif-spec#353](https://github.com/oasis-tcs/sarif-spec/issues/353)
* API BRK: Update how reporting descriptors describe their taxonomic relationships. [oasis-tcs/sarif-spec#356](https://github.com/oasis-tcs/sarif-spec/issues/356)
* API NON-BRK: Add `initialState` and `immutableState` properties to thread flow object. Add `immutableState` to `graphTraversal` object. [oasis-tcs/sarif-spec#168](https://github.com/oasis-tcs/sarif-spec/issues/168)

## **v2.0.0-csd.2.beta.2019.04-03.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.04-03.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.04-03.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.04-03.1)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.04-03.1))

* API BRK: Rename `message.messageId` property to `message.id`. <https://github.com/oasis-tcs/sarif-spec/issues/352>

## **v2.0.0-csd.2.beta.2019.04-03.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.04-03.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.04-03.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.04-03.0)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.04-03.0))

* API NON-BRK: Introduce new localization mechanism (post ballot changes). [oasis-tcs/sarif-spec#338](https://github.com/oasis-tcs/sarif-spec/issues/338)
* API BRK: Add `address` property to a `location` object (post ballot changes). [oasis-tcs/sarif-spec#302](https://github.com/oasis-tcs/sarif-spec/issues/302)
* API NON-BRK: Define result `taxonomies`. [oasis-tcs/sarif-spec#314](https://github.com/oasis-tcs/sarif-spec/issues/314)
* API NON-BRK: Define a `reportingDescriptorReference` object. [oasis-tcs/sarif-spec#324](https://github.com/oasis-tcs/sarif-spec/issues/324)
* API BRK: Change `run.graphs` and `result.graphs` from objects to arrays. [oasis-tcs/sarif-spec#326](https://github.com/oasis-tcs/sarif-spec/issues/326)
* API BRK: External property file related renames (post ballot changes). [oasis-tcs/sarif-spec#335](https://github.com/oasis-tcs/sarif-spec/issues/335)
* API NON-BRK: Allow toolComponents to be externalized. [oasis-tcs/sarif-spec#337](https://github.com/oasis-tcs/sarif-spec/issues/337)
* API BRK: Rename all `instanceGuid` properties to `guid`. [oasis-tcs/sarif-spec#341](https://github.com/oasis-tcs/sarif-spec/issues/341)
* API NON-BRK: Add `reportingDescriptor.deprecatedNames` and `deprecatedGuids` to match `deprecatedIds` property. [oasis-tcs/sarif-spec#346](https://github.com/oasis-tcs/sarif-spec/issues/346)
* API NON-BRK: Add `referencedOnCommandLine` as a role. [oasis-tcs/sarif-spec#347](https://github.com/oasis-tcs/sarif-spec/issues/347)
* API NON-BRK: Rename `reportingConfigurationOverride` to `configurationOverride`. [oasis-tcs/sarif-spec#350](https://github.com/oasis-tcs/sarif-spec/issues/350)

## **v2.0.0-csd.2.beta.2019.02-20** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.02-20) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.02-20) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.02-20)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.02-20))

* COMMAND-LINE BRK: Rename `--sarif-version` to `--sarif-output-version`. Remove duplicative tranform `--target-version` command-line argument.
* COMMAND-LINE NON-BRK: add `--inline` option to multitool `rebaseuri` verb, to write output directly into input files.
* API NON-BRK: Add additional properties to `toolComponent`. [oasis-tcs/sarif-spec#336](https://github.com/oasis-tcs/sarif-spec/issues/336)
* API NON-BRK: Provide a caching mechanism for duplicated code flow data. [oasis-tcs/sarif-spec#320](https://github.com/oasis-tcs/sarif-spec/issues/320)
* API NON-BRK: Add `inlineExternalPropertyFiles` at the log level. [oasis-tcs/sarif-spec#321](https://github.com/oasis-tcs/sarif-spec/issues/321)
* API NON-BRK: Update logical location kinds to accommodate XML and JSON paths. [oasis-tcs/sarif-spec#291](https://github.com/oasis-tcs/sarif-spec/issues/291)
* API NON-BRK: Define result taxonomies. [oasis-tcs/sarif-spec#314](https://github.com/oasis-tcs/sarif-spec/issues/314)
* API BRK: Remove `invocation.attachments`, now replaced by `run.tool.extensions`. [oasis-tcs/sarif-spec#327](https://github.com/oasis-tcs/sarif-spec/issues/327)
* API NON-BRK: Introduce new localization mechanism. [oasis-tcs/sarif-spec#338](https://github.com/oasis-tcs/sarif-spec/issues/338)
* API BRK: Remove `tool.language` and localization support. [oasis-tcs/sarif-spec#325](https://github.com/oasis-tcs/sarif-spec/issues/325)
* API NON-BRK: Add additional properties to toolComponent. [oasis-tcs/sarif-spec#336](https://github.com/oasis-tcs/sarif-spec/issues/336)
* API BRK: Rename `invocation.toolNotifications` and `invocation.configurationNotifications` to `toolExecutionNotifications` and `toolConfigurationNotifications`. [oasis-tcs/sarif-spec#330](https://github.com/oasis-tcs/sarif-spec/issues/330)
* API BRK: Add address property to a location object (and other nodes). [oasis-tcs/sarif-spec#302](https://github.com/oasis-tcs/sarif-spec/issues/302)
* API BRK: External property file related renames. [oasis-tcs/sarif-spec#335](https://github.com/oasis-tcs/sarif-spec/issues/335)

## **v2.0.0-csd.2.beta.2019.01-24.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.01-24.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.01-24.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.01-24.1)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.01-24.1))

* BUG: `region.charOffset` default value should be -1 (invalid value) rather than 0. Fixes an issue where `region.charLength` is > 0 but `region.charOffset` is absent (because its value of 0 was incorrectly elided due to being the default value).

## **v2.0.0-csd.2.beta.2019.01-24** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.01-24) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.01-24) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.01-24)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.01-24))

* BUG: SDK compatibility update for sample apps.
* BUG: Add Sarif.Multitool.exe.config file to multitool package to resolve "Could not load file or assembly `Newtonsoft.Json, Version=9.0.0.0`" exception on using validate command.
* API BRK: rename baselineState `existing` value to `unchanged`. Add new baselineState value `updated`. [oasis-tcs/sarif-spec#312](https://github.com/oasis-tcs/sarif-spec/issues/312)
* API BRK: unify result and notification failure levels (`note`, `warning`, `error`). Break out result evaluation state into `result.kind` property with values `pass`, `fail`, `open`, `review`, `notApplicable`. [oasis-tcs/sarif-spec#317](https://github.com/oasis-tcs/sarif-spec/issues/317)
* API BRK: remove IRule entirely, in favor of utilizing ReportingDescriptor base class.
* API BRK: define `toolComponent` object to persist tool data. The `tool.driver` component documents the standard driver metadata. `tool.extensions` is an array of `toolComponent` instances that describe extensions to the core analyzer. This change also deletes `tool.sarifLoggerVersion` (from the newly created `toolComponent` object) due to its lack of utility. Adds `result.extensionIndex` to allow results to be associated with a plug-in. `toolComponent` also added as a new file role. [oasis-tcs/sarif-spec#179](https://github.com/oasis-tcs/sarif-spec/issues/179)
* API BRK: Remove `run.resources` object. Rename `rule` object to `reportingDescriptor`. Move rule and notification reportingDescriptor objects to `tool.notificationDescriptors` and `tool.ruleDescriptors`. `resources.messageStrings` now located at `toolComponent.globalMessageStrings`. `rule.configuration` property now named `reportingDescriptor.defaultConfiguration`. `reportingConfiguration.defaultLevel` and `reportingConfiguration.defaultRank` simplified to `reportingConfiguration.level` and `reportingConfiguration.rank`. Actual runtime reportingConfiguration persisted to new array of reportingConfiguration objects at `invocation.reportingConfiguration`. [oasis-tcs/sarif-spec#311](https://github.com/oasis-tcs/sarif-spec/issues/311)
* API BRK: `run.richTextMessageMimeType` renamed to `run.markdownMessageMimeType`. `message.richText` renamed to `message.markdown`. `message.richMessageId` deleted. Create `multiformatMessageString` object, that holds plain text and markdown message format strings. `reportingDescriptor.messageStrings` is now a dictionary of these objects, keyed by message id. `reporting.Descriptor.richMessageStrings` dictionary is deleted. [oasis-tcs/sarif-spec#319](https://github.com/oasis-tcs/sarif-spec/issues/319)
* API BRK: `threadflowLocation.kind` is now `threadflowLocation.kinds`, an array of strings that categorize the thread flow location. [oasis-tcs/sarif-spec#202](https://github.com/oasis-tcs/sarif-spec/issues/202)
* API BRK: `file` renamed to `artifact`. `fileLocation` renamed to `artifactLocation`. `run.files` renamed to `run.artifacts`. [oasis-tcs/sarif-spec#309](https://github.com/oasis-tcs/sarif-spec/issues/309)

## **v2.0.0-csd.2.beta.2019-01-09** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019-01-09) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019-01-09) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019-01-09) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019-01-09)

* BUG: Result matching improvements in properties persistence.
* NEW: Fortify FPR converter improvements.
* API NON-BRK: Remove uniqueness requirement from `result.locations`.
* API NON-BRK: Add `run.newlineSequences` to schema. [oasis-tcs/sarif-spec#169](https://github.com/oasis-tcs/sarif-spec/issues/169)
* API NON-BRK: Add `rule.deprecatedIds` to schema. [oasis-tcs/sarif-spec#293](https://github.com/oasis-tcs/sarif-spec/issues/293)
* API NON-BRK: Add `versionControlDetails.mappedTo`. [oasis-tcs/sarif-spec#248](https://github.com/oasis-tcs/sarif-spec/issues/248)
* API NON-BRK: Add result.rank`. Add`ruleConfiguration.defaultRank`.
* API NON-BRK: Add `file.sourceLocation` and `region.sourceLanguage` to guide in snippet colorization. `run.defaultSourceLanguage` provides a default value. [oasis-tcs/sarif-spec#286](https://github.com/oasis-tcs/sarif-spec/issues/286)
* API NON-BRK: default values for `result.rank` and `ruleConfiguration.defaultRank` is now -1.0 (from 0.0). [oasis-tcs/sarif-spec#303](https://github.com/oasis-tcs/sarif-spec/issues/303)
* API BRK: Remove `run.architecture` [oasis-tcs/sarif-spec#262](https://github.com/oasis-tcs/sarif-spec/issues/262)
* API BRK: `result.message` is now a required property [oasis-tcs/sarif-spec#283](https://github.com/oasis-tcs/sarif-spec/issues/283)
* API BRK: Rename `tool.fileVersion` to `tool.dottedQuadFileVersion` [oasis-tcs/sarif-spec#274](https://github.com/oasis-tcs/sarif-spec/issues/274)
* API BRK: Remove `open` from valid rule default configuration levels. The transformer remaps this value to `note`. [oasis-tcs/sarif-spec#288](https://github.com/oasis-tcs/sarif-spec/issues/288)
* API BRK: `run.columnKind` default value is now `unicodeCodePoints`. The transformer will inject `utf16CodeUnits`, however, when this property is absent, as this value is a more appropriate default for the Windows platform. [#1160](https://github.com/Microsoft/sarif-sdk/pull/1160)
* API BRK: Make `run.logicalLocations` an array, not a dictionary. Add result.logicalLocationIndex to point to associated logical location.
* API BRK: `run.externalFiles` renamed to `run.externalPropertyFiles`, which is not a bundle of external property file objects. NOTE: no transformation will be provided for legacy versions of the external property files API.
* API BRK: rework `result.provenance` object, including moving result.conversionProvenance to `result.provenance.conversionSources`. NOTE: no transformation currently exists for this update.
* API BRK: Make `run.files` an array, not a dictionary. Add fileLocation.fileIndex to point to a file object associated with the location within `run.files`.
* API BRK: Make `resources.rules` an array, not a dictionary. Add result.ruleIndex to point to a rule object associated with the result within `resources.rules`.
* API BRK: `run.logicalLocations` now requires unique array elements. [oasis-tcs/sarif-spec#304](https://github.com/oasis-tcs/sarif-spec/issues/304)

## **v2.0.0-csd.2.beta.2018-10-10.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2018-10-10.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2018-10-10.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2018-10-10.2) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2018-10-10.2)

* BUG: Don`t emit v2 analysisTarget if there is no v1 resultFile.
* BUILD: Bring NuGet publishing scripts into conformance with new Microsoft requirements.

## **v2.0.0-csd.2.beta.2018-10-10.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2018-10-10.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2018-10-10.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2018-10-10.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2018-10-10.1)

* BUG: Persist region information associated with analysis target

## **v2.0.0-csd.2.beta.2018-10-10** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2018-10-10) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2018-10-10) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2018-10-10) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2018-10-10)

* NEW:Add --sarif-version command to driver (to transform SARIF output to v1 format)
* BUG: Drop erroneous persistence of redaction tokens as files objects.
* API NON-BRK: Add `result.occurrenceCount` (denotes # of occurrences of an identical results within an analysisRun)
* API NON-BRK: Add `run.externalFiles` object to schema. Sync generally to OASIS TC schema.
* API BRK: `originalUriBaseIds` is now a dictionary of file locations, not strings.
* API BRK: Suffix `invocation.startTime`, `invocation.endTime`, `file.lastModifiedTime` and `notification.time` with Utc (`startTimeUtc`, `endTimeUtc`, etc.).
* API BRK: `threadflowLocation.timestamp` renamed to `executionTimeUtc`.
* API BRK: `versionControlDetails.timestamp` renamed to `asOfTimeUtc`.
* API BRK: `versionControlDetails.uri` renamed to `repositoryUri`.
* API BRK: `versionControlDetails.tag` renamed to `revisionTag`
* API BRK: `exception.message` type converted from string to message object.
* API BRK: `file.hashes` is now a string/string dictionary, not an array of `hash` objects (the type for which is deleted)
* API BRK: `run.instanceGuid`, `run.correlationGuid`, `run.logicalId`, `run.description` combined into new `runAutomationDetails` object instance defined at `run.id`.
* API BRK: `run.automationLogicalId` subsumed by `run.aggregateIds`, an array of `runAutomationDetails` objects.
* API BRK: Remove `threadFlowLocation.step`
* API BRK: `invocation.workingDirectory` is now a FileLocation object (and not a URI expressed as a string)

## **v2.0.0-csd.1.0.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.1.0.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.1.0.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.1.0.2) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.1.0.2)

* BUG: In result matching algorithm, an empty or null previous log no longer causes a NullReferenceException.
* BUG: In result matching algorithm, duplicate data is no longer incorrectly detected across files. Also: changed a "NotImplementedException" to the correct "InvalidOperationException".

## **v2.0.0-csd.1.0.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.1.0.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.1.0.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.1.0.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.1.0.1)

* API BREAKING CHANGE: Fix weakly typed CreateNotification calls and make API more strongly typed
* API BREAKING CHANGE: Rename OptionallyEmittedData.ContextCodeSnippets to ContextRegionSnippets
* API BREAKING CHANGE: Eliminate result.ruleMessageId (in favor of result.message.messageId)

## **v2.0.0-csd.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.1)

* Convert object model to conform to SARIF v2 CSD.1 draft specification
* Distinguish textual vs. binary file persistence in rewrite option (and allow for both in multitool rewrite verb)
* NOTE: the change above introduces a command-line breaking change. --persist-file-contents is now renamed to --insert
* Add ComprehensiveRegionProperties, RegionSnippets and ContextCodeSnippets as possible qualifier to --insert option
* Provide SARIF v1.0 object model and v1 <-> v2 transformation API

## **v1.7.5** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/1.7.5) | [Driver](https://www.nuget.org/packages/Sarif.Driver/1.7.5) | [Converters](https://www.nuget.org/packages/Sarif.Converters/1.7.5) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/1.7.5)

* Disabling skimmers text fix
* Fix a serialization bug with strings in a PropertyBag (not correctly escaped after a reserializing the data structure).
* Multitool improvements--added "rebaseUri" and "absoluteUri" tasks, which will either make the URIs in a SARIF log relative to some base URI, or take base URIs stored in SARIF and make the URIs absolute again.
* Added a "processing pipeline" model to the SARIF SDK in order to allow easy chaining of operations on SARIF logs (like making all URIs relative/absolute).

## **v1.7.4** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/1.7.4) | [Driver](https://www.nuget.org/packages/Sarif.Driver/1.7.4) | [Converters](https://www.nuget.org/packages/Sarif.Converters/1.7.4) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/1.7.4)

* Platform Specific Tooling Text Fix
* Skimmers can now be disabled via the configuration file
* The Driver will now pull configuration from a default location to allow for easier re-packaging of tools with custom configurations

## **v1.7.3** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/1.7.3) | [Driver](https://www.nuget.org/packages/Sarif.Driver/1.7.3) | [Converters](https://www.nuget.org/packages/Sarif.Converters/1.7.3) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/1.7.3)

* Make SupportedPlatform a first class concept for skimmers
* Rename --pretty argument to --pretty-print

## **v1.7.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/1.7.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/1.7.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/1.7.2) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/1.7.2)

* Update Multitool nuget package build
* Enable "pretty print" .sarif formatting via --pretty argument
* Code sign 3rd party dependency assemblies (CommandLineParser, CsvHelper, Newtonsoft.Json)
* Remove -beta flag from Driver and Multitool packages

## **v1.7.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/1.7.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/1.7.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/1.7.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/1.7.1)

* Update nuget package build

## **v1.7.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/1.7.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/1.7.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/1.7.0) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/1.7.0)

* Security and accessibility clean-up
* TSLint converter fixes
* Provide .NET core version
* VSIX improvements (including auto-expansion of file contents persisted to SARIF logs)

## **v1.6.0** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.6.0) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.6.0)

* Enable persistence of base64-encoded file contents via SarifLogger.
* Rename AnalyzeOptions.ComputeTargetsHash to ComputeFileHashes
* Fix bug in Semmle conversion (crash on embedded file:// scheme links)

## **v1.5.47** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.47) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.47)

* Enable converter plugins
* Adjust RuntimeConditions enum so that `command line parse` error is 0x1.

## **v1.5.46** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.46) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.46)

* Resolved crash deserializing empty property bags

## **v1.5.45** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.45) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.45)

* Track RuntimeConditions.OneOrMoreWarnings|ErrorsFired in RuleUtilities.BuildResult

## **v1.5.44** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.44) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.44)

* Update default AnalyzeCommandBase behavior to utilize rich return code, if specified.

## **v1.5.43** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.43) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.43)

* Expose EntryPointUtilities helpers as public

## **v1.5.42** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.42) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.42)

* Add EntryPointUtilities class that provides response file injection assistance
* Rich return code support

## **v1.5.41** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.41) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.41)

* Control invocation property logging

## **v1.5.40** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.40) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.40)

* Add JSON settings persistence
* Populate context objects from configuration file argument

## **v1.5.39** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.39) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.39)

* Loosen requirement to explicitly provide --config argument for default configuration
* Convert Semmle embedded links to related locations
* Add File/Open of Semmle CSV to VS add-ing
* Eliminate redundant output of notifications
* Update FileSpecifier to resolve patternts such as File* properly

## **v1.5.38** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.38) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.38)

* Preliminary Semmle converter

## **v1.5.37** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.37) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.37)

* Further refinements to output on analysis completion.

## **v1.5.36** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.36) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.36)

* Provide better reporting for non-fatal messages.

## **v1.5.35** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.35) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.35)

* Add `configuration` member to rule objects

## **v1.5.34** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.34) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.34)

* Update schema for `annotations` object required properties

## **v1.5.33** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.33) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.33)

* Resolve crash generating `not applicable` messages

## **v1.5.32** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.32) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.32)

* Add `annotations` member to annotatedCodeLocation object.
* Rename annotatedCodeLocation `variables` member to `state`
* Rename annotatedCodeLocation `parameters` member to `values`

## **v1.5.31** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.31) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.31)

* API BREAKING CHANGE: RuleUtilities.BuildResult no longer automatically prepends the target file path to the list of FormattedRuleMessage.Arguments array in the Result object being built.

## **v1.5.30** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.30) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.30)

* Add static helper method `AnalyzeCommandBase.LogToolNotification`.

## **v1.5.29** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.29) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.29)

* Add `--quiet` option to suppress console output.

## **v1.5.28** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.28) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.28)

* API BREAKING change: rename PropertyBagDictionary to PropertiesDictionary
* Add `functionReturn` to annotatedCodeLocation.kind
* Remove `source`, `sink` and `sanitizer` from annotatedCodeLocation.kind
* Add `taint` enum to annotatedCodeLocation with values `source`, `sink` and `sanitizer`
* Add `parameters` and `variables` members to annotatedCodeLocation
* Rename annotatedCodeLocation.callee member to `target`
* Rename annotatedCodeLocation.calleeKey member to `targetKey`

## **v1.5.27** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.27) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.27)

* Ship checked in CommandLine.dll in order to allow this `beta` NuGet component to ship in Driver non-beta release

## **v1.5.26** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.26) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.26)  

* API BREAKING change on SarifLogger to explicitly specify hash computation for all files  
* SarifLogger now automatically persists file data for all URIs through format  
* Add run.stableId, a consistent run-over-run log identifier  
* Add annotatedCodeLocation.callee and annotatedCodeLocation.calleeKey for annotation call sites  
* Add invocation.responseFiles to capture response file contents
* Drop .NET framework dependency to 4.5 (from 4.5.1)

## **v1.5.25** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.25) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.25)

* NOTE: NON-BETA RELEASE
* Add a converter for Static Driver Verifier trace files
* Add SuppressedExternally to SuppressionStates enum

## **v1.5.24-beta** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.24-beta) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.24-beta)

* Permit annotatedCodeLocation.id to be a numeric value (in addition to a string)

## **v1.5.23-beta** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.23-beta) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.23-beta)

* Rename `codeSnippet` to `snippet`
* Remove requirement to specify `description` on code fixes
* Add `architecture` back to `run` object

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

## **v1.5.21-beta** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.21-beta) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.21-beta)

* Persist mime-type for files in SarifLogger
* Remove stack persistence for configuration notification exceptions
* Reclassify `could not parse target` as a configuration notification
* Fix diffing visitor to diff using value type semantics rather than by reference equality

## **v1.5.20-beta** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.20-beta) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.20-beta)

* Rename Microsoft.CodeAnalysis.Sarif.Sdk namespace to Microsoft.CodeAnalysis.Sarif
* Rename Microsoft.CodeAnalysis.Sarif.Driver namespace to Microsoft.CodeAnalysis.Driver
* Eliminate some tool version details. Add SarifLogger version as tool property

## **v1.5.19-beta** [Driver](https://www.nuget.org/packages/Sarif.Driver/1.5.19-beta) | [SDK](https://www.nuget.org/packages/Sarif.Sdk/1.5.19-beta)

* Moved SarifLogger and its dependencies from driver to SDK package
* Include this file and JSON schema in packages
