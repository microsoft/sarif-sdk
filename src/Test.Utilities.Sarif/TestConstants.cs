// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Test.Utilities.Sarif
{
    public static class TestConstants
    {
        public const string AutomationDetailsGuid = "D41BF9F2-225D-4254-984E-DFD659702E4D";
        public const string ConverterName = "TestConverter";
        public const string LanguageIdentifier = "xx-XX";
        public const string PolicyName = "TestPolicy";
        public const string TaxonomyName = "TestTaxonomy";
        public const string ToolName = "TestTool";
        public const string TranslationName = "TestTranslation";
        public const string TranslationMetadataName = "TestTranslationMetadata";

        public static class RuleIds
        {
            public const string Rule1  = "TST0001";
            public const string Rule2  = "TST0002";
            public const string Rule3  = "TST0003";
            public const string Rule4  = "TST0004";
            public const string Rule5  = "TST0005";
            public const string Rule6  = "TST0006";
            public const string Rule7  = "TST0007";
            public const string Rule8  = "TST0008";
            public const string Rule9  = "TST0009";
            public const string Rule10 = "TST0010";
        }

        public static class FileLocations
        {
            public const string Location1 = @"C:\Test\Data\File1.sarif";
            public const string Location2 = @"C:\Test\Data\File2.sarif";
            public const string Location3 = @"C:\Test\Data\File3.sarif";
            public const string Location4 = @"C:\Test\Data\File4.sarif";
        }

        public static class TaxonIds
        {
            public const string Taxon1 = "TAX0001";
            public const string Taxon2 = "TAX0002";
        }
    }
}
