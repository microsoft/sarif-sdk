// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
                ISet<string> tags = GetTags();
                return tags != null ? tags.Count : 0;
            }
        }

        public bool IsReadOnly => false;

        public bool Add(string item)
        {
            ISet<string> tags = GetTags() ?? new HashSet<string>();
            bool wasAdded = tags.Add(item);
            if (wasAdded)
            {
                SetTags(tags);
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
            ISet<string> tags = GetTags();
            if (tags != null)
            {
                tags.CopyTo(array, arrayIndex);
            }
        }

        public void ExceptWith(IEnumerable<string> other)
        {
            ISet<string> tags = GetTags();
            if (tags != null)
            {
                tags.ExceptWith(other);
                SetTags(tags);
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return GetEnumeratorCore();
        }

        public void IntersectWith(IEnumerable<string> other)
        {
            ISet<string> tags = GetTags();
            if (tags != null)
            {
                tags.IntersectWith(other);
                SetTags(tags);
            }
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
            return GetEnumeratorCore();
        }

        private HashSet<string> GetTags()
        {
            HashSet<string> tags;
            return _propertyBagHolder.TryGetProperty(TagsPropertyName, out tags)
                ? tags
                : null;
        }

        private void SetTags(ISet<string> tags)
        {
            _propertyBagHolder.SetProperty(TagsPropertyName, tags);
        }

        private IEnumerator<string> GetEnumeratorCore()
        {
            ISet<string> tags = GetTags();
            return tags != null
                ? tags.GetEnumerator()
                : Enumerable.Empty<string>().GetEnumerator();
        }
    }
}
