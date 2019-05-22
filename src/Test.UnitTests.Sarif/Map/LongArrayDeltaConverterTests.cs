// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Map
{
    public class LongArrayDeltaConverterTests
    {
        internal class Container
        {
            [JsonConverter(typeof(LongArrayDeltaConverter))]
            public List<long> Values;
        }

        [Fact]
        public void LongArrayDeltaConverter_Basics()
        {
            RoundTrip(null);
            RoundTrip(new long[] { });
            RoundTrip(new long[] { -1 });
            RoundTrip(new long[] { 0, 0, 0, 0, 0 });
            RoundTrip(new long[] { 100, 101, 102, 103, 104, 105 });
            RoundTrip(new long[] { 100, -100, 200, -200, 300, -300 });
        }

        private static void RoundTrip(long[] values)
        {
            Container before = new Container(); 
            if(values != null)
            {
                before.Values = new List<long>(values);
            }

            string serialized = JsonConvert.SerializeObject(before);

            Container after = (Container)JsonConvert.DeserializeObject(serialized, typeof(Container));
            Assert.Equal(values, after.Values);
        }
   }
}
