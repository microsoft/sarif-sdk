// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>
    /// An interface for accessing the contents of an application's config file.
    /// </summary>
    /// <remarks>
    /// Clients wishing to access an application's config file should instantiate an
    /// AppConfig object, rather than directly using the ConfigurationManager class,
    /// so they can mock the IAppConfig interface in unit tests.
    /// </remarks>
    public interface IAppConfig
    {
        /// <summary>
        /// Gets the specified member of the AppSettings collection. 
        /// </summary>
        /// <param name="name">
        /// The name of the app setting to return.
        /// </param>
        /// <returns>
        /// The string value of the specified app setting, or null if the application's
        /// config file does not contain the specified setting.
        /// </returns>
        string GetAppSetting(string name);
    }
}
