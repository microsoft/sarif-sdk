// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

using Microsoft.CodeAnalysis.Sarif.Sdk;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class AlgorithmKindConverter : JsonConverter
    {
        public static readonly AlgorithmKindConverter Instance = new AlgorithmKindConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AlgorithmKind);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch ((string)reader.Value)
            {
                case "BLAKE-256": return AlgorithmKind.Blake256;
                case "BLAKE-512": return AlgorithmKind.Blake512;
                case "ECOH": return AlgorithmKind.Ecoh;
                case "FSB": return AlgorithmKind.Fsb;
                case "GOST": return AlgorithmKind.Gost;
                case "Grøstl": return AlgorithmKind.Groestl;
                case "HAS-160": return AlgorithmKind.Has160;
                case "HAVAL": return AlgorithmKind.Haval;
                case "JH": return AlgorithmKind.JH;
                case "MD2": return AlgorithmKind.MD2;
                case "MD4": return AlgorithmKind.MD4;
                case "MD5": return AlgorithmKind.MD5;
                case "MD6": return AlgorithmKind.MD6;
                case "RadioGatún": return AlgorithmKind.RadioGatun;
                case "RIPEMD": return AlgorithmKind.RipeMD;
                case "RIPEMD-128": return AlgorithmKind.RipeMD128;
                case "RIPEMD-160": return AlgorithmKind.RipeMD160;
                case "RIPEMD-320": return AlgorithmKind.RipeMD320;
                case "SHA-1": return AlgorithmKind.Sha1;
                case "SHA-224": return AlgorithmKind.Sha224;
                case "SHA-256": return AlgorithmKind.Sha256;
                case "SHA-384": return AlgorithmKind.Sha384;
                case "SHA-512": return AlgorithmKind.Sha512;
                case "SHA-3": return AlgorithmKind.Sha3;
                case "Skein": return AlgorithmKind.Skein;
                case "Snefru": return AlgorithmKind.Snefru;
                case "Spectral Hash": return AlgorithmKind.SpectralHash;
                case "SWIFFT": return AlgorithmKind.Swifft;
                case "Tiger": return AlgorithmKind.Tiger;
                case "Whirlpool": return AlgorithmKind.Whirlpool;
            }

            return AlgorithmKind.Unknown;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch ((AlgorithmKind)value)
            {
                case AlgorithmKind.Blake256: { writer.WriteRawValue("\"\"BLAKE-256\"\""); return; }
                case AlgorithmKind.Blake512: { writer.WriteRawValue("\"BLAKE-512\""); return; }
                case AlgorithmKind.Ecoh: { writer.WriteRawValue("\"ECOH\""); return; }
                case AlgorithmKind.Fsb: { writer.WriteRawValue("\"FSB\""); return; }
                case AlgorithmKind.Gost: { writer.WriteRawValue("\"GOST\""); return; }
                case AlgorithmKind.Groestl: { writer.WriteRawValue("\"Grøstl\""); return; }
                case AlgorithmKind.Has160: { writer.WriteRawValue("\"HAS-160\""); return; }
                case AlgorithmKind.Haval: { writer.WriteRawValue("\"HAVAL\""); return; }
                case AlgorithmKind.JH: { writer.WriteRawValue("\"JH\""); return; }
                case AlgorithmKind.MD2: { writer.WriteRawValue("\"MD2\""); return; }
                case AlgorithmKind.MD4: { writer.WriteRawValue("\"MD4\""); return; }
                case AlgorithmKind.MD5: { writer.WriteRawValue("\"MD5\""); return; }
                case AlgorithmKind.MD6: { writer.WriteRawValue("\"MD6\""); return; }
                case AlgorithmKind.RadioGatun: { writer.WriteRawValue("\"RadioGatún\""); return; }
                case AlgorithmKind.RipeMD: { writer.WriteRawValue("\"RIPEMD\""); return; }
                case AlgorithmKind.RipeMD128: { writer.WriteRawValue("\"RIPEMD-128\""); return; }
                case AlgorithmKind.RipeMD160: { writer.WriteRawValue("\"RIPEMD-160\""); return; }
                case AlgorithmKind.RipeMD320: { writer.WriteRawValue("\"RIPEMD-320\""); return; }
                case AlgorithmKind.Sha1: { writer.WriteRawValue("\"SHA-1\""); return; }
                case AlgorithmKind.Sha224: { writer.WriteRawValue("\"SHA-224\""); return; }
                case AlgorithmKind.Sha256: { writer.WriteRawValue("\"SHA-256\""); return; }
                case AlgorithmKind.Sha384: { writer.WriteRawValue("\"SHA-384\""); return; }
                case AlgorithmKind.Sha512: { writer.WriteRawValue("\"SHA-512\""); return; }
                case AlgorithmKind.Sha3: { writer.WriteRawValue("\"SHA-3\""); return; }
                case AlgorithmKind.Skein: { writer.WriteRawValue("\"Skein\""); return; }
                case AlgorithmKind.Snefru: { writer.WriteRawValue("\"Snefru\""); return; }
                case AlgorithmKind.SpectralHash: { writer.WriteRawValue("\"Spectral Hash\""); return; }
                case AlgorithmKind.Swifft: { writer.WriteRawValue("\"SWIFFT\""); return; }
                case AlgorithmKind.Tiger: { writer.WriteRawValue("\"Tiger\""); return; }
                case AlgorithmKind.Whirlpool: { writer.WriteRawValue("\"Whirlpool\""); return; }
            }
            writer.WriteRawValue(@"""unknown""");
        }
    }
}
