// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class GenericCommandTests
    {
        [Fact]
        public void CommandsDoNotHaveConflictingShortNames()
        {
            Assembly asm = typeof(Microsoft.CodeAnalysis.Sarif.Multitool.Program).Assembly;

            // Reflect over all 'Options' types in Sarif.Multitool and verify none have multiple options
            // with the same ShortName.
            foreach (Type type in asm.GetTypes().Where((t) => t.Name.EndsWith("Options")))
            {
                CheckTypeForDuplicateArgumentNames(type);
            }
        }

        private void CheckTypeForDuplicateArgumentNames(Type type)
        {
            Dictionary<char, Argument> parameters = new Dictionary<char, Argument>();

            foreach (PropertyInfo property in type.GetProperties())
            {
                Argument result;

                foreach (CustomAttributeData customAttribute in property.CustomAttributes)
                {
                    if (customAttribute.AttributeType == typeof(CommandLine.OptionAttribute))
                    {
                        result = ParseOptionAttribute(customAttribute);

                        if (result.ShortName != default(char))
                        {
                            if (parameters.ContainsKey(result.ShortName))
                            {
                                Assert.True(false, $"{type.Name} has a conflict for {result.ShortName} between {result.LongName} and {parameters[result.ShortName].LongName}.");
                            }

                            parameters[result.ShortName] = result;
                        }
                    }
                }
            }
        }

        private Argument ParseOptionAttribute(CustomAttributeData attribute)
        {
            Argument result = new Argument();

            foreach (CustomAttributeTypedArgument argument in attribute.ConstructorArguments)
            {
                if (argument.ArgumentType == typeof(char))
                {
                    result.ShortName = (char)argument.Value;
                }
                else if (argument.ArgumentType == typeof(string))
                {
                    result.LongName = (string)argument.Value;
                }
            }

            foreach (CustomAttributeNamedArgument namedArgument in attribute.NamedArguments)
            {
                if (namedArgument.MemberName == "ShortName")
                {
                    result.ShortName = (char)namedArgument.TypedValue.Value;
                }
                else if (namedArgument.MemberName == "LongName")
                {
                    result.LongName = (string)namedArgument.TypedValue.Value;
                }
            }

            return result;
        }

        private struct Argument
        {
            public char ShortName;
            public string LongName;
        }
    }
}
