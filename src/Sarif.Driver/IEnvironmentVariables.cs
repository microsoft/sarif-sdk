// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>
    /// An interface for accessing environment variables.
    /// </summary>
    /// <remarks>
    /// Clients wishing to access environment variables should instantiate an EnvironmentVariables
    /// object rather than directly using the System.Environment class, so they can mock the
    /// IEnvironmentVariables interface in unit tests.
    /// </remarks>
    public interface IEnvironmentVariables
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
        string ExpandEnvironmentVariables(string name);
    }
}
