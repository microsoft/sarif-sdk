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
* Rename 'suppressedInBaseline' to 'suppressed'



