// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Interface exposed by objects that can hold properties of arbitrary types.
    /// </summary>
    public interface IPropertyBagHolder
    {
        string GetProperty(string propertyName);
        T GetProperty<T>(string propertyName);
        void SetProperty(string propertyName, string value);
        IList<string> PropertyNames { get; }
    }
}