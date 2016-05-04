// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Security.Cryptography;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class HashUtilities
    {
        public static void ComputeHashes(string fileName, out string md5, out string sha1, out string sha256)
        {
            sha1 = sha256 = md5 = null;
            try
            {
                using (FileStream stream = File.OpenRead(fileName))
                {
                    using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
                    {
                        var md5Cng = new MD5Cng();
                        byte[] checksum = md5Cng.ComputeHash(bufferedStream);
                        md5 = BitConverter.ToString(checksum).Replace("-", String.Empty);

                        stream.Seek(0, SeekOrigin.Begin);
                        bufferedStream.Seek(0, SeekOrigin.Begin);

                        var sha1Cng = new SHA1Cng();
                        checksum = sha1Cng.ComputeHash(bufferedStream);
                        sha1 = BitConverter.ToString(checksum).Replace("-", String.Empty);

                        stream.Seek(0, SeekOrigin.Begin);
                        bufferedStream.Seek(0, SeekOrigin.Begin);

                        var sha256Cng = new SHA256Cng();
                        checksum = sha256Cng.ComputeHash(bufferedStream);
                        sha256 = BitConverter.ToString(checksum).Replace("-", String.Empty);                            
                    }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

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

        public static string ComputeSha1Hash(string fileName)
        {
            string sha1 = null;

            try
            {
                using (FileStream stream = File.OpenRead(fileName))
                {
                    using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
                    {
                        var sha = new SHA1Cng();
                        byte[] checksum = sha.ComputeHash(bufferedStream);
                        sha1 = BitConverter.ToString(checksum).Replace("-", String.Empty);
                    }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return sha1;
        }

        public static string ComputeMd5Hash(string fileName)
        {
            string md5 = null;

            try
            {
                using (FileStream stream = File.OpenRead(fileName))
                {
                    using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
                    {
                        var sha = new MD5Cng();
                        byte[] checksum = sha.ComputeHash(bufferedStream);
                        md5 = BitConverter.ToString(checksum).Replace("-", String.Empty);
                    }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return md5;
        }
    }
}
