// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Serialization
{
    /// <summary>
    /// Reads and writes SARIF-compatible JSON without exposing a JSON implementation in its public API.
    /// </summary>
    public static class SarifJson
    {
        public static string Serialize<T>(T value, SarifJsonOptions options = null)
        {
            using (var textWriter = new StringWriter(options?.Culture))
            {
                Serialize(textWriter, value, options);
                return textWriter.ToString();
            }
        }

        public static void Serialize<T>(TextWriter textWriter, T value, SarifJsonOptions options = null)
        {
            if (textWriter == null)
            {
                throw new ArgumentNullException(nameof(textWriter));
            }

            try
            {
                JsonSerializer serializer = CreateSerializer(options);
                using (var jsonWriter = new JsonTextWriter(textWriter)
                {
                    CloseOutput = false,
                    Formatting = options?.WriteIndented == true ? Formatting.Indented : Formatting.None
                })
                {
                    serializer.Serialize(jsonWriter, value);
                }
            }
            catch (JsonException ex)
            {
                throw new SarifJsonSerializationException(ex.Message, ex)
                {
                    Path = GetPath(ex)
                };
            }
            catch (ArgumentException ex)
            {
                throw new SarifJsonSerializationException(ex.Message, ex);
            }
            catch (FormatException ex)
            {
                throw new SarifJsonSerializationException(ex.Message, ex);
            }
        }

        public static void SerializeToFile<T>(string filePath, T value, SarifJsonOptions options = null)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            using (StreamWriter writer = File.CreateText(filePath))
            {
                Serialize(writer, value, options);
            }
        }

        public static T Deserialize<T>(string json, SarifJsonOptions options = null)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            using (var reader = new StringReader(json))
            {
                return Deserialize<T>(reader, options);
            }
        }

        public static T Deserialize<T>(TextReader textReader, SarifJsonOptions options = null)
        {
            return (T)Deserialize(textReader, typeof(T), options);
        }

        public static object Deserialize(
            TextReader textReader,
            Type objectType,
            SarifJsonOptions options = null)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException(nameof(textReader));
            }

            if (objectType == null)
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            string json = textReader.ReadToEnd();
            ValidateJson(json);

            try
            {
                JsonSerializer serializer = CreateSerializer(options);
                using (var stringReader = new StringReader(json))
                using (var jsonReader = new JsonTextReader(stringReader)
                {
                    DateParseHandling = DateParseHandling.None
                })
                {
                    return serializer.Deserialize(jsonReader, objectType);
                }
            }
            catch (JsonException ex)
            {
                throw new SarifJsonSerializationException(ex.Message, ex)
                {
                    Path = GetPath(ex)
                };
            }
            catch (ArgumentException ex)
            {
                throw new SarifJsonSerializationException(ex.Message, ex);
            }
            catch (FormatException ex)
            {
                throw new SarifJsonSerializationException(ex.Message, ex);
            }
        }

        public static T DeserializeFromFile<T>(string filePath, SarifJsonOptions options = null)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            using (StreamReader reader = File.OpenText(filePath))
            {
                return Deserialize<T>(reader, options);
            }
        }

        /// <summary>
        /// Creates a streaming result writer that does not take ownership of <paramref name="textWriter"/>.
        /// </summary>
        public static ResultLogWriter CreateResultLogWriter(TextWriter textWriter, bool writeIndented = false)
        {
            return new ResultLogWriter(textWriter, writeIndented);
        }

        /// <summary>
        /// Updates prerelease SARIF JSON to the current version.
        /// </summary>
        public static SarifLog UpdateToCurrentVersion(
            string prereleaseSarifLog,
            bool writeIndented,
            out string updatedLog)
        {
            try
            {
                return PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(
                    prereleaseSarifLog,
                    writeIndented ? Formatting.Indented : Formatting.None,
                    out updatedLog);
            }
            catch (JsonReaderException ex)
            {
                throw CreateReaderException(ex);
            }
            catch (JsonSerializationException ex)
            {
                throw CreateSerializationException(ex);
            }
            catch (ArgumentException ex)
            {
                throw new SarifJsonSerializationException(ex.Message, ex);
            }
            catch (FormatException ex)
            {
                throw new SarifJsonSerializationException(ex.Message, ex);
            }
        }

        private static JsonSerializer CreateSerializer(SarifJsonOptions options)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new SarifContractResolver(),
                Culture = options?.Culture ?? CultureInfo.InvariantCulture,
                Formatting = options?.WriteIndented == true ? Formatting.Indented : Formatting.None,
                NullValueHandling = options?.IgnoreNullValues == true
                    ? NullValueHandling.Ignore
                    : NullValueHandling.Include,
                StringEscapeHandling = options?.EscapeHtml == true
                    ? StringEscapeHandling.EscapeHtml
                    : StringEscapeHandling.Default
            };

            if (options?.Error != null)
            {
                settings.Error = (sender, args) =>
                {
                    var errorArgs = new SarifJsonErrorEventArgs(
                        args.CurrentObject,
                        args.ErrorContext.OriginalObject,
                        args.ErrorContext.Member,
                        args.ErrorContext.Path,
                        args.ErrorContext.Error);

                    options.Error(errorArgs);
                    args.ErrorContext.Handled = errorArgs.Handled;
                };
            }

            return JsonSerializer.Create(settings);
        }

        private static void ValidateJson(string json)
        {
            using (var stringReader = new StringReader(json))
            using (var jsonReader = new JsonTextReader(stringReader)
            {
                DateParseHandling = DateParseHandling.None
            })
            {
                try
                {
                    JToken.ReadFrom(jsonReader);
                    while (jsonReader.Read())
                    {
                        if (jsonReader.TokenType != JsonToken.Comment)
                        {
                            throw new JsonReaderException(
                                "Additional text encountered after finished reading JSON content.");
                        }
                    }
                }
                catch (JsonReaderException ex)
                {
                    throw CreateReaderException(ex);
                }
            }
        }

        private static SarifJsonReaderException CreateReaderException(JsonReaderException ex)
        {
            return new SarifJsonReaderException(ex.Message, ex)
            {
                Path = ex.Path,
                LineNumber = ex.LineNumber,
                LinePosition = ex.LinePosition
            };
        }

        private static SarifJsonSerializationException CreateSerializationException(JsonSerializationException ex)
        {
            return new SarifJsonSerializationException(ex.Message, ex)
            {
                Path = ex.Path
            };
        }

        private static string GetPath(JsonException ex)
        {
            if (ex is JsonReaderException readerException)
            {
                return readerException.Path;
            }

            if (ex is JsonSerializationException serializationException)
            {
                return serializationException.Path;
            }

            return null;
        }
    }
}
