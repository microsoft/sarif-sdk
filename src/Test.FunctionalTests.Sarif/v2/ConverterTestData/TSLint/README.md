# TSLint Test Data Regeneration

## Steps to regenerate TSLint test data
* mkdir c:\testdata
* cd c:\testdata
* yarn global add tslint typescript 
* mkdir src
* pushd src
* git clone https://github.com/palantir/tslint.git
* pushd tslint
* git pull
* git checkout 788f64c9b101ef719290a62f7d67ca5162d821e4
* popd
* popd
* tslint --init
* tslint -c tslint.json 'src/tslint/**/*.ts' -t json -o tslint.results.json
* Sarif.Multitool.exe convert "c:\testdata\tslint.results.json" --tool "TSLint" --output "c:\testdata\tslint.results.json.sarif" --pretty-print --force
* Sarif.Multitool.exe rebaseuri "c:\testdata\tslint.results.json.sarif" --base-path-token "SRCROOT" --base-path-value "file:///c:/testdata/" --o "c:\testdata" --pretty-print --force --rebase-relative-uris true
* Sarif.Multitool.exe rewrite "c:\testdata\tslint.results.json-rebased.sarif" --insert "Hashes;TextFiles;RegionSnippets;FlattenedMessages;ComprehensiveRegionProperties" --output  "c:\testdata\TSLint.sarif" --force -pretty-print
