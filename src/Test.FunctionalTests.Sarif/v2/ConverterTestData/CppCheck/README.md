# CppCheck Test Data Regeneration

## Steps to regenerate CppCheck test data
* mkdir c:\testdata
* cd c:\testdata
* Install cppcheck tool - https://github.com/danmar/cppcheck
* mkdir src
* pushd src
* git clone https://github.com/danmar/cppcheck.git
* pushd cppcheck
* git pull
* git checkout c5ebf26f9f972e9fd3a758ae62f9924618c121ec
* popd
* popd
* Launch CppCheck UI
* Analyze -> Directory -> c:\testdata\src\cppcheck
* Save Results to File -> c:\testdata\CppCheck.xml
* Sarif.Multitool.exe convert "c:\testdata\CppCheck.xml" --tool "CppCheck" --output "c:\testdata\CppCheck.xml.sarif" --pretty-print --force
* Sarif.Multitool.exe rebaseuri "c:\testdata\CppCheck.sarif" --base-path-token "SRCROOT" --base-path-value "file:///c:/testdata/src/cppcheck/" --o "c:\testdata" --pretty-print --force --rebase-relative-uris true
* Sarif.Multitool.exe rewrite "c:\testdata\CppCheck.xml-rebased.sarif" --insert "Hashes;TextFiles;RegionSnippets;FlattenedMessages;ComprehensiveRegionProperties" --output  "c:\testdata\CppCheck.sarif" --force -pretty-pri
