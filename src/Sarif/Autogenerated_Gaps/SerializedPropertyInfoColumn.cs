// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using BSOA.IO;
using BSOA.Model;

using Microsoft.CodeAnalysis.Sarif;

namespace BSOA.Column
{
    public class SerializedPropertyInfoColumn : LimitedList<SerializedPropertyInfo>, IColumn<SerializedPropertyInfo>
    {
        private StringColumn _serializedValue { get; }
        private BooleanColumn _isString { get; }

        public SerializedPropertyInfoColumn()
        {
            _serializedValue = new StringColumn();
            _isString = new BooleanColumn(false);
        }

        public override int Count => _serializedValue.Count;

        public override SerializedPropertyInfo this[int index]
        {
            get
            {
                string serializedValue = _serializedValue[index];
                return (serializedValue == null ? null : new SerializedPropertyInfo(serializedValue, _isString[index]));
            }

            set
            {
                if (value == null)
                {
                    _serializedValue[index] = null;
                    _isString[index] = false;
                }
                else
                {
                    _serializedValue[index] = value.SerializedValue;
                    _isString[index] = value.IsString;
                }
            }
        }

        public override void Clear()
        {
            _serializedValue.Clear();
            _isString.Clear();
        }

        public override void RemoveFromEnd(int count)
        {
            _serializedValue.RemoveFromEnd(count);
            _isString.RemoveFromEnd(count);
        }

        public void Trim()
        {
            _serializedValue.Trim();
            _isString.Trim();
        }

        private static Dictionary<string, Setter<SerializedPropertyInfoColumn>> setters = new Dictionary<string, Setter<SerializedPropertyInfoColumn>>()
        {
            [Names.IsNull] = (r, me) => me._isString.Read(r),
            [Names.Values] = (r, me) => me._serializedValue.Read(r)
        };

        public void Read(ITreeReader reader)
        {
            reader.ReadObject(this, setters);
        }

        public void Write(ITreeWriter writer)
        {
            writer.WriteStartObject();
            writer.Write(Names.IsNull, _isString);
            writer.Write(Names.Values, _serializedValue);
            writer.WriteEndObject();
        }
    }
}
