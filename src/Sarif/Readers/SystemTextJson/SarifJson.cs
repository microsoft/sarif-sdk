// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.CodeAnalysis.Sarif.Readers.SystemTextJson
{
    /// <summary>
    /// System.Text.Json (de)serialization entry points for the SARIF object model
    /// during the v6 Newtonsoft → STJ transition (issue #3038).
    /// </summary>
    /// <remarks>
    /// <para>The model is dual-stacked: every property in the frozen object model
    /// carries both Newtonsoft attributes (unchanged) and additive
    /// <c>[Stj.JsonPropertyName]</c> / <c>[Stj.JsonIgnore(Condition=…)]</c>
    /// attributes (applied mechanically by <c>scripts/StjAttributeTransform.ps1</c>).
    /// Existing callers of <see cref="SarifLog.Load(string)"/> /
    /// <see cref="SarifLog.LoadDeferred(string)"/> / <see cref="SarifLog.Save(string)"/>
    /// continue to use Newtonsoft and behave identically.</para>
    /// <para>This class is the new STJ path: <see cref="Load(Stream)"/>,
    /// <see cref="Save(SarifLog, Stream, bool)"/>, and the configured
    /// <see cref="Options"/>. Phase 0 covers the eager path only; the
    /// deferred-reader port is sequenced last (#3038 §"The one genuinely hard
    /// piece").</para>
    /// <para><see cref="SarifJsonContext"/> is the source-generated metadata
    /// provider that makes this path NativeAOT-safe. It is composed with the
    /// reflection resolver during the spike so any type the source generator
    /// missed still binds; the AOT publish step removes that fallback once
    /// the type closure is known.</para>
    /// </remarks>
    public static class SarifJson
    {
        /// <summary>
        /// The configured <see cref="JsonSerializerOptions"/> for the SARIF object
        /// model. Mirrors the Newtonsoft <see cref="SarifContractResolver"/>:
        /// per-type converters are registered globally rather than via per-property
        /// attributes.
        /// </summary>
        public static JsonSerializerOptions Options { get; } = CreateOptions(writeIndented: false);

        /// <summary>Indented variant of <see cref="Options"/>.</summary>
        public static JsonSerializerOptions IndentedOptions { get; } = CreateOptions(writeIndented: true);

        public static SarifLog Load(string path)
        {
            using FileStream stream = File.OpenRead(path);
            return Load(stream);
        }

        public static SarifLog Load(Stream source)
            => JsonSerializer.Deserialize<SarifLog>(source, Options) ?? new SarifLog();

        public static void Save(SarifLog log, Stream target, bool prettyPrint = true)
            => JsonSerializer.Serialize(target, log, prettyPrint ? IndentedOptions : Options);

        public static string Serialize(SarifLog log, bool prettyPrint = true)
            => JsonSerializer.Serialize(log, prettyPrint ? IndentedOptions : Options);

        private static JsonSerializerOptions CreateOptions(bool writeIndented)
        {
            var options = new JsonSerializerOptions
            {
                // Every [DataMember] in the frozen model carries
                // EmitDefaultValue = false, which Newtonsoft honors via its
                // DataContract integration: a CLR-default value (0, false,
                // default(DateTime), null) is never written. WhenWritingDefault
                // is the STJ equivalent. Properties with a [DefaultValue(x)]
                // attribute additionally suppress at x via the
                // HonorDefaultValueAttribute modifier below.
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,

                // Newtonsoft is permissive about comments and trailing commas; the
                // golden-file corpus does not rely on either, but tolerating them
                // keeps the eager path no stricter than the path it replaces.
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,

                // STJ's default escaper aggressively escapes non-ASCII and HTML-
                // sensitive characters; Newtonsoft does not. Relax to keep
                // round-tripped message text byte-comparable in the common case.
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

                WriteIndented = writeIndented,

                // Reflection resolver for the Phase-0 spike so the opt-in
                // modifier below can prune non-[DataMember] properties. The
                // source-gen context (SarifJsonContext) compiles and is the
                // AOT vehicle, but its property list is fixed at compile time;
                // re-enable it once StjAttributeTransform.ps1 also stamps
                // [Stj.JsonIgnore] on every non-DataMember public property,
                // at which point this modifier is dead code and fast-path
                // serialization can come back.
                TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            };

            // The model's `Properties` bag is `internal virtual` on
            // PropertyBagHolder and `internal override` on every node. STJ
            // skips non-public members by default; this modifier admits the
            // one internal member that carries serialized state.
            options.TypeInfoResolver = options.TypeInfoResolver
                .WithAddedModifier(EnforceDataContractOptIn)
                .WithAddedModifier(HonorDefaultValueAttribute)
                .WithAddedModifier(AdmitInternalPropertiesBag);

            // Type-level converter bindings, mirroring SarifContractResolver.
            options.Converters.Add(new StjBigIntegerConverter());
            options.Converters.Add(new StjUriConverter());
            options.Converters.Add(new StjDateTimeConverter());
            options.Converters.Add(new StjVersionConverter());
            options.Converters.Add(new StjSarifVersionConverter());
            options.Converters.Add(new StjPropertyBagConverter());
            options.Converters.Add(new StjSerializedPropertyInfoConverter());
            options.Converters.Add(new StjSarifEnumConverterFactory());
            options.Converters.Add(new StjFlagsEnumConverterFactory());

            return options;
        }

        /// <summary>
        /// Newtonsoft honors <c>[DataContract]</c> opt-in: on a class so attributed,
        /// only <c>[DataMember]</c> properties serialize. STJ does not — it includes
        /// every public property — which surfaces computed members like
        /// <c>SarifNodeKind</c> and <c>Tags</c> in the output. The transform script
        /// stamped every <c>[DataMember]</c> property with <c>[Stj.JsonPropertyName]</c>,
        /// so this modifier replicates opt-in by dropping any property on a SARIF
        /// model type that lacks that attribute.
        /// </summary>
        private static void EnforceDataContractOptIn(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object) { return; }
            if (typeInfo.Type.Namespace == null
                || !typeInfo.Type.Namespace.StartsWith("Microsoft.CodeAnalysis.Sarif", StringComparison.Ordinal))
            {
                return;
            }

            for (int i = typeInfo.Properties.Count - 1; i >= 0; i--)
            {
                JsonPropertyInfo p = typeInfo.Properties[i];
                if (p.AttributeProvider is PropertyInfo pi
                    && pi.GetCustomAttribute<JsonPropertyNameAttribute>() == null)
                {
                    typeInfo.Properties.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Newtonsoft's <c>DefaultValueHandling.IgnoreAndPopulate</c> uses the
        /// <c>[DefaultValue(x)]</c> attribute as the suppression target on write
        /// (and the populate target on read). STJ's
        /// <c>JsonIgnoreCondition.WhenWritingDefault</c> only knows the CLR
        /// type's default. The model's parameterless constructors already
        /// initialize each such property to its <c>[DefaultValue]</c> (covering
        /// the read/populate side); this modifier covers the write side by
        /// suppressing emission when the value equals the attribute value.
        /// </summary>
        private static void HonorDefaultValueAttribute(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object) { return; }

            foreach (JsonPropertyInfo p in typeInfo.Properties)
            {
                if (p.AttributeProvider is not PropertyInfo pi) { continue; }

                var dva = pi.GetCustomAttribute<DefaultValueAttribute>();
                if (dva == null) { continue; }

                object defaultValue = dva.Value;
                p.ShouldSerialize = (_, value) => !Equals(value, defaultValue);
            }
        }

        private static void AdmitInternalPropertiesBag(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object) { return; }
            if (!typeof(PropertyBagHolder).IsAssignableFrom(typeInfo.Type)) { return; }

            // Already present (e.g. via [JsonInclude] in a future pass)?
            foreach (JsonPropertyInfo existing in typeInfo.Properties)
            {
                if (existing.Name == "properties") { return; }
            }

            PropertyInfo pi = typeInfo.Type.GetProperty(
                nameof(PropertyBagHolder.Properties),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (pi == null) { return; }

            JsonPropertyInfo jpi = typeInfo.CreateJsonPropertyInfo(pi.PropertyType, "properties");
            jpi.Get = pi.GetValue;
            jpi.Set = pi.SetValue;
            jpi.CustomConverter = new StjPropertyBagConverter();
            jpi.ShouldSerialize = (obj, value) => value != null;
            typeInfo.Properties.Add(jpi);
        }
    }

    /// <summary>
    /// Source-generated (de)serialization metadata for the SARIF object model.
    /// Rooting <see cref="SarifLog"/> pulls in the full eager-load type closure
    /// (Run, Result, ReportingDescriptor, …); additional roots can be added as
    /// the AOT publish step surfaces them.
    /// </summary>
    // Metadata mode (not Default) so the runtime serializer — which honors
    // TypeInfoResolver modifiers — is used. Fast-path serialization bakes the
    // property list at compile time and would bypass EnforceDataContractOptIn.
    // Metadata mode is still NativeAOT-safe; fast-path is a throughput
    // optimization on top, re-enabled once the model carries [Stj.JsonIgnore]
    // on every non-DataMember property and the modifier is retired.
    [JsonSourceGenerationOptions(
        GenerationMode = JsonSourceGenerationMode.Metadata,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false)]
    [JsonSerializable(typeof(SarifLog))]
    internal partial class SarifJsonContext : JsonSerializerContext
    {
    }
}
