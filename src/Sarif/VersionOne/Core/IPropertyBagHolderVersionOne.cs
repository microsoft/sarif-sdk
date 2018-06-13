// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Interface exposed by objects that can hold properties of arbitrary types.
    /// </summary>
    public interface IPropertyBagHolderVersionOne
    {
        IList<string> PropertyNames { get; }
        bool TryGetProperty(string propertyName, out string value);
        string GetProperty(string propertyName);
        bool TryGetProperty<T>(string propertyName, out T value);
        T GetProperty<T>(string propertyName);
        void SetProperty<T>(string propertyName, T value);
        void SetPropertiesFrom(IPropertyBagHolderVersionOne other);
        void RemoveProperty(string propertyName);
    }
}