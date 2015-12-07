// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>Entry point for the data model generator.</summary>
    public static class Program
    {
        //private static IDisposable sw;

        /// <summary>Main entry-point for this application.</summary>
        /// <param name="args">Array of command-line argument strings.</param>
        /// <returns>Exit-code for the process - 0 for success, else an error code.</returns>
        public static int Main(string[] args)
        {
            using (var config = new DataModelGeneratorConfiguration())
            {
                if (!config.ParseArgs(args))
                {
                    return 1;
                }

                string inputText;
                using (var sr = new StreamReader(config.InputStream))
                {
                    inputText = sr.ReadToEnd();
                }

                GrammarSymbol grammar;
                try
                {
                    var factory = new TokenTextIndex(inputText);
                    System.Collections.Immutable.ImmutableArray<Token> tokens = Lexer.Lex(factory);
                    var parser = new Parser(tokens);
                    grammar = parser.ParseGrammar();
                }
                catch (G4ParseFailureException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return 1;
                }

                var builder = new DataModelBuilder();
                for (int idx = 1; idx < grammar.Children.Length; ++idx)
                {
                    try
                    {
                        GrammarSymbol prod = grammar.Children[idx];
                        builder.CompileProduction(prod);
                    }
                    catch (G4ParseFailureException ex)
                    {
                        Console.WriteLine(Strings.FailedToCompileProduction + ex.Message);
                    }
                }

                string generatedCSharp;
                string generatedJsonSchema;
                try
                {
                    DataModel model = builder.Link(config.InputFilePath, DataModelMetadata.FromGrammar(grammar));
                    model = MemberHoister.HoistTypes(model);
                    var cr = new CodeWriter();
                    CodeGenerator.WriteDataModel(cr, model);
                    generatedCSharp = cr.ToString();

                    cr = new CodeWriter(2, ' ');
                    JsonSchemaGenerator.WriteSchema(cr, model);
                    generatedJsonSchema = cr.ToString();
                }
                catch (G4ParseFailureException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return 1;
                }

                if (config.ToConsole)
                {
                    Console.WriteLine(generatedCSharp);
                }

                using (var sw = new StreamWriter(config.OutputStream))
                {
                    sw.Write(generatedCSharp);
                }

                string jsonSchemaPath = Path.GetFileNameWithoutExtension(config.OutputFilePath);
                jsonSchemaPath = Path.Combine(Path.GetDirectoryName(config.OutputFilePath), jsonSchemaPath + ".schema.json");

                File.WriteAllText(jsonSchemaPath, generatedJsonSchema);

                return 0;
            }
        }
    }
}
