// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Sarif.Viewer
{
    /// <summary>
    /// A custom type descriptor which enables the SARIF properties to be displayed
    /// in the Properties window.
    /// </summary>
    public class CodeLocationObjectTypeDescriptor : ICustomTypeDescriptor
    {
        CodeLocationObject _item;

        public CodeLocationObjectTypeDescriptor(CodeLocationObject item)
        {
            _item = item;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(_item, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(_item, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(_item, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(_item, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(_item, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(_item, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(_item, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(_item, attributes, true);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return TypeDescriptor.GetProperties(_item, true);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();

            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(_item, true))
            {
                if (propertyDescriptor.Name.Equals("Properties") && propertyDescriptor.PropertyType == typeof(Dictionary<string, string>))
                {
                    // These are the SARIF properties.
                    // Convert the key value pairs to individual properties.
                    Dictionary<string, string> propertyBag = propertyDescriptor.GetValue(_item) as Dictionary<string, string>;

                    foreach (string key in propertyBag.Keys)
                    {
                        properties.Add(new KeyValuePairPropertyDescriptor(key, propertyBag[key]));
                    }
                }
                else
                {
                    properties.Add(propertyDescriptor);
                }
            }
            
            return new PropertyDescriptorCollection(properties.ToArray(), true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _item;
        }
    }
}
