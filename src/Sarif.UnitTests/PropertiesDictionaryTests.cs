using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif
{
    [TestClass]
    public class PropertiesDictionaryTests
    {
        private const string FEATURE = "Sarif.Sdk";
        private const string OPTIONS = FEATURE + ".Options";

        private static bool BOOL_DEFAULT = true;
        private static StringSet STRINGSET_DEFAULT = new StringSet(new string[] { "a", "b", "c" });
        private static IntegerSet INTEGERSET_DEFAULT = new IntegerSet(new int[] { -1, 0, 1, 2 });

        [TestMethod]
        public void PropertiesDictionary_RetrieveBoolean()
        {
            var properties = new PropertiesDictionary();
            properties.GetProperty(BooleanProperty).Should().Be(BOOL_DEFAULT);

            var nonDefaultValue = false;
            properties.SetProperty(BooleanProperty, nonDefaultValue);
            properties.GetProperty(BooleanProperty).Should().Be(nonDefaultValue);

            properties = RoundTripThroughXml(properties);
            properties.GetProperty(BooleanProperty).Should().Be(nonDefaultValue);

            properties = RoundTripThroughJson(properties);
            properties.GetProperty(BooleanProperty).Should().Be(nonDefaultValue);
        }


        [TestMethod]
        public void PropertiesDictionary_RetrieveStringSet()
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

        [TestMethod]
        public void PropertiesDictionary_RetrieveIntegerSet()
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
    }
}
