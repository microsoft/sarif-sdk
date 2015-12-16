// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif.Sdk
{
    public static class HashUtilities
    {
        public static string ComputeSha256Hash(string fileName)
        {
            string sha256Hash = null;

            try
            {
                using (FileStream stream = File.OpenRead(fileName))
                {
                    using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
                    {
                        var sha = new SHA256Cng();
                        byte[] checksum = sha.ComputeHash(bufferedStream);
                        sha256Hash = BitConverter.ToString(checksum).Replace("-", String.Empty);
                    }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return sha256Hash;
        }
    }
}
