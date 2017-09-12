// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ConverterTestsBase<T> where T : ToolFileConverterBase, new()
    {
        public SarifLog RunTestCase(string inputData, string expectedResult, bool prettyPrint = true)
        {
            var converter = new T();

            // First retrieve converter JSON. This code will raise appropriate exceptions 
            // for malformed data.
            string actualJson = Utilities.GetConverterJson(converter, inputData);

            // Next, let's ensure that the JSON is actually valid. The resulting
            // SARIF will be returned, allowing tests to perform additional validations.
            // We need to explicitly provide a settings object here to handle 
            // special-cases in deserialization. If our grammar -> C# code hints 
            // provided an ability to apply additional attributes to emitted code, 
            // we wouldn't be required to do this. Filed an issue on this.
            // https://github.com/Microsoft/jschema/issues/6

            var settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance
            };

            var log = JsonConvert.DeserializeObject<SarifLog>(actualJson, settings);

            // Pretty-printed JSON literals can consume significant space in test code,
            // so we'll provide an option to collapse into minimal form
            actualJson = prettyPrint ? actualJson : JsonConvert.SerializeObject(log, Formatting.None);

            // Hard-coded JSON comparisons are useful for sniffing out small unexpected changes but
            // are fragile. It would be better for our testing to have a dedicated set of data-driven
            // tests that flag changes and for the unit-tests to work exclusively against the 
            // object model.
            actualJson.Should().Be(expectedResult); ;

            return log;
        }
    }
}
