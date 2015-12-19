// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Configuration
{
    internal class ConfigurationParseContext
    {
        public readonly Dictionary<PropertyInfo, List<object>> MultipleUseArgumentValues = new Dictionary<PropertyInfo, List<object>>();
        public readonly HashSet<PropertyInfo> FlagsSetOnCommandLine = new HashSet<PropertyInfo>();
        public readonly HashSet<PropertyInfo> FlagsSetInConfigurationFile = new HashSet<PropertyInfo>();
        private readonly Dictionary<string, ErrorEntry> _errors = new Dictionary<string, ErrorEntry>(StringComparer.OrdinalIgnoreCase);

        public void AddConfigurationError(string argument, string errorCondition)
        {
            // Configuration errors take lower priority; the console always wins.
            if (!_errors.ContainsKey(argument))
            {
                _errors.Add(argument, new ErrorEntry { Source = ErrorSource.ConfigurationFile, ErrorText = errorCondition });
            }
        }

        public void RemoveConfigurationError(string argument)
        {
            // This happens when a bad configuration error is overridden by a good command line option.
            ErrorEntry configError;
            if (_errors.TryGetValue(argument, out configError) && configError.Source == ErrorSource.ConfigurationFile)
            {
                _errors.Remove(argument);
            }
        }

        public void AddCommandLineError(string argument, string errorCondition)
        {
            ErrorEntry configError;
            if (!_errors.TryGetValue(argument, out configError))
            {
                // No previous error for that argument
                _errors.Add(argument, new ErrorEntry { Source = ErrorSource.CommandLine, ErrorText = errorCondition });
            }
            else if (configError.Source == ErrorSource.ConfigurationFile)
            {
                // Previous error for that argument was a configuration file error;
                // pave over it
                configError.Source = ErrorSource.CommandLine;
                configError.ErrorText = errorCondition;
            }
            // Otherwise there was a previous error with source == CommandLine, so do nothing.
        }

        public Dictionary<string, string> GenerateErrorDictionary()
        {
            return _errors.ToDictionary(x => x.Key, x => x.Value.ErrorText, StringComparer.OrdinalIgnoreCase);
        }

        public void RemoveAllErrors()
        {
            _errors.Clear();
        }

        private enum ErrorSource
        {
            CommandLine,
            ConfigurationFile
        }

        private class ErrorEntry
        {
            public ErrorSource Source { get; set; }
            public string ErrorText { get; set; }
        }
    }
}
