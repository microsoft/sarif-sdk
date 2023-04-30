// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class HttpMockHelper : DelegatingHandler
    {
        public const string AnyContentText = "29f8354b-8b0d-4d21-91ac-bd04c47b85fb";

        public static StringContent AnyContent()
        {
            return new StringContent(AnyContentText);
        }

        public static HttpResponseMessage CreateOKResponse()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public static readonly HttpResponseMessage OKResponse =
            new HttpResponseMessage(HttpStatusCode.OK);

        public static readonly HttpResponseMessage NotFoundResponse =
            new HttpResponseMessage(HttpStatusCode.NotFound);

        public static readonly HttpResponseMessage ForbiddenResponse =
            new HttpResponseMessage(HttpStatusCode.Forbidden);

        public static readonly HttpResponseMessage BadGatewayResponse =
            new HttpResponseMessage(HttpStatusCode.BadGateway);

        public static readonly HttpResponseMessage BadRequestResponse =
            new HttpResponseMessage(HttpStatusCode.BadRequest);

        public static readonly HttpResponseMessage UnauthorizedResponse =
            new HttpResponseMessage(HttpStatusCode.Unauthorized);

        public static readonly HttpResponseMessage InternalServerErrorResponse =
            new HttpResponseMessage(HttpStatusCode.InternalServerError);

        public static readonly HttpResponseMessage NonAuthoritativeInformationResponse =
            new HttpResponseMessage(HttpStatusCode.NonAuthoritativeInformation);

        private readonly List<Tuple<HttpRequestMessage, string, HttpResponseMessage>> mockedResponses =
            new List<Tuple<HttpRequestMessage, string, HttpResponseMessage>>();

        public static HttpResponseMessage GetResponseForStatusCode(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.OK: { return OKResponse; }
                case HttpStatusCode.NotFound: { return NotFoundResponse; }
                case HttpStatusCode.Forbidden: { return ForbiddenResponse; }
                case HttpStatusCode.BadGateway: { return BadGatewayResponse; }
                case HttpStatusCode.BadRequest: { return BadRequestResponse; }
                case HttpStatusCode.Unauthorized: { return UnauthorizedResponse; }
                case HttpStatusCode.InternalServerError: { return InternalServerErrorResponse; }
                case HttpStatusCode.NonAuthoritativeInformation: { return NonAuthoritativeInformationResponse; }
            }

            throw new NotImplementedException();
        }

        public void Mock(HttpRequestMessage httpRequestMessage, HttpStatusCode httpStatusCode, HttpContent responseContent)
        {
            string requestContent = httpRequestMessage?.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            this.mockedResponses.Add(new Tuple<HttpRequestMessage, string, HttpResponseMessage>(
                httpRequestMessage,
                requestContent ?? string.Empty,
                new HttpResponseMessage(httpStatusCode) { RequestMessage = httpRequestMessage, Content = responseContent }));
        }

        public void Mock(HttpRequestMessage httpRequestMessage, HttpResponseMessage httpResponseMessage)
        {
            string requestContent = httpRequestMessage?.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            this.mockedResponses.Add(new Tuple<HttpRequestMessage, string, HttpResponseMessage>(
                httpRequestMessage,
                requestContent ?? string.Empty,
                httpResponseMessage));
        }

        public void Clear()
        {
            this.mockedResponses.Clear();
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
            Tuple<HttpRequestMessage, string, HttpResponseMessage> fakeResponse;

            string content = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult() ?? string.Empty;

            if (request.Headers.IsEmptyEnumerable())
            {
                fakeResponse = this.mockedResponses.Find(fr =>
                    fr.Item1.RequestUri == request.RequestUri &&
                    fr.Item1.Headers.IsEmptyEnumerable()
                    && (fr.Item2 == content || fr.Item2 == AnyContentText));
            }
            else
            {
                fakeResponse = this.mockedResponses.Find(fr =>
                    fr.Item1.RequestUri == request.RequestUri
                    && CompareHeaders(request.Headers, fr.Item1.Headers)
                    && (fr.Item2 == content || fr.Item2 == AnyContentText));
            }

            return Task.FromResult(fakeResponse.Item3);
        }
    }
}
