// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.ComponentModel;

namespace Microsoft.Sarif.Viewer
{
    /// <summary>
    /// A custom property descriptor which represents one key-value-pair.
    /// The property is always grouped/displayed in the Properties category
    /// in the Properties tool window.
    /// </summary>
    class KeyValuePairPropertyDescriptor : PropertyDescriptor
    {
        string _key, _value;

        internal KeyValuePairPropertyDescriptor(string key, string value)
            : base(key, null)
        {
            _key = key;
            _value = value;
        }

        public override Type PropertyType
        {
            get
            {
                return _key.GetType();
            }
        }

        public override void SetValue(object component, object value)
        {
            _value = value.ToString();
        }

        public override object GetValue(object component)
        {
            return _value;
        }

        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public override Type ComponentType
        {
            get
            {
                return null;
            }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override string Category
        {
            get
            {
                return "Properties";
            }
        }
    }
}
