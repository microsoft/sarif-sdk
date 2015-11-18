// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Driver;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>
    /// Exception for signalling .g4 parse failures. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="T:System.Exception"/>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public sealed class G4ParseFailureException : Exception
    {
        /// <summary>The location in the source text where the parse failure occurred.</summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public readonly OffsetInfo Location;

        /// <summary>
        /// Initializes a new instance of the <see cref="G4ParseFailureException"/> class.
        /// </summary>
        /// <param name="location">The location in the source text where the parse failure occurred.</param>
        /// <param name="message">The message indicating why the failure occurred.</param>
        public G4ParseFailureException(OffsetInfo location, string message)
            : base(FormatMessage(location, message))
        {
            this.Location = location;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="G4ParseFailureException"/> class.
        /// </summary>
        /// <param name="location">The location in the source text where the parse failure occurred.</param>
        /// <param name="message">The message indicating why the failure occurred.</param>
        /// <param name="args">The arguments to use when formatting <paramref name="message"/>.</param>
        public G4ParseFailureException(OffsetInfo location, string message, params string[] args)
            : base(FormatMessage(location, String.Format(CultureInfo.InvariantCulture, message, args)))
        {
            this.Location = location;
        }

        /// <summary>Initializes a new instance of the <see cref="G4ParseFailureException"/> class.</summary>
        /// <param name="info">The serialization info from which the value shall be deserialized.</param>
        /// <param name="context">The streaming context from which the value shall be deserialized.</param>
        private G4ParseFailureException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.Location = (OffsetInfo)info.GetValue("Location", typeof(OffsetInfo));
        }

        /// <summary>
        /// Makes a <see cref="G4ParseFailureException"/> instance representing an unexpected token.
        /// </summary>
        /// <param name="current">The unexpected token.</param>
        /// <returns>A <see cref="G4ParseFailureException"/> representing the unexpected token.</returns>
        internal static G4ParseFailureException UnexpectedToken(Token current)
        {
            return new G4ParseFailureException(current.GetLocation(), Strings.UnexpectedToken, current.ToString());
        }

        /// <summary>Gets object data for serialization.</summary>
        /// <param name="info">The serialization info into which the value shall be serialized.</param>
        /// <param name="context">The streaming context into which the value shall be serialized.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("Location", this.Location);
            base.GetObjectData(info, context);
        }

        private static string FormatMessage(OffsetInfo location, string message)
        {
            return String.Concat(
                location.LineNumber.ToString(CultureInfo.InvariantCulture),
                ":",
                location.ColumnNumber.ToString(CultureInfo.InvariantCulture),
                ": ",
                message
                );
        }
    }
}
