# AndroidStudio Test Data Regeneration

## Steps to regenerate AndroidStudio test data
* mkdir c:\testdata
* cd c:\testdata
* Install AndroidStudio
* Launch AndroidStudio
* Import an Android code sample
* Select "Action BarCompat - Share Action Provider"
* Analyzer -> Inspect Code
* Export To -> XML -> c:\testdata

* Sarif.Multitool.exe convert "c:\testdata\AndroidDomInspection.xml" --tool "AndroidStudio" --output "c:\testdata\AndroidDomInspection.xml.sarif" --pretty-print --force

* Sarif.Multitool.exe convert "c:\testdata\CanBeFinal.xml" --tool "AndroidStudio" --output "c:\testdata\CanBeFinal.xml.sarif" --pretty-print --force

* Sarif.Multitool.exe convert "c:\testdata\ConstantConditions.xml" --tool "AndroidStudio" --output "c:\testdata\ConstantConditions.xml.sarif" --pretty-print --force

* Sarif.Multitool.exe convert "c:\testdata\Convert2Diamond.xml" --tool "AndroidStudio" --output "c:\testdata\Convert2Diamond.xml.sarif" --pretty-print --force

* Sarif.Multitool.exe convert "c:\testdata\DanglingJavadoc.xml" --tool "AndroidStudio" --output "c:\testdata\DanglingJavadoc.xml.sarif" --pretty-print --force

* Sarif.Multitool.exe convert "c:\testdata\DeprecatedClassUsageInspection.xml" --tool "AndroidStudio" --output "c:\testdata\DeprecatedClassUsageInspection.xml.sarif" --pretty-print --force

* Sarif.Multitool.exe convert "c:\testdata\Deprecation.xml" --tool "AndroidStudio" --output "c:\testdata\Deprecation.xml.sarif" --pretty-print --force

* Sarif.Multitool.exe convert "c:\testdata\NullableProblems.xml" --tool "AndroidStudio" --output "c:\testdata\NullableProblems.xml.sarif" --pretty-print --force

* Sarif.Multitool.exe convert "c:\testdata\SpellCheckingInspection.xml" --tool "AndroidStudio" --output "c:\testdata\SpellCheckingInspection.xml.sarif" --pretty-print --force

* Sarif.Multitool.exe convert "c:\testdata\WeakerAccess.xml" --tool "AndroidStudio" --output "c:\testdata\WeakerAccess.xml.sarif" --pretty-print --force

* Sarif.Multitool.exe merge --output-folder "c:\testdata" --output-file "AndroidStudio-merged.sarif" --force --pretty-print c:\testdata\*.xml.sarif 
* Sarif.Multitool.exe rebaseuri "c:\testdata\AndroidStudio-merged.sarif" --base-path-token "SRCROOT" --base-path-value "file:///c:/testdata/src/androidstudio/" --o "c:\testdata" --pretty-print --force --rebase-relative-uris true

* Sarif.Multitool.exe rewrite "c:\testdata\AndroidStudio-merged-rebased.sarif" --insert "Hashes;TextFiles;RegionSnippets;FlattenedMessages;ComprehensiveRegionProperties" --output  "c:\testdata\AndroidStudio.sarif" --force -pretty-print
