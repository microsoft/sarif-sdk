Use the SARIF Multitool to transform, enrich, filter, result match, and do other common operations against SARIF files.

## Modes
| Mode | Purpose |
| ---- | ------- |
| help | See Usage |
| convert | Convert a tool output log to SARIF format |
| match-results-forward | Match Results run over run to identify New, Absent, and Unchanged Results |
| file-work-items | Send SARIF results to a work item tracking system such as GitHub or Azure DevOps |
| rewrite | Transform a SARIF file to a reformatted version |
| transform | Transform a SARIF log to a different SARIF version |
| merge | Merge multiple SARIF files into one |
| rebaseuri | Rebase the URIs in one or more sarif files. |
| absoluteuri | Turn all relative Uris into absolute URIs (to be used after rebaseUri is run) |
| page | Extract a subset of results from a source SARIF file. |
| query | Find the matching subset of a SARIF file and output it or log it. |
| validate | Validate a SARIF File against the schema and against additional correctness rules. |
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
Sarif.Multitool rewrite Current.sarif --insert "TextFiles,RegionSnippets" --inline

: Rebase URIs from local paths to a token (for later resolution against remote URLs)
Sarif.Multitool rebaseuri Current.sarif --base-path-value "C:\Local\Path\To\Repo" --base-path-token REPO

: Compare to previous baseline to identify new Results
Sarif.Multitool match-results-forward Current.sarif --previous Baseline.sarif --output-file-path NewBaseline.sarif

: Extract new Results only from New Baseline
Sarif.Multitool query NewBaseline.sarif --expression "BaselineState == 'New'" --output Current.NewResults.sarif

: ----
: Transform to latest SARIF version (if older)
Sarif.Multitool transform OlderFormat.sarif --output CurrentFormat.sarif --sarif-output-version Current

: Validate a SARIF file conforms to the schema
Sarif.Multitool validate Other.sarif
```

## Supported Converters
Run ```Sarif.Multitool convert --help``` for the current list.

- AndroidStudio
- ClangAnalyzer
- CppCheck
- ContrastSecurity
- Fortify
- FortifyFpr
- FxCop
- PREfast
- Pylint
- SemmleQL
- StaticDriverVerifier
- TSLint

## Common Arguments

| Name | Purpose |
| ---- | ------- |
| pretty-print | Produce pretty-printed JSON output rather than compact form. |
| force | Force overwrite of output file if it exists. |
