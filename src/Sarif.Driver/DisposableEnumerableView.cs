// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>A view of a disposable object inside of a collection.</summary>
    /// <typeparam name="T">The type of disposable object in the collection.</typeparam>
    public sealed class DisposableEnumerableView<T>
        where T : IDisposable
    {
        private readonly T _value;
        // Using "nonOwning" rather than "owning" so that the default value, false,
        // is what we want most of the time.
        private bool _nonOwning;

        /// <summary>Initializes a new instance of the <see cref="DisposableEnumerableView{T}"/>
        /// class that owns a value.</summary>
        /// <param name="value">The value this view wraps.</param>
        public DisposableEnumerableView(T value)
        {
            _value = value;
        }

        /// <summary>Gets a non-owning reference to the element stored in this view.</summary>
        /// <value>The underlying value in this view.</value>
        /// <exception cref="InvalidOperationException">Thrown if this view no longer owns
        /// the underlying value.</exception>
        public T Value
        {
            get
            {
                if (_nonOwning)
                {
                    throw new InvalidOperationException(ExceptionStrings.NonOwningDisposableViewAccess);
                }

                return _value;
            }
        }

        /// <summary>Takes ownership of the underlying element stored in this view.</summary>
        /// <exception cref="InvalidOperationException">Thrown if this view no longer owns the
        /// underlying value.</exception>
        /// <returns>A the underlying value in this view.</returns>
        public T StealValue()
        {
            if (_nonOwning)
            {
                throw new InvalidOperationException(ExceptionStrings.NonOwningDisposableViewAccess);
            }

            _nonOwning = true;
            return _value;
        }

        /// <summary>If this instance owns the underlying value, destroys (calls
        /// <see cref="M:IDisposable.Dispose"/>) on the underlying value.</summary>
        internal void Destroy()
        {
            if (!_nonOwning && _value != null)
            {
                _value.Dispose();
            }
        }
    }
}
