// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.Json.Schema;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// This visitor will exhaustively populate a SARIF log with default values, starting at 
    /// whatever level in the tree is visited. This has the effect of comprehehsively 
    /// 'hydrating' all possible SARIF constructs as defined by the C# OM.
    /// </summary>
    public class DefaultObjectPopulatingVisitor : SarifRewritingVisitor
    {
        public delegate object PrimitiveValueBuilder(bool isRequired);

        // The default builders create a placeholder value for all required property primitives
        // and emits default (null, 0, false, etc.) values for the remainder. As a result, a
        // default hydration of this class should contain no non-default values for 
        // JSON primitive-valued properties that aren't required
        public static IDictionary<Type, PrimitiveValueBuilder> GetBuildersForRequiredPrimitives()
        {
            return new Dictionary<Type, PrimitiveValueBuilder>()
            {
                { typeof(bool),    (isRequired) => { return isRequired; } },
                { typeof(int),     (isRequired) => { return isRequired ? int.MaxValue : 0; } },
                { typeof(double),  (isRequired) => { return isRequired ? double.MaxValue : 0; } },
                { typeof(string),  (isRequired) => { return isRequired ? "Required string value." : null; } },
                { typeof(DateTime),(isRequired) => { return isRequired ? DateTime.UtcNow : new DateTime(); } },
                { typeof(Uri),     (isRequired) => { return isRequired ? new Uri("https://required.uri.contoso.com") : null; } }
            };
        }

        // These builders exhaustively populate the generated SARIF file, including non-required JSON primitives
        public static IDictionary<Type, PrimitiveValueBuilder> GetBuildersForAllPrimitives()
        {
            // The values in this output are designed to allow conformance to the schema. In our schema,.
            // for example, some integer values must be > 0. And so, our optional integer value is 
            // written as 1.
            return new Dictionary<Type, PrimitiveValueBuilder>()
            {
                { typeof(bool),    (isRequired) => { return true; } },
                { typeof(int),     (isRequired) => { return isRequired ? int.MaxValue : 1; } },
                { typeof(double),  (isRequired) => { return isRequired ? double.MaxValue : 1; } },
                { typeof(string),  (isRequired) => { return isRequired ? "[Required string value]" : "[Optional string value]"; } },
                { typeof(DateTime),(isRequired) => { return isRequired ? new DateTime(2018, 10, 31).ToUniversalTime() : new DateTime(1776, 7, 4).ToUniversalTime(); } },
                { typeof(Uri),     (isRequired) => { return isRequired ? new Uri("https://required.uri.value.contoso.com") : new Uri("https://optional.uri.value.contoso.com"); } }
            };
        }

        public DefaultObjectPopulatingVisitor(JsonSchema schema, IDictionary<Type, PrimitiveValueBuilder> primitiveValueBuilders = null)
        {
            _schema = schema;
            _typeToPropertyValueConstructorMap = primitiveValueBuilders ?? GetBuildersForRequiredPrimitives();
        }

        private readonly JsonSchema _schema;
        private readonly IDictionary<Type, PrimitiveValueBuilder> _typeToPropertyValueConstructorMap;

        // There are two places in the format where objects nest references to the defining 
        // type. 'exception.innerExceptions' is an array of exception objects. 
        // 'node.children' is another collection of nodes. We need to watch for these
        // scenarios to prevent re-entrancy and eventual stack consumption when 
        // producing samples of these types.
        private int graphNodeParentCount;
        private int exceptionDataParentCount;

        public override object VisitActual(ISarifNode node)
        {
            PopulateInstanceWithDefaultMemberValues(node);

            return base.VisitActual(node);
        }

        // Retain nesting level for visiting exceptions to prevent
        // unbounded re-entrance populating exception.innerExceptions
        public override ExceptionData VisitExceptionData(ExceptionData node)
        {
            if (exceptionDataParentCount > 1) { throw new InvalidOperationException(); };

            exceptionDataParentCount++;
            node = base.VisitExceptionData(node);
            exceptionDataParentCount--;

            return node;
        }

        // Retain nesting level for visiting graph nodes, to prevent
        // unbounded re-entrance populating exception.innerExceptions
        public override Node VisitNode(Node node)
        {
            if (graphNodeParentCount > 1) { throw new InvalidOperationException(); };

            graphNodeParentCount++;
            node = base.VisitNode(node);
            graphNodeParentCount--;

            return node;
        }

        private void PopulateInstanceWithDefaultMemberValues(ISarifNode node)        {
           
            Type nodeType = node.GetType();
            var binding = BindingFlags.Public | BindingFlags.Instance;
            foreach (PropertyInfo property in nodeType.GetProperties(binding))
            {
                // The node kind is always properly set in the OM and
                // isn't relevant to the SARIF schema itself
                if (property.Name == "SarifNodeKind") { continue; }

                // Property bags and tags are populated via special methods on 
                // the class rather than direct access of properties.
                if (property.Name == "PropertyNames" || 
                    property.Name == "Tags")
                {
                    continue;
                }

                PopulatePropertyWithDefaultValue(node, property);
            }

            // If we have a property bag holder (as most SARIF things are), we will 
            // add and then immediately remove both a property and a tag. This has
            // the effect of leaving non-null but empty properties collection.
            var propertyBagHolder = node as PropertyBagHolder;
            if (propertyBagHolder != null)
            {
                string meaninglessValue = "ToBeRemoved";
                propertyBagHolder.SetProperty(propertyName: meaninglessValue, value: meaninglessValue);
                propertyBagHolder.RemoveProperty(propertyName: meaninglessValue);

                propertyBagHolder.Tags.Add(meaninglessValue);
                propertyBagHolder.Tags.Remove(meaninglessValue);
            }
        }

        private void PopulatePropertyWithDefaultValue(ISarifNode node, PropertyInfo property)
        {
            object propertyValue = null;
            Type propertyType = property.PropertyType;

            // If we can't set this property, it is not of interest
            if (property.GetAccessors().Length != 2) { return; }

            // This special-casing is required to account for the fact that an 
            // exception instance itself contains an array of exceptions
            // (exception.innerExceptions). We don't want to hydrate 
            // the innerExceptions of any innerExceptions, which results
            // in re-entrancy that ends up consuming all stack space.
            //
            // Once we have populated a single exceptions.innerExceptions array, 
            // we have accomplished all the testing we need in any case.
            if (node.SarifNodeKind == SarifNodeKind.ExceptionData &&
                property.Name == "InnerExceptions" &&
                exceptionDataParentCount > 0) { return; }

            // Sample approach applies for graph node.children
            if (node.SarifNodeKind == SarifNodeKind.Node &&
                property.Name == "Children" && 
                graphNodeParentCount > 0) { return; }

            bool isRequired = PropertyIsRequiredBySchema(node.GetType().Name, property.Name);

            if (_typeToPropertyValueConstructorMap.TryGetValue(propertyType, out PrimitiveValueBuilder propertyValueConstructor))
            {
                propertyValue = propertyValueConstructor(isRequired);
            }
            else if (HasParameterlessConstructor(property.PropertyType))
            {
                propertyValue = Activator.CreateInstance(propertyType);
            }
            else if (IsList(propertyType))
            {
                propertyValue = CreateEmptyList(propertyType);

                Type genericTypeArgument = propertyType.GenericTypeArguments[0];
                object listElement = null;

                // For arrays that are populated with SARIF types, we will instantiate a 
                // single object instance and put it into the array. This allows the 
                // default populating visitor to continue to explore the object model and
                // exercise nested types. This approach prevents comprehensive testing of
                // the arrays populated in this way (because they are non-empty
                if (genericTypeArgument.FullName.StartsWith("Microsoft.CodeAnalysis.Sarif."))
                {
                    listElement = Activator.CreateInstance(propertyType.GenericTypeArguments[0]);
                }
                else if (_typeToPropertyValueConstructorMap.TryGetValue(genericTypeArgument, out PrimitiveValueBuilder propertyValueBuilder))
                {
                    listElement = propertyValueBuilder(isRequired);
                }

                AddElementToList(propertyValue, listElement);
            }
            else if (IsDictionary(propertyType))
            {
                Type genericTypeArgument = propertyType.GenericTypeArguments[1];

                // We do not populate any propert bags directly. Instead, we will use the
                // IPropertyBagHolder API to instantiate and then empty these constructs
                if (genericTypeArgument != typeof(SerializedPropertyInfo))
                {
                    propertyValue = CreateEmptyDictionary(propertyType);

                    // For dictionaries that are populated with SARIF types, we will instantiate a 
                    // single object instance and store it using an arbitrary key. This approach
                    // ensures we populate the SARIF sample with all possible object types
                    if (genericTypeArgument.FullName.StartsWith("Microsoft.CodeAnalysis.Sarif."))
                    {
                        object dictionaryValue = Activator.CreateInstance(genericTypeArgument);
                        AddElementToDictionary(propertyValue, dictionaryValue);
                    }
                }
            }

            property.SetValue(node, propertyValue);
        }

        // Converts a .NET object + property name to their JSON equivalents and 
        // determines whether that property is required according to the schema.
        private bool PropertyIsRequiredBySchema(string objectTypeName, string propertyName)
        {
            // SarifLog.Version will be converterd to sarifLog.version

            string jsonTypeName = GetJsonNameFor(objectTypeName);
            string jsonPropertyName = GetJsonNameFor(propertyName);

            // If _schema.Definitions does not contain the jsonTypeName, we are operating
            // against the root sarifLog schema, which is what's stored in _schema
            JsonSchema propertySchema = _schema.Definitions.ContainsKey(jsonTypeName) ? _schema.Definitions[jsonTypeName] : _schema;

            return propertySchema.Required != null && propertySchema.Required.Contains(jsonPropertyName);
        }

        private string GetJsonNameFor(string name)
        {
            return name.Substring(0, 1).ToLower() + name.Substring(1);
        }

        private void AddElementToDictionary(object dictionary, object dictionaryValue)
        {
            var dictionaryType = dictionary.GetType();
            MethodInfo method = dictionaryType.GetMethod("Add");
            method.Invoke(dictionary, new[] { "PlaceholderDictionaryKey", dictionaryValue });
        }

        private void AddElementToList(object list, object listElement)
        {
            var listType = list.GetType();
            MethodInfo method = listType.GetMethod("Add");
            method.Invoke(list, new [] { listElement});
        }

        private object CreateEmptyList(Type propertyType)
        {
            // The SARIF OM defines various properties as IList<T>. This abstract type
            // cannot be instantiated directly. We will construct an instance of the 
            // concrete List<T> class, using the generic type argument from the OM.
            // These constructed types are creatable via a parameterless constructor
            // (which is what is ultimately invoked by Activator.CreateInstance).
            var listType = typeof(List<>);
            var constructedType = listType.MakeGenericType(propertyType.GenericTypeArguments[0]);
            return Activator.CreateInstance(constructedType);
        }

        private object CreateEmptyDictionary(Type propertyType)
        {
            // The SARIF OM defines various properties as IDictionary<string, T>. This abstract
            // type cannot be instantiated directly. We will construct an instance of the 
            // concrete Dictionary<string, T> class, using the generic type argument from the OM.
            // These constructed types are creatable via a parameterless constructor
            // (which is what is ultimately invoked by Activator.CreateInstance).
            var listType = typeof(Dictionary<,>);
            var constructedType = listType.MakeGenericType(propertyType.GenericTypeArguments[0], propertyType.GenericTypeArguments[1]);
            return Activator.CreateInstance(constructedType);
        }

        private static bool IsList(Type propertyType)
        {
            return propertyType.Name.StartsWith("IList");
        }

        private bool IsDictionary(Type propertyType)
        {
            return propertyType.Name.StartsWith("IDictionary");
        }

        private bool HasParameterlessConstructor(Type propertyType)
        {
            return propertyType.GetConstructor(new Type[] { }) != null;
        }
    }
}