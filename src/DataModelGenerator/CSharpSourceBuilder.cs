// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    public class CSharpSourceBuilder
    {
        private readonly List<ClassInformation> classInformationList;
        private readonly Grammar grammar;
        private bool verbose;
        private bool generateLocations;
        private const string GetOrSetFactoryName = "GetOrCreate";

        private enum VisitorType
        {
            Normal,
            Rewriting
        };

        public CSharpSourceBuilder(Grammar grammar)
        {
            if (grammar == null)
            {
                throw new ArgumentNullException("grammar");
            }

            this.classInformationList = new List<ClassInformation>();
            this.grammar = grammar;
        }

        /// <summary>
        /// Generates the source for the input grammar.
        /// </summary>
        /// <param name="verbose">Verbose ouput.</param>
        /// <returns>A list containing the generated code for each class or enum.</returns>
        public IList<ClassInformation> Run(bool verbose, bool generateLocations)
        {
            List<string> builtClassesList = new List<string>();

            this.verbose = verbose;
            this.generateLocations = generateLocations;
            this.classInformationList.Clear();
            WriteStatus("Starting class code generation");

            foreach (KeyValuePair<Production, TypeInformation> entry in this.grammar.TypedProductions)
            {
                if (!entry.Value.IsBaseType || entry.Value.IsEnumType)
                {
                    ClassInformation classInformation = this.BuildSource(entry.Key, builtClassesList);
                    this.classInformationList.Add(classInformation);
                }
            }

            ClassInformation enumFactoryData = this.BuildClassEnum(builtClassesList);
            this.classInformationList.Add(enumFactoryData);

            ClassInformation syntaxInterface = this.BuildSyntaxInterface();
            this.classInformationList.Add(syntaxInterface);

            ClassInformation basicVisitor = this.BuildVisitor(VisitorType.Normal);
            this.classInformationList.Add(basicVisitor);

            ClassInformation rewritingVisitor = this.BuildVisitor(VisitorType.Rewriting);
            this.classInformationList.Add(rewritingVisitor);
            WriteStatus("Completed class code generation");

            return this.classInformationList;
        }

        /// <summary>
        /// Builds a class for a given production.
        /// </summary>
        /// <param name="sourceCode">Code writer to use to build the class.</param>
        /// <param name="production">Grammar production with class definition.</param>
        /// <param name="className">Class name to be built.</param>
        private void BuildClass(CodeWriter sourceCode, Production production, string className)
        {
            sourceCode.WriteLine("[CompilerGenerated]");
            sourceCode.WriteLine("[DataContract]");
            sourceCode.OpenBrace("public class " + className + " : ISyntax");
            this.BuildConstructors(sourceCode, production, className);

            this.BuildPropertyFactory(sourceCode, production);

            this.BuildProperties(sourceCode, production);
            this.BuildImplementISyntax(sourceCode, production);
            sourceCode.WriteLine();
            this.BuildToString(sourceCode, production);
            sourceCode.WriteLine();
            this.BuildEquals(sourceCode, production);
            sourceCode.CloseBrace(); // class
        }

        /// <summary>
        /// Build a collection class.
        /// </summary>
        /// <param name="sourceCode">Code writer to use.</param>
        /// <param name="production">Grammar production with the data.</param>
        /// <param name="className">Class Name to generate.</param>
        private void BuildCollectionClass(CodeWriter sourceCode, Production production, string className)
        {
            NonTerminal nonTerminal = TryInterpretAsNonTerminal(production.RHS[0]);
            if (nonTerminal == null)
            {
                return;
            }

            string listType = nonTerminal.Name;
            TypeInformation typeInformation = this.grammar.LookupType(nonTerminal);
            if (typeInformation != null)
            {
                listType = typeInformation.TypeName;
            }

            if (listType.Equals(Common.StringName))
            {
                listType = "string";
            }

            if (listType.Equals(Common.RegExName))
            {
                listType = "Regex";
            }

            sourceCode.WriteLine("/// <summary>A collection class giving an alias to <see cref=\"List{" + listType + "}\" /> in the representation of a data model.</summary>");
            sourceCode.WriteLine("[CompilerGenerated]");
            sourceCode.WriteLine("[CollectionDataContract(ItemName = \"" + nonTerminal.Name + "\")]");
            sourceCode.OpenBrace("public class " + className + " : List<" + listType + ">, ISyntax");
            this.BuildConstructorsCollection(sourceCode, className, typeInformation);
            this.BuildImplementCollectionISyntax(sourceCode, production);
            sourceCode.WriteLine();
            this.BuildToStringCollection(sourceCode, production);
            sourceCode.WriteLine();
            this.BuildEqualsCollection(sourceCode, production);
            sourceCode.CloseBrace(); // class
            sourceCode.WriteLine();
        }

        /// <summary>
        /// Build a Dictionary class.
        /// </summary>
        /// <param name="sourceCode">Code writer to use.</param>
        /// <param name="production">Grammar production with the data.</param>
        /// <param name="className">Class Name to generate.</param>
        private void BuildDictionaryClass(CodeWriter sourceCode, Production production, string className)
        {
            NonTerminal nonTerminal = production.RHS[0] as NonTerminal;

            if (nonTerminal == null)
            {
                return;
            }

            string valueType = nonTerminal.Type.Equals(Common.DictionaryName) ? "string" : nonTerminal.Type;

            string itemName;
            if (!nonTerminal.Annotations.TryGetValue("itemName", out itemName))
            {
                itemName = "property";
            }

            string keyName;
            if (!nonTerminal.Annotations.TryGetValue("keyName", out keyName))
            {
                keyName = "key";
            }

            string valueName;
            if (!nonTerminal.Annotations.TryGetValue("valueName", out valueName))
            {
                keyName = "value";
            }

            string productionName = production.LHS.Name;
            sourceCode.WriteLine("/// <summary>A dictionary with attributes controlling serialization parameters for the purposes of the data model " + this.grammar.Name + "</summary>");
            sourceCode.WriteLine("[CompilerGenerated]");
            sourceCode.WriteLine("[CollectionDataContract(");
            sourceCode.IncrementIndentLevel();
            sourceCode.WriteLine("Name = \"" + productionName + "\",");
            sourceCode.WriteLine("ItemName = \"" + itemName + "\",");
            sourceCode.WriteLine("KeyName = \"" + keyName + "\",");
            sourceCode.WriteLine("ValueName = \"" + valueName + "\")]");
            sourceCode.DecrementIndentLevel();

            sourceCode.OpenBrace("public class " + className + " : Dictionary<string, " + valueType + ">, ISyntax");
            sourceCode.WriteLine("/// <summary>Initializes a new instance of <see cref=\"" + className + "\" /> with no entries.</summary>");
            sourceCode.OpenBrace("public " + className + "() : base (StringComparer.InvariantCultureIgnoreCase)");
            sourceCode.CloseBrace(); // default constructor
            sourceCode.WriteLine();
            sourceCode.WriteLine("/// <summary>Initializes a new instance of <see cref=\"" + className + "\" /> containing the supplied entries.</summary>");
            sourceCode.WriteLine("/// <param name=\"" + productionName + "\">The instance from which entries are supplied.</param>");
            sourceCode.OpenBrace(
                "public " + className + "(IDictionary<string, " + valueType + "> " + productionName
                + ") : base (" + productionName + ", StringComparer.InvariantCultureIgnoreCase)");
            sourceCode.CloseBrace(); // copy constructor
            sourceCode.WriteLine();
            this.BuildImplementCollectionISyntax(sourceCode, production);
            sourceCode.WriteLine();
            this.BuildToStringDictionary(sourceCode, production);
            sourceCode.WriteLine();
            this.BuildEqualsDictionary(sourceCode, production, valueType);

            sourceCode.CloseBrace(); // class
        }

        /// <summary>
        /// Builds constructors for a normal data model class.
        /// </summary>
        /// <param name="sourceCode">The source code writer to which the class is being written.</param>
        /// <param name="production">The production describing the class.</param>
        /// <param name="thisName">The name of the class being generated (which will be the type assigned to <c>this</c> in the generated code).</param>
        private void BuildConstructors(CodeWriter sourceCode, Production production, string thisName)
        {
            WriteConstructorsPreamble(sourceCode, thisName);
            sourceCode.OpenBrace();
            foreach (IGrammarSymbol symbol in production.RHS)
            {
                NonTerminal nonTerminal = TryInterpretAsNonTerminal(symbol);
                if (nonTerminal == null)
                {
                    continue;
                }

                TypeInformation typeInformation = this.grammar.LookupType(nonTerminal);
                if (typeInformation == null)
                {
                    continue;
                }

                string cloneStatement;
                if (typeInformation.IsBaseType || typeInformation.IsEnumType)
                {
                    cloneStatement = "this.{0} = other.{0};";
                }
                else if (typeInformation.IsNullable)
                {
                    cloneStatement = "this.{0} = other.{0} == null ? null : new {1}(other.{0});";
                }
                else
                {
                    cloneStatement = "this.{0} = new {1}(other);";
                }

                sourceCode.WriteLine(String.Format(CultureInfo.InvariantCulture, cloneStatement, nonTerminal.Name, typeInformation.ClassName));
            }

            sourceCode.CloseBrace(); // Copy constructor
        }

        /// <summary>
        /// Builds constructors for a collection data model class.
        /// </summary>
        /// <param name="sourceCode">The source code writer to which the class is being written.</param>
        /// <param name="thisName">The name of the class being generated (which will be the type assigned to <c>this</c> in the generated code).</param>
        /// <param name="containedTypeInformation">Type information regarding </param>
        private void BuildConstructorsCollection(CodeWriter sourceCode, string thisName, TypeInformation containedTypeInformation)
        {
            WriteConstructorsPreamble(sourceCode, thisName);

            sourceCode.Write("    : base(");
            string cloneStatement;
            if (containedTypeInformation == null || containedTypeInformation.IsBaseType || containedTypeInformation.IsEnumType)
            {
                cloneStatement = "other";
            }
            else if (containedTypeInformation.IsNullable)
            {
                cloneStatement = "other.Select(entry => entry == null ? null : new " + containedTypeInformation.ClassName + "(entry))";
            }
            else
            {
                cloneStatement = "other.Select(entry => new " + containedTypeInformation.ClassName + "(entry))";
            }

            sourceCode.Append(cloneStatement);
            sourceCode.AppendLine(")");

            sourceCode.OpenBrace(); // Copy constructor
            sourceCode.CloseBrace();
        }

        /// <summary>
        /// Writes the prefix common among constructor implementations.
        /// </summary>
        /// <param name="sourceCode">The <see cref="CodeWriter"/> to which the constructor preamble shall be written.</param>
        /// <param name="thisName">The name of the type for which constructors are being generated.</param>
        private static void WriteConstructorsPreamble(CodeWriter sourceCode, string thisName)
        {
            // Empty default constructor
            sourceCode.WriteLine("/// <summary>Initializes a new instance of the <see cref=\"" + thisName + "\" /> class.</summary>");
            sourceCode.OpenBrace("public " + thisName + "()");
            sourceCode.CloseBrace();

            // Copy constructor
            sourceCode.WriteLine("/// <summary>Initializes a new instance of the <see cref=\"" + thisName + "\" /> class, with content copied from another instance.</summary>");
            sourceCode.WriteLine("/// <param name=\"other\">The instance from which data shall be copied.</param>");
            sourceCode.WriteLine("public " + thisName + "(" + thisName + " other)");
        }

        /// <summary>
        /// Build an enum.
        /// </summary>
        /// <param name="sourceCode">Code writer to use.</param>
        /// <param name="production">Grammar production with the data.</param>
        /// <param name="enumName">Enum name to build.</param>
        private void BuildEnum(CodeWriter sourceCode, Production production, string enumName)
        {
            if (production.RHS.Count != 1)
            {
                throw new InvalidDataException("Expected one group argument but found " + production.RHS.Count);
            }

            Alternative values = production.RHS[0] as Alternative;

            if (values == null)
            {
                throw new InvalidDataException("Expected Alternative type in production");
            }

            string typeToUse = (values.Type.Equals("System.Enum")) ? "enum" : values.Type;

            string enumType;
            if (!values.Annotations.TryGetValue("enumType", out enumType))
            {
                enumType = String.Empty;
            }

            string outputType = String.IsNullOrEmpty(enumType) ? String.Empty : " : " + enumType;
            sourceCode.OpenBrace("public " + typeToUse + " " + enumName + outputType);

            foreach (Group group in values.Symbols)
            {
                Terminal symbol = group.Symbols[0] as Terminal;
                if (symbol != null)
                {
                    WriteDocumentCommentsForAnnotationSet(sourceCode, symbol.Annotations);
                    sourceCode.Write(symbol.Value);
                    sourceCode.AppendLine(",");
                }
            }

            sourceCode.CloseBrace(); // enum
        }

        /// <summary>
        /// Build an enum with a entry for each generated class. This is used by the Visitor code.
        /// </summary>
        /// <param name="builtClassesList">List of classed which were generated.</param>
        /// <returns>Code with an enum of the generated class names.</returns>
        private ClassInformation BuildClassEnum(List<string> builtClassesList)
        {
            CodeWriter sourceCode = new CodeWriter(4);
            string name = this.grammar.Name + "Kind";
            WriteHeader(sourceCode, name);
            WriteStatus("  Creating " + name);

            sourceCode.OpenBrace("namespace " + this.grammar.GrammarNamespace);
            sourceCode.WriteLine("/// <summary>An enumeration containing all the types which implement <see cref=\"ISyntax\" />.</summary>");
            sourceCode.OpenBrace("public enum " + this.grammar.Name + "Kind");
            sourceCode.WriteLine("/// <summary>An uninitialized kind.</summary>");
            sourceCode.WriteLine("None,");

            builtClassesList.Sort();
            foreach (string enumName in builtClassesList)
            {
                sourceCode.WriteLine();
                sourceCode.WriteLine("/// <summary>An entry indicating that the <see cref=\"ISyntax\" /> object is of type " + enumName + ".</summary>");
                sourceCode.WriteLine(enumName + ",");
            }

            sourceCode.CloseBrace(); // enum
            sourceCode.CloseBrace(); // namespace
            return new ClassInformation { Name = name, ClassDefinition = sourceCode.ToString() };
        }

        /// <summary>
        /// Builds the interface method for the generated collection class.
        /// </summary>
        /// <param name="sourceCode">Code Writer to use.</param>
        /// <param name="production">Grammar production to use to generate the ISyntax implementation.</param>
        private void BuildImplementCollectionISyntax(CodeWriter sourceCode, Production production)
        {
            string className;
            string enumName = this.grammar.Name + "Kind";

            if (!production.LHS.Annotations.TryGetValue("className", out className))
            {
                className = production.LHS.Name;
            }

            WriteIdDocComments(sourceCode);
            sourceCode.WriteLine("public " + enumName + " Id { get { return " + this.grammar.Name + "Kind." + className + "; } }");
            sourceCode.WriteLine();
            if (this.generateLocations == true)
            {
                this.BuildISyntaxProperty(sourceCode, "System.Int32", "Offset", "offset");
                this.BuildISyntaxProperty(sourceCode, "System.Int32", "Length", "length");
                sourceCode.WriteLine();
            }

            BuildSetProperty(sourceCode, null);

        }

        /// <summary>
        /// Builds the interface method for the generated class.
        /// </summary>
        /// <param name="sourceCode">Code Writer to use.</param>
        /// <param name="production">Grammar production to use to generate the ISyntax implementation.</param>
        private void BuildImplementISyntax(CodeWriter sourceCode, Production production)
        {
            string enumName = this.grammar.Name + "Kind";
            string className;

            if (!production.LHS.Annotations.TryGetValue("className", out className))
            {
                className = production.LHS.Name;
            }

            WriteIdDocComments(sourceCode);
            sourceCode.WriteLine("public " + enumName + " Id { get { return " + enumName + "." + className + "; } }");
            sourceCode.WriteLine();
            BuildSetProperty(sourceCode, production);
        }

        private void WriteIdDocComments(CodeWriter sourceCode)
        {
            string thisGrammarName = this.grammar.Name;
            sourceCode.WriteLine("/// <summary>Gets the kind of object referred to by this instance in the overall data model " + thisGrammarName + "</summary>");
            sourceCode.WriteLine("/// <value>The kind of object referred to by this instance in the overall data model " + thisGrammarName + "</value>");
        }


        /// <summary>
        /// Build the ISyntax properties. This is used to build the length and offset properties in cases where they
        /// are not specified in the grammar when the generateLocations flag is true. 
        /// The original MSR code had a length and offset property on every node by basing all nodes from a common 
        /// base class. Adding these to the interface supports this functionality.
        /// </summary>
        /// <param name="sourceCode">Code writer to use.</param>
        /// <param name="realType">Type to declare the property.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="serializedName">Serialized Name.</param>
        private void BuildISyntaxProperty(CodeWriter sourceCode, string realType, string propertyName, string serializedName)
        {
            sourceCode.WriteLine("[DataMember(Name = \"" + serializedName + "\", IsRequired = false, EmitDefaultValue = false)]");
            sourceCode.WriteLine("public " + realType + " " + propertyName + " { get; set; }");
            sourceCode.WriteLine();
        }

        /// <summary>
        /// Builds the private fields for a production
        /// </summary>
        /// <param name="sourceCode">Code writer to use.</param>
        /// <param name="production">Grammar production.</param>
        private void BuildPropertyFactory(CodeWriter sourceCode, Production production)
        {
            foreach (IGrammarSymbol symbol in production.RHS)
            {
                NonTerminal nonTerminal = TryInterpretAsNonTerminal(symbol);
                if (nonTerminal == null)
                {
                    continue;
                }

                TypeInformation typeInformation = this.grammar.LookupType(nonTerminal);
                if (typeInformation == null)
                {
                    continue;
                }

                string realType = typeInformation.TypeName;
                if (realType.StartsWith("Dictionary"))
                {
                    realType = typeInformation.ClassName;
                }

                if (!typeInformation.IsBaseType)
                {
                    string propertyName = nonTerminal.Name;
                    sourceCode.WriteLine("/// <summary>");
                    sourceCode.WriteLine("/// Provides a mechanism that gets a non-null value of <see cref=\"P:" + propertyName + "\" />; initializing that property");
                    sourceCode.WriteLine("/// with a blank instance of <see cref=\"" + realType + "\" /> if necessary.");
                    sourceCode.WriteLine("/// </summary>");
                    sourceCode.OpenBrace("public " + realType + " " + propertyName + GetOrSetFactoryName + "()");
                    sourceCode.OpenBrace("if (this." + propertyName + " == null)");
                    sourceCode.WriteLine("this." + propertyName + " = new " + realType + "();");
                    sourceCode.CloseBrace(); // if
                    sourceCode.WriteLine();
                    sourceCode.WriteLine("return this." + propertyName + ";");
                    sourceCode.CloseBrace(); // GetOrSetXxx
                    sourceCode.WriteLine();
                }
            }
        }

        /// <summary>
        /// Builds the properties for a production.
        /// </summary>
        /// <param name="sourceCode">Code writer to use.</param>
        /// <param name="production">Grammar production.</param>
        private void BuildProperties(CodeWriter sourceCode, Production production)
        {
            bool mustBuildOffset = this.generateLocations;
            bool mustBuildLength = this.generateLocations;

            foreach (IGrammarSymbol symbol in production.RHS)
            {
                NonTerminal nonTerminal = TryInterpretAsNonTerminal(symbol);
                if (nonTerminal == null)
                {
                    continue;
                }

                bool isRequired = !(symbol is QuestionMark);
                this.BuildProperty(sourceCode, nonTerminal, isRequired);
                if (nonTerminal.Name == "Offset")
                {
                    mustBuildOffset = false;
                }
                if (nonTerminal.Name == "Length")
                {
                    mustBuildLength = false;
                }
            }

            if (mustBuildOffset)
            {
                this.BuildISyntaxProperty(sourceCode, "System.Int32", "Offset", "offset");
            }

            if (mustBuildLength)
            {
                this.BuildISyntaxProperty(sourceCode, "System.Int32", "Length", "length");
            }
        }

        /// <summary>
        /// Build a property statement.
        /// </summary>
        /// <param name="sourceCode">Code writer to use.</param>
        /// <param name="nonTerminal">Property information.</param>
        /// <param name="isRequired">Is Required flag used to generate attributes.</param>
        private void BuildProperty(CodeWriter sourceCode, NonTerminal nonTerminal, bool isRequired)
        {
            string serializedName;
            if (!nonTerminal.Annotations.TryGetValue("serializedName", out serializedName))
            {
                serializedName = nonTerminal.Type;
            }

            string required = isRequired ? ", IsRequired = true" : ", IsRequired = false";
            string emit = isRequired ? String.Empty : ", EmitDefaultValue = false";
            string realType = nonTerminal.Name;

            TypeInformation typeInformation = this.grammar.LookupType(nonTerminal);
            if (typeInformation != null)
            {
                realType = typeInformation.TypeName;
                if (realType.StartsWith("Dictionary"))
                {
                    realType = typeInformation.ClassName;
                }
            }

            WriteDocumentCommentsForAnnotationSet(sourceCode, nonTerminal.Annotations);
            sourceCode.WriteLine("[DataMember(Name = \"" + serializedName + "\"" + required + emit + ")]");
            sourceCode.WriteLine("public " + realType + " " + nonTerminal.Name + " { get; set; }");
            sourceCode.WriteLine();
        }

        /// <summary>
        /// Build the SetProperty method for a given grammar production.
        /// </summary>
        /// <param name="sourceCode">Code writer to use.</param>
        /// <param name="production">Grammar production for which to build the method.</param>
        private void BuildSetProperty(CodeWriter sourceCode, Production production)
        {
            bool mustBuildOffset = this.generateLocations;
            bool mustBuildLength = this.generateLocations;

            // Why isn't the caller just using reflection for this?
            sourceCode.WriteLine("/// <summary>Sets a property named <paramref name=\"name\" /> to the value <paramref name=\"value\" /></summary>");
            sourceCode.WriteLine("/// <param name=\"name\">The name of the property to set.</param>");
            sourceCode.WriteLine("/// <param name=\"value\">The value to which the property shall be set.</param>");
            sourceCode.OpenBrace("public virtual bool SetProperty(string name, object value)");
            if (production != null)
            {
                foreach (IGrammarSymbol symbol in production.RHS)
                {
                    NonTerminal nonTerminal = TryInterpretAsNonTerminal(symbol);
                    if (nonTerminal == null)
                    {
                        continue;
                    }

                    TypeInformation typeInformation = this.grammar.LookupType(nonTerminal);
                    if (typeInformation == null)
                    {
                        if (Common.IsReserved(nonTerminal.Type))
                        {
                            continue;
                        }

                        throw new InvalidDataException("Type \"" + nonTerminal.Type + "\" not defined in the grammar.");
                    }

                    if (nonTerminal.Name == "Offset")
                    {
                        mustBuildOffset = false;
                    }

                    if (nonTerminal.Name == "Length")
                    {
                        mustBuildLength = false;
                    }

                    string castName;
                    if (typeInformation.IsBaseType)
                    {
                        castName = typeInformation.TypeName;
                    }
                    else if (typeInformation.TypeName.StartsWith("Dictionary<"))
                    {
                        castName = typeInformation.ClassName;
                    }
                    else
                    {
                        castName = typeInformation.TypeName;
                    }

                    sourceCode.OpenBrace("if (name.Equals(\"" + nonTerminal.Name + "\"))");
                    sourceCode.WriteLine("this." + nonTerminal.Name + " = (" + castName + ")value;");
                    sourceCode.CloseBrace(); // if
                    sourceCode.WriteLine();
                }
            }

            if (mustBuildOffset)
            {
                sourceCode.OpenBrace("if (name.Equals(\"Offset\"))");
                sourceCode.WriteLine("this.Offset = (System.Int32)value;");
                sourceCode.CloseBrace(); // if
                sourceCode.WriteLine();
            }

            if (mustBuildLength)
            {
                sourceCode.OpenBrace("if (name.Equals(\"Length\"))");
                sourceCode.WriteLine("this.Length = (System.Int32)value;");
                sourceCode.CloseBrace(); // if
                sourceCode.WriteLine();
            }

            sourceCode.WriteLine("return false;");
            sourceCode.CloseBrace(); // SetProperty
        }

        /// <summary>
        /// Build the class for the given production.
        /// </summary>
        /// <param name="production">Grammar production with the information to build the class.</param>
        /// <param name="classNames">List of names of the classes actaully built.</param>
        /// <returns>Generated code for the grammar production.</returns>
        private ClassInformation BuildSource(Production production, List<string> classNames)
        {
            CodeWriter sourceCode = new CodeWriter(4);
            bool buildDefault = true;

            string className;
            if (!production.LHS.Annotations.TryGetValue("className", out className))
            {
                className = production.LHS.Name;
            }

            WriteStatus("  Creating " + className);
            this.WriteHeader(sourceCode, className);
            this.WriteUsingStatements(sourceCode);
            sourceCode.OpenBrace("namespace " + this.grammar.GrammarNamespace);
            WriteDocumentCommentsForAnnotationSet(sourceCode, production.LHS.Annotations);

            if (production.RHS.Count == 1)
            {
                if (production.RHS[0] is Alternative)
                {
                    this.BuildEnum(sourceCode, production, className);
                    buildDefault = false;
                }

                if (production.RHS[0] is StructuralSymbol)
                {
                    this.BuildCollectionClass(sourceCode, production, className);
                    classNames.Add(className);
                    buildDefault = false;
                }

                NonTerminal nonTerminal = production.RHS[0] as NonTerminal;
                if (nonTerminal != null && nonTerminal.GrammarType.Equals(Common.DictionaryName))
                {
                    this.BuildDictionaryClass(sourceCode, production, className);
                    classNames.Add(className);
                    buildDefault = false;
                }
            }

            if (buildDefault)
            {
                this.BuildClass(sourceCode, production, className);
                classNames.Add(className);
            }

            sourceCode.CloseBrace(); // namespace
            return new ClassInformation { Name = className, ClassDefinition = sourceCode.ToString() };
        }

        /// <summary>
        /// Build the ISyntax interface code which is the basis for the visitor pattern.
        /// </summary>
        /// <returns>ISyntax interface code.</returns>
        private ClassInformation BuildSyntaxInterface()
        {
            WriteStatus("   Building ISyntax interface");

            CodeWriter sourceCode = new CodeWriter(4);
            string name = "ISyntax";
            WriteHeader(sourceCode, name);
            DisableXmlDocCommentErrors(sourceCode);
            sourceCode.OpenBrace("namespace " + this.grammar.GrammarNamespace);
            sourceCode.WriteLine("/// <summary>");
            sourceCode.WriteLine("/// Interface for generated classes which will contain");
            sourceCode.WriteLine("/// an id used by the visitor pattern.");
            sourceCode.WriteLine("/// </summary>");
            sourceCode.OpenBrace("public interface ISyntax");
            sourceCode.WriteLine();
            sourceCode.WriteLine("/// <summary>");
            sourceCode.WriteLine("/// Sets a property in this instance based on the name of the property.");
            sourceCode.WriteLine("/// </summary>");
            sourceCode.WriteLine("/// <param name=\"name\">The name of the property to set.</param>");
            sourceCode.WriteLine("/// <param name=\"value\">The value to which the property shall be set.</param>");
            sourceCode.WriteLine("bool SetProperty(string name, object value);");
            sourceCode.WriteLine();
            sourceCode.WriteLine(this.grammar.Name + "Kind Id { get; }");
            if (this.generateLocations == true)
            {
                sourceCode.WriteLine();
                sourceCode.WriteLine("System.Int32 Offset { get; set; }");
                sourceCode.WriteLine();
                sourceCode.WriteLine("System.Int32 Length { get; set; }");
            }

            sourceCode.CloseBrace(); // interface
            sourceCode.CloseBrace(); // namespace
            return new ClassInformation { Name = name, ClassDefinition = sourceCode.ToString() };
        }

        private static void DisableXmlDocCommentErrors(CodeWriter sourceCode)
        {
            sourceCode.WriteLine("#pragma warning disable 1591");
        }

        /// <summary>Attempts to coerce the given symbol to a non terminal symbol.</summary>
        /// <param name="symbol">The symbol to convert.</param>
        /// <returns><paramref name="symbol"/> if the symbol can be interpreted as a
        /// <see cref="NonTerminal"/>; otherwise, null.</returns>
        private static NonTerminal TryInterpretAsNonTerminal(IGrammarSymbol symbol)
        {
            StructuralSymbol structural = symbol as StructuralSymbol;
            if (structural == null)
            {
                return symbol as NonTerminal;
            }
            else
            {
                return structural.Symbol as NonTerminal;
            }
        }

        /// <summary>Build a ToString override.</summary>
        /// <param name="sourceCode">Code Writer to use.</param>
        /// <param name="production">The production for the class definition.</param>
        private void BuildToString(CodeWriter sourceCode, Production production)
        {
            List<string> argumentToString = new List<string>();
            List<string> argumentNames = new List<string>();

            foreach (IGrammarSymbol symbol in production.RHS)
            {
                NonTerminal nonTerminal = TryInterpretAsNonTerminal(symbol);
                if (nonTerminal == null)
                {
                    continue;
                }

                TypeInformation typeInformation = this.grammar.LookupType(nonTerminal);
                if (typeInformation == null)
                {
                    continue;
                }

                string argument;
                if (typeInformation.TypeName == "string")
                {
                    argument = "string arg{0} = this.{1} == null ? \"{1}(null)\" : this.{1};";
                }
                else if (typeInformation.IsNullable)
                {
                    argument = "string arg{0} = this.{1} == null ? \"{1}(null)\" : this.{1}.ToString();";
                }
                else
                {
                    argument = "string arg{0} = this.{1}.ToString();";
                }

                argumentToString.Add(String.Format(CultureInfo.InvariantCulture, argument, argumentToString.Count, nonTerminal.Name));
                argumentNames.Add(nonTerminal.Name);
            }

            sourceCode.WriteLine("/// <summary>Generates a debugging string containing the data stored in this instance.</summary>");
            sourceCode.OpenBrace("public override string ToString()");

            List<string> formatData = new List<string>();
            StringBuilder parameters = new StringBuilder();
            for (int index = 0; index < argumentNames.Count; index++)
            {
                sourceCode.WriteLine(argumentToString[index]);
                formatData.Add(argumentNames[index] + " = {" + index + "}");
                parameters.Append(", arg" + index);
            }

            string returnLine = argumentNames.Count > 0 ?
                "return String.Format(CultureInfo.InvariantCulture, \"" + String.Join(", ", formatData) + "\"" + parameters + ");" :
                "return String.Empty";

            sourceCode.WriteLine(returnLine);
            sourceCode.CloseBrace(); // ToString
        }

        /// <summary>Build a ToString override for collections.</summary>
        /// <param name="sourceCode">Code Writer to use.</param>
        /// <param name="production">The production for the class definition.</param>
        private void BuildToStringCollection(CodeWriter sourceCode, Production production)
        {
            sourceCode.WriteLine("/// <summary>Generates a debugging string containing all instances stored in this collection.</summary>");
            sourceCode.OpenBrace("public override string ToString()");
            sourceCode.WriteLine("return \"[\" + String.Join(\", \", this) + \"]\";");
            sourceCode.CloseBrace(); // ToString
        }

        /// <summary>
        /// Build a ToString override for collections.
        /// </summary>
        /// <param name="sourceCode">Code Writer to use.</param>
        /// <param name="entry">Grammar entry for the object.</param>
        private void BuildToStringDictionary(CodeWriter sourceCode, Production production)
        {
            sourceCode.WriteLine("/// <summary>Generates a debugging string containing a non-ordered list of the key/value pairs stored in this dictionary.</summary>");
            sourceCode.OpenBrace("public override string ToString()");
            sourceCode.WriteLine(@"return ""{"" + String.Join("", "", this.Select(entry => ""\"""" + entry.Key + ""\"": \"""" + entry.Value + ""\"""")) + ""}"";");
            sourceCode.CloseBrace(); // ToString
        }

        /// <summary>Builds the Equals and GetHashCode overrides.</summary>
        /// <param name="sourceCode">Code writer to use to build the class.</param>
        /// <param name="production">The production for the class definition.</param>
        private void BuildEquals(CodeWriter sourceCode, Production production)
        {
            TypeInformation thisType = this.grammar.LookupType(production);
            List<string> hashCodeLines = new List<string>();
            List<string> equalsLines = new List<string>();
            foreach (IGrammarSymbol symbol in production.RHS)
            {
                NonTerminal nonTerminal = TryInterpretAsNonTerminal(symbol);
                if (nonTerminal == null)
                {
                    continue;
                }

                TypeInformation typeInformation = this.grammar.LookupType(nonTerminal);
                if (typeInformation == null)
                {
                    continue;
                }

                string hash;
                string equals;
                if (typeInformation.TypeName == "int")
                {
                    hash = "hash = (hash * 31) + this.{0};";
                    equals = "    && this.{0} == other.{0}";
                }
                else if (typeInformation.IsEnumType)
                {
                    hash = "hash = (hash * 31) + (int)this.{0};";
                    equals = "    && this.{0} == other.{0}";
                }
                else if (typeInformation.IsNullable)
                {
                    hash = "hash = (hash * 31) + (this.{0} == null ? 0 : this.{0}.GetHashCode());";
                    equals = "    && Object.Equals(this.{0}, other.{0})";
                }
                else
                {
                    hash = "hash = (hash * 31) + this.{0}.GetHashCode();";
                    equals = "    && this.{0}.Equals(other.{0})";
                }

                hashCodeLines.Add(String.Format(CultureInfo.InvariantCulture, hash, nonTerminal.Name));
                equalsLines.Add(String.Format(CultureInfo.InvariantCulture, equals, nonTerminal.Name));
            }

            WriteGetHashCodeDocComments(sourceCode);
            sourceCode.OpenBrace("public override int GetHashCode()");
            sourceCode.OpenBrace("unchecked");
            sourceCode.WriteLine("int hash = 17;");
            foreach (string hashEntry in hashCodeLines)
            {
                sourceCode.WriteLine(hashEntry);
            }

            sourceCode.WriteLine("return hash;");
            sourceCode.CloseBrace(); // unchecked
            sourceCode.CloseBrace(); // GetHashCode

            sourceCode.WriteLine();
            WriteEqualsDocComments(sourceCode);
            sourceCode.OpenBrace("public override bool Equals(object o)");
            sourceCode.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0} other = o as {0};", thisType.TypeName));
            sourceCode.Write("return other != null");
            foreach (string equalsEntry in equalsLines)
            {
                sourceCode.WriteLine();
                sourceCode.Write(equalsEntry);
            }

            sourceCode.AppendLine(";");
            sourceCode.CloseBrace(); // Equals
        }

        /// <summary>Builds an Equals override for a collection class.</summary>
        /// <param name="sourceCode">Code writer to use to build the class.</param>
        /// <param name="production">The production for the class definition.</param>
        private void BuildEqualsCollection(CodeWriter sourceCode, Production production)
        {
            TypeInformation thisType = this.grammar.LookupType(production);
            WriteGetHashCodeDocComments(sourceCode);
            sourceCode.OpenBrace("public override int GetHashCode()");
            sourceCode.OpenBrace("unchecked");
            sourceCode.WriteLine("int hash = 17;");
            sourceCode.OpenBrace("foreach (var item in this)");
            sourceCode.WriteLine("hash = (hash * 31) + item.GetHashCode();");
            sourceCode.CloseBrace(); // foreach
            sourceCode.WriteLine();
            sourceCode.WriteLine("return hash;");
            sourceCode.CloseBrace(); // unchecked
            sourceCode.CloseBrace(); // GetHashCode

            sourceCode.WriteLine();
            WriteEqualsDocComments(sourceCode);
            sourceCode.OpenBrace("public override bool Equals(object o)");
            sourceCode.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0} other = o as {0};", thisType.TypeName));
            sourceCode.WriteLine("return other != null && this.SequenceEqual(other);");
            sourceCode.CloseBrace(); // Equals
        }

        /// <summary>Builds an Equals override for a dictionary class.</summary>
        /// <param name="sourceCode">Code writer to use to build the class.</param>
        /// <param name="production">The production.</param>
        /// <param name="valueType">Type of the value stored in the dictionary.</param>
        private void BuildEqualsDictionary(CodeWriter sourceCode, Production production, string valueType)
        {
            TypeInformation thisType = this.grammar.LookupType(production);

            // Normally xor GetHashCode is terrible, but using xor here means we
            // don't need to define a total ordering of entries in the dictionary.
            // (We're accepting a statistically bad hash in return for not
            // needing to copy the dictionary contents and sort them)
            WriteGetHashCodeDocComments(sourceCode);
            sourceCode.OpenBrace("public override int GetHashCode()");
            sourceCode.WriteLine("var comparer = this.Comparer;");
            sourceCode.WriteLine("int hash = 0;");
            sourceCode.OpenBrace("foreach (var entry in this)");
            sourceCode.WriteLine("hash = hash ^ comparer.GetHashCode(entry.Key) ^ entry.Value.GetHashCode();");
            sourceCode.CloseBrace(); // foreach
            sourceCode.WriteLine();
            sourceCode.WriteLine("return hash;");
            sourceCode.CloseBrace(); // GetHashCode

            sourceCode.WriteLine();
            WriteEqualsDocComments(sourceCode);
            sourceCode.OpenBrace("public override bool Equals(object o)");
            sourceCode.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0} other = o as {0};", thisType.TypeName));
            sourceCode.OpenBrace("if (other == null || this.Count != other.Count)");
            sourceCode.WriteLine("return false;");
            sourceCode.CloseBrace(); // if
            sourceCode.WriteLine();
            sourceCode.OpenBrace("foreach (var entry in this)");
            sourceCode.WriteLine(valueType + " value;");
            sourceCode.OpenBrace("if (!other.TryGetValue(entry.Key, out value) || !entry.Value.Equals(value))");
            sourceCode.WriteLine("return false;");
            sourceCode.CloseBrace(); // if
            sourceCode.CloseBrace(); // foreach
            sourceCode.WriteLine();
            sourceCode.WriteLine("return true;");
            sourceCode.CloseBrace(); // Equals
        }

        private static void WriteGetHashCodeDocComments(CodeWriter sourceCode)
        {
            sourceCode.WriteLine("/// <summary>Gets a hash code for this instance.</summary>");
        }

        private static void WriteEqualsDocComments(CodeWriter sourceCode)
        {
            sourceCode.WriteLine("/// <summary>Compares this instance with another object, and returns whether or not they contain equivalent data.</summary>");
            sourceCode.WriteLine("/// <param name=\"o\">The object with which this instance shall be compared.</param>");
        }

        /// <summary>
        /// Build the Visitor class.
        /// </summary>
        /// <returns>Visitor class code.</returns>
        private ClassInformation BuildVisitor(VisitorType visitorType)
        {
            CodeWriter switchStatement = new CodeWriter();
            CodeWriter sourceCode = new CodeWriter();
            CodeWriter visitorMethods = new CodeWriter();

            WriteStatus("   Building Visitor");
            switchStatement.IncrementIndentLevel();
            switchStatement.IncrementIndentLevel();
            switchStatement.IncrementIndentLevel();
            switchStatement.OpenBrace("switch (node.Id)");
            visitorMethods.IncrementIndentLevel();
            visitorMethods.IncrementIndentLevel();

            foreach (Production production in this.grammar.Productions)
            {
                TypeInformation typeInfo = this.grammar.LookupType(production);
                if (!(typeInfo != null && typeInfo.IsBaseType))
                {
                    BuildSwitchStatement(switchStatement, production);
                    BuildVisitorMethod(visitorMethods, production, visitorType);
                }
            }

            switchStatement.CloseBrace(); // switch
            string typeOfVisitor = (visitorType == VisitorType.Rewriting) ? "RewritingVisitor" : "Visitor";
            string visitorName = this.grammar.Name + typeOfVisitor;
            this.WriteHeader(sourceCode, visitorName);
            this.WriteUsingStatements(sourceCode);
            DisableXmlDocCommentErrors(sourceCode);
            sourceCode.OpenBrace("namespace " + this.grammar.GrammarNamespace);

            sourceCode.WriteLine("[CompilerGenerated]");
            string genericType = (visitorType == VisitorType.Rewriting) ? "" : "<T>";
            sourceCode.OpenBrace("public class " + visitorName + genericType);
            BuildVisitMethod(sourceCode, visitorType);
            BuildVisitActualMethod(sourceCode, switchStatement.ToString(), visitorType);
            sourceCode.Append(visitorMethods.ToString());
            sourceCode.CloseBrace(); // class
            sourceCode.CloseBrace(); // Namespace

            return new ClassInformation
            {
                Name = visitorName,
                ClassDefinition = sourceCode.ToString()
            };
        }

        /// <summary>
        /// Build the VisitActual method which is calls the visitor correct method.
        /// </summary>
        /// <param name="sourceCode">Code writer to build the code.</param>
        /// <param name="switchStatement">Code for the switch statement.</param>
        /// <param name="visitorType">Generate a rewriting visitor.</param>
        private void BuildVisitActualMethod(CodeWriter sourceCode, string switchStatement, VisitorType visitorType)
        {
            string returnType = (visitorType == VisitorType.Rewriting) ? "object" : "T";
            sourceCode.OpenBrace("public virtual " + returnType + " VisitActual(ISyntax node)");
            sourceCode.OpenBrace("if (node == null)");
            sourceCode.WriteLine("throw new ArgumentNullException(\"node\");");
            sourceCode.CloseBrace(); // if (node == null)
            sourceCode.WriteLine();
            sourceCode.Append(switchStatement);
            sourceCode.WriteLine();
            if (visitorType == VisitorType.Rewriting)
            {
                sourceCode.WriteLine("return node;");
            }
            else
            {
                sourceCode.WriteLine("object obj = node;");
                sourceCode.WriteLine("var result = (T)obj;");
                sourceCode.WriteLine("return result;");
            }
            sourceCode.CloseBrace(); // VisitActual
        }

        /// <summary>
        /// Builds a visitor method for a give production.
        /// </summary>
        /// <param name="visitorMethods">Code writer to use to generate the code.</param>
        /// <param name="production">Grammar production for which to generate the method.</param>
        /// <param name="visitorType">Generate a rewriting visitor.</param>
        private void BuildVisitorMethod(CodeWriter visitorMethods, Production production, VisitorType visitorType)
        {
            // No method is generated for Alternative productions as they represent Enums.
            if (production.RHS[0] is Alternative)
            {
                return;
            }

            string className;
            if (!production.LHS.Annotations.TryGetValue("className", out className))
            {
                className = production.LHS.Name;
            }

            string returnType = (visitorType == VisitorType.Rewriting) ? className : "T";
            visitorMethods.WriteLine();
            visitorMethods.OpenBrace(
                String.Format(CultureInfo.InvariantCulture, "public virtual {0} Visit{1}({1} node)",
                returnType,
                className));
            foreach (IGrammarSymbol symbol in production.RHS)
            {
                NonTerminal nonTerminal = TryInterpretAsNonTerminal(symbol);
                if (nonTerminal == null)
                {
                    continue;
                }

                TypeInformation typeInformation = this.grammar.LookupType(nonTerminal);
                if (typeInformation == null)
                {
                    continue;
                }

                if (typeInformation.IsBaseType)
                {
                    continue;
                }

                // Visit the property.
                string propertyName = nonTerminal.Name;
                Plus plus = production.RHS[0] as Plus;
                Star star = production.RHS[0] as Star;
                if (plus != null || star != null)
                {
                    // For collection generate a for each loop and visit each item.
                    if (visitorType == VisitorType.Rewriting)
                    {
                        visitorMethods.OpenBrace("for (int index = 0; index < node.Count; index++)");
                        visitorMethods.WriteLine(
                            "node[index] = (" +
                            typeInformation.ClassName +
                            ")this.Visit(node[index]);");
                        visitorMethods.CloseBrace(); // for each node
                    }
                    else
                    {
                        visitorMethods.OpenBrace("foreach(var item in node)");
                        visitorMethods.WriteLine("this.Visit(item);");
                        visitorMethods.CloseBrace(); // foreach
                    }
                }
                else
                {
                    // Not a collection, visit the item directly
                    visitorMethods.OpenBrace("if (node." + propertyName + " != null)");
                    string visitProperty;
                    if (visitorType == VisitorType.Rewriting)
                    {
                        visitProperty = String.Format(
                            CultureInfo.InvariantCulture,
                            "node.{0} = ({1})this.Visit(node.{0});",
                            propertyName,
                            typeInformation.ClassName);
                    }
                    else
                    {
                        visitProperty = "this.Visit(node." + propertyName + "); ";
                    }

                    visitorMethods.WriteLine(visitProperty);
                    visitorMethods.CloseBrace(); // if
                }
            }

            visitorMethods.WriteLine();
            if (visitorType == VisitorType.Rewriting)
            {
                visitorMethods.WriteLine("return node;");
            }
            else
            {
                visitorMethods.WriteLine("object obj = node;");
                visitorMethods.WriteLine("var result = (T)obj;");
                visitorMethods.WriteLine("return result;");
            }

            visitorMethods.CloseBrace(); // Visit
        }

        /// <summary>
        /// Build a single case statement for the VisitActual method. 
        /// </summary>
        /// <param name="switchStatement">Code writer to use to generate the code.</param>
        /// <param name="production">The Grammar production with the information to use.</param>
        private void BuildSwitchStatement(CodeWriter switchStatement, Production production)
        {
            string className;
            if (!production.LHS.Annotations.TryGetValue("className", out className))
            {
                className = production.LHS.Name;
            }

            // Alternative symbols represent enums. They may not be visited so don't emit code for them.
            if (production.RHS[0] is Alternative)
            {
                return;
            }

            switchStatement.WriteLine("case " + this.grammar.Name + "Kind." + className + ":");
            switchStatement.IncrementIndentLevel();
            switchStatement.WriteLine("return Visit" + className + "((" + className + ")node);");
            switchStatement.DecrementIndentLevel();
        }

        /// <summary>
        /// Build the generic Visit method.
        /// </summary>
        /// <param name="sourceCode">Code writer to use to generate the code.</param>
        /// <param name="visitorType">Generating a rewriting visitor.</param>
        private void BuildVisitMethod(CodeWriter sourceCode, VisitorType visitorType)
        {
            string returnType = (visitorType == VisitorType.Rewriting) ? "object" : "T";
            sourceCode.OpenBrace("public virtual " + returnType + " Visit(ISyntax node)");
            sourceCode.WriteLine("return VisitActual(node);");
            sourceCode.CloseBrace(); // visit method
            sourceCode.WriteLine();
        }

        /// <summary>
        /// Write the standard set of XML documentation comments from an annotation block.
        /// </summary>
        /// <param name="sourceCode">Code writer to use.</param>
        /// <param name="annotations">Annotations dictionary with comments.</param>
        private void WriteDocumentCommentsForAnnotationSet(CodeWriter sourceCode, IDictionary<string, string> annotations)
        {
            WriteDocumentCommentsForAnnotation(sourceCode, annotations, "summary");
            WriteDocumentCommentsForAnnotation(sourceCode, annotations, "remarks");
        }

        /// <summary>
        /// Writes an annotation with the indicated name as an XML doc comment into the C#.
        /// </summary>
        /// <param name="sourceCode">The code writer to which source shall be emitted.</param>
        /// <param name="annotations">The set of annotations from which an annotation shall be retrieved.</param>
        /// <param name="selectedAnnotation">The annotation to emit from the set.</param>
        private void WriteDocumentCommentsForAnnotation(CodeWriter sourceCode, IDictionary<string, string> annotations, string selectedAnnotation)
        {
            string annotationContent;
            if (annotations.TryGetValue(selectedAnnotation, out annotationContent))
            {
                this.WriteXmlDocComment(sourceCode, selectedAnnotation, annotationContent);
            }
        }

        /// <summary>
        /// Writs an XML doc comment node with the supplied content.
        /// </summary>
        /// <param name="sourceCode">Code writer to use.</param>
        /// <param name="commentType">TagName for the comment: i.e "summary"</param>
        /// <param name="comments">Comments to be added to the tag.</param>
        private void WriteXmlDocComment(CodeWriter sourceCode, string commentType, string comments)
        {
            comments = CommentSanitizer.Sanitize(comments);
            var xml = new XElement(commentType, comments);
            var xmlStr = xml.ToString();
            foreach (string xmlStrLine in CommentSanitizer.SplitByNewlines(xmlStr))
            {
                sourceCode.WriteLine("/// " + xmlStrLine);
            }
        }


        /// <summary>
        /// Write a generic header to the code.
        /// </summary>
        /// <param name="sourceCode">Code writer to use.</param>
        /// <param name="className">Name of generated object.</param>
        private void WriteHeader(CodeWriter sourceCode, string className)
        {
            string inputGrammarName = "//     Input Grammar      : " + this.grammar.Name;
            string inputGrammarFile = "//     Input Grammar file : " + Path.GetFileName(this.grammar.SourcePath);
            string generatedClass = "//     Generated object   : " + className;
            sourceCode.WriteLine("// *********************************************************");
            sourceCode.WriteLine("// *                                                       *");
            sourceCode.WriteLine("// *   Copyright (C) Microsoft. All rights reserved.       *");
            sourceCode.WriteLine("// *                                                       *");
            sourceCode.WriteLine("// *********************************************************");
            sourceCode.WriteLine();
            sourceCode.WriteLine("//----------------------------------------------------------");
            sourceCode.WriteLine("// <auto-generated>");
            sourceCode.WriteLine("//     This code was generated by a tool.");
            sourceCode.WriteLine(inputGrammarName);
            sourceCode.WriteLine(inputGrammarFile);
            sourceCode.WriteLine(generatedClass);
            sourceCode.WriteLine("//     ");
            sourceCode.WriteLine("//     Changes to this file may cause incorrect behavior and ");
            sourceCode.WriteLine("//     will be lost when the code is regenerated.");
            sourceCode.WriteLine("// </auto-generated>");
            sourceCode.WriteLine("//----------------------------------------------------------");
            sourceCode.WriteLine();
        }

        /// <summary>
        /// Write required using statements.
        /// </summary>
        /// <param name="sourceCode">Code writer to use.</param>
        private void WriteUsingStatements(CodeWriter sourceCode)
        {
            sourceCode.WriteLine("using System;");
            sourceCode.WriteLine("using System.Collections.Generic;");
            sourceCode.WriteLine("using System.Globalization;");
            sourceCode.WriteLine("using System.Linq;");
            sourceCode.WriteLine("using System.Runtime.CompilerServices;");
            sourceCode.WriteLine("using System.Runtime.Serialization;");
            sourceCode.WriteLine("using System.Text;");
            sourceCode.WriteLine("using System.Text.RegularExpressions;");
            sourceCode.WriteLine("using Microsoft.SecurityDevelopmentLifecycle.SdlCommon;");
            sourceCode.WriteLine();
        }

        private void WriteStatus(string text)
        {
            if (verbose)
            {
                Console.WriteLine(text);
            }
        }
    }
}
