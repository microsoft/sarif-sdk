using System;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Query;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Query
{
    public class StringSliceTests
    {
        [Fact]
        public void StringSlice_Empty()
        {
            EmptyChecks("");
            EmptyChecks(null);
            EmptyChecks(new StringSlice(null, 0, 0));
            EmptyChecks(new StringSlice("hello", 4, 0));
        }

        private void EmptyChecks(StringSlice empty)
        {
            // Length should be zero
            Assert.Equal(0, empty.Length);

            // StartsWith should fail gracefully
            Assert.False(empty.StartsWith(':'));
            Assert.False(empty.StartsWith((StringSlice)":"));

            // CompareTo should fail gracefully
            Assert.Equal(-1, empty.CompareTo(":"));

            // AppendTo shouldn't do anything
            StringBuilder result = new StringBuilder();
            empty.AppendTo(result);
            Assert.Equal(0, result.Length);

            // Substring(0) should work
            StringSlice other = empty.Substring(0);
        }

        [Fact]
        public void StringSlice_Basics()
        {
            StringSlice slice = "Hello, there!";

            Assert.Equal(13, slice.Length);
            Assert.Equal('H', slice[0]);
            Assert.Equal(' ', slice[6]);

            Assert.True(slice.StartsWith('H'));
            Assert.False(slice.StartsWith('h'));

            Assert.True(slice.StartsWith("Hello"));
            Assert.False(slice.StartsWith("hello"));
            Assert.True(slice.StartsWith("hello", StringComparison.OrdinalIgnoreCase));

            slice = slice.Substring(7, 3);
            Assert.Equal(3, slice.Length);
            Assert.Equal(0, slice.CompareTo("the"));

            StringBuilder result = new StringBuilder();
            slice.AppendTo(result);
            Assert.Equal(3, result.Length);
            Assert.Equal("the", result.ToString());

            slice = slice.Substring(3);
            Assert.Equal(0, slice.Length);
        }
    }
}
