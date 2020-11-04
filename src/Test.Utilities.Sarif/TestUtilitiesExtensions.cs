// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.Test.Utilities.Sarif
{
    public static class TestUtilitiesExtensions
    {
        public static TestRuleBehaviors AccessibleOutsideOfContextOnly(this TestRuleBehaviors behaviors)
        {
            // These are the only test behavior flags that may need to be retrieved outside of
            // a context object (because the data is accessed when a context object isn't at
            // hand). For example, if a skimmer needs to raise an exception in its constructor,
            // which isn't paramterized with a context object, the test rule behavior that 
            // specifies this can be retrieved from a thread static property.
            return behaviors &
            (
                TestRuleBehaviors.RaiseExceptionAccessingId |
                TestRuleBehaviors.RaiseExceptionAccessingName |
                TestRuleBehaviors.RaiseExceptionInvokingConstructor
            );
        }

        public static TestRuleBehaviors AccessibleWithinContextOnly(this TestRuleBehaviors behaviors)
        {
            // These are the only test behavior flags that *must* be retrieved via
            // a context object. For example, if a skimmer needs to raise an exception
            // when it is initialized, it should retrieve the test rule behavior that 
            // specifies this from the context object that parameterizes the call.
            return behaviors & ~behaviors.AccessibleOutsideOfContextOnly();
        }
    }
}
