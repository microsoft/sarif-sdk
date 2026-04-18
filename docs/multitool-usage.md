Use the SARIF Multitool to rewrite, enrich, filter, result match, and do other common operations against SARIF files.

## Modes
| Mode | Purpose |
| ---- | ------- |
| absoluteuri | Turn all relative Uris into absolute URIs (to be used after rebaseUri is run) |
| apply-policy | Apply policies from SARIF log |
| convert | Convert a tool output log to SARIF format |
| export-validation-config | Export validation rule options to an XML or JSON file that can be edited and used to configure subsequent analysis |
| export-validation-rules | Export validation rules metadata to a SARIF or SonarQube XML file |
| file-work-items | Send SARIF results to a work item tracking system such as GitHub or Azure DevOps |
| match-results-forward | Match Results run over run to identify New, Absent, and Unchanged Results |
| merge | Merge multiple SARIF files into one |
| page | Extract a subset of results from a source SARIF file |
| query | Find the matching subset of a SARIF file and output it or log it |
| rebaseuri | Rebase the URIs in one or more sarif files |
| rewrite | Transform a SARIF file to a reformatted version |
| suppress | Suppress results from a SARIF file |
| validate | Validate a SARIF File against the schema and against additional correctness rules. |
| help | See Usage |
| version | Display version information |

## Examples
```
: Install the current Sarif.Multitool to the local machine cache (requires Node.js)
npm i -g @microsoft/sarif-multitool

: Run the Sarif.Multitool using the NPM-installed copy
npx @microsoft/sarif-multitool <args>

: See Usage
Sarif.Multitool @microsoft/sarif-multitool help

: Convert a Fortify file to SARIF
Sarif.Multitool convert Current.fpr -tool FortifyFpr -output Current.sarif

: Add file contents from analyzed files and snippets from result regions to SARIF
Sarif.Multitool rewrite Current.sarif --insert TextFiles;RegionSnippets --log Inline

: Remove codeFlows from results regions to SARIF
Sarif.Multitool rewrite Current.sarif --remove CodeFlows --log Inline

: Transform to latest SARIF version (if older)
Sarif.Multitool rewrite OlderFormat.sarif --output CurrentFormat.sarif --sarif-output-version Current

: Rebase URIs from local paths to a token (for later resolution against remote URLs)
Sarif.Multitool rebaseuri Current.sarif --base-path-value "C:\Local\Path\To\Repo" --base-path-token REPO

: Compare to previous baseline to identify new Results
Sarif.Multitool match-results-forward Current.sarif --previous Baseline.sarif --output-file-path NewBaseline.sarif

: Export validation config (this can be used to validate and rewrite the default policies)
Sarif.Multitool export-validation-config validation.xml

: Export validation rules metadata
Sarif.Multitool export-validation-rules ValidationRules.md

: Merge multiple SARIF files into one
Sarif.Multitool merge C:\Input\*.sarif --recurse true --output-directory=C:\Output\ --output-file=MergeResult.sarif

: Extract new Results only from New Baseline
Sarif.Multitool query NewBaseline.sarif --expression "BaselineState == 'New'" --output Current.NewResults.sarif

: Suppress Results
Sarif.Multitool suppress current.sarif --justification "some justification" --alias "some alias" --guids --timestamps --expiryInDays 5 --status Accepted --output suppressed.sarif

: Validate a SARIF file conforms to the schema
Sarif.Multitool validate Other.sarif
```

## Supported Converters

Run ```Sarif.Multitool convert --help``` for the current list.

- AndroidStudio
- CisCat
- ClangAnalyzer
- ClangTidy
- CppCheck
- ContrastSecurity
- Fortify
- FortifyFpr
- FxCop
- Nessus
- PREfast
- Pylint
- SemmleQL
- StaticDriverVerifier
- TSLint

### Observation

For Clang-tidy you can also provide an extra log file with file name [report file name with extension].log (e.g. report.yml.log) that will ingest and enhance the SARIF file with line number and column number.  
Example:
```
clang-tidy --checks=* --header-filter=.* --system-headers --export-fixes=report.yml &>report.yml.log
```
This will generate an extra report.yml.log, leave in the same folder with the input report.yml file.

## Common parameter `--log` log file persistence  options

| Name | Purpose |
| ---- | ------- |
| ForceOverwrite | Force overwrite of output file if it exists. |
| Inline | Inline outputs to files where appropriate. |
| PrettyPrint | Produce pretty-printed JSON output rather than compact form. |
| Minify | Produce compact JSON output (all white space removed) rather than pretty-printed output. |
| Optimize | Produce a smaller but non-human-readable log omitting redundant properties. |
