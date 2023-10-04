// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
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

        public static HttpResponseMessage CreateNotFoundResponse()
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        public static HttpResponseMessage CreateForbiddenResponse()
        {
            return new HttpResponseMessage(HttpStatusCode.Forbidden);
        }

        public static HttpResponseMessage CreateBadGatewayResponse()
        {
            return new HttpResponseMessage(HttpStatusCode.BadGateway);
        }

        public static HttpResponseMessage CreateBadRequestResponse()
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        public static HttpResponseMessage CreateUnauthorizedResponse()
        {
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        public static HttpResponseMessage CreateInternalServerErrorResponse()
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }

        public static HttpResponseMessage CreateNonAuthoritativeInformationResponse()
        {
            return new HttpResponseMessage(HttpStatusCode.NonAuthoritativeInformation);
        }

        private readonly ConcurrentQueue<HttpResponseMessage> mockedResponses =
            new ConcurrentQueue<HttpResponseMessage>();

        public static HttpResponseMessage GetResponseForStatusCode(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.OK: { return CreateOKResponse(); }
                case HttpStatusCode.NotFound: { return CreateNotFoundResponse(); }
                case HttpStatusCode.Forbidden: { return CreateForbiddenResponse(); }
                case HttpStatusCode.BadGateway: { return CreateBadGatewayResponse(); }
                case HttpStatusCode.BadRequest: { return CreateBadRequestResponse(); }
                case HttpStatusCode.Unauthorized: { return CreateUnauthorizedResponse(); }
                case HttpStatusCode.InternalServerError: { return CreateInternalServerErrorResponse(); }
                case HttpStatusCode.NonAuthoritativeInformation: { return CreateNonAuthoritativeInformationResponse(); }
            }

            throw new NotImplementedException();
        }

        public void Mock(HttpRequestMessage httpRequestMessage, HttpStatusCode httpStatusCode, HttpContent responseContent)
        {
            this.mockedResponses.Enqueue(
                new HttpResponseMessage(httpStatusCode) { RequestMessage = httpRequestMessage, Content = responseContent });
        }

        public void Mock(HttpResponseMessage httpResponseMessage)
        {
            this.mockedResponses.Enqueue(httpResponseMessage);
        }

        public void Clear()
        {
            while (!this.mockedResponses.IsEmpty)
            {
                this.mockedResponses.TryDequeue(out _);
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.mockedResponses.TryDequeue(out HttpResponseMessage message);
            HttpRequestMessage _ = message.RequestMessage;
            return Task.FromResult(message);
        }
    }
}
