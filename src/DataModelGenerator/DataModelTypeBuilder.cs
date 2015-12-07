// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>Base class for data model type builders.</summary>
    internal abstract class DataModelTypeBuilder
    {
        /// <summary>The symbol declaring this type.</summary>
        public readonly GrammarSymbol DeclSymbol;
        /// <summary>The summary text to emit for this type.</summary>
        public string SummaryText;
        /// <summary>The remarks text to emit for this type.</summary>
        public string RemarksText;
        /// <summary>The production ("G4") declaration name for this type.</summary>
        public string G4DeclaredName;
        /// <summary>The name of this type when emitted in C#.</summary>
        public string CSharpName;
        /// <summary>The C# name of the type which is this type's base class.</summary>
        public string Base;
        /// <summary>This type is the root object of a JSON representation</summary>summary>
        public bool RootObject;
        /// <summary>The serialized value names for an enum/set
        public IList<string> SerializedValues;
        /// <summary>The set of G4 declared values for an enum/set
        public IList<string> G4DeclaredValues;
        /// <summary>An option pattern to restrict member values
        public string Pattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataModelTypeBuilder"/> class.
        /// </summary>
        /// <param name="decl">The declaration declaring this type.</param>
        protected DataModelTypeBuilder(GrammarSymbol decl)
        {
            this.DeclSymbol = decl;
            this.G4DeclaredName = decl.GetLogicalText();
            this.SummaryText = decl.Annotations.GetAnnotationValue("summary");
            this.RemarksText = decl.Annotations.GetAnnotationValue("remarks");
            this.CSharpName
                = decl.Annotations.GetAnnotationValue("className")
                  ?? LinguisticTransformer.ToCSharpName(this.G4DeclaredName);

            this.Pattern = decl.Annotations.GetAnnotationValue("pattern");

            this.RootObject = decl.Annotations.GetAnnotationValue("rootObject") != null;

            this.G4DeclaredValues = new List<string>();

            string serializedValuesText = decl.Annotations.GetAnnotationValue("serializedValues");
            if (!string.IsNullOrEmpty(serializedValuesText))
            {
                this.SerializedValues = new List<string>(serializedValuesText.Split(','));
            }
            else
            {
                this.SerializedValues = new List<string>();
            }
        }

        /// <summary>Converts this instance to an immutable <see cref="DataModelType"/>.</summary>
        /// <returns>This instance as a <see cref="DataModelType"/>.</returns>
        public abstract DataModelType ToImmutable();
    }
}