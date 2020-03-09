// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
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
                { typeof(string),  (isRequired) => { return isRequired ? "string/required" : null; } },
                { typeof(DateTime),(isRequired) => { return isRequired ? DateTime.UtcNow : new DateTime(); } },
                { typeof(Uri),     (isRequired) => { return isRequired ? new Uri("https://required.uri.contoso.com") : null; } }
            };
        }

        // These builders exhaustively populate the generated SARIF file, including non-required JSON primitives.
        public static IDictionary<Type, PrimitiveValueBuilder> GetBuildersForAllPrimitives()
        {
            // The values in this output are designed to allow conformance to the schema. In our schema,
            // for example, some integer values must be > 0. And so, our optional integer value is 
            // written as 1.
            return new Dictionary<Type, PrimitiveValueBuilder>()
            {
                { typeof(bool),    (isRequired) => { return true; } },
                { typeof(int),     (isRequired) => { return isRequired ? int.MaxValue : 1; } },
                { typeof(double),  (isRequired) => { return isRequired ? double.MaxValue : 1; } },
                { typeof(string),  (isRequired) => { return isRequired ? "string/required" : "string/optional"; } },
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
        private bool _visitingGraphNode;
        private bool _visitingExceptionData;


        public override object Visit(ISarifNode node)
        {
            // We override a very low level visit, one that occurs for every SARIF object instance.
            // We do this for two reasons: 1) it would be unnecessarily burdensome to override
            // every specific VisitXXX method in order to populate it, 2) this approach, even
            // if undertaken, would be quite fragile. Visit methods that disappear would break our 
            // build. More worrisome, the code would miss newly introduced types on the visitor.
            //
            // We could have avoided the visitor altogether and simply performed recursive reflection
            // over a root SarifLog instance. We use this mechanism in order to minimize introducing
            // an entirely one-off construct, to glean whatever small measure of reuse is afforded 
            // by the visitor, and based on an assumption that future check-ins will utilize 
            // chained visitors and/or utilize more core visitor functionality. 
            PopulateInstanceWithDefaultMemberValues(node);

            return base.Visit(node);
        }

        public override Result VisitResult(Result node)
        {
            return base.VisitResult(node);
        }

        // Retain nesting level for visiting exceptions to prevent
        // unbounded re-entrance populating exception.innerExceptions
        public override ExceptionData VisitExceptionData(ExceptionData node)
        {
            _visitingExceptionData = true;
            node = base.VisitExceptionData(node);
            _visitingExceptionData = false;

            return node;
        }

        // Retain nesting level for visiting graph nodes, to prevent
        // unbounded re-entrance populating exception.innerExceptions
        public override Node VisitNode(Node node)
        {
            _visitingGraphNode = true;
            node = base.VisitNode(node);
            _visitingGraphNode = false;

            return node;
        }

        private void PopulateInstanceWithDefaultMemberValues(ISarifNode node)
        {
            Type nodeType = node.GetType();

            BindingFlags binding = BindingFlags.Public | BindingFlags.Instance;
            foreach (PropertyInfo property in nodeType.GetProperties(binding))
            {
                // The node kind is always properly set in the OM and
                // isn't relevant to the SARIF schema itself
                if (property.Name == "SarifNodeKind") { continue; }

                // Property bags and tags are populated via special methods on 
                // the class rather than direct access of properties. These 
                // property names extend from the PropertyBagHolder base type
                // that all properties-bearing types extend (nearly every SARIF
                // class at this point).
                if (property.Name == "PropertyNames" ||
                    property.Name == "Tags")
                {
                    continue;
                }

                if (ShouldExcludePopulationDueToOneOfCriteria(node.GetType().Name, property.Name))
                {
                    continue;
                }

                object defaultValue = null;

                MemberInfo member = nodeType.GetMember(property.Name)[0];
                foreach (CustomAttributeData attributeData in member?.GetCustomAttributesData())
                {
                    if (attributeData.AttributeType.FullName != "System.ComponentModel.DefaultValueAttribute") { continue; }
                    defaultValue = attributeData.ConstructorArguments[0].Value;

                    // DefaultValue of -1 is to ensure an actual value of 0 will not be ignored during serialization,
                    // Hence, we will substitute them with 0.
                    if (defaultValue is int defaultIntValue && defaultIntValue == -1)
                    {
                        defaultValue = 0;
                    }
                }

                // If the member is decorated with a default value, we'll inject it. Otherwise,
                // we'll compute a default value based on the node type
                if (defaultValue != null)
                {
                    property.SetValue(node, defaultValue);
                    continue;
                }
                PopulatePropertyWithGeneratedDefaultValue(node, property);
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

        // Converts the property name to it's JSON equivalent and 
        // determines whether that property should be excluded from population if it's in the "OneOf" list according to the schema.
        private bool ShouldExcludePopulationDueToOneOfCriteria(string objectTypeName, string propertyName)
        {
            string jsonPropertyName = GetJsonNameFor(propertyName);
            JsonSchema propertySchema = GetJsonSchemaForObject(objectTypeName);

            if (propertySchema.OneOf == null || propertySchema.OneOf.Count == 0)
            {
                return false;
            }

            bool isPropertyInOneOfSubset = false;

            foreach (JsonSchema item in propertySchema.OneOf)
            {
                if (item.Required != null && item.Required.Contains(jsonPropertyName))
                {
                    isPropertyInOneOfSubset = true;
                    break;
                }
            }

            // The current property is not in any oneOf.required subset, hence treat like a regular property
            if (!isPropertyInOneOfSubset)
            {
                return false;
            }

            return !ShouldThisOneOfPropertyPopulate(propertySchema, jsonPropertyName);
        }

        private bool ShouldThisOneOfPropertyPopulate(JsonSchema propertySchema, string jsonPropertyName)
        {
            // Only one of the subsets in 'OneOf' list must be present to validate successfully.
            // ex:
            //  OneOf : [
            //      {
            //          "required" : ["p1", "p2"]
            //      },
            //      {
            //          "required": [ "q1", "q2", "q3" ]
            //      },
            //  ]
            //  Either (p1 & p2) should be populated or (q1, q2, q3) should be populated, but NEVER both.

            // We should populate only the first property in the list and ignore others.
            // This means this method must return true only for first property and false for others.

            int indexToPopulate = GenerateIndexToPopulateForOneOfProperties();

            if (propertySchema.OneOf[indexToPopulate].Required != null && propertySchema.OneOf[indexToPopulate].Required.Contains(jsonPropertyName))
            {
                return true;
            }

            // if we reach here, this must be a property in a oneOf.required subset but which is not at indexToPopulate.
            // return false to ensure this property is not populated.
            return false;
        }

        private int GenerateIndexToPopulateForOneOfProperties()
        {
            // TODO: Have randomization logic to ensure a valid random index is generated during population.
            // For now, we simply populate the first "OneOf" set.
            return 0;
        }

        private void PopulatePropertyWithGeneratedDefaultValue(ISarifNode node, PropertyInfo property)
        {
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
                _visitingExceptionData) { return; }

            // Similar approach applies for graph node.children
            if (node.SarifNodeKind == SarifNodeKind.Node &&
                property.Name == "Children" &&
                _visitingGraphNode) { return; }

            object propertyValue = null;
            Type propertyType = property.PropertyType;

            // isRequired flag ensures we don't end up generating a SARIF file that's missing a required property or an anyOf required property,
            // because such a file wouldn't validate.
            bool isRequired = PropertyIsRequiredBySchema(node.GetType().Name, property.Name) ||
                PropertyIsAnyOfRequiredBySchema(node.GetType().Name, property.Name);

            if (GetPropertyFormatPattern(node.GetType().Name, property.Name) is string propertyFormatPattern)
            {
                propertyValue = GetFormattedStringValue(propertyFormatPattern);
            }
            else if (_typeToPropertyValueConstructorMap.TryGetValue(propertyType, out PrimitiveValueBuilder propertyValueBuilder))
            {
                propertyValue = propertyValueBuilder(isRequired);
            }
            else if (HasParameterlessConstructor(propertyType))
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
                else if (_typeToPropertyValueConstructorMap.TryGetValue(genericTypeArgument, out propertyValueBuilder))
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
            else if ((property.PropertyType.BaseType == typeof(Enum)))
            {
                // This code sets any enum to the first non-zero value we encounter
                foreach (object enumValue in Enum.GetValues(property.PropertyType))
                {
                    if ((int)enumValue != 0)
                    {
                        propertyValue = enumValue;
                        break;
                    }
                }

                // This code ensures both that we encounter an enum value that is non-zero,
                // and that no enum definitions skips the value of one in its definition
                if ((int)propertyValue != 1)
                {
                    throw new InvalidOperationException();
                }
            }

            property.SetValue(node, propertyValue);
        }

        // Converts a .NET object + property name to their JSON equivalents and 
        // determines whether that property is required according to the schema.
        private bool PropertyIsRequiredBySchema(string objectTypeName, string propertyName)
        {
            string jsonPropertyName = GetJsonNameFor(propertyName);
            JsonSchema propertySchema = GetJsonSchemaForObject(objectTypeName);

            return propertySchema.Required != null && propertySchema.Required.Contains(jsonPropertyName);
        }

        private bool PropertyIsAnyOfRequiredBySchema(string objectTypeName, string propertyName)
        {
            string jsonPropertyName = GetJsonNameFor(propertyName);
            JsonSchema propertySchema = GetJsonSchemaForObject(objectTypeName);

            if (propertySchema.AnyOf?.Count > 0 == true)
            {
                // TODO: Add additional logic to randomly select one or more required subsets and populate only those.

                // As a quick fix, we will populate all properties which are in any required subset.
                // This means this method must return true for all properties in any required subset in the list.
                foreach (JsonSchema item in propertySchema.AnyOf)
                {
                    if (item.Required != null && item.Required.Contains(jsonPropertyName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private JsonSchema GetJsonSchemaForObject(string objectTypeName)
        {
            string jsonTypeName = GetJsonNameFor(objectTypeName);

            // If _schema.Definitions does not contain the jsonTypeName, we are operating
            // against the root sarifLog schema, which is what's stored in _schema
            JsonSchema propertySchema = _schema.Definitions.ContainsKey(jsonTypeName) ? _schema.Definitions[jsonTypeName] : _schema;
            return propertySchema;
        }

        private string GetJsonNameFor(string name)
        {
            // E.g.: SarifLog.Version will be converted to sarifLog.version
            return name.Substring(0, 1).ToLowerInvariant() + name.Substring(1);
        }

        private void AddElementToDictionary(object dictionary, object dictionaryValue)
        {
            Type dictionaryType = dictionary.GetType();
            MethodInfo method = dictionaryType.GetMethod("Add");
            method.Invoke(dictionary, new[] { "key", dictionaryValue });
        }

        private void AddElementToList(object list, object listElement)
        {
            Type listType = list.GetType();
            MethodInfo method = listType.GetMethod("Add");
            method.Invoke(list, new[] { listElement });
        }

        private object CreateEmptyList(Type propertyType)
        {
            // The SARIF OM defines various properties as IList<T>. This abstract type
            // cannot be instantiated directly. We will construct an instance of the 
            // concrete List<T> class, using the generic type argument from the OM.
            // These constructed types are creatable via a parameterless constructor
            // (which is what is ultimately invoked by Activator.CreateInstance).
            Type listType = typeof(List<>);
            Type constructedType = listType.MakeGenericType(propertyType.GenericTypeArguments[0]);
            return Activator.CreateInstance(constructedType);
        }

        private object CreateEmptyDictionary(Type propertyType)
        {
            // The SARIF OM defines various properties as IDictionary<string, T>. This abstract
            // type cannot be instantiated directly. We will construct an instance of the 
            // concrete Dictionary<string, T> class, using the generic type argument from the OM.
            // These constructed types are creatable via a parameterless constructor
            // (which is what is ultimately invoked by Activator.CreateInstance).
            Type listType = typeof(Dictionary<,>);
            Type constructedType = listType.MakeGenericType(propertyType.GenericTypeArguments[0], propertyType.GenericTypeArguments[1]);
            return Activator.CreateInstance(constructedType);
        }

        private static bool IsList(Type propertyType)
        {
            return propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IList<>);
        }

        private bool IsDictionary(Type propertyType)
        {
            return propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }

        private bool HasParameterlessConstructor(Type propertyType)
        {
            return propertyType.GetConstructor(new Type[] { }) != null;
        }

        private string GetFormattedStringValue(string propertyFormatPattern)
        {
            switch (propertyFormatPattern)
            {
                case "^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$":
                {
                    return "0DB2CD87-8185-49F8-8EEA-CE07A0E95241";
                }
                case "[^/]+/.+":
                {
                    return "text/x-csharp";
                }
                case "^[a-z]{2}-[A-Z]{2}$":
                {
                    return "en-ZA";
                }
                case "[0-9]+(\\.[0-9]+){3}":
                {
                    return "2.7.1500.12";
                }
            }
            return null;
        }

        private string GetPropertyFormatPattern(string objectTypeName, string propertyName)
        {
            string jsonPropertyName = GetJsonNameFor(propertyName);
            JsonSchema propertySchema = GetJsonSchemaForObject(objectTypeName);

            return propertySchema.Properties.ContainsKey(jsonPropertyName) ? propertySchema.Properties[jsonPropertyName].Pattern : null;
        }
    }
}