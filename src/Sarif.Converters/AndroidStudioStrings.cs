// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// Strings used for parsing Android Studio logs.
    /// </summary>
    public class AndroidStudioStrings
    {
        /// <summary>The constant "problems".</summary>
        public readonly string Problems;
        /// <summary>The constant "problem".</summary>
        public readonly string Problem;
        /// <summary>The constant "file".</summary>
        public readonly string File;
        /// <summary>The constant "line".</summary>
        public readonly string Line;
        /// <summary>The constant "module".</summary>
        public readonly string Module;
        /// <summary>The constant "package".</summary>
        public readonly string Package;
        /// <summary>The constant "entry_point".</summary>
        public readonly string EntryPoint;
        /// <summary>The constant "problem_class".</summary>
        public readonly string ProblemClass;
        /// <summary>The constant "hints".</summary>
        public readonly string Hints;
        /// <summary>The constant "hint".</summary>
        public readonly string Hint;
        /// <summary>The constant "description".</summary>
        public readonly string Description;

        /// <summary>The constant "TYPE".</summary>
        public readonly string Type;
        /// <summary>The constant "FQNAME".</summary>
        public readonly string FQName;
        /// <summary>The constant "severity".</summary>
        public readonly string Severity;
        /// <summary>The constant "attribute_key".</summary>
        public readonly string AttributeKey;
        /// <summary>The constant "value".</summary>
        public readonly string Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidStudioStrings"/> class.
        /// </summary>
        /// <param name="nameTable">The name table from which strings are generated.</param>
        public AndroidStudioStrings(XmlNameTable nameTable)
        {
            this.Problems = nameTable.Add("problems");
            this.Problem = nameTable.Add("problem");
            this.File = nameTable.Add("file");
            this.Line = nameTable.Add("line");
            this.Module = nameTable.Add("module");
            this.Package = nameTable.Add("package");
            this.EntryPoint = nameTable.Add("entry_point");
            this.ProblemClass = nameTable.Add("problem_class");
            this.Hints = nameTable.Add("hints");
            this.Hint = nameTable.Add("hint");
            this.Description = nameTable.Add("description");

            this.Type = nameTable.Add("TYPE");
            this.FQName = nameTable.Add("FQNAME");
            this.Severity = nameTable.Add("severity");
            this.AttributeKey = nameTable.Add("attribute_key");
            this.Value = nameTable.Add("value");
        }
    }
}
