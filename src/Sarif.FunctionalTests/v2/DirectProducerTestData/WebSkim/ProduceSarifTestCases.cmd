set WEBSKIM=D:\src\SecDevTools\Main\bin\x64\Debug\ModernCop\mod50.exe
set OUTPUT=.\
set TEST_ROOT=d:\src\SecDevTools\Main\src\ModernCop\Source\WebRules.Test.Unit\ModernCopTestData\WebRules_PackageRules\
set BUNDLE_ROOT=d:\src\SecDevTools\Main\src\ModernCop\Source\Mod50.Test.Unit\ModernCopTestData\Mod50\CompiledPackages\

%WEBSKIM% analyze /file %TEST_ROOT%..\WebRules_JavaScriptRules\Semantic\VariableDeclaredMultipleTimes.js /out %OUTPUT%RelatedLocations.sarif

%WEBSKIM% analyze /appx %TEST_ROOT%MinificationRuleApp.appx /out %OUTPUT%NestedLocationsFromAppX.sarif

%WEBSKIM% analyze /appx %TEST_ROOT%MinificationRuleApp /out %OUTPUT%NestedLocationsFromDirectoryNoTrailingSlash.sarif

%WEBSKIM% analyze /appx %TEST_ROOT%MinificationRuleApp /out %OUTPUT%NestedLocationsFromDirectoryTrailingSlash.sarif

%WEBSKIM% analyze /appx %BUNDLE_ROOT%SimpleJSApp.appxbundle /out %OUTPUT%SimpleBundleDoubleNesting.sarif

%WEBSKIM% analyze /appx %BUNDLE_ROOT%FoodAndDrink_4.0.35.0_x86.appxbundle /out %OUTPUT%LocalizedBundleDoubleNesting.sarif

echo COMMAND LINES
echo.
echo %WEBSKIM% analyze /file %TEST_ROOT%..\WebRules_JavaScriptRules\Semantic\VariableDeclaredMultipleTimes.js /out %OUTPUT%RelatedLocations.sarif
echo %WEBSKIM% analyze /appx %TEST_ROOT%MinificationRuleApp.appx /out %OUTPUT%NestedLocationsFromAppX.sarif
echo %WEBSKIM% analyze /appx %TEST_ROOT%MinificationRuleApp /out %OUTPUT%NestedLocationsFromDirectoryNoTrailingSlash.sarif
echo %WEBSKIM% analyze /appx %TEST_ROOT%MinificationRuleApp\ /out %OUTPUT%NestedLocationsFromDirectoryTrailingSlash.sarif
echo %WEBSKIM% analyze /appx %BUNDLE_ROOT%SimpleJSApp.appxbundle /out %OUTPUT%SimpleBundle.sarif
echo %WEBSKIM% analyze /appx %BUNDLE_ROOT%FoodAndDrink_4.0.35.0_x86.appxbundle /out %OUTPUT%ComplexBundle.sarif
