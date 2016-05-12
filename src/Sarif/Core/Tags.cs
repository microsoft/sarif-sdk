// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class Tags : ISet<string>
    {
        private const string TagsPropertyName = "tags";

        private readonly IPropertyBagHolder _propertyBagHolder;

        public Tags(IPropertyBagHolder propertyBagHolder)
        {
            if (propertyBagHolder == null)
            {
                throw new ArgumentNullException(nameof(propertyBagHolder));
            }

            _propertyBagHolder = propertyBagHolder;
        }

        public int Count
        {
            get
            {
                return GetTags().Count;
            }
        }

        public bool IsReadOnly => false;

        public bool Add(string item)
        {
            ISet<string> tags = GetTags() ?? new HashSet<string>();
            bool wasAdded = tags.Add(item);
            if (wasAdded)
            {
                _propertyBagHolder.SetProperty("tags", tags);
            }

            return wasAdded;
        }

        public void Clear()
        {
            _propertyBagHolder.RemoveProperty(TagsPropertyName);
        }

        public bool Contains(string item)
        {
            ISet<string> tags = GetTags();
            return tags != null ? tags.Contains(item) : false;
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public void ExceptWith(IEnumerable<string> other)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return GetTags().GetEnumerator();
        }

        public void IntersectWith(IEnumerable<string> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSubsetOf(IEnumerable<string> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<string> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<string> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<string> other)
        {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<string> other)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string item)
        {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<string> other)
        {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<string> other)
        {
            throw new NotImplementedException();
        }

        public void UnionWith(IEnumerable<string> other)
        {
            throw new NotImplementedException();
        }

        void ICollection<string>.Add(string item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetTags().GetEnumerator();
        }

        private HashSet<string> GetTags()
        {
            HashSet<string> tags;
            return _propertyBagHolder.TryGetProperty(TagsPropertyName, out tags)
                ? tags
                : new HashSet<string>();
        }
    }
}
