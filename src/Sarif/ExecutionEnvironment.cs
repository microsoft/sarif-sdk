// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A wrapper class for accessing the execution environment.
    /// </summary>
    /// <remarks>
    /// Clients should use this class rather than directly using the System.Environment
    /// class, so they can mock the IExecutionEnvironment interface in unit tests.
    /// </remarks>
    public class ExecutionEnvironment : IExecutionEnvironment
    {
        /// <summary>
        /// Replaces the name of each environment variable embedded in the specified string with
        /// the string equivalent of the value of the variable, then returns the resulting string.
        /// </summary>
        /// <param name="name">
        /// A string containing the names of zero or more environment variables. Each environment
        /// variable is quoted with the percent sign character (%).
        /// </param>
        /// <returns>
        /// A string with each environment variable replaced by its value.
        /// </returns>
        public string ExpandEnvironmentVariables(string name) => Environment.ExpandEnvironmentVariables(name);

        /// <summary>
        /// Gets the fully qualified path of the current working directory.
        /// </summary>
        public string CurrentDirectory => Environment.CurrentDirectory;
    }
}
