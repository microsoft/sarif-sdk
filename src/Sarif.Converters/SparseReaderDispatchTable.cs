// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// Sparse reader dispatch table. This is a table of element name to delegate that allows a
    /// caller to amortize the SparseReader delegate construction.
    /// </summary>
    /// <seealso cref="T:System.Collections.Generic.IEnumerable{KeyValuePair{string, Action{SparseReader, Object}}}"/>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed class SparseReaderDispatchTable : IEnumerable<KeyValuePair<string, Action<SparseReader, object>>>
    {
        private readonly SortedList<string, Action<SparseReader, object>> _elementHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseReaderDispatchTable"/> class.
        /// </summary>
        public SparseReaderDispatchTable()
        {
            _elementHandlers = new SortedList<string, Action<SparseReader, object>>();
        }

        /// <summary>Adds a handler for an element name.</summary>
        /// <param name="elementName">Name of the element for which a handler shall be installed.</param>
        /// <param name="handler">The handler delegate.</param>
        public void Add(string elementName, Action<SparseReader, object> handler)
        {
            _elementHandlers.Add(elementName, handler);
        }

        /// <summary>Handles .</summary>
        /// <param name="elementName">Name of the element for which a handler was installed.</param>
        /// <param name="source">Source for the.</param>
        /// <param name="context">The context.</param>
        /// <returns>true if a dispatcher was registered for that element name; otherwise, false.</returns>
        public bool Dispatch(string elementName, SparseReader source, object context)
        {
            Action<SparseReader, object> handler;
            if (_elementHandlers.TryGetValue(elementName, out handler))
            {
                handler(source, context);
                return true;
            }

            return false;
        }

        /// <summary>Gets the enumerator.</summary>
        /// <returns>The enumerator.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _elementHandlers.GetEnumerator();
        }

        /// <summary>Gets the enumerator.</summary>
        /// <returns>The enumerator.</returns>
        IEnumerator<KeyValuePair<string, Action<SparseReader, object>>> IEnumerable<KeyValuePair<string, Action<SparseReader, object>>>.GetEnumerator()
        {
            return _elementHandlers.GetEnumerator();
        }
    }
}
