// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Multitool.Jschema;

namespace Microsoft.Json.Schema.Validation.Private
{
    // TODO Not best name. Maybe RuleDictionary.Instance and have it implement IDictionary.
    public static class RuleFactory
    {
        // TODO: Make list immutable

        public const string DefaultRuleMessageId = "default";
        private const string ErrorCodePrefix = "JSON";
        internal static readonly string ErrorCodeFormat = ErrorCodePrefix + "{0:D4}";

        private static MessageDescriptor MakeRule(ErrorNumber errorNumber, string fullDescription, string messageFormat)
        {
            string messageStringWithPath = string.Format(CultureInfo.CurrentCulture, JschemaRuleResources.ErrorMessageStringWithPath, messageFormat);

            return new MessageDescriptor
            {
                Id = ResultFactory.RuleIdFromErrorNumber(errorNumber),
                DefaultConfiguration = new RuleConfiguration
                {
                    Level = FailureLevel.Error
                },
                Name = new Message
                {
                    Text = errorNumber.ToString()
                },
                FullDescription = new Message
                {
                    Text = fullDescription
                },
                MessageStrings = new Dictionary<string, string>
                {
                    [DefaultRuleMessageId] = messageStringWithPath
                }
            };
        }

        private static readonly Dictionary<ErrorNumber, MessageDescriptor> s_ruleDictionary = new Dictionary<ErrorNumber, MessageDescriptor>
        {
            [ErrorNumber.SyntaxError] = MakeRule(
                ErrorNumber.SyntaxError,
                JschemaRuleResources.RuleDescriptionSyntaxError,
                JschemaRuleResources.ErrorSyntaxError),

            [ErrorNumber.NotAString] = MakeRule(
                ErrorNumber.NotAString,
                JschemaRuleResources.RuleDescriptionNotAString,
                JschemaRuleResources.ErrorNotAString),

            [ErrorNumber.InvalidAdditionalPropertiesType] = MakeRule(
                ErrorNumber.InvalidAdditionalPropertiesType,
                JschemaRuleResources.RuleDescriptionInvalidAdditionalPropertiesType,
                JschemaRuleResources.ErrorInvalidAdditionalPropertiesType),

            [ErrorNumber.InvalidItemsType] = MakeRule(
                ErrorNumber.InvalidItemsType,
                JschemaRuleResources.RuleDescriptionInvalidItemsType,
                JschemaRuleResources.ErrorInvalidItemsType),

            [ErrorNumber.InvalidTypeType] = MakeRule(
                ErrorNumber.InvalidTypeType,
                JschemaRuleResources.RuleDescriptionInvalidTypeType,
                JschemaRuleResources.ErrorInvalidTypeType),

            [ErrorNumber.InvalidTypeString] = MakeRule(
                ErrorNumber.InvalidTypeString,
                JschemaRuleResources.RuleDescriptionInvalidTypeString,
                JschemaRuleResources.ErrorInvalidTypeString),

            [ErrorNumber.InvalidAdditionalItemsType] = MakeRule(
                ErrorNumber.InvalidAdditionalItemsType,
                JschemaRuleResources.RuleDescriptionInvalidAdditionalItemsType,
                JschemaRuleResources.ErrorInvalidAdditionalItemsType),

            [ErrorNumber.InvalidDependencyType] = MakeRule(
                ErrorNumber.InvalidDependencyType,
                JschemaRuleResources.RuleDescriptionInvalidDependencyType,
                JschemaRuleResources.ErrorInvalidDependencyType),

            [ErrorNumber.InvalidPropertyDependencyType] = MakeRule(
                ErrorNumber.InvalidPropertyDependencyType,
                JschemaRuleResources.RuleDescriptionInvalidPropertyDependencyType,
                JschemaRuleResources.ErrorInvalidPropertyDependencyType),

            [ErrorNumber.WrongType] = MakeRule(
                ErrorNumber.WrongType,
                JschemaRuleResources.RuleDescriptionWrongType,
                JschemaRuleResources.ErrorWrongType),

            [ErrorNumber.RequiredPropertyMissing] = MakeRule(
                ErrorNumber.RequiredPropertyMissing,
                JschemaRuleResources.RuleDescriptionRequiredPropertyMissing,
                JschemaRuleResources.ErrorRequiredPropertyMissing),

            [ErrorNumber.TooFewArrayItems] = MakeRule(
                ErrorNumber.TooFewArrayItems,
                JschemaRuleResources.RuleDescriptionTooFewArrayItems,
                JschemaRuleResources.ErrorTooFewArrayItems),

            [ErrorNumber.TooManyArrayItems] = MakeRule(
                ErrorNumber.TooManyArrayItems,
                JschemaRuleResources.RuleDescriptionTooManyArrayItems,
                JschemaRuleResources.ErrorTooManyArrayItems),

            [ErrorNumber.AdditionalPropertiesProhibited] = MakeRule(
                ErrorNumber.AdditionalPropertiesProhibited,
                JschemaRuleResources.RuleDescriptionAdditionalPropertiesProhibited,
                JschemaRuleResources.ErrorAdditionalPropertiesProhibited),

            [ErrorNumber.ValueTooLarge] = MakeRule(
                ErrorNumber.ValueTooLarge,
                JschemaRuleResources.RuleDescriptionValueTooLarge,
                JschemaRuleResources.ErrorValueTooLarge),

            [ErrorNumber.ValueTooLargeExclusive] = MakeRule(
                ErrorNumber.ValueTooLargeExclusive,
                JschemaRuleResources.RuleDescriptionValueTooLargeExclusive,
                JschemaRuleResources.ErrorValueTooLargeExclusive),

            [ErrorNumber.ValueTooSmall] = MakeRule(
                ErrorNumber.ValueTooSmall,
                JschemaRuleResources.RuleDescriptionValueTooSmall,
                JschemaRuleResources.ErrorValueTooSmall),

            [ErrorNumber.ValueTooSmallExclusive] = MakeRule(
                ErrorNumber.ValueTooSmallExclusive,
                JschemaRuleResources.RuleDescriptionValueTooSmallExclusive,
                JschemaRuleResources.ErrorValueTooSmallExclusive),

            [ErrorNumber.TooManyProperties] = MakeRule(
                ErrorNumber.TooManyProperties,
                JschemaRuleResources.RuleDescriptionTooManyProperties,
                JschemaRuleResources.ErrorTooManyProperties),

            [ErrorNumber.TooFewProperties] = MakeRule(
                ErrorNumber.TooFewProperties,
                JschemaRuleResources.RuleDescriptionTooFewProperties,
                JschemaRuleResources.ErrorTooFewProperties),

            [ErrorNumber.NotAMultiple] = MakeRule(
                ErrorNumber.NotAMultiple,
                JschemaRuleResources.RuleDescriptionNotAMultiple,
                JschemaRuleResources.ErrorNotAMultiple),

            [ErrorNumber.StringTooLong] = MakeRule(
                ErrorNumber.StringTooLong,
                JschemaRuleResources.RuleDescriptionStringTooLong,
                JschemaRuleResources.ErrorStringTooLong),

            [ErrorNumber.StringTooShort] = MakeRule(
                ErrorNumber.StringTooShort,
                JschemaRuleResources.RuleDescriptionStringTooShort,
                JschemaRuleResources.ErrorStringTooShort),

            [ErrorNumber.StringDoesNotMatchPattern] = MakeRule(
                ErrorNumber.StringDoesNotMatchPattern,
                JschemaRuleResources.RuleDescriptionStringDoesNotMatchPattern,
                JschemaRuleResources.ErrorStringDoesNotMatchPattern),

            [ErrorNumber.NotAllOf] = MakeRule(
                ErrorNumber.NotAllOf,
                JschemaRuleResources.RuleDescriptionNotAllOf,
                JschemaRuleResources.ErrorNotAllOf),

            [ErrorNumber.NotAnyOf] = MakeRule(
                ErrorNumber.NotAnyOf,
                JschemaRuleResources.RuleDescriptionNotAnyOf,
                JschemaRuleResources.ErrorNotAnyOf),

            [ErrorNumber.NotOneOf] = MakeRule(
                ErrorNumber.NotOneOf,
                JschemaRuleResources.RuleDescriptionNotOneOf,
                JschemaRuleResources.ErrorNotOneOf),

            [ErrorNumber.InvalidEnumValue] = MakeRule(
                ErrorNumber.InvalidEnumValue,
                JschemaRuleResources.RuleDescriptionInvalidEnumValue,
                JschemaRuleResources.ErrorInvalidEnumValue),

            [ErrorNumber.NotUnique] = MakeRule(
                ErrorNumber.NotUnique,
                JschemaRuleResources.RuleDescriptionNotUnique,
                JschemaRuleResources.ErrorNotUnique),

            [ErrorNumber.TooFewItemSchemas] = MakeRule(
                ErrorNumber.TooFewItemSchemas,
                JschemaRuleResources.RuleDescriptionTooFewItemSchemas,
                JschemaRuleResources.ErrorTooFewItemSchemas),

            [ErrorNumber.ValidatesAgainstNotSchema] = MakeRule(
                ErrorNumber.ValidatesAgainstNotSchema,
                JschemaRuleResources.RuleDescriptionValidatesAgainstNotSchema,
                JschemaRuleResources.ErrorValidatesAgainstNotSchema),

            [ErrorNumber.DependentPropertyMissing] = MakeRule(
                ErrorNumber.DependentPropertyMissing,
                JschemaRuleResources.RuleDescriptionDependentPropertyMissing,
                JschemaRuleResources.ErrorDependentPropertyMissing)
        };

        public static MessageDescriptor GetRuleFromRuleId(string ruleId)
        {
            var errorNumber = (ErrorNumber)int.Parse(ruleId.Substring(ErrorCodePrefix.Length));
            return GetRuleFromErrorNumber(errorNumber);
        }

        public static MessageDescriptor GetRuleFromErrorNumber(ErrorNumber errorNumber)
        {
            return s_ruleDictionary[errorNumber];
        }
    }
}
