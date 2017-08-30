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
        // Contents of file. May or may not be Base64 encoded.
        private string _contents;
        
        private bool _isBase64Encoded;

        public FileDetailsModel(FileData fileData)
        {
            Sha256Hash = fileData.Hashes.First(x => x.Algorithm == AlgorithmKind.Sha256).Value;
            _contents = fileData.Contents;
            _isBase64Encoded = true;
        }

        public string Sha256Hash { get; }

        public string GetContents()
        {
            // If the contents are encoded, decode them.
            if (_isBase64Encoded)
            {
                DecodeContents();
            }

            return _contents;
        }

        private void DecodeContents()
        {
            byte[] data = Convert.FromBase64String(_contents);
            _contents = Encoding.UTF8.GetString(data);
            _isBase64Encoded = false;
        }
    }
}
