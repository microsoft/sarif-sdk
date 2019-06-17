using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class InvocationTests
    {
        [Fact]
        public void Invocation_ExecutionSuccessfulDefaultOnInstantiationIsTrue()
        {
            // ExecutionSuccessful is a required property and so our code gen does
            // not provide a default. Absent a code change, this means we will pick
            // up the .NET boolean default of 'false', which is a bad de facto default.
            // We should assume an invocation is successful until a producer 
            // explicitly determines otherwise. We accomplish this by setting 
            // executionSuccessful to true on object instantiation.

            var invocation = new Invocation();
            invocation.ExecutionSuccessful.Should().BeTrue();
        }
    }
}
