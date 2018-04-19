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
        private FileContent _fileContent;
        
        private readonly Lazy<string> _decodedContents;

        public FileDetailsModel(FileData fileData)
        {
            Sha256Hash = fileData.Hashes.First(x => x.Algorithm == AlgorithmKind.Sha256).Value;
            _fileContent = fileData.Contents;
            _decodedContents = new Lazy<string>(DecodeContents);
        }

        public string Sha256Hash { get; }

        public string GetContents()
        {
            return _decodedContents.Value;
        }

        private string DecodeContents()
        {
            string content = _fileContent.Text;

            if (string.IsNullOrWhiteSpace(content))
            {
                byte[] data = Convert.FromBase64String(_fileContent.Binary);
                content = Encoding.UTF8.GetString(data);
            }

            _fileContent = null; // Clear _fileContent to save memory.
            return content;
        }
    }
}
