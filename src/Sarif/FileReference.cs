// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif
{
    public sealed class FileReference
    {
        // Thread safe due to readonly
        public readonly Uri Uri;
        // Thread safe lazy initialization of text
        private readonly Lazy<Task<string>> text;

        public FileReference(Uri uri)
            : this(uri, null)
        { }

        public FileReference(Uri uri, string text)
        {
            this.Uri = uri;

            // TODO: We should probably accept file URIs too.
            if (text != null || !this.Uri.IsAbsoluteUri)
            {
                // Text explicitly set or we don't have the absolute URI, always use the text.
                this.text = new Lazy<Task<string>>(() => Task.FromResult<string>(text), LazyThreadSafetyMode.PublicationOnly);
                return;
            }

            // Text is not explicitly set and we have an absolute URI, attempt to download the URI.

            // Allow test infrastructure to override the download behavior with a custom implementation.
            Func<Uri, Task<string>> downloadCallback = OverriddenDownloadImplementation;
            if (downloadCallback == null)
            {
                downloadCallback = DefaultDownload;
            }

            this.text = new Lazy<Task<string>>(() => downloadCallback(this.Uri), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public object Tag { get; set; }

        public string GetText()
        {
            return this.GetTextAsync().Result;
        }

        public Task<string> GetTextAsync()
        {
            return this.text.Value;
        }

        public string Substring(int offset, int length)
        {
            return this.GetText().Substring(offset, length);
        }

        public int GetLength()
        {
            return this.GetText().Length;
        }

        // Download infrastructure; this allows the test harness
        // to replace the download implementation with something else.
        public static Func<Uri, Task<string>> OverriddenDownloadImplementation = null;
        private static Task<string> DefaultDownload(Uri uri)
        {
            var wc = new WebClient();
            Task<string> backgroundDownload = wc.DownloadStringTaskAsync(uri);
            backgroundDownload.ContinueWith((downloadTask) => { wc.Dispose(); });
            return backgroundDownload;
        }
    }
}
