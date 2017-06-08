// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public abstract class ConverterFactory
    {
        public abstract ToolFileConverterBase CreateConverter(string toolFormat);
    }
}
