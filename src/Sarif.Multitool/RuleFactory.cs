// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.Json.Schema;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    // TODO Not best name. Maybe RuleDictionary.Instance and have it implement IDictionary.
    public static class RuleFactoryCopy
    {
        // TODO: Make list immutable

        public const string DefaultMessageFormatId = "default";
        private const string ErrorCodeFormat = "JS{0:D4}";

        private static Rule MakeRule(ErrorNumber errorNumber, string fullDescription, string messageFormat)
        {
            string messageFormatWithPath = string.Format(CultureInfo.CurrentCulture, ValidatorResources.ErrorMessageFormatWithPath, messageFormat);

            return new Rule
            {
                Id = ResultFactory.RuleIdFromErrorNumber(errorNumber),
                DefaultLevel = ResultLevel.Error,
                Name = errorNumber.ToString(),
                FullDescription = fullDescription,
                MessageFormats = new Dictionary<string, string>
                {
                    [DefaultMessageFormatId] = messageFormatWithPath
                }
            };
        }

        private static readonly Dictionary<ErrorNumber, Rule> s_ruleDictionary = new Dictionary<ErrorNumber, Rule>
        {
            [ErrorNumber.SyntaxError] = MakeRule(
                ErrorNumber.SyntaxError,
                ValidatorResources.RuleDescriptionSyntaxError,
                ValidatorResources.ErrorSyntaxError),

            [ErrorNumber.NotAString] = MakeRule(
                ErrorNumber.NotAString,
                ValidatorResources.RuleDescriptionNotAString,
                ValidatorResources.ErrorNotAString),

            [ErrorNumber.InvalidAdditionalPropertiesType] = MakeRule(
                ErrorNumber.InvalidAdditionalPropertiesType,
                ValidatorResources.RuleDescriptionInvalidAdditionalPropertiesType,
                ValidatorResources.ErrorInvalidAdditionalPropertiesType),

            [ErrorNumber.InvalidItemsType] = MakeRule(
                ErrorNumber.InvalidItemsType,
                ValidatorResources.RuleDescriptionInvalidItemsType,
                ValidatorResources.ErrorInvalidItemsType),

            [ErrorNumber.InvalidTypeType] = MakeRule(
                ErrorNumber.InvalidTypeType,
                ValidatorResources.RuleDescriptionInvalidTypeType,
                ValidatorResources.ErrorInvalidTypeType),

            [ErrorNumber.InvalidTypeString] = MakeRule(
                ErrorNumber.InvalidTypeString,
                ValidatorResources.RuleDescriptionInvalidTypeString,
                ValidatorResources.ErrorInvalidTypeString),

            [ErrorNumber.InvalidAdditionalItemsType] = MakeRule(
                ErrorNumber.InvalidAdditionalItemsType,
                ValidatorResources.RuleDescriptionInvalidAdditionalItemsType,
                ValidatorResources.ErrorInvalidAdditionalItemsType),

            [ErrorNumber.InvalidDependencyType] = MakeRule(
                ErrorNumber.InvalidDependencyType,
                ValidatorResources.RuleDescriptionInvalidDependencyType,
                ValidatorResources.ErrorInvalidDependencyType),

            [ErrorNumber.InvalidPropertyDependencyType] = MakeRule(
                ErrorNumber.InvalidPropertyDependencyType,
                ValidatorResources.RuleDescriptionInvalidPropertyDependencyType,
                ValidatorResources.ErrorInvalidPropertyDependencyType),

            [ErrorNumber.WrongType] = MakeRule(
                ErrorNumber.WrongType,
                ValidatorResources.RuleDescriptionWrongType,
                ValidatorResources.ErrorWrongType),

            [ErrorNumber.RequiredPropertyMissing] = MakeRule(
                ErrorNumber.RequiredPropertyMissing,
                ValidatorResources.RuleDescriptionRequiredPropertyMissing,
                ValidatorResources.ErrorRequiredPropertyMissing),

            [ErrorNumber.TooFewArrayItems] = MakeRule(
                ErrorNumber.TooFewArrayItems,
                ValidatorResources.RuleDescriptionTooFewArrayItems,
                ValidatorResources.ErrorTooFewArrayItems),

            [ErrorNumber.TooManyArrayItems] = MakeRule(
                ErrorNumber.TooManyArrayItems,
                ValidatorResources.RuleDescriptionTooManyArrayItems,
                ValidatorResources.ErrorTooManyArrayItems),

            [ErrorNumber.AdditionalPropertiesProhibited] = MakeRule(
                ErrorNumber.AdditionalPropertiesProhibited,
                ValidatorResources.RuleDescriptionAdditionalPropertiesProhibited,
                ValidatorResources.ErrorAdditionalPropertiesProhibited),

            [ErrorNumber.ValueTooLarge] = MakeRule(
                ErrorNumber.ValueTooLarge,
                ValidatorResources.RuleDescriptionValueTooLarge,
                ValidatorResources.ErrorValueTooLarge),

            [ErrorNumber.ValueTooLargeExclusive] = MakeRule(
                ErrorNumber.ValueTooLargeExclusive,
                ValidatorResources.RuleDescriptionValueTooLargeExclusive,
                ValidatorResources.ErrorValueTooLargeExclusive),

            [ErrorNumber.ValueTooSmall] = MakeRule(
                ErrorNumber.ValueTooSmall,
                ValidatorResources.RuleDescriptionValueTooSmall,
                ValidatorResources.ErrorValueTooSmall),

            [ErrorNumber.ValueTooSmallExclusive] = MakeRule(
                ErrorNumber.ValueTooSmallExclusive,
                ValidatorResources.RuleDescriptionValueTooSmallExclusive,
                ValidatorResources.ErrorValueTooSmallExclusive),

            [ErrorNumber.TooManyProperties] = MakeRule(
                ErrorNumber.TooManyProperties,
                ValidatorResources.RuleDescriptionTooManyProperties,
                ValidatorResources.ErrorTooManyProperties),

            [ErrorNumber.TooFewProperties] = MakeRule(
                ErrorNumber.TooFewProperties,
                ValidatorResources.RuleDescriptionTooFewProperties,
                ValidatorResources.ErrorTooFewProperties),

            [ErrorNumber.NotAMultiple] = MakeRule(
                ErrorNumber.NotAMultiple,
                ValidatorResources.RuleDescriptionNotAMultiple,
                ValidatorResources.ErrorNotAMultiple),

            [ErrorNumber.StringTooLong] = MakeRule(
                ErrorNumber.StringTooLong,
                ValidatorResources.RuleDescriptionStringTooLong,
                ValidatorResources.ErrorStringTooLong),

            [ErrorNumber.StringTooShort] = MakeRule(
                ErrorNumber.StringTooShort,
                ValidatorResources.RuleDescriptionStringTooShort,
                ValidatorResources.ErrorStringTooShort),

            [ErrorNumber.StringDoesNotMatchPattern] = MakeRule(
                ErrorNumber.StringDoesNotMatchPattern,
                ValidatorResources.RuleDescriptionStringDoesNotMatchPattern,
                ValidatorResources.ErrorStringDoesNotMatchPattern),

            [ErrorNumber.NotAllOf] = MakeRule(
                ErrorNumber.NotAllOf,
                ValidatorResources.RuleDescriptionNotAllOf,
                ValidatorResources.ErrorNotAllOf),

            [ErrorNumber.NotAnyOf] = MakeRule(
                ErrorNumber.NotAnyOf,
                ValidatorResources.RuleDescriptionNotAnyOf,
                ValidatorResources.ErrorNotAnyOf),

            [ErrorNumber.NotOneOf] = MakeRule(
                ErrorNumber.NotOneOf,
                ValidatorResources.RuleDescriptionNotOneOf,
                ValidatorResources.ErrorNotOneOf),

            [ErrorNumber.InvalidEnumValue] = MakeRule(
                ErrorNumber.InvalidEnumValue,
                ValidatorResources.RuleDescriptionInvalidEnumValue,
                ValidatorResources.ErrorInvalidEnumValue),

            [ErrorNumber.NotUnique] = MakeRule(
                ErrorNumber.NotUnique,
                ValidatorResources.RuleDescriptionNotUnique,
                ValidatorResources.ErrorNotUnique),

            [ErrorNumber.TooFewItemSchemas] = MakeRule(
                ErrorNumber.TooFewItemSchemas,
                ValidatorResources.RuleDescriptionTooFewItemSchemas,
                ValidatorResources.ErrorTooFewItemSchemas),

            [ErrorNumber.ValidatesAgainstNotSchema] = MakeRule(
                ErrorNumber.ValidatesAgainstNotSchema,
                ValidatorResources.RuleDescriptionValidatesAgainstNotSchema,
                ValidatorResources.ErrorValidatesAgainstNotSchema),

            [ErrorNumber.DependentPropertyMissing] = MakeRule(
                ErrorNumber.DependentPropertyMissing,
                ValidatorResources.RuleDescriptionDependentPropertyMissing,
                ValidatorResources.ErrorDependentPropertyMissing)
        };

        public static Rule GetRuleFromRuleId(string ruleId)
        {
            var errorNumber = (ErrorNumber)int.Parse(ruleId.Substring(2));
            return GetRuleFromErrorNumber(errorNumber);
        }

        public static Rule GetRuleFromErrorNumber(ErrorNumber errorNumber)
        {
            return s_ruleDictionary[errorNumber];
        }
    }
}
