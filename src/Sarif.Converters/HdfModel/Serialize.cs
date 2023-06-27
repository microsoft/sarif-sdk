// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    public static class Serialize
    {
        public static string ToJson(this HdfFile self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
}
