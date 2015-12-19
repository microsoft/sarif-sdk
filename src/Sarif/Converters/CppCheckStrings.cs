// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>String constants for CppCheck from an <see cref="XmlNameTable"/>.</summary>
    internal class CppCheckStrings
    {
        /// <summary>The string constant "results".</summary>
        public readonly string Results;

        /// <summary>The string constant "cppcheck".</summary>
        public readonly string CppCheck;
        /// <summary>The string constant "version".</summary>
        public readonly string Version;
        /// <summary>The string constant "errors".</summary>
        public readonly string Errors;
        /// <summary>The string constant "error".</summary>
        public readonly string Error;

        /// <summary>The string constant "id".</summary>
        public readonly string Id;
        /// <summary>The string constant "msg".</summary>
        public readonly string Msg;
        /// <summary>The string constant "verbose".</summary>
        public readonly string Verbose;
        /// <summary>The string constant "severity".</summary>
        public readonly string Severity;

        /// <summary>The string constant "location".</summary>
        public readonly string Location;
        /// <summary>The string constant "file".</summary>
        public readonly string File;
        /// <summary>The string constant "line".</summary>
        public readonly string Line;

        /// <summary>Initializes a new instance of the <see cref="CppCheckStrings"/> class.</summary>
        /// <param name="nameTable">The name table from which strings shall be obtained.</param>
        public CppCheckStrings(XmlNameTable nameTable)
        {
            this.Results = nameTable.Add("results");

            this.CppCheck = nameTable.Add("cppcheck");
            this.Version = nameTable.Add("version");
            this.Errors = nameTable.Add("errors");
            this.Error = nameTable.Add("error");

            this.Id = nameTable.Add("id");
            this.Msg = nameTable.Add("msg");
            this.Verbose = nameTable.Add("verbose");
            this.Severity = nameTable.Add("severity");

            this.Location = nameTable.Add("location");
            this.File = nameTable.Add("file");
            this.Line = nameTable.Add("line");
        }
    }
}
