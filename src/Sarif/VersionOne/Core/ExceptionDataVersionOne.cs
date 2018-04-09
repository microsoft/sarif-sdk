// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Describes a condition relevant to the tool itself, as opposed to being relevant to a file being analyzed by the tool.
    /// </summary>
    public partial class ExceptionDataVersionOne
    {
        public static ExceptionDataVersionOne Create(Exception exception)
        {
            return new ExceptionDataVersionOne
            {
                Kind = exception.GetType().Name,
                Message = exception.Message,
                InnerExceptions = GetInnerExceptions(exception),
                Stack = StackVersionOne.Create(exception.StackTrace)
            };
        }

        private static IList<ExceptionDataVersionOne> GetInnerExceptions(Exception exception)
        {
            var innerExceptions = new List<ExceptionDataVersionOne>();

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
