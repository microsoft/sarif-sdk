using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WorkItems.Logging
{
    /// <summary>
    /// MetricsLogValues is variation of FormattedLogValues that accepts a dictionary to store customDimensions Application Insights.
    /// https://github.com/aspnet/Logging/blob/master/src/Microsoft.Extensions.Logging.Abstractions/Internal/FormattedLogValues.cs
    /// </summary>
    internal readonly struct MetricsLogValues : IReadOnlyList<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable, IReadOnlyCollection<KeyValuePair<string, object>>
    {
        private readonly List<KeyValuePair<string, object>> _values;
        private readonly string _originalMessage;
        private readonly EventId _eventId;

        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException("index");
                }

                return _values[index];
            }
        }

        public int Count
        {
            get
            {
                return _values.Count;
            }
        }

        public MetricsLogValues(string format, EventId eventId, IDictionary<string, object> values)
        {
            if (values == null || values.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(values), $"The {nameof(values)} dictionary cannot be null or empty.");
            }

            _originalMessage = format;
            _values = values.ToList();
            _eventId = eventId;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            for (int i = 0; i < Count; ++i)
            {
                yield return this[i];
            }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(_originalMessage))
            {
                return _originalMessage;
            }
            else
            {
                return $"Event {_eventId.Id} ({_eventId.Name})";
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
