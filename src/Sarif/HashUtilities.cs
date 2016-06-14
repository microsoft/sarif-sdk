﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Security.Cryptography;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class HashUtilities
    {
        public static HashData ComputeHashes(string fileName)
        {
            try
            {
                using (FileStream stream = File.OpenRead(fileName))
                {
                    using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
                    {
                        string md5, sha1, sha256;
                        byte[] checksum;

                        using (var md5Cng = new MD5Cng())
                        {
                            checksum = md5Cng.ComputeHash(bufferedStream);
                            md5 = BitConverter.ToString(checksum).Replace("-", string.Empty);
                        }

                        stream.Seek(0, SeekOrigin.Begin);
                        bufferedStream.Seek(0, SeekOrigin.Begin);

                        using (var sha1Cng = new SHA1Cng())
                        {
                            checksum = sha1Cng.ComputeHash(bufferedStream);
                            sha1 = BitConverter.ToString(checksum).Replace("-", string.Empty);
                        }

                        stream.Seek(0, SeekOrigin.Begin);
                        bufferedStream.Seek(0, SeekOrigin.Begin);

                        using (var sha256Cng = new SHA256Cng())
                        {
                            checksum = sha256Cng.ComputeHash(bufferedStream);
                            sha256 = BitConverter.ToString(checksum).Replace("-", string.Empty);
                        }

                        return new HashData(md5, sha1, sha256);
                    }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return null;
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
                        using (var sha = new SHA256Cng())
                        {
                            byte[] checksum = sha.ComputeHash(bufferedStream);
                            sha256Hash = BitConverter.ToString(checksum).Replace("-", string.Empty);
                        }
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
                        using (var sha = new SHA1Cng())
                        {
                            byte[] checksum = sha.ComputeHash(bufferedStream);
                            sha1 = BitConverter.ToString(checksum).Replace("-", string.Empty);
                        }
                    }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return sha1;
        }

        public static string ComputeMD5Hash(string fileName)
        {
            string md5 = null;

            try
            {
                using (FileStream stream = File.OpenRead(fileName))
                {
                    using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
                    {
                        using (var sha = new MD5Cng())
                        {
                            byte[] checksum = sha.ComputeHash(bufferedStream);
                            md5 = BitConverter.ToString(checksum).Replace("-", string.Empty);
                        }
                    }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return md5;
        }
    }
}
