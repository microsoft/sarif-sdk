// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>Test-only <see cref="IEnvironmentVariableGetter"/> backed by a dictionary.</summary>
    internal sealed class FakeEnvironmentVariableGetter : IEnvironmentVariableGetter
    {
        private readonly Dictionary<string, string> _values;

        public FakeEnvironmentVariableGetter()
            : this(new Dictionary<string, string>())
        { }

        public FakeEnvironmentVariableGetter(Dictionary<string, string> values)
        {
            _values = values ?? new Dictionary<string, string>();
        }

        public string GetEnvironmentVariable(string variable)
            => _values.TryGetValue(variable, out string v) ? v : null;

        public FakeEnvironmentVariableGetter With(string name, string value)
        {
            _values[name] = value;
            return this;
        }
    }

    /// <summary>An <see cref="IEnvironmentVariableGetter"/> that returns <c>null</c> for every lookup.</summary>
    internal sealed class EmptyEnvironmentVariableGetter : IEnvironmentVariableGetter
    {
        public string GetEnvironmentVariable(string variable) => null;
    }
}
