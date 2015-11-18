// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************
namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>
    /// Class Information contains the name and the code generated for a single class from the grammar.
    /// The CSharpSourceBuilder BuildSource creates ClassInformation objects for each production in a grammar.
    /// </summary>
    public class ClassInformation
    {
        public string Name { get; set; }
        public string ClassDefinition { get; set; }
    }
}