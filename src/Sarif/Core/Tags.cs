// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class Tags : ISet<string>
    {
        private const string TagsPropertyName = "tags";
        private static readonly ISet<string> Empty = ImmutableHashSet<string>.Empty;

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
                ISet<string> tags = GetTags() ?? Empty;
                return tags.Count;
            }
        }

        public bool IsReadOnly => false;

        public bool Add(string item)
        {
            return AddCore(item);
        }

        public void Clear()
        {
            _propertyBagHolder.RemoveProperty(TagsPropertyName);
        }

        public bool Contains(string item)
        {
            ISet<string> tags = GetTags() ?? Empty;
            return tags.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            ISet<string> tags = GetTags() ?? Empty;
            tags.CopyTo(array, arrayIndex);
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
            ISet<string> tags = GetTags() ?? Empty;
            return tags.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<string> other)
        {
            ISet<string> tags = GetTags() ?? Empty;
            return tags.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<string> other)
        {
            ISet<string> tags = GetTags() ?? Empty;
            return tags.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<string> other)
        {
            ISet<string> tags = GetTags() ?? Empty;
            return tags.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<string> other)
        {
            ISet<string> tags = GetTags() ?? Empty;
            return tags.Overlaps(other);
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
            AddCore(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorCore();
        }

        private ISet<string> GetTags()
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

        private bool AddCore(string item)
        {
            ISet<string> tags = GetTags() ?? new HashSet<string>();
            bool wasAdded = tags.Add(item);
            if (wasAdded)
            {
                SetTags(tags);
            }

            return wasAdded;
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
