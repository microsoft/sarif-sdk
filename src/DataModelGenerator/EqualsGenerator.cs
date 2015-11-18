// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>Generates Equals(object), Equals(T), and GetHashCode members in generated code.</summary>
    internal struct EqualsGenerator
    {
        private const string StartingConstant = "17";
        private const string MultiplicativeConstant = "31";
        private readonly CodeWriter _codeWriter;
        private readonly DataModel _model;
        private readonly DataModelType _writtenType;

        private readonly LocalVariableNamer _valueNamer;
        private readonly LocalVariableNamer _indexNamer;
        private readonly LocalVariableNamer _maxNamer;
        private readonly LocalVariableNamer _xorNamer;

        /// <summary>Generates Equals(object), Equals(T), and GetHashCode members for the supplied type from the supplied model.</summary>
        /// <param name="codeWriter">The code writer into which generated code shall be written.</param>
        /// <param name="model">The model from which code is being generated.</param>
        /// <param name="writtenType">Data model type being written.</param>
        public static void Generate(CodeWriter codeWriter, DataModel model, DataModelType writtenType)
        {
            if (!model.MetaData.GenerateEquals || writtenType.Kind != DataModelTypeKind.Leaf)
            {
                return;
            }

            new EqualsGenerator(codeWriter, model, writtenType).Generate();
        }

        private EqualsGenerator(CodeWriter codeWriter, DataModel model, DataModelType writtenType)
        {
            _codeWriter = codeWriter;
            _model = model;
            _writtenType = writtenType;

            _valueNamer = new LocalVariableNamer("value");
            _indexNamer = new LocalVariableNamer("index");
            _maxNamer = new LocalVariableNamer("max");
            _xorNamer = new LocalVariableNamer("xor");
        }

        private void Generate()
        {
            _codeWriter.WriteLine();
            _codeWriter.WriteLine("/// <summary>Generates a hash code for this instance.</summary>");
            _codeWriter.WriteLine("/// <returns>A hash code for this instance; suitable for putting this instance into a hashtable.</returns>");
            _codeWriter.OpenBrace("public override int GetHashCode()");
            if (_writtenType.Members.Length == 0)
            {
                _codeWriter.WriteLine("// All instances of this type are equivalent.");
                _codeWriter.WriteLine("return 0;");
            }
            else
            {
                _codeWriter.WriteLine("int result = {0};", StartingConstant);
                _codeWriter.OpenBrace("unchecked");
                WriteGetHashCodeImpl();
                _codeWriter.CloseBrace(); // unchecked
                _codeWriter.WriteLine();
                _codeWriter.WriteLine("return result;");
            }

            _codeWriter.CloseBrace(); // GetHashCode()

            _codeWriter.WriteLine();
            _codeWriter.WriteLine("/// <summary>Compares this instance with another instance.</summary>");
            _codeWriter.WriteLine("/// <param name=\"o\">The instance to compare with this instance.</param>");
            _codeWriter.WriteLine("/// <returns>true if this instance and <paramref name=\"o\" /> contain the same data; otherwise, false.</returns>");
            _codeWriter.OpenBrace("public override bool Equals(object o)");
            _codeWriter.WriteLine("return this.Equals(o as {0});", _writtenType.CSharpName);
            _codeWriter.CloseBrace(); // Equals(object)

            _codeWriter.WriteLine();
            _codeWriter.WriteLine("/// <summary>Compares this instance with another instance.</summary>");
            _codeWriter.WriteLine("/// <param name=\"other\">The instance to compare with this instance.</param>");
            _codeWriter.WriteLine("/// <returns>true if this instance and <paramref name=\"other\" /> contain the same data; otherwise, false.</returns>");
            _codeWriter.OpenBrace("public bool Equals({0} other)", _writtenType.CSharpName);
            _codeWriter.OpenBrace("if (other == null)");
            _codeWriter.WriteLine("return false;");
            _codeWriter.CloseBrace(); // if (other == null)
            _codeWriter.WriteLine();

            foreach (DataModelMember member in _writtenType.Members)
            {
                this.WriteEqualsForMember(member);
            }

            _codeWriter.WriteLine("return true;");
            _codeWriter.CloseBrace(); // Equals(T)
        }

        private void WriteGetHashCodeImpl()
        {
            ImmutableArray<DataModelMember> members = _writtenType.Members;
            if (members.Length == 0)
            {
                return;
            }

            this.WriteGetHashCodeMember(members[0]);
            for (int idx = 1; idx < members.Length; ++idx)
            {
                if (members[idx - 1].Rank != 0)
                {
                    _codeWriter.WriteLine();
                }

                this.WriteGetHashCodeMember(members[idx]);
            }
        }

        private void WriteGetHashCodeMember(DataModelMember member)
        {
            DataModelType memberType = _model.GetTypeForMember(member);
            DataModelTypeKind typeKind = memberType.Kind;
            string sourceVariable = "this." + member.CSharpName;

            // (rank - 1) * 2 OpenBrace calls
            for (int idx = 1; idx < member.Rank; ++idx)
            {
                string nextVariable = _valueNamer.MakeName();
                _codeWriter.OpenBrace("foreach (var {0} in {1})", nextVariable, sourceVariable);
                _codeWriter.WriteLine("result = result * {0};", MultiplicativeConstant);
                _codeWriter.OpenBrace("if ({0} != null)", nextVariable);
                sourceVariable = nextVariable;
            }

            // rank != 0 ? 1 : 0 OpenBrace calls
            if (member.Rank != 0)
            {
                string nextVariable = _valueNamer.MakeName();
                _codeWriter.OpenBrace("foreach (var {0} in {1})", nextVariable, sourceVariable);
                _codeWriter.WriteLine("result = result * {0};", MultiplicativeConstant);

                sourceVariable = nextVariable;
            }

            // rank
            if (memberType.IsNullable)
            {
                _codeWriter.OpenBrace("if ({0} != null)", sourceVariable);
            }

            switch (typeKind)
            {
                case DataModelTypeKind.Base:
                case DataModelTypeKind.Leaf:
                case DataModelTypeKind.BuiltInString:
                case DataModelTypeKind.BuiltInUri:
                case DataModelTypeKind.BuiltInVersion:
                    _codeWriter.WriteLine("result = (result * {1}) + {0}.GetHashCode();", sourceVariable, MultiplicativeConstant);
                    break;
                case DataModelTypeKind.BuiltInNumber:
                    _codeWriter.WriteLine("result = (result * {1}) + (int){0};", sourceVariable, MultiplicativeConstant);
                    break;
                case DataModelTypeKind.BuiltInBoolean:
                    _codeWriter.OpenBrace("if (" + sourceVariable + ")");
                    _codeWriter.WriteLine("result = (result * " + MultiplicativeConstant + ") + 1;");
                    _codeWriter.CloseBrace();
                    break;
                case DataModelTypeKind.BuiltInDictionary:
                    string xorTempVal = _xorNamer.MakeName();
                    string iterVal = _valueNamer.MakeName();
                    _codeWriter.WriteLine("// Use xor for dictionaries to be order-independent");
                    _codeWriter.WriteLine("int {0} = 0;", xorTempVal);
                    _codeWriter.OpenBrace("foreach (var {0} in {1})", iterVal, sourceVariable);
                    _codeWriter.WriteLine("{0} ^= ({1}.Key ?? String.Empty).GetHashCode();", xorTempVal, iterVal);
                    _codeWriter.WriteLine("{0} ^= ({1}.Value ?? String.Empty).GetHashCode();", xorTempVal, iterVal);
                    _codeWriter.CloseBrace(); // foreach
                    _codeWriter.WriteLine();
                    _codeWriter.WriteLine("result = (result * {1}) + {0};", xorTempVal, MultiplicativeConstant);
                    break;
                default:
                    Debug.Fail("Unrecognized DataModelTypeKind");
                    break;
            }

            int closeBraces;
            if (member.Rank == 0)
            {
                closeBraces = 0;
            }
            else
            {
                closeBraces = (member.Rank - 1) * 2 + 1;
            }

            if (memberType.IsNullable)
            {
                ++closeBraces;
            }

            for (int idx = 0; idx < closeBraces; ++idx)
            {
                _codeWriter.CloseBrace();
            }
        }

        private void WriteEqualsForMember(DataModelMember member)
        {
            DataModelTypeKind typeKind = _model.GetTypeForMember(member).Kind;

            string sourceVariable = "this." + member.CSharpName;
            string otherVariable = "other." + member.CSharpName;
            for (int idx = 0; idx < member.Rank; ++idx)
            {
                _codeWriter.OpenBrace("if (!global::System.Object.ReferenceEquals({0}, {1}))", sourceVariable, otherVariable);
                _codeWriter.OpenBrace("if ({0} == null || {1} == null)", sourceVariable, otherVariable);
                _codeWriter.WriteLine("return false;");
                _codeWriter.CloseBrace(); // null check
                _codeWriter.WriteLine();

                _codeWriter.OpenBrace("if ({0}.Count != {1}.Count)", sourceVariable, otherVariable);
                _codeWriter.WriteLine("return false;");
                _codeWriter.CloseBrace(); // different lengths check
                _codeWriter.WriteLine();

                string max = _maxNamer.MakeName();
                string index = _indexNamer.MakeName();
                _codeWriter.WriteLine("int {0} = {1}.Count;", max, sourceVariable);
                _codeWriter.OpenBrace("for (int {0} = 0; {0} < {1}; ++{0})", index, max);
                string newSource = _valueNamer.MakeName();
                string newOther = _valueNamer.MakeName();
                _codeWriter.WriteLine("var {0} = {1}[{2}];", newSource, sourceVariable, index);
                _codeWriter.WriteLine("var {0} = {1}[{2}];", newOther, otherVariable, index);
                sourceVariable = newSource;
                otherVariable = newOther;
            }

            switch (typeKind)
            {
                case DataModelTypeKind.Base:
                case DataModelTypeKind.Leaf:
                    _codeWriter.OpenBrace("if (!global::System.Object.Equals({0}, {1}))", sourceVariable, otherVariable);
                    _codeWriter.WriteLine("return false;");
                    _codeWriter.CloseBrace();
                    break;
                case DataModelTypeKind.BuiltInString:
                case DataModelTypeKind.BuiltInNumber:
                case DataModelTypeKind.BuiltInBoolean:
                case DataModelTypeKind.BuiltInUri:
                case DataModelTypeKind.BuiltInVersion:
                    _codeWriter.OpenBrace("if ({0} != {1})", sourceVariable, otherVariable);
                    _codeWriter.WriteLine("return false;");
                    _codeWriter.CloseBrace();
                    break;
                case DataModelTypeKind.BuiltInDictionary:
                    string sourceKvp = _valueNamer.MakeName();
                    string otherVal = _valueNamer.MakeName();
                    _codeWriter.OpenBrace("if (!global::System.Object.ReferenceEquals({0}, {1}))", sourceVariable, otherVariable);
                    _codeWriter.OpenBrace("if ({0} == null || {1} == null || {0}.Count != {1}.Count)", sourceVariable, otherVariable);
                    _codeWriter.WriteLine("return false;");
                    _codeWriter.CloseBrace();

                    _codeWriter.OpenBrace("foreach (var {0} in {1})", sourceKvp, sourceVariable);
                    _codeWriter.WriteLine("string " + otherVal + ";");
                    _codeWriter.OpenBrace("if (!{0}.TryGetValue({1}.Key, out {2}) || !global::System.Object.Equals({1}.Value, {2}))", otherVariable, sourceKvp, otherVal);
                    _codeWriter.WriteLine("return false;");
                    _codeWriter.CloseBrace(); // if
                    _codeWriter.CloseBrace(); // foreach
                    _codeWriter.CloseBrace(); // if (!referenceequals)
                    break;
                default:
                    Debug.Fail("Unrecognized DataModelTypeKind");
                    throw new InvalidOperationException();
            }


            for (int idx = 0; idx < member.Rank; ++idx)
            {
                _codeWriter.CloseBrace(); // for
                _codeWriter.CloseBrace(); // ReferenceEquals
            }

            _codeWriter.WriteLine();
        }
    }
}
