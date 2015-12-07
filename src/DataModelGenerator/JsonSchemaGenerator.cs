using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    public static class JsonSchemaGenerator
    {
        private const string SARIF_URL = "http://json.schemastore.org/sarif";
        internal static void WriteSchema(CodeWriter codeWriter, DataModel model)
        {
            WriteHeader(codeWriter, model.SourceFilePath, model.MetaData);
            WriteTypes(codeWriter, model);
            WriteFooter(codeWriter);
        }

        private static void WriteHeader(CodeWriter codeWriter, string sourceFilePath, DataModelMetadata metaData)
        {
            // TODO this file shouldn't contain any references whatsoever to SARIF
            codeWriter.OpenBrace();
            codeWriter.WriteLine(@"""id"": """ + SARIF_URL + @""",");
            codeWriter.WriteLine(@"""$schema"": ""http://json-schema.org/draft-04/schema#"",");
            codeWriter.WriteLine(@"""title"": ""Static Analysis Results Format (SARIF) Version 1.0 JSON Schema (Draft 0.4)"",");
        }

        private static void WriteFooter(CodeWriter codeWriter)
        {
            codeWriter.CloseBrace();
        }

        private static void WriteTypes(CodeWriter codeWriter, DataModel model)
        {
            DataModelType rootType = model.Types.Where((dmt) => dmt.RootObject == true).First();
            DataModelType[] types = model.Types.ToArray();

            WriteTypeMembers(codeWriter, model, rootType, false);


            codeWriter.WriteLine(@"""definitions"":");
            codeWriter.OpenBrace();

            for (int i = 0; i < types.Length; i++)
            {
                DataModelType type = types[i];
                bool lastType = (i == types.Length - 1);
            
                if (type == rootType) { continue;  }

                switch (type.Kind)
                {
                    case DataModelTypeKind.Base:
                    case DataModelTypeKind.Leaf:                    
                    WriteDefinition(codeWriter, model, type, lastType);
                    break;
                    case DataModelTypeKind.BuiltInNumber:
                    case DataModelTypeKind.BuiltInString:
                    case DataModelTypeKind.BuiltInDictionary:
                    case DataModelTypeKind.BuiltInBoolean:
                    case DataModelTypeKind.BuiltInVersion:
                    case DataModelTypeKind.BuiltInUri:
                    case DataModelTypeKind.Enum:
                    // Don't write builtin types
                    break;
                    case DataModelTypeKind.Default:
                    default:
                    Debug.Fail("Unexpected data model type kind in a data model " + type.Kind);
                    break;
                }
            }
            codeWriter.CloseBrace();
        }

        private static void WriteTypeMembers(CodeWriter codeWriter, DataModel model, DataModelType type, bool lastMember)
        {
            if (!string.IsNullOrEmpty(type.SummaryText))
            {
                codeWriter.WriteLine(@"""description"": """ + BuildDescription(type.SummaryText) + @""",");
            }

            codeWriter.WriteLine(@"""type"": ""object"",");
            codeWriter.WriteLine(@"""properties"": {");
            codeWriter.IncrementIndentLevel();

            WriteMembers(codeWriter, model, type);
            codeWriter.CloseBrace(",");

            WriteRequiredMember(codeWriter, model, type);
            codeWriter.WriteLine(@"""additionalProperties"": false" + (lastMember ? "" : ","));
        }
        private static void WriteDefinition(CodeWriter codeWriter, DataModel model, DataModelType type, bool lastType)
        {
            codeWriter.WriteLine(@"""" + type.G4DeclaredName + @""": ");
            codeWriter.OpenBrace();
            WriteTypeMembers(codeWriter, model, type, true);
            codeWriter.CloseBrace(lastType ? "" : ","); 
        }

        private static void WriteRequiredMember(CodeWriter codeWriter, DataModel model, DataModelType type)
        {
            List<string> requiredMembers = new List<string>();

            foreach (DataModelMember member in type.Members)
            {
                if (member.Required)
                {
                    requiredMembers.Add(member.SerializedName);
                }
            }

            if (requiredMembers.Count > 0)
            {                
                var sb = new StringBuilder(@"""required"": [");
                for (int i = 0; i < requiredMembers.Count; i++)
                {
                    string name = requiredMembers[i];
                    sb.Append(@"""" + name + @"""" + (i != requiredMembers.Count - 1 ? ", " : ""));
                }
                codeWriter.WriteLine(sb.ToString() + "],");
            }
        }

        private static void WriteMembers(CodeWriter codeWriter, DataModel model, DataModelType type)
        {
            DataModelMember[] members = type.Members.ToArray();

            for (int i = 0; i < members.Length; i++)
            {
                DataModelMember member = members[i];
                bool lastMember = (i == members.Length - 1);

                if (model.MetaData.GenerateLocations && (member.CSharpName == "Length" || member.CSharpName == "Offset"))
                {
                    continue;
                }

                if (i > 0) { codeWriter.WriteLine(); }

                WriteProperty(codeWriter, model, member, lastMember);
            }
        }

        private static void WriteProperty(CodeWriter codeWriter, DataModel model, DataModelMember member, bool lastMember)
        {
            string memberName = member.SerializedName;
            DataModelType memberType = model.GetTypeByG4Name(member.DeclaredName);
            string memberTypeName = memberType.G4DeclaredName;

            codeWriter.WriteLine(@"""" + memberName + @""": ");
            codeWriter.OpenBrace();

            codeWriter.WriteLine(@"""description"": """ + BuildDescription(member.SummaryText) + @""",");

            string canonicalizedName, format;
            bool isPrimitive = CanonicalizeTypeName(memberTypeName, out canonicalizedName, out format);

            int rank = member.Rank;
            while (rank > 0) { WriteArrayStart(codeWriter, member); rank--; }

            string defaultValue = member.Default;
            bool hasDefault = !string.IsNullOrEmpty(defaultValue);

            if (isPrimitive)
            {
                string pattern = member.Pattern;
                string minimum = member.Minimum;

                bool hasMinimum = !(string.IsNullOrEmpty(minimum));
                bool hasPattern = !string.IsNullOrEmpty(pattern);
                bool hasFormat = !string.IsNullOrEmpty(format);

                codeWriter.WriteLine(@"""type"": """ + canonicalizedName + @"""" + ((hasPattern || hasFormat || hasMinimum || hasDefault) ? "," : ""));

                if (hasFormat)
                {
                    codeWriter.WriteLine(@"""format"": """ + format + @"""" + ((hasPattern || hasMinimum || hasDefault) ? "," : ""));
                }

                if (hasPattern)
                {
                    codeWriter.WriteLine(@"""pattern"": """ + pattern + @"""" + ((hasMinimum || hasDefault) ? "," : ""));
                }

                if (hasMinimum)
                {
                    codeWriter.WriteLine(@"""minimum"": " + minimum + (hasDefault ? "," : ""));
                }

                if (hasDefault)
                {
                    codeWriter.WriteLine(@"""default"": " + defaultValue);
                }
            }
            else if (memberType.Kind == DataModelTypeKind.Enum)
            {
                var sb = new StringBuilder();
                sb.Append(@"""enum"": [ ");

                if (hasDefault)
                {
                    codeWriter.WriteLine(@"""default"": " + defaultValue + ",");
                }

                for (int i = 0; i < memberType.G4DeclaredValues.Length; i++)
                {
                    bool isFinal = i == memberType.G4DeclaredValues.Length - 1;
                    string name = memberType.G4DeclaredValues[i].Replace("'", "");
                    sb.Append(@"""" + name + @"""" + (isFinal ? "" : ","));
                }
                codeWriter.WriteLine(sb.ToString() + "]");
            }
            else
            {
                codeWriter.WriteLine(@"""$ref"": ""#/definitions/" + canonicalizedName + @"""");
            }
            rank = member.Rank;
            while (rank > 0) { WriteArrayEnd(codeWriter); rank--; }
            codeWriter.CloseBrace(lastMember ? "" : ",");

        }

        private static string BuildDescription(string summaryText)
        {
            summaryText = summaryText.Replace(Environment.NewLine, " ");
            summaryText = summaryText.Replace("\t", "");

            int currentLength;
            do
            {
                currentLength = summaryText.Length;
                summaryText = summaryText.Replace("  ", " ");
            } while (summaryText.Length != currentLength);

            return summaryText.Trim();
        }

        private static void WriteArrayEnd(CodeWriter codeWriter)
        {
            codeWriter.CloseBrace();
        }

    private static void WriteArrayStart(CodeWriter codeWriter, DataModelMember member)
        {
            codeWriter.WriteLine(@"""type"": ""array"",");

            if (!string.IsNullOrEmpty(member.MinItems))
            {
                codeWriter.WriteLine(@"""minItems"": " + member.MinItems +",");
            }

            codeWriter.WriteLine(@"""items"":");
            codeWriter.OpenBrace();
        }

        private static bool CanonicalizeTypeName(string memberType, out string canonicalizedName, out string format)
        {
            format = null;
            switch (memberType)
            {
                case ("STRING"): { canonicalizedName = "string"; return true; }
                case ("DICTIONARY"): { canonicalizedName = "object"; return true; }
                case ("INTEGER"): { canonicalizedName = "integer"; return true; }
                case ("BOOLEAN"): { canonicalizedName = "boolean"; return true; }
                case ("URI"): { canonicalizedName = "string"; format = "uri";  return true; }
            }

            canonicalizedName = memberType;
            return false;
        }
    }
}
