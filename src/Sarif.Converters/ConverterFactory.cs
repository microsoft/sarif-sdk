// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public abstract class ConverterFactory
    {
        // Create the converter for the specified toolFormat, if possible;
        // otherwise, delegate to the next converter in the Chain of Responsibility,
        // if any.
        public ToolFileConverterBase CreateConverter(string toolFormat)
        {
            ToolFileConverterBase converter = this.CreateConverterCore(toolFormat);
            if (converter != null)
            {
                return converter;
            }

            if (this.Next != null)
            {
                return this.Next.CreateConverter(toolFormat);
            }

            return null;
        }

        // The next converter in the Chain of Responsibility.
        public ConverterFactory Next { get; set; }

        public abstract ToolFileConverterBase CreateConverterCore(string toolFormat);
    }
}
