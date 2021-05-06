// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Describes a condition relevant to the tool itself, as opposed to being relevant to a file being analyzed by the tool.
    /// </summary>
    public partial class ExceptionData
    {
        public static ExceptionData Create(Exception exception)
        {
            return new ExceptionData
            {
                Kind = exception.GetType().Name,
                Message = exception.Message,
                InnerExceptions = GetInnerExceptions(exception),
                Stack = Stack.Create(exception.StackTrace)
            };
        }

        /// <summary>
        /// This is an implementation similar to Exception.ToString()
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            if (string.IsNullOrEmpty(this.Message))
            {
                sb.Append(this.Kind);
            }
            else
            {
                sb.Append(this.Kind).Append(": ").Append(this.Message);
            }

            if (this.InnerExceptions != null)
            {
                foreach (ExceptionData innerException in this.InnerExceptions)
                {
                    sb.Append("---> ").AppendLine(innerException.ToString());
                }
            }

            if (this.Stack != null)
            {
                sb.AppendLine().Append(this.Stack.ToString());
            }

            return sb.ToString();
        }

        private static IList<ExceptionData> GetInnerExceptions(Exception exception)
        {
            var innerExceptions = new List<ExceptionData>();

            IReadOnlyCollection<Exception> aggregateInnerExceptions = (exception as AggregateException)?.InnerExceptions;
            if (aggregateInnerExceptions != null)
            {
                foreach (Exception innerException in aggregateInnerExceptions)
                {
                    innerExceptions.Add(Create(innerException));
                }
            }
            else if (exception.InnerException != null)
            {
                innerExceptions.Add(Create(exception.InnerException));
            }

            return innerExceptions;
        }
    }
}
