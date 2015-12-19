// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>A wrapper for enumerables of disposable objects that ensures that they are
    /// destroyed as they are produced.</summary>
    /// <remarks><para>This wrapper breaks several of the built in <see cref="System.Linq"/>
    /// extension methods that attempt to copy elements into backing collections, such as
    /// <see cref="Enumerable.Distinct{TSource}(IEnumerable{TSource})"/>. Callers must be
    /// informed of the consequences of not being able to copy elements or references to elements
    /// as usual.</para>
    /// <para>Note however that most of these mechanisms are not safe to use if the underlying
    /// type is disposable, because they remove elements from the source stream that the caller
    /// would need to dispose.</para>
    /// <para>Note that this class should only be used for enumerables that "stream" their
    /// elements and produce new disposable values when reset or similar. If you create one of
    /// these wrappers around a collection that actually owns the disposable item, such as
    /// <see cref="List{T}"/>, enumerating the collection will destroy the collection's
    /// contents (which is not what the caller expects).</para></remarks>
    /// <typeparam name="T">The <see cref="IDisposable"/> type wrapped in this enumerable.</typeparam>
    /// <seealso cref="T:System.Collections.Generic.IEnumerable{Microsoft.CodeAnalysis.Driver.DisposableEnumerableView{T}}"/>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")] //Justification: This isn't actually a collection type.
    public sealed class DisposableEnumerable<T> : IEnumerable<DisposableEnumerableView<T>>
        where T : IDisposable
    {
        private readonly IEnumerable<T> _backingEnumerable;

        /// <summary>Initializes a new instance of the <see cref="DisposableEnumerable{T}"/> class.</summary>
        /// <param name="backingEnumerable">An enumerable implementation that this instance will wrap.</param>
        public DisposableEnumerable(IEnumerable<T> backingEnumerable)
        {
            _backingEnumerable = backingEnumerable;
        }

        /// <summary>Gets an enumerator to walk a view of the underlying collection.</summary>
        /// <returns>An enumerator that iterates over elements in this collection.</returns>
        public IEnumerator<DisposableEnumerableView<T>> GetEnumerator()
        {
            return new Enumerator(_backingEnumerable.GetEnumerator());
        }

        /// <summary>Gets an enumerator to walk a view of the underlying collection.</summary>
        /// <returns>An enumerator that iterates over elements in this collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private sealed class Enumerator : IEnumerator<DisposableEnumerableView<T>>
        {
            private readonly IEnumerator<T> _backingEnumerator;
            private DisposableEnumerableView<T> _current;

            private void DestroyCurrent()
            {
                if (_current != null)
                {
                    _current.Destroy();
                }

                _current = null;
            }

            public Enumerator(IEnumerator<T> backingEnumerator)
            {
                _backingEnumerator = backingEnumerator;
            }

            public DisposableEnumerableView<T> Current
            {
                get { return _current; }
            }

            public void Dispose()
            {
                this.DestroyCurrent();
                _backingEnumerator.Dispose();
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                // The caller may have stashed a reference to the "view" away somewhere, so we need
                // to allocate a new view object.
                // The common use for this at the moment will be where T is a COM RCW class; and as
                // such one would expect construction / destruction of T to dwarf the 24 or 32 bytes
                // of GC overhead per element when iterating. (Particularly given that view objects
                // should be short lived and stay on ephemeral GC generations)
                // If GC profiling shows that DisposableEnumerableView objects are a lot of GC overhead,
                // one may need to replace this wrapper with something that sacrifices some amount of
                // safety for perf.

                this.DestroyCurrent();
                if (_backingEnumerator.MoveNext())
                {
                    _current = new DisposableEnumerableView<T>(_backingEnumerator.Current);
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                this.DestroyCurrent();
                _backingEnumerator.Reset();
            }
        }
    }
}
