# PyLint Test Data Regeneration

## Steps to regenerate PyLint test data
* mkdir c:\testdata
* cd c:\testdata
* pip install pylint
* mkdir src
* pushd src
* git clone https://github.com/PyCQA/pylint.git
* pushd pylint
* git pull
* git checkout 40a305e83d27b4821ff2b6556b4bb18271f0adb8
* copy NUL __init__.py
* popd
* popd
* pylint -f json ./src/pylint/ > .\pylint.results.json
* Sarif.Multitool.exe convert "c:\testdata\pylint.results.json" --tool "Pylint" --output "c:\testdata\pylint.results.json.sarif" --pretty-print --force
* Sarif.Multitool.exe rebaseuri "c:\testdata\pylint.results.json.sarif" --base-path-token "SRCROOT" --base-path-value "file:///c:/testdata/" --o "c:\testdata" --pretty-print --force --rebase-relative-uris true
* Sarif.Multitool.exe rewrite "c:\testdata\pylint.results.json-rebased.sarif" --insert "Hashes;TextFiles;RegionSnippets;FlattenedMessages;ComprehensiveRegionProperties" --output  "c:\testdata\PyLint.sarif" --force -pretty-print
