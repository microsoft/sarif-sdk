// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class PropertiesDictionaryTests
    {
        private const string FEATURE = "Sarif.Sdk";
        private const string OPTIONS = FEATURE + ".Options";

        private static readonly bool BOOL_DEFAULT = true;
        private static readonly StringSet STRINGSET_DEFAULT = new StringSet(new string[] { "a", "b", "c" });
        private static readonly IntegerSet INTEGERSET_DEFAULT = new IntegerSet(new int[] { -1, 0, 1, 2 });
        private static readonly PropertiesDictionary PROPERTIES_DEFAULT = new PropertiesDictionary
        {
            { "TestKey", "TestValue" }
        };


        [Fact]
        public void PropertiesDictionary_RoundTripBoolean()
        {
            var properties = new PropertiesDictionary();
            properties.GetProperty(BooleanProperty).Should().Be(BOOL_DEFAULT);

            bool nonDefaultValue = false;
            properties.SetProperty(BooleanProperty, nonDefaultValue);
            properties.GetProperty(BooleanProperty).Should().Be(nonDefaultValue);

            properties = RoundTripThroughXml(properties);
            properties.GetProperty(BooleanProperty).Should().Be(nonDefaultValue);

            properties = RoundTripThroughJson(properties);
            properties.GetProperty(BooleanProperty).Should().Be(nonDefaultValue);
        }


        [Fact]
        public void PropertiesDictionary_RoundTripStringSet()
        {
            var properties = new PropertiesDictionary();
            ValidateSets(properties.GetProperty(StringSetProperty), STRINGSET_DEFAULT);


            var nonDefaultValue = new StringSet(new string[] { "d", "e" });
            properties.SetProperty(StringSetProperty, nonDefaultValue);

            properties = RoundTripThroughXml(properties);
            ValidateSets(properties.GetProperty(StringSetProperty), nonDefaultValue);

            properties = RoundTripThroughJson(properties);
            ValidateSets(properties.GetProperty(StringSetProperty), nonDefaultValue);
        }

        [Fact]
        public void PropertiesDictionary_RoundTripIntegerSet()
        {
            var properties = new PropertiesDictionary();
            ValidateSets(properties.GetProperty(IntegerSetProperty), INTEGERSET_DEFAULT);

            var nonDefaultValue = new IntegerSet(new int[] { 3, 4 });
            properties.SetProperty(IntegerSetProperty, nonDefaultValue);

            properties = RoundTripThroughXml(properties);
            ValidateSets(properties.GetProperty(IntegerSetProperty), nonDefaultValue);

            properties = RoundTripThroughJson(properties);
            ValidateSets(properties.GetProperty(IntegerSetProperty), nonDefaultValue);
        }

        [Fact]
        public void PropertiesDictionary_RoundTripNestedPropertiesDictionary()
        {
            var properties = new PropertiesDictionary();
            ValidateProperties(properties.GetProperty(PropertiesDictionaryProperty), PROPERTIES_DEFAULT);

            var nonDefaultValue = new PropertiesDictionary { { "NewKey", 1337 }, { "AnotherKey", true } };
            properties.SetProperty(PropertiesDictionaryProperty, nonDefaultValue);

            properties = RoundTripThroughXml(properties);
            ValidateProperties(properties.GetProperty(PropertiesDictionaryProperty), nonDefaultValue);

            properties = RoundTripThroughJson(properties);
            ValidateProperties(properties.GetProperty(PropertiesDictionaryProperty), nonDefaultValue);
        }

        [Fact]
        public void PropertiesDictionary_RoundTripEmptyStringToVersionMap()
        {
            const string MapKey = "MapKey";
            const string ValueKey = "NewKey";
            var properties = new PropertiesDictionary();
            ValidateProperties(properties.GetProperty(PropertiesDictionaryProperty), PROPERTIES_DEFAULT);

            var version = new Version(1, 2, 3, 4);

            var nonDefaultValue = new StringToVersionMap();
            properties[MapKey] = nonDefaultValue;

            properties = RoundTripThroughXml(properties);
            ValidateProperties(properties.GetProperty(PropertiesDictionaryProperty), PROPERTIES_DEFAULT);
            ((TypedPropertiesDictionary<Version>)properties[MapKey]).ContainsKey(ValueKey).Should().Be(false);
        }

        [Fact]
        public void PropertiesDictionary_RoundTripStringToVersionMap()
        {
            const string MapKey = "MapKey";
            const string ValueKey = "NewKey";
            var properties = new PropertiesDictionary();
            ValidateProperties(properties.GetProperty(PropertiesDictionaryProperty), PROPERTIES_DEFAULT);

            var version = new Version(1, 2, 3, 4);

            var nonDefaultValue = new StringToVersionMap { { ValueKey, version } };
            properties[MapKey] = nonDefaultValue;

            properties = RoundTripThroughXml(properties);
            ((TypedPropertiesDictionary<Version>)properties[MapKey])[ValueKey].Should().Be(version);
        }

        private void ValidateProperties(PropertiesDictionary actual, PropertiesDictionary expected)
        {
            actual.Keys.Count.Should().Be(expected.Keys.Count);

            foreach (string key in actual.Keys)
            {
                actual[key].GetType().Should().Be(expected[key].GetType());
                actual[key].Should().Be(expected[key]);
            }
        }

        private void ValidateSets<T>(HashSet<T> actual, HashSet<T> expected)
        {
            foreach (T member in expected)
            {
                actual.Should().Contain(expected);
            }
            actual.Count.Should().Be(expected.Count);
        }

        private PropertiesDictionary RoundTripThroughXml(PropertiesDictionary properties)
        {
            string temp = Path.GetTempFileName();

            try
            {
                properties.SaveToXml(temp);
                properties = new PropertiesDictionary();
                properties.LoadFromXml(temp);
            }
            finally
            {
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }
            }
            return properties;
        }

        private PropertiesDictionary RoundTripThroughJson(PropertiesDictionary properties)
        {
            string temp = Path.GetTempFileName();

            try
            {
                properties.SaveToJson(temp);
                properties = new PropertiesDictionary();
                properties.LoadFromJson(temp);
            }
            finally
            {
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }
            }
            return properties;
        }

        public static PerLanguageOption<bool> BooleanProperty { get; } =
            new PerLanguageOption<bool>(
                FEATURE, nameof(BooleanProperty), defaultValue: () => { return BOOL_DEFAULT; });

        public static PerLanguageOption<StringSet> StringSetProperty { get; } =
            new PerLanguageOption<StringSet>(
                FEATURE, nameof(StringSetProperty), defaultValue: () => { return STRINGSET_DEFAULT; });

        public static PerLanguageOption<IntegerSet> IntegerSetProperty { get; } =
            new PerLanguageOption<IntegerSet>(
                FEATURE, nameof(IntegerSetProperty), defaultValue: () => { return INTEGERSET_DEFAULT; });

        public static PerLanguageOption<PropertiesDictionary> PropertiesDictionaryProperty { get; } =
            new PerLanguageOption<PropertiesDictionary>(
                FEATURE, nameof(PropertiesDictionaryProperty), defaultValue: () => { return PROPERTIES_DEFAULT; });
    }
}
