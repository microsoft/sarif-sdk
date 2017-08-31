// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.Sarif.Viewer.Models
{
    public class FileDetailsModel
    {
        // Base64 encoded contents of file.
        private string _rawContents;
        
        private readonly Lazy<string> _decodedContents;

        public FileDetailsModel(FileData fileData)
        {
            Sha256Hash = fileData.Hashes.First(x => x.Algorithm == AlgorithmKind.Sha256).Value;
            _rawContents = fileData.Contents;
            _decodedContents = new Lazy<string>(DecodeContents);
        }

        public string Sha256Hash { get; }

        public string GetContents()
        {
            return _decodedContents.Value;
        }

        private string DecodeContents()
        {
            byte[] data = Convert.FromBase64String(_rawContents);
            string decodedContents = Encoding.UTF8.GetString(data);
            _rawContents = null; // Clear _rawContents to save memory.
            return decodedContents;
        }
    }
}
