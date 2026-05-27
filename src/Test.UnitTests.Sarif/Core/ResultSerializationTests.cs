// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class ResultSerializationTests
    {
        public class DerivedResult : Result
        {
            // Tell Newtonsoft.Json to always serialize RuleId, even if Rule.Id is present.
            public override bool ShouldSerializeRuleId() => true;
        }

        [Fact]
        public void CanSerializeBothRuleIdAndRuleDotId()
        {
            DerivedResult dr = new()
            {
                RuleId = "RuleId",
                Rule = new ReportingDescriptorReference()
                {
                    Id = "RuleId"
                },
            };

            string actual = JsonConvert.SerializeObject(dr);

            actual.Should().StartWith(@"{""ruleId"":""RuleId"",""rule"":{""id"":""RuleId""}");
        }
    }
}
