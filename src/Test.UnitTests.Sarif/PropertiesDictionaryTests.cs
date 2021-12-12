// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class PropertiesDictionaryTests
    {
        private const string FEATURE = "Sarif.Sdk";
        private const string OPTIONS = FEATURE + ".Options";

        private static readonly bool BOOL_DEFAULT = true;
        private static readonly bool BOOL_NONDEFAULT = false;
        private static readonly StringSet STRINGSET_DEFAULT = new StringSet(new string[] { "a", "b", "c" });
        private static readonly StringSet STRINGSET_NONDEFAULT = new StringSet(new string[] { "d", "e", "f" });
        private static readonly IntegerSet INTEGERSET_DEFAULT = new IntegerSet(new int[] { -1, 0, 1, 2 });
        private static readonly IntegerSet INTEGERSET_NONDEFAULT = new IntegerSet(new int[] { -3, 0, 6, 9 });
        private static readonly PropertiesDictionary PROPERTIES_DEFAULT = new PropertiesDictionary
        {
            { "TestKey", "TestValue" }
        };
        private static readonly PropertiesDictionary PROPERTIES_NONDEFAULT = new PropertiesDictionary
        {
            { "TestNonDefaultKey", "TestNonDefaultValue" },
            { "NewKey", 1337 },
            { "AnotherKey", true }
        };

        [Fact]
        public void PropertiesDictionary_RoundTrip_Boolean()
        {
            var properties = new PropertiesDictionary();
            properties.GetProperty(BooleanProperty).Should().Be(BOOL_DEFAULT);

            properties.SetProperty(BooleanProperty, BOOL_NONDEFAULT);
            properties.GetProperty(BooleanProperty).Should().Be(BOOL_NONDEFAULT);

            properties = RoundTripThroughXml(properties);
            properties.GetProperty(BooleanProperty).Should().Be(BOOL_NONDEFAULT);

            properties = RoundTripThroughJson(properties);
            properties.GetProperty(BooleanProperty).Should().Be(BOOL_NONDEFAULT);
        }

        [Fact]
        public void PropertiesDictionary_RoundTrip_StringSet()
        {
            var properties = new PropertiesDictionary();
            ValidateSets(properties.GetProperty(StringSetProperty), STRINGSET_DEFAULT);

            properties.SetProperty(StringSetProperty, STRINGSET_NONDEFAULT);

            properties = RoundTripThroughXml(properties);
            ValidateSets(properties.GetProperty(StringSetProperty), STRINGSET_NONDEFAULT);

            properties = RoundTripThroughJson(properties);
            ValidateSets(properties.GetProperty(StringSetProperty), STRINGSET_NONDEFAULT);
        }

        [Fact]
        public void PropertiesDictionary_RoundTrip_IntegerSet()
        {
            var properties = new PropertiesDictionary();
            ValidateSets(properties.GetProperty(IntegerSetProperty), INTEGERSET_DEFAULT);

            properties.SetProperty(IntegerSetProperty, INTEGERSET_NONDEFAULT);

            properties = RoundTripThroughXml(properties);
            ValidateSets(properties.GetProperty(IntegerSetProperty), INTEGERSET_NONDEFAULT);

            properties = RoundTripThroughJson(properties);
            ValidateSets(properties.GetProperty(IntegerSetProperty), INTEGERSET_NONDEFAULT);
        }

        [Fact]
        public void PropertiesDictionary_RoundTrip_NestedPropertiesDictionary()
        {
            var properties = new PropertiesDictionary();
            ValidateProperties(properties.GetProperty(PropertiesDictionaryProperty), PROPERTIES_DEFAULT);

            properties.SetProperty(PropertiesDictionaryProperty, PROPERTIES_NONDEFAULT);

            properties = RoundTripThroughXml(properties);
            ValidateProperties(properties.GetProperty(PropertiesDictionaryProperty), PROPERTIES_NONDEFAULT);

            properties = RoundTripThroughJson(properties);
            ValidateProperties(properties.GetProperty(PropertiesDictionaryProperty), PROPERTIES_NONDEFAULT);
        }

        [Fact]
        public void PropertiesDictionary_RoundTrip_EmptyStringToVersionMap()
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
        public void PropertiesDictionary_RoundTrip_StringToVersionMap()
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

        [Fact]
        public void PropertiesDictionary_ConcurrentReadAndWrite()
        {
            var properties = new PropertiesDictionary();

            Exception exception = Record.Exception(() =>
            {
                List<Task> taskList = new List<Task>();

                for (int i = 0; i < 1000; i++)
                {
                    // repeatedly set to same field
                    taskList.Add(Task.Factory.StartNew(() =>
                    properties.SetProperty(StringSetProperty, new StringSet(new string[] { i.ToString() }))));

                    // repeatedly set to different fields
                    taskList.Add(Task.Factory.StartNew(() =>
                    properties.SetProperty(GenerateStringSetProperty(i), new StringSet(new string[] { i.ToString() }))));

                    // repeatedly read from same field
                    taskList.Add(Task.Factory.StartNew(() =>
                    properties.GetProperty(StringSetProperty)));

                    // repeatedly read from different fields
                    taskList.Add(Task.Factory.StartNew(() =>
                    properties.GetProperty(GenerateStringSetProperty(i))));
                }

                Task.WaitAll(taskList.ToArray());
            });
            Assert.Null(exception);
        }

        [Fact]
        public void PropertiesDictionary_TestCacheDescription()
        {
            PropertiesDictionary_TestCacheDescription_Helper(GenerateBooleanProperty, BOOL_NONDEFAULT);
            PropertiesDictionary_TestCacheDescription_Helper(GenerateStringSetProperty, STRINGSET_NONDEFAULT);
            PropertiesDictionary_TestCacheDescription_Helper(GenerateIntegerSetProperty, INTEGERSET_NONDEFAULT);
            PropertiesDictionary_TestCacheDescription_Helper(GeneratePropertiesDictionaryProperty, PROPERTIES_NONDEFAULT);
        }

        private void PropertiesDictionary_TestCacheDescription_Helper(Func<int, string, IOption> GeneratePropertyMethod, object value)
        {
            string textLoaded = string.Empty;
            string desc = "desc to save, no: ";

            var properties = new PropertiesDictionary();

            properties.SetProperty(GeneratePropertyMethod(1, desc + 1), value, true);
            properties.SetProperty(GeneratePropertyMethod(2, null), value, true);
            properties.SetProperty(GeneratePropertyMethod(3, desc + 3), value, true);
            properties.SetProperty(GeneratePropertyMethod(4, desc + 4), value, false);
            properties.SetProperty(GeneratePropertyMethod(5, desc + 5), value, true);

            textLoaded = RoundTripWriteToXmlAndReadAsText(properties);
            textLoaded.Should().Contain(desc + 1);
            textLoaded.Should().NotContain(desc + 2);
            textLoaded.Should().Contain(desc + 3);
            textLoaded.Should().NotContain(desc + 4);
            textLoaded.Should().Contain(desc + 5);
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
                actual.Should().Contain(member);
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

        private string RoundTripWriteToXmlAndReadAsText(PropertiesDictionary properties)
        {
            string temp = Path.GetTempFileName();
            string textLoaded = string.Empty;

            try
            {
                properties.SaveToXml(temp);
                textLoaded = File.ReadAllText(temp);
            }
            finally
            {
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }
            }
            return textLoaded;
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

        public static PerLanguageOption<bool> GenerateBooleanProperty(int i, string description = null) =>
            new PerLanguageOption<bool>(
                FEATURE, nameof(BooleanProperty) + i, defaultValue: () => { return BOOL_DEFAULT; }, description);

        public static PerLanguageOption<StringSet> StringSetProperty { get; } =
            new PerLanguageOption<StringSet>(
                FEATURE, nameof(StringSetProperty), defaultValue: () => { return STRINGSET_DEFAULT; });

        public static PerLanguageOption<StringSet> GenerateStringSetProperty(int i, string description = null) =>
            new PerLanguageOption<StringSet>(
                FEATURE, nameof(StringSetProperty) + i, defaultValue: () => { return STRINGSET_DEFAULT; }, description);

        public static PerLanguageOption<IntegerSet> IntegerSetProperty { get; } =
            new PerLanguageOption<IntegerSet>(
                FEATURE, nameof(IntegerSetProperty), defaultValue: () => { return INTEGERSET_DEFAULT; });

        public static PerLanguageOption<IntegerSet> GenerateIntegerSetProperty(int i, string description = null) =>
            new PerLanguageOption<IntegerSet>(
                FEATURE, nameof(IntegerSetProperty) + i, defaultValue: () => { return INTEGERSET_DEFAULT; }, description);

        public static PerLanguageOption<PropertiesDictionary> PropertiesDictionaryProperty { get; } =
            new PerLanguageOption<PropertiesDictionary>(
                FEATURE, nameof(PropertiesDictionaryProperty), defaultValue: () => { return PROPERTIES_DEFAULT; });

        public static PerLanguageOption<PropertiesDictionary> GeneratePropertiesDictionaryProperty(int i, string description = null) =>
            new PerLanguageOption<PropertiesDictionary>(
                FEATURE, nameof(PropertiesDictionaryProperty) + i, defaultValue: () => { return PROPERTIES_DEFAULT; }, description);
    }
}
