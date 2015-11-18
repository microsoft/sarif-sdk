// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    internal struct ToStringGenerator
    {
        private readonly CodeWriter _codeWriter;
        private readonly DataModel _model;
        private readonly DataModelType _writtenType;

        private readonly LocalVariableNamer _indexNamer;
        private readonly LocalVariableNamer _valueNamer;
        private readonly LocalVariableNamer _labelNamer;

        /// <summary>
        /// Generates a "ToString" override for the supplied type from the supplied model into the
        /// supplied <see cref="CodeWriter"/>.
        /// </summary>
        /// <param name="codeWriter">The code writer.</param>
        /// <param name="model">The model.</param>
        /// <param name="writtenType">Type of the written.</param>
        public static void Generate(CodeWriter codeWriter, DataModel model, DataModelType writtenType)
        {
            new ToStringGenerator(codeWriter, model, writtenType).Generate();
        }

        private ToStringGenerator(CodeWriter codeWriter, DataModel model, DataModelType writtenType)
        {
            _codeWriter = codeWriter;
            _model = model;
            _writtenType = writtenType;
            _indexNamer = new LocalVariableNamer("index");
            _valueNamer = new LocalVariableNamer("value");
            _labelNamer = new LocalVariableNamer("label");
        }

        private void Generate()
        {
            System.Collections.Immutable.ImmutableArray<ToStringEntry> entries = _writtenType.ToStringEntries;
            if (entries.Length == 0)
            {
                return;
            }

            _codeWriter.WriteLine();
            _codeWriter.WriteLine("/// <summary>Converts this instance into a debugging string.</summary>");
            _codeWriter.WriteLine("/// <returns>A string containing the data stored in this instance.</summary>");
            _codeWriter.OpenBrace("public override string ToString()");
            _codeWriter.WriteLine("var sb = new StringBuilder();");

            foreach (ToStringEntry toStringEntry in entries)
            {
                this.WriteToStringEntry(toStringEntry);
            }

            _codeWriter.WriteLine("return sb.ToString();");
            _codeWriter.CloseBrace(); // ToString()
        }


        private void WriteToStringEntry(ToStringEntry toStringEntry)
        {
            if (toStringEntry.Text != null && toStringEntry.Variable == null)
            {
                this.WriteLiteralTextAppend(toStringEntry.Text);
                return;
            }

            this.WriteToStringEntryRecursive(toStringEntry, _model.GetTypeForMember(toStringEntry.Variable).Kind, "this." + toStringEntry.Variable.CSharpName, toStringEntry.Variable.Rank);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.CodeAnalysis.DataModelGenerator.ToStringGenerator.WriteLiteralTextAppend(System.String)")]
        private void WriteToStringEntryRecursive(ToStringEntry toStringEntry, DataModelTypeKind memberTypeKind, string sourceVariable, int ranksRemaining)
        {
            if (ranksRemaining == 0)
            {
                switch (memberTypeKind)
                {
                    case DataModelTypeKind.Leaf:
                    case DataModelTypeKind.Base:
                    case DataModelTypeKind.BuiltInUri:
                    case DataModelTypeKind.BuiltInVersion:
                        _codeWriter.WriteLine("sb.Append({0} == null ? \"{1}(null)\" : {0}.ToString());", sourceVariable, toStringEntry.Variable.CSharpName);
                        break;
                    case DataModelTypeKind.BuiltInString:
                        _codeWriter.WriteLine("sb.Append({0} == null ? \"{1}(null)\" : {0});", sourceVariable, toStringEntry.Variable.CSharpName);
                        break;
                    case DataModelTypeKind.BuiltInNumber:
                    case DataModelTypeKind.BuiltInBoolean:
                        _codeWriter.WriteLine("sb.Append({0}.ToString(CultureInfo.InvariantCulture));", sourceVariable);
                        break;
                    case DataModelTypeKind.BuiltInDictionary:
                        string value = _valueNamer.MakeName();
                        _codeWriter.OpenBrace("if (this.{0} == null)", toStringEntry.Variable.CSharpName);
                        _codeWriter.WriteLine("sb.Append(\"{0}(null)\");", toStringEntry.Variable.CSharpName);
                        _codeWriter.CloseBrace();
                        _codeWriter.OpenBrace("else");
                        _codeWriter.WriteLine("sb.Append('{');");
                        _codeWriter.OpenBrace("using (var {0} = this.{1}.GetEnumerator())", value, toStringEntry.Variable.CSharpName);
                        _codeWriter.OpenBrace("if ({0}.MoveNext())", value);
                        _codeWriter.WriteLine("sb.Append(\"{{\" + {0}.Current.Key + \", \" + {0}.Current.Value + \"}}\");", value);
                        _codeWriter.OpenBrace("while ({0}.MoveNext())", value);
                        _codeWriter.WriteLine("sb.Append(\", {{\" + {0}.Current.Key + \", \" + {0}.Current.Value + \"}}\");", value);
                        _codeWriter.CloseBrace(); // while
                        _codeWriter.CloseBrace(); // if
                        _codeWriter.CloseBrace(); // using
                        _codeWriter.WriteLine("sb.Append('}');");
                        _codeWriter.CloseBrace(); // else
                        break;
                    default:
                        Debug.Fail("Bad DataModelTypeKind " + _writtenType.Kind);
                        break;
                }
            }
            else
            {
                string indexVariable = _indexNamer.MakeName();
                string valueVariable = _valueNamer.MakeName();
                string skipDelimiterLabel = _labelNamer.MakeName();
                string loopHeadLabel = _labelNamer.MakeName();

                // This code makes the:
                // Write(0)
                // for (i = 1 -> N) {
                //   Delimeter();
                //   Write(i);
                // }
                // pattern; the "goto" mess is to avoid n^2 code size explosion due to repeating the "Write" call.
                // (If this were not generated code it would make sense to stamp out separate functions)
                //
                // if (source.Count != 0) {
                //   int index = 0;
                //   goto skipDelimeter;
                // loopHead:
                //   WriteDelimeter();
                // skipDelimeter:
                //   Write();
                //   ++index;
                //   if (index < source.Count) {
                //     goto loopHead;
                //   }
                // }
                //   


                _codeWriter.OpenBrace("if ({0}.Count != 0)", sourceVariable);
                _codeWriter.WriteLine("int {0} = 0;", indexVariable);
                _codeWriter.WriteLine("goto {0};", skipDelimiterLabel);
                _codeWriter.WriteLine(loopHeadLabel + ":");
                this.WriteLiteralTextAppend(toStringEntry.Text ?? ", ");
                _codeWriter.WriteLine(skipDelimiterLabel + ":");
                _codeWriter.WriteLine("var {0} = {1}[{2}];", valueVariable, sourceVariable, indexVariable);
                this.WriteToStringEntryRecursive(toStringEntry, memberTypeKind, valueVariable, ranksRemaining - 1);
                _codeWriter.WriteLine("++{0};", indexVariable);
                _codeWriter.OpenBrace("if ({0} < {1}.Count)", indexVariable, sourceVariable);
                _codeWriter.WriteLine("goto {0};", loopHeadLabel);
                _codeWriter.CloseBrace(); // if (index < source.Count)
                _codeWriter.CloseBrace(); // if (count != 0)
                _codeWriter.WriteLine();
            }
        }

        private void WriteLiteralTextAppend(string text)
        {
            _codeWriter.WriteLine("sb.Append(\"" + text.Replace("\\", "\\\\") + "\");");
        }
    }
}
