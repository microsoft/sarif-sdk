# SARIF Package Release History (SDK, Driver, Converters, and Multitool)

## **v2.3.3** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.3) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.3) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.3) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.3)
* FEATURE: Improve `SarifSdkSample` application: use `uriBaseIds`.
* FEATURE: Add additional checks to SARIF analysis rule `SARIF2004.OptimizeFileSize`.
* FEATURE: Introduce new SARIF analysis rule `SARIF2016.FileUrisShouldBeRelative`.
* BUGFIX: If you created a URI from an absolute file path (for example, `C:\test\file.c`), then it would be serialized with that exact string, which is not a valid URI. This is now fixed. [#2001](https://github.com/microsoft/sarif-sdk/issues/2001)

## **v2.3.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.2) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.2)
* FEATURE: The `Sarif.Multitool` command line verbs are now exposed programmatically. For example, the `validate` verb is exposed through the classes `ValidateCommand` and `ValidateOptions`.

## **v2.3.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.1)
* FEATURE: Revised and improved validation rules in `Sarif.Multitool`.
* FEATURE: Properties serialization performance improved (~20% faster load when Results use Properties).
* FEATURE: Allow result messages to be truncated for display. [#1915](https://github.com/microsoft/sarif-sdk/issues/1915)
* BUGFIX: Rebase URI command now honors `--insert` and `--remove` arguments for injecting or eliding optional data (such as region snippets).
* BUGFIX: Ensure all DateTimes on object model are using DateTimeConverter consistently.
* BUGFIX: Fix DateTime roundtripping in properties collections to follow normal DateTime output format.

## **v2.3.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.3.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.3.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.3.0) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.3.0)
* BUGFIX: `ResultLogJsonWriter` now creates an empty `results` array if there are no results, rather than leaving `results` as `null`. [#1821](https://github.com/microsoft/sarif-sdk/issues/1821)
* BUGFIX: In validation rules, `shortDescription` is now calculated by `GetFirstSentence` method, fixing a bug in sentence breaking. [#1887](https://github.com/microsoft/sarif-sdk/issues/1887)
* BUGFIX: `WorkItemFiler` now logs correctly the details for `LogMetricsForProcessedModel` method [#1896](https://github.com/microsoft/sarif-sdk/issues/1896)
* FEATURE: Add validation rule `SARIF1019`, which requires every result to have at least one of `result.ruleId` and `result.rule.id`. If both are present, they must be equal. [#1880](https://github.com/microsoft/sarif-sdk/issues/1880)
* FEATURE: Add validation rule `SARIF1020`, which requires that the $schema property should be present, and must refer to the final version of the SARIF 2.1.0 schema. [#1890](https://github.com/microsoft/sarif-sdk/issues/1890)
* FEATURE: Expose `Run.MergeResultsFrom(Run)` to merge Results from multiple Runs using code from result matching algorithm.
* BREAKING: Rename `RemapIndicesVisitor` to `RunMergingVisitor` and redesign to control how much merging occurs internally.

## **v2.2.5** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.2.5) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.2.5) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.2.5) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.2.5)
* BUGFIX: Fix SDK doubling Uris with certain escaped characters (ex: '-' and '_') on every Load/Save cycle (cause: https://github.com/dotnet/runtime/issues/36288)

## **v2.2.4** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.2.4) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.2.4) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.2.4) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.2.4)
* BUGFIX: Validation rule SARIF1018 was not checking for a trailing slash on `uri` properties in `originalUriBaseIds` if `uriBaseId` was present.
* BUGFIX: Build Sarif.Multitool NPM package non-trimmed to avoid more assembly load problems.
* FEATURE: DeferredList will cache last item returned and won't throw if same instance written. (SarifRewritingVisitor + Deferred OM usable)

## **v2.2.3** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.2.3) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.2.3) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.2.3) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.2.3)
* FEATURE: Introduce `SarifConstants.SarifFileExtension` with value `".sarif"`.
* FEATURE: In validation rule SARIF1018, require `uri` values in `originalUriBaseIds` to end with a slash, per the SARIF spec.
* BUGFIX: Result.GetRule will look up by RuleId if RuleIndex not present.
* BUGFIX: Baselining will properly persist Run.Tool.Driver.Rules if Results reference by RuleId.
* BUGFIX: DeferredOM will properly load files with a BOM. (LineMappingStreamReader fix)
* BUGFIX: Remove CsvHelper dependency to avoid assembly load problem in Sarif.Multitool NPM package.

## **v2.2.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.2.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.2.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.2.2) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.2.2)
* BUGFIX: `dotnet tool install` command for Multitool now produces a working installation rather than reporting missing `Sarif.Converters` binary.
* BUGFIX: Result.GetRule will look up by RuleId if RuleIndex not present.
* BUGFIX: Baselining will properly persist Run.Tool.Driver.Rules if Results reference by RuleId.
* BUGFIX: DeferredOM will properly load files with a BOM. (LineMappingStreamReader fix)

## **v2.2.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.2.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.2.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.2.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.2.1)
* FEATURE: Multitool `remove` option now supports `Guids` value to remove `Result.Guid`.
* FEATURE: Significant Baselining algorithm improvements: dynamic `partialFingerprint` trust, location-specific unique what property matching, 'nearby' matching, correct omitted `Region` property handling, correct `ReportingDescriptor.DeprecatedIds` handling.
* DEPENDENCY BREAKING: SARIF now requires Newtonsoft.JSON 10.0.3 (rather than 9.0.x).

## **v2.2.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.2.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.2.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.2.0) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.2.0)
* PACKAGE BREAKING: Update tool directory to netstandard2.1, to reflect use of that version of .NET Core.
* FEATURE: Multitool `rewrite` command performance when populating regions and snippets is greatly improved.
* FEATURE: Multitool `insert` option now supports `Guids` value to populate `Result.Guid`.
* API + SCHEMA BREAKING: Fix typo in schema: suppression.state should be suppression.status according to the spec. [#1785](https://github.com/microsoft/sarif-sdk/issues/1785)
* BUGFIX: Multitool `rewrite` no longer throws when it encounters an invalid value (such as -1) for a region property.
* BUGFIX: ESLint SARIF formatter no longer produces invalid SARIF when given an ESLint message with no rule id. It is treated as a `toolConfigurationNotification`. [#1791](https://github.com/microsoft/sarif-sdk/issues/1791)
* BUGFIX: Resolve crash on converting PREfast log files with non-null but empty help URLs.

## **v2.1.25** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.25) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.25) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.25) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.25)
* FEATURE: The baseliner (available through the Multitool's `match-results-forward` command) now populates `result.provenance.firstDetectionTimeUtc` so you can now track the age of each issue. [#1737](https://github.com/microsoft/sarif-sdk/issues/1737)

## **v2.1.24** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.24) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.24) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.24) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.24)
* FEATURE: Introduce API to partition log files by arbitrary criteria (method `SarifPartitioner.Partition` and class `PartitioningVisitor`).
* BUGFIX: `Tool.CreateFromAssembly` now properly handles file versions that contain extra characters after the "dotted quad" string. [#1728](https://github.com/microsoft/sarif-sdk/issues/1728)

## **v2.1.23** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.23) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.23) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.23) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.23)
* API BREAKING: Remove 'Errors.LogExceptionLoadingPdb' helper (as not relevant to core SDK).
* FEATURE: Allow emitting non-failure tool notifications as debug/informational messages.
* FEATURE: `SarifLogger` now populates `tool.driver`'s `organization` and `product` properties instead of adding `"Company"` and `"ProductName"` to `tool.driver'`s property bag. [#1716](https://github.com/microsoft/sarif-sdk/issues/1716)
* FEATURE: Add `closeWriterOnDispose` argument (with a default of 'true') that indicates whether SarifLogger writers are closed during its Dispose() method. Providing a value of `false` to this argument allows SarifLogger to work against a stream that can subsequently be reused (for example, to deserialize the logged content back to a `SarifLog` instance).
* FEATURE: Update PREfast converter to render optional suppression data.
* BUGFIX: Update PREfast converter to handle paths with no trailing slash.
* BUGFIX: Baselining now matches the first and last Result per URI as an additional pass.

## **v2.1.22** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.22) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.22) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.22) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.22)
* BUGFIX: Fix bug in validation rule `EndTimeMustNotBeBeforeStartTime`, which threw if `invocation.startTimeUtc` was present but `endTimeUtc` was absent.

## **v2.1.21** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.21) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.21) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.21) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.21)
* FEATURE: Provide an API `SarifPartitioner.Filter` that selects results according to a predicate, and filters `run.artifacts` to only those artifacts used by the included results.

## **v2.1.20** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.20) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.20) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.20) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.20)
* FEATURE: Added Stream-based SarifLog.Load and Save overloads
* FEATURE: Enhanced property bag serialization unit testing. [#1673](https://github.com/microsoft/sarif-sdk/issues/1673)
* BUGFIX: Fix packaging warning NU5048 during build. [#1687](https://github.com/microsoft/sarif-sdk/issues/1687)
* BUGFIX: SarifLogger.Optimized could not be set from the command line. [#1695](https://github.com/microsoft/sarif-sdk/issues/1695)
* BUGFIX: Result Matching now omits previously Absent results.
* BUGFIX: Result Matching properly compares results from the same RuleID when multiple Rules match the same source line.
* BUGFIX: Result Matching works when a result moves and has the line number in the message.
* BUGFIX: Result Matching always assigns Result.CorrelationGuid and Result.Guid.
* BUGFIX: Null hardening in Result Matching
* BUGFIX: Console logger now outputs file location, if available, when writing notifications.

## **v2.1.19** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.19) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.19) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.19) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.19)
* Sort driver skimmers by rule id + name during analysis, in order to improve deterministic ordering of log file data.
* API BREAKING: Convert various public SARIF Driver framework API to prefer abstract ISet<string> type over HashSet<string>.
* API BREAKING: Remove helper method `SarifUtilities.DeserializeObject` introduced in 2.1.15 to fix. [#1577](https://github.com/microsoft/sarif-sdk/issues/1577)
Now that an underlying bug in `PropertyBagConverter` has been fixed, there is no need to work around it with this helper method. `JsonConvert.DeserializeObject` works fine.
* FEATURE: Expanding Sarif SDK query mode to support Result.Uri, string StartsWith/EndsWith/Contains.
* FEATURE: Adding Result.Run and a populating method, so that methods which need the Run context for a given Result have an integrated way to retrieve it.

## **v2.1.17** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.17) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.17) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.17) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.17)
* API NON-BREAKING: emit all core object model members as 'virtual'.
* FEATURE: Introduce SarifConsolidator to shrink large log files. [#1675](https://github.com/microsoft/sarif-sdk/issues/1675)
* BUGFIX: Analysis rule SARIF1017 incorrectly rejected index-valued properties that referred to taxonomies. [#1678](https://github.com/microsoft/sarif-sdk/issues/1678)
* BUGFIX: `match-results-forward-command` dropped log contents and mishandled `rules` array. [#1684](https://github.com/microsoft/sarif-sdk/issues/1684)

## **v2.1.16** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.16) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.16) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.16) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.16)
* BUGFIX, BREAKING: In the Multitool `page` command, the default for `--force` was `true` and it could not be changed. [#1630](https://github.com/microsoft/sarif-sdk/issues/1630)
* BUGFIX: The Multitool `match-results-forward` command failed if results included logical locations. [#1656](https://github.com/microsoft/sarif-sdk/issues/1656)
* BUGFIX: `SarifLogger(ReportingDescriptor rule, Result result)` failed if it tried to log a result whose `ruleId` was a sub-rule; for example, `rule.Id == "TEST0001"` but `result.ruleId == "TEST0001/1"`. [#1668](https://github.com/microsoft/sarif-sdk/issues/1668)
* FEATURE: Implement results and notifications caching when `--hashes` is specified on the SARIF driver command line.

## **v2.1.15** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.15) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.15) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.15) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.15)
* BUGFIX: Validation rule `SARIF1015` incorrectly required `originalUriBaseIds` to be contain URIs. [#1485](https://github.com/microsoft/sarif-sdk/issues/1485)
* BUGFIX: Persist Fortify rule metadata properties. [#1490](https://github.com/microsoft/sarif-sdk/issues/1490)
* BUGFIX: Multitool transform mishandled dottedQuadFileVersion. [#1532](https://github.com/microsoft/sarif-sdk/issues/1532)
* BUGFIX: Restore missing FxCop converter unit test. [#1575](https://github.com/microsoft/sarif-sdk/issues/1575)
* BUGFIX: Multitool transform mishandled date/time values in property bags. [#1577](https://github.com/microsoft/sarif-sdk/issues/1577)
* BUGFIX: Multitool transform could not upgrade SARIF files from the sarif-2.1.0-rtm.1 schema. [#1584](https://github.com/microsoft/sarif-sdk/issues/1584)
* BUGFIX: Multitool merge command produced invalid SARIF if there were 0 input files. [#1592](https://github.com/microsoft/sarif-sdk/issues/1592)
* BUGFIX: FortifyFpr converter produced invalid SARIF. [#1593](https://github.com/microsoft/sarif-sdk/issues/1593)
* BUGFIX: FxCop converter produced empty `result.message` objects. [#1594](https://github.com/microsoft/sarif-sdk/issues/1594)
* BUGFIX: Some Multitool commands required --force even if --inline was specified. [#1642](https://github.com/microsoft/sarif-sdk/issues/1642)
* FEATURE: Add validation rule to ensure correctness of `originalUriBaseIds` entries. [#1485](https://github.com/microsoft/sarif-sdk/issues/1485)
* FEATURE: Improve presentation of option validation messages from the Multitool `page` command. [#1629](https://github.com/microsoft/sarif-sdk/issues/1629)

## **v2.1.14** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.14) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.14) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.14) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.14)
* BUGFIX: FxCop converter produced logicalLocation.index but did not produce the run.logicalLocations array. [#1571](https://github.com/microsoft/sarif-sdk/issues/1571)
* BUGFIX: Include Sarif.WorkItemFiling.dll in the Sarif.Multitool NuGet package. [#1636](https://github.com/microsoft/sarif-sdk/issues/1636)
* FEATURE: Add validation rule to ensure that all array-index-valued properties are consistent with their respective arrays.

## **v2.1.13** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.13) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.13) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.13) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.13)
* BUGFIX: Respect the --force option in Sarif.Multitool rather than overwriting the output file. [#1340](https://github.com/microsoft/sarif-sdk/issues/1340)
* BUGFIX: Accept URI-valued properties whose value is the empty string. [#1632](https://github.com/microsoft/sarif-sdk/issues/1632)

## **v2.1.12** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.12) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.12) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.12) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.12)
* BUGFIX: Improve handling of `null` values in property bags. [#1581](https://github.com/microsoft/sarif-sdk/issues/1581)

## **v2.1.11** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.11) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.11) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.11) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.11)
* BUGFIX: Result matching should prefer the suppression info from the current run. [#1600](https://github.com/microsoft/sarif-sdk/issues/1600)

## **v2.1.10** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.10) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.10) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.10) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.10)
* BUGFIX: Resolve a performance issue in web request parsing code. https://github.com/microsoft/sarif-sdk/issues/1608

## **v2.1.9** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.9) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.9) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.9) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.9)
* FEATURE: add --remove switch to eliminate certain properties (currently timestamps only) from log file output.
* BUGFIX: remove verbose 'Analyzing file..' reporting for drivers.

## **v2.1.8** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.8) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.8) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.8) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.8)
* BUGFIX: Add missing `"additionalProperties": false` constraints to schema; add missing object descriptions and improve other object descriptions in schema; update schema version to -rtm.4.

## **v2.1.7** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.7) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.7) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.7) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.7)
* BUGFIX: Multitool rewrite InsertOptionalData operations fail if a result object references `run.artifacts` using the `index` property.
* BUGFIX: The `SarifCurrentToVersionOneVisitor` was not translating v2 `result.partialFingerprints` to v1 `result.toolFingerprintContribution`. [#1556](https://github.com/microsoft/sarif-sdk/issues/1556)
* BUGFIX: The `SarifCurrentToVersionOneVisitor` was dropping `run.id` and emitting an empty `run.stableId`. [#1557](https://github.com/microsoft/sarif-sdk/issues/1557)

## **v2.1.6** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.6) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.6) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.6) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.6)
* BUGFIX: Fortify FPR converter does not populate originalUriBaseIds if the source is a drive letter (e.g. C:)
* BUGFIX: Multitool rebaseUri command throws null reference exception if results reference run.artifacts using the index property.
* BUGFIX: Pre-release transformer does not upgrade schema uri if input version is higher than rtm.1.

## **v2.1.5** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.5) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.5) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.5) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.5)
* Change schemas back to draft-04 to reenable Intellisense in the Visual Studio JSON editor.

## **v2.1.4** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.4) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.4) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.4) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.4)
* BUGFIX: Fix bugs related to parsing the query portion of a URI, and to the parsing of header strings.
* API NON-BREAKING: Introduce `WebRequest.TryParse` and `WebResponse.TryParse` to accompany existing `Parse` methods.

## **v2.1.3** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.3) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.3) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.3) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.3)
* Change schema uri to secure (https) instance.
* BUGFIX: Fix tranformer bug where schema id would not be updated if no other transformation occurred.
* BUGFIX: `ThreadFlowLocation.Kind` value is getting lost during pre-release transformation. [#1502](https://github.com/microsoft/sarif-sdk/issues/1502)
* BUGFIX: `Location.LogicalLocation` convenience setter mishandles null. [#1514](https://github.com/microsoft/sarif-sdk/issues/1514)
* BUGFIX: Upgrade schemas to latest version (remove `draft-04` from `$schema` property and change `id` to `$id`). This is necessary because the schemas use the `uri-reference` format, which was not defined in draft-04. [#1521](https://github.com/microsoft/sarif-sdk/issues/1521)
* API BREAKING: The `Init` methods in the Autogenerated SARIF object model classes are now `protected virtual`. This enables derived classes to add additional properties without having to copy the entire code of the `Init` method.
* BUGFIX: Transformation from SARIF 1.0 to 2.x throws `ArgumentOutOfRangeException`, if `result.locations` is an empty array. [#1526](https://github.com/microsoft/sarif-sdk/issues/1526)
* BUGFIX: Add `Result.Level` (and remove `Result.Rank`) for Fortify Converter based on MicroFocus feedback.
* BUGFIX: Invocation constructor should set `executionSuccessful` to true by default.
* BUGFIX: Contrast security converter now populates `ThreadFlowLocation.Location`. [#1530](https://github.com/microsoft/sarif-sdk/issues/1530)
* BUGFIX: Contrast Security converter no longer emits incomplete `Artifact` objects. [#1529](https://github.com/microsoft/sarif-sdk/issues/1529)
* BUGFIX: Fix crashing bugs and logic flaws in `ArtifactLocation.TryReconstructAbsoluteUri`.
* FEATURE: Provide a SARIF converter for Visual Studio log files.
* FEATURE: Extend the `PrereleaseCompatibilityTransformer` to handle SARIF v1 files.
* API NON-BREAKING: Introduce `WebRequest.Parse` and `WebResponse.Parse` to parse web traffic strings into SARIF `WebRequest` and `WebResponse` objects.
* API NON-BREAKING: Introduce `PropertyBagHolder.{Try}GetSerializedPropertyInfo`, a safe way of retrieving a property whose type is unknown.

## **v2.1.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.2) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.2)
* API BREAKING: Change location.logicalLocation to logicalLocations array. [oasis-tcs/sarif-spec#414](https://github.com/oasis-tcs/sarif-spec/issues/414)

## **v2.1.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.1)
* BUGFIX: Multitool crashes on launch: Can't find CommandLine.dll. [#1487](https://github.com/microsoft/sarif-sdk/issues/1487)

## **v2.1.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.0) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.0)
* API NON-BREAKING: `PhysicalLocation.id` property is getting lost during 2.1.0 pre-release transformation. [#1479](https://github.com/microsoft/sarif-sdk/issues/1479)
* Add support for converting TSLint logs to SARIF
* Add support for converting Pylint logs to SARIF

## **v2.1.0-rtm.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.0-rtm.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.0-rtm.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.0-rtm.0)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.0-rtm.0)
* API BREAKING: OneOf `graphTraversal.runGraphIndex` and `graphTraversal.resultGraphIndex` is required.
* API NON-BREAKING: Add address.kind well-known values "instruction" and "data". [oasis-tcs/sarif-spec#397](https://github.com/oasis-tcs/sarif-spec/issues/397)
* API BREAKING: Rename `invocation.toolExecutionSuccessful` to `invocation.executionSuccessful`. [oasis-tcs/sarif-spec#399](https://github.com/oasis-tcs/sarif-spec/issues/399)
* API BREAKING: Add regex patterns for guid and language in schema.
* API NON-BREAKING: Add `run.specialLocations` in schema. [oasis-tcs/sarif-spec#396](https://github.com/oasis-tcs/sarif-spec/issues/396)
* API BREAKING: Improve `address` object design. [oasis-tcs/sarif-spec#401](https://github.com/oasis-tcs/sarif-spec/issues/401)

## **v2.1.0-beta.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.0-beta.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.0-beta.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.0-beta.2)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.0-beta.2)
* API NON-BREAKING: Change `request.target` type to string. [oasis-tcs/sarif-spec#362](https://github.com/oasis-tcs/sarif-spec/issues/362)
* API BREAKING: anyOf `physicalLocation.artifactLocation` and `physicalLocation.address` is required. [oasis-tcs/sarif-spec#353](https://github.com/oasis-tcs/sarif-spec/issues/353)
* API BREAKING: Rename `run.defaultFileEncoding` to `run.defaultEncoding`.
* API NON-BREAKING: Add `threadFlowLocation.taxa`. [oasis-tcs/sarif-spec#381](https://github.com/oasis-tcs/sarif-spec/issues/381)
* API BREAKING: anyOf `message.id` and `message.text` is required.
* API NON-BREAKING: Add `request.noResponseReceived` and `request.failureReason`. [oasis-tcs/sarif-spec#378](https://github.com/oasis-tcs/sarif-spec/issues/378)
* API BREAKING: anyOf `externalPropertyFileReference.guid` and `externalPropertyFileReference.location` is required.
* API BREAKING: `artifact.length` should have `default: -1, minimum: -1` values.
* API BREAKING: Rename `fix.changes` to `fix.artifactChanges`.
* API BREAKING: Each redaction token in an originalUriBaseId represents a unique location. [oasis-tcs/sarif-spec#377](https://github.com/oasis-tcs/sarif-spec/issues/377)
* API BREAKING: Rename file related enums in `artifact.roles`.
* API BREAKING: anyOf `artifactLocation.uri` and `artifactLocation.index` is required.
* API BREAKING: `multiformatMessageString.text` is required.
* API BREAKING: `inlineExternalProperties` array must have unique items.
* API BREAKING: `run.externalPropertyFileReferences`, update unique flag and minItems on every item according to spec.
* API BREAKING: `run.markdownMessageMimeType` should be removed from schema.
* API BREAKING: `externalPropertyFileReference.itemCount` should have a minimum value of 1.
* API NON-BREAKING: Add `toolComponent.informationUri` property.
* API NON-BREAKING: `toolComponent.isComprehensive` default value should be false.
* API BREAKING: `artifact.offset` minimum value allowed should be 0.
* API NON-BREAKING: Add `directory` enum value in `artifact.roles`.
* API BREAKING: `result.suppressions` array items should be unique and default to null.
* API NON-BREAKING: Add `suppression.guid` in schema.
* API BREAKING: `graph.id` should be removed from schema.
* API BREAKING: `edgeTraversal.stepOverEdgeCount` minimum should be 0.
* API BREAKING: `threadFlowLocation.nestingLevel` minimum should be 0.
* API BREAKING: `threadFlowLocation.importance` should default to `important`.
* API BREAKING: `request.index` should have default: -1, minimum: -1.
* API BREAKING: `response.index` should have default: -1, minimum: -1.
* API NON-BREAKING: `externalProperties.version` is not a required property if it is not root element.
* API NON-BREAKING: Add artifact roles for configuration files. [oasis-tcs/sarif-spec#372](https://github.com/oasis-tcs/sarif-spec/issues/372)
* API NON-BREAKING: Add suppression.justification. [oasis-tcs/sarif-spec#373](https://github.com/oasis-tcs/sarif-spec/issues/373)
* API NON-BREAKING: Associate descriptor metadata with thread flow locations. [oasis-tcs/sarif-spec#381](https://github.com/oasis-tcs/sarif-spec/issues/381)
* API BREAKING: Move `location.physicalLocation.id` to `location.id`. [oasis-tcs/sarif-spec#375](https://github.com/oasis-tcs/sarif-spec/issues/375)
* API BREAKING: `result.stacks` array should have unique items.
* API BREAKING: `result.relatedLocations` array should have unique items.
* API BREAKING: Separate `suppression` `status` from `kind`. [oasis-tcs/sarif-spec#371](https://github.com/oasis-tcs/sarif-spec/issues/371)
* API BREAKING: `reportingDescriptorReference` requires anyOf (`index`, `guid`, `id`).
* API BREAKING: Rename `request` object and related properties to `webRequest`.
* API BREAKING: Rename `response` object and related properties to `webResponse`.
* API NON-BREAKING: Add `locationRelationship` object. [oasis-tcs/sarif-spec#375](https://github.com/oasis-tcs/sarif-spec/issues/375)
* API BREAKING: `externalPropertyFileReference.itemCount` can be 0 and defaults to minimum: -1, default: -1.
* API BREAKING: `threadFlowLocation.executionOrder` can be 0 and defaults to -1, so minimum: -1, default: -1
* API BREAKING: Rename artifact role `traceFile` to `tracedFile`.
* API NON-BREAKING: Add artifact role `debugOutputFile`.
* API NON-BREAKING: Add `value` to `threadFlowLocation.kinds`.
* API NON-BREAKING: Add a new value to `result.kind`: `informational`.
* API NON-BREAKING: add `address.kind`values `function` and `page`.
* API NON-BREAKING: `run.columnKind` has no default value.
* API NON-BREAKING: In the `reportingDescriptorRelationship` object, add a property `description` of type `message`, optional.
* API NON-BREAKING: In the `locationRelationship` object, add a property `description` of type `message`, optional.
* API BREAKING: `region.byteOffset` should have default: -1, minimum: -1.
* API BREAKING: Change `notification.physicalLocation` of type `physicalLocation` to `notification.locations` of type `locations`.

## **v2.1.0-beta.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.0-beta.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.0-beta.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.0-beta.1)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.0-beta.1))
* API BREAKING: Change `request.uri` to `request.target`. [oasis-tcs/sarif-spec#362](https://github.com/oasis-tcs/sarif-spec/issues/362)

## **v2.1.0-beta.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.1.0-beta.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.1.0-beta.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.1.0-beta.0)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.1.0-beta.0))
* API BREAKING: All SARIF state dictionaries now contains multiformat strings as values. [oasis-tcs/sarif-spec#361](https://github.com/oasis-tcs/sarif-spec/issues/361)
* API NON-BREAKING: Define `request` and `response` objects. [oasis-tcs/sarif-spec#362](https://github.com/oasis-tcs/sarif-spec/issues/362)

## **v2.0.0-csd.2.beta.2019.04-03.3** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.04-03.3) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.04-03.3) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.04-03.3)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.04-03.3))
* API BREAKING: Rename `reportingDescriptor.descriptor` to `reportingDescriptor.target`. [oasis-tcs/sarif-spec#356](https://github.com/oasis-tcs/sarif-spec/issues/356)
* API NON-BREAKING: Remove `canPrecedeOrFollow` from relationship kind list. [oasis-tcs/sarif-spec#356](https://github.com/oasis-tcs/sarif-spec/issues/356)

## **v2.0.0-csd.2.beta.2019.04-03.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.04-03.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.04-03.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.04-03.2)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.04-03.2))
* API NON-BREAKING: Add `module` to `address.kind`. [oasis-tcs/sarif-spec#353](https://github.com/oasis-tcs/sarif-spec/issues/353)
* API BREAKING: `address.baseAddress` & `address.offset` to int. [oasis-tcs/sarif-spec#353](https://github.com/oasis-tcs/sarif-spec/issues/353)
* API BREAKING: Update how reporting descriptors describe their taxonomic relationships. [oasis-tcs/sarif-spec#356](https://github.com/oasis-tcs/sarif-spec/issues/356)
* API NON-BREAKING: Add `initialState` and `immutableState` properties to thread flow object. Add `immutableState` to `graphTraversal` object. [oasis-tcs/sarif-spec#168](https://github.com/oasis-tcs/sarif-spec/issues/168)

## **v2.0.0-csd.2.beta.2019.04-03.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.04-03.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.04-03.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.04-03.1)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.04-03.1))
* API BREAKING: Rename `message.messageId` property to `message.id`. https://github.com/oasis-tcs/sarif-spec/issues/352

## **v2.0.0-csd.2.beta.2019.04-03.0** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.04-03.0) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.04-03.0) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.04-03.0)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.04-03.0))
* API NON-BREAKING: Introduce new localization mechanism (post ballot changes). [oasis-tcs/sarif-spec#338](https://github.com/oasis-tcs/sarif-spec/issues/338)
* API BREAKING: Add `address` property to a `location` object (post ballot changes). [oasis-tcs/sarif-spec#302](https://github.com/oasis-tcs/sarif-spec/issues/302)
* API NON-BREAKING: Define result `taxonomies`. [oasis-tcs/sarif-spec#314](https://github.com/oasis-tcs/sarif-spec/issues/314)
* API NON-BREAKING: Define a `reportingDescriptorReference` object. [oasis-tcs/sarif-spec#324](https://github.com/oasis-tcs/sarif-spec/issues/324)
* API BREAKING: Change `run.graphs` and `result.graphs` from objects to arrays. [oasis-tcs/sarif-spec#326](https://github.com/oasis-tcs/sarif-spec/issues/326)
* API BREAKING: External property file related renames (post ballot changes). [oasis-tcs/sarif-spec#335](https://github.com/oasis-tcs/sarif-spec/issues/335)
* API NON-BREAKING: Allow toolComponents to be externalized. [oasis-tcs/sarif-spec#337](https://github.com/oasis-tcs/sarif-spec/issues/337)
* API BREAKING: Rename all `instanceGuid` properties to `guid`. [oasis-tcs/sarif-spec#341](https://github.com/oasis-tcs/sarif-spec/issues/341)
* API NON-BREAKING: Add `reportingDescriptor.deprecatedNames` and `deprecatedGuids` to match `deprecatedIds` property. [oasis-tcs/sarif-spec#346](https://github.com/oasis-tcs/sarif-spec/issues/346)
* API NON-BREAKING: Add `referencedOnCommandLine` as a role. [oasis-tcs/sarif-spec#347](https://github.com/oasis-tcs/sarif-spec/issues/347)
* API NON-BREAKING: Rename `reportingConfigurationOverride` to `configurationOverride`. [oasis-tcs/sarif-spec#350](https://github.com/oasis-tcs/sarif-spec/issues/350)

## **v2.0.0-csd.2.beta.2019.02-20** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.02-20) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.02-20) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.02-20)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.02-20))
* COMMAND-LINE BREAKING: Rename `--sarif-version` to `--sarif-output-version`. Remove duplicative tranform `--target-version` command-line argument.
* COMMAND-LINE NON-BREAKING: add `--inline` option to multitool `rebaseuri` verb, to write output directly into input files.
* API NON-BREAKING: Add additional properties to `toolComponent`. [oasis-tcs/sarif-spec#336](https://github.com/oasis-tcs/sarif-spec/issues/336)
* API NON-BREAKING: Provide a caching mechanism for duplicated code flow data. [oasis-tcs/sarif-spec#320](https://github.com/oasis-tcs/sarif-spec/issues/320)
* API NON-BREAKING: Add `inlineExternalPropertyFiles` at the log level. [oasis-tcs/sarif-spec#321](https://github.com/oasis-tcs/sarif-spec/issues/321)
* API NON-BREAKING: Update logical location kinds to accommodate XML and JSON paths. [oasis-tcs/sarif-spec#291](https://github.com/oasis-tcs/sarif-spec/issues/291)
* API NON-BREAKING: Define result taxonomies. [oasis-tcs/sarif-spec#314](https://github.com/oasis-tcs/sarif-spec/issues/314)
* API BREAKING: Remove `invocation.attachments`, now replaced by `run.tool.extensions`. [oasis-tcs/sarif-spec#327](https://github.com/oasis-tcs/sarif-spec/issues/327)
* API NON-BREAKING: Introduce new localization mechanism. [oasis-tcs/sarif-spec#338](https://github.com/oasis-tcs/sarif-spec/issues/338)
* API BREAKING: Remove `tool.language` and localization support. [oasis-tcs/sarif-spec#325](https://github.com/oasis-tcs/sarif-spec/issues/325)
* API NON-BREAKING: Add additional properties to toolComponent. [oasis-tcs/sarif-spec#336](https://github.com/oasis-tcs/sarif-spec/issues/336)
* API BREAKING: Rename `invocation.toolNotifications` and `invocation.configurationNotifications` to `toolExecutionNotifications` and `toolConfigurationNotifications`. [oasis-tcs/sarif-spec#330](https://github.com/oasis-tcs/sarif-spec/issues/330)
* API BREAKING: Add address property to a location object (and other nodes). [oasis-tcs/sarif-spec#302](https://github.com/oasis-tcs/sarif-spec/issues/302)
* API BREAKING: External property file related renames. [oasis-tcs/sarif-spec#335](https://github.com/oasis-tcs/sarif-spec/issues/335)

## **v2.0.0-csd.2.beta.2019.01-24.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.01-24.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.01-24.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.01-24.1)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.01-24.1))
* BUGFIX: `region.charOffset` default value should be -1 (invalid value) rather than 0. Fixes an issue where `region.charLength` is > 0 but `region.charOffset` is absent (because its value of 0 was incorrectly elided due to being the default value). 

## **v2.0.0-csd.2.beta.2019.01-24** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019.01-24) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019.01-24) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019.01-24)) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019.01-24))
* BUGFIX: SDK compatibility update for sample apps.
* BUGFIX: Add Sarif.Multitool.exe.config file to multitool package to resolve "Could not load file or assembly `Newtonsoft.Json, Version=9.0.0.0`" exception on using validate command.
* API BREAKING: rename baselineState `existing` value to `unchanged`. Add new baselineState value `updated`. [oasis-tcs/sarif-spec#312](https://github.com/oasis-tcs/sarif-spec/issues/312)
* API BREAKING: unify result and notification failure levels (`note`, `warning`, `error`). Break out result evaluation state into `result.kind` property with values `pass`, `fail`, `open`, `review`, `notApplicable`. [oasis-tcs/sarif-spec#317](https://github.com/oasis-tcs/sarif-spec/issues/317)
* API BREAKING: remove IRule entirely, in favor of utilizing ReportingDescriptor base class.
* API BREAKING: define `toolComponent` object to persist tool data. The `tool.driver` component documents the standard driver metadata. `tool.extensions` is an array of `toolComponent` instances that describe extensions to the core analyzer. This change also deletes `tool.sarifLoggerVersion` (from the newly created `toolComponent` object) due to its lack of utility. Adds `result.extensionIndex` to allow results to be associated with a plug-in. `toolComponent` also added as a new file role. [oasis-tcs/sarif-spec#179](https://github.com/oasis-tcs/sarif-spec/issues/179)
* API BREAKING: Remove `run.resources` object. Rename `rule` object to `reportingDescriptor`. Move rule and notification reportingDescriptor objects to `tool.notificationDescriptors` and `tool.ruleDescriptors`. `resources.messageStrings` now located at `toolComponent.globalMessageStrings`. `rule.configuration` property now named `reportingDescriptor.defaultConfiguration`. `reportingConfiguration.defaultLevel` and `reportingConfiguration.defaultRank` simplified to `reportingConfiguration.level` and `reportingConfiguration.rank`. Actual runtime reportingConfiguration persisted to new array of reportingConfiguration objects at `invocation.reportingConfiguration`. [oasis-tcs/sarif-spec#311](https://github.com/oasis-tcs/sarif-spec/issues/311)
* API BREAKING: `run.richTextMessageMimeType` renamed to `run.markdownMessageMimeType`. `message.richText` renamed to `message.markdown`. `message.richMessageId` deleted. Create `multiformatMessageString` object, that holds plain text and markdown message format strings. `reportingDescriptor.messageStrings` is now a dictionary of these objects, keyed by message id. `reporting.Descriptor.richMessageStrings` dictionary is deleted. [oasis-tcs/sarif-spec#319](https://github.com/oasis-tcs/sarif-spec/issues/319)
* API BREAKING: `threadflowLocation.kind` is now `threadflowLocation.kinds`, an array of strings that categorize the thread flow location. [oasis-tcs/sarif-spec#202](https://github.com/oasis-tcs/sarif-spec/issues/202)
* API BREAKING: `file` renamed to `artifact`. `fileLocation` renamed to `artifactLocation`. `run.files` renamed to `run.artifacts`. [oasis-tcs/sarif-spec#309](https://github.com/oasis-tcs/sarif-spec/issues/309)

## **v2.0.0-csd.2.beta.2019-01-09** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2019-01-09) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2019-01-09) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2019-01-09) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2019-01-09)
* BUGFIX: Result matching improvements in properties persistence.
* FEATURE: Fortify FPR converter improvements.
* API NON-BREAKING: Remove uniqueness requirement from `result.locations`.
* API NON-BREAKING: Add `run.newlineSequences` to schema. [oasis-tcs/sarif-spec#169](https://github.com/oasis-tcs/sarif-spec/issues/169)
* API NON-BREAKING: Add `rule.deprecatedIds` to schema. [oasis-tcs/sarif-spec#293](https://github.com/oasis-tcs/sarif-spec/issues/293)
* API NON-BREAKING: Add `versionControlDetails.mappedTo`. [oasis-tcs/sarif-spec#248](https://github.com/oasis-tcs/sarif-spec/issues/248)
* API NON-BREAKING: Add result.rank`. Add `ruleConfiguration.defaultRank`.
* API NON-BREAKING: Add `file.sourceLocation` and `region.sourceLanguage` to guide in snippet colorization. `run.defaultSourceLanguage` provides a default value. [oasis-tcs/sarif-spec#286](https://github.com/oasis-tcs/sarif-spec/issues/286)
* API NON-BREAKING: default values for `result.rank` and `ruleConfiguration.defaultRank` is now -1.0 (from 0.0). [oasis-tcs/sarif-spec#303](https://github.com/oasis-tcs/sarif-spec/issues/303)
* API BREAKING: Remove `run.architecture` [oasis-tcs/sarif-spec#262](https://github.com/oasis-tcs/sarif-spec/issues/262)
* API BREAKING: `result.message` is now a required property [oasis-tcs/sarif-spec#283](https://github.com/oasis-tcs/sarif-spec/issues/283)
* API BREAKING: Rename `tool.fileVersion` to `tool.dottedQuadFileVersion` [oasis-tcs/sarif-spec#274](https://github.com/oasis-tcs/sarif-spec/issues/274)
* API BREAKING: Remove `open` from valid rule default configuration levels. The transformer remaps this value to `note`. [oasis-tcs/sarif-spec#288](https://github.com/oasis-tcs/sarif-spec/issues/288)
* API BREAKING: `run.columnKind` default value is now `unicodeCodePoints`. The transformer will inject `utf16CodeUnits`, however, when this property is absent, as this value is a more appropriate default for the Windows platform. [#1160](https://github.com/Microsoft/sarif-sdk/pull/1160)
* API BREAKING: Make `run.logicalLocations` an array, not a dictionary. Add result.logicalLocationIndex to point to associated logical location.
* API BREAKING: `run.externalFiles` renamed to `run.externalPropertyFiles`, which is not a bundle of external property file objects. NOTE: no transformation will be provided for legacy versions of the external property files API.
* API BREAKING: rework `result.provenance` object, including moving result.conversionProvenance to `result.provenance.conversionSources`. NOTE: no transformation currently exists for this update.
* API BREAKING: Make `run.files` an array, not a dictionary. Add fileLocation.fileIndex to point to a file object associated with the location within `run.files`.
* API BREAKING: Make `resources.rules` an array, not a dictionary. Add result.ruleIndex to point to a rule object associated with the result within `resources.rules`.
* API BREAKING: `run.logicalLocations` now requires unique array elements. [oasis-tcs/sarif-spec#304](https://github.com/oasis-tcs/sarif-spec/issues/304)

## **v2.0.0-csd.2.beta.2018-10-10.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2018-10-10.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2018-10-10.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2018-10-10.2) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2018-10-10.2)
* BUGFIX: Don`t emit v2 analysisTarget if there is no v1 resultFile.
* BUILD: Bring NuGet publishing scripts into conformance with new Microsoft requirements.

## **v2.0.0-csd.2.beta.2018-10-10.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2018-10-10.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2018-10-10.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2018-10-10.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2018-10-10.1)
* BUGFIX: Persist region information associated with analysis target

## **v2.0.0-csd.2.beta.2018-10-10** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.2.beta.2018-10-10) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.2.beta.2018-10-10) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.2.beta.2018-10-10) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.2.beta.2018-10-10)
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

## **v2.0.0-csd.1.0.2** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.1.0.2) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.1.0.2) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.1.0.2) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.1.0.2)
* BUGFIX: In result matching algorithm, an empty or null previous log no longer causes a NullReferenceException.
* BUGFIX: In result matching algorithm, duplicate data is no longer incorrectly detected across files. Also: changed a "NotImplementedException" to the correct "InvalidOperationException".

## **v2.0.0-csd.1.0.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.1.0.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.1.0.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.1.0.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.1.0.1)
* API BREAKING CHANGE: Fix weakly typed CreateNotification calls and make API more strongly typed
* API BREAKING CHANGE: Rename OptionallyEmittedData.ContextCodeSnippets to ContextRegionSnippets
* API BREAKING CHANGE: Eliminate result.ruleMessageId (in favor of result.message.messageId)

## **v2.0.0-csd.1** [Sdk](https://www.nuget.org/packages/Sarif.Sdk/2.0.0-csd.1) | [Driver](https://www.nuget.org/packages/Sarif.Driver/2.0.0-csd.1) | [Converters](https://www.nuget.org/packages/Sarif.Converters/2.0.0-csd.1) | [Multitool](https://www.nuget.org/packages/Sarif.Multitool/2.0.0-csd.1)
* Convert object model to conform to SARIF v2 CSD.1 draft specification
* Distinguish textual vs. binary file persistence in rewrite option (and allow for both in multitool rewrite verb)
*   NOTE: the change above introduces a command-line breaking change. --persist-file-contents is now renamed to --insert
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
