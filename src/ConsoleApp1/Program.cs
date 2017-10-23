using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var originalFile = @"C:\Projects\sarif-sdk\bld\bin\Sarif.FunctionalTests\AnyCPU_Debug\netcoreapp2.0\ConverterTestData\PREfast\Expected-001.xml.sarif";
            var actualFile = @"C:\Projects\sarif-sdk\bld\bin\Sarif.FunctionalTests\AnyCPU_Debug\netcoreapp2.0\ConverterTestData\PREfast\Expected-001.xml.actual.sarif";
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance
            };

            SarifLog original = JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(originalFile), settings);
            SarifLog actual = JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(actualFile), settings);

            var result = PublicInstancePropertiesEqual(original, actual);
        }

        public static bool PublicInstancePropertiesEqual<T>(T self, T to) where T : class
        {
            if (self != null && to != null)
            {
                Type type = typeof(T);
                foreach (System.Reflection.PropertyInfo pi in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    object selfValue = type.GetProperty(pi.Name).GetValue(self, null);
                    object toValue = type.GetProperty(pi.Name).GetValue(to, null);

                    if (pi.PropertyType.IsClass)
                    {
                        var equal = PublicInstancePropertiesEqual(selfValue, toValue);
                    }

                    if (selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue)))
                    {
                        return false;
                    }
                }
                return true;
            }
            return self == to;
        }
    }
}
