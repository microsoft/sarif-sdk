// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class HttpMockHelper : DelegatingHandler
    {
        public static readonly HttpResponseMessage OKResponse =
            new HttpResponseMessage(HttpStatusCode.OK);

        public static readonly HttpResponseMessage NotFoundResponse =
            new HttpResponseMessage(HttpStatusCode.NotFound);

        public static readonly HttpResponseMessage ForbiddenResponse =
            new HttpResponseMessage(HttpStatusCode.Forbidden);

        public static readonly HttpResponseMessage BadRequestResponse =
            new HttpResponseMessage(HttpStatusCode.BadRequest);

        public static readonly HttpResponseMessage UnauthorizedResponse =
            new HttpResponseMessage(HttpStatusCode.Unauthorized);

        public static readonly HttpResponseMessage InternalServerErrorResponse =
            new HttpResponseMessage(HttpStatusCode.InternalServerError);

        public static readonly HttpResponseMessage NonAuthoritativeInformationResponse =
            new HttpResponseMessage(HttpStatusCode.NonAuthoritativeInformation);

        private readonly List<Tuple<HttpRequestMessage, HttpResponseMessage>> fakeResponses =
            new List<Tuple<HttpRequestMessage, HttpResponseMessage>>();

        public void Mock(HttpRequestMessage httpRequestMessage, HttpStatusCode httpStatusCode, HttpContent httpContent)
        {
            this.fakeResponses.Add(new Tuple<HttpRequestMessage, HttpResponseMessage>(
                httpRequestMessage,
                new HttpResponseMessage(httpStatusCode) { RequestMessage = httpRequestMessage, Content = httpContent }));
        }

        public void Mock(HttpRequestMessage httpRequestMessage, HttpResponseMessage httpResponseMessage)
        {
            this.fakeResponses.Add(new Tuple<HttpRequestMessage, HttpResponseMessage>(
                httpRequestMessage,
                httpResponseMessage));
        }

        public void Clear()
        {
            this.fakeResponses.Clear();
        }

        private static bool CompareHeaders(HttpRequestHeaders headers1, HttpRequestHeaders headers2)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers1)
            {
                string headerName = header.Key;
                if (!headers2.TryGetValues(headerName, out IEnumerable<string> values))
                {
                    return false;
                }

                string headerContent1 = string.Join(",", header.Value);
                string headerContent2 = string.Join(",", values);

                if (headerContent1 != headerContent2)
                {
                    return false;
                }
            }

            return true;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Tuple<HttpRequestMessage, HttpResponseMessage> fakeResponse;

            if (request.Headers.IsEmptyEnumerable())
            {
                fakeResponse = this.fakeResponses.Find(fr =>
                    fr.Item1.RequestUri == request.RequestUri
                    && fr.Item1.Headers.IsEmptyEnumerable());
            }
            else
            {
                fakeResponse = this.fakeResponses.Find(fr =>
                    fr.Item1.RequestUri == request.RequestUri
                    && CompareHeaders(request.Headers, fr.Item1.Headers));
            }

            return Task.FromResult(fakeResponse.Item2);
        }
    }
}
