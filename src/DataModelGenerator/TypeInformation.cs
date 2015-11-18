// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************
namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>
    /// TypeInformation contains the classname and typename from the input grammars and a flag to indicate if the 
    /// type is a base type which does not require code generation.
    /// </summary>
    public class TypeInformation
    {
        public TypeInformation(string className, string typeName, string serializedName, bool isBaseType, bool isEnumType, bool isNullable)
        {
            this.ClassName = className;
            this.TypeName = typeName;
            this.SerializedName = serializedName;
            this.IsBaseType = isBaseType;
            this.IsEnumType = isEnumType;
            this.IsNullable = isNullable;
        }

        // The un-translated class name as annotated in the grammar with @className; e.g. NUMBER
        public readonly string ClassName;
        // The name that should be displayed in C# for the type name (e.g. MyClass)
        public readonly string TypeName;
        // The name that should be written into JSON (e.g. myClass)
        public readonly string SerializedName;
        // Whether or not this type is generated as a class type. (e.g. string doesn't have anything generated; MyFooType does)
        public readonly bool IsBaseType;
        // Whether or not the type is an enum.
        public readonly bool IsEnumType;
        // Whether or not the type makes sense to compare with null.
        public readonly bool IsNullable;
    }
}