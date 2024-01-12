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

        public static HttpResponseMessage CreateOKResponse(HttpContent content = null)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            if (content != null)
            {
                response.Content = content;
            }
            return response;
        }

        public static HttpResponseMessage CreateNotFoundResponse(HttpContent content = null)
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            if (content != null)
            {
                response.Content = content;
            }
            return response;
        }

        public static HttpResponseMessage CreateForbiddenResponse(HttpContent content = null)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            if (content != null)
            {
                response.Content = content;
            }
            return response;
        }

        public static HttpResponseMessage CreateBadGatewayResponse(HttpContent content = null)
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadGateway);
            if (content != null)
            {
                response.Content = content;
            }
            return response;
        }

        public static HttpResponseMessage CreateBadRequestResponse(HttpContent content = null)
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            if (content != null)
            {
                response.Content = content;
            }
            return response;
        }

        public static HttpResponseMessage CreateUnauthorizedResponse(HttpContent content = null)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            if (content != null)
            {
                response.Content = content;
            }
            return response;
        }

        public static HttpResponseMessage CreateInternalServerErrorResponse(HttpContent content = null)
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            if (content != null)
            {
                response.Content = content;
            }
            return response;
        }

        public static HttpResponseMessage CreateNonAuthoritativeInformationResponse(HttpContent content = null)
        {
            var response = new HttpResponseMessage(HttpStatusCode.NonAuthoritativeInformation);
            if (content != null)
            {
                response.Content = content;
            }
            return response;
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
            HttpRequestMessage requestInResponse = message.RequestMessage;
            if (requestInResponse != null && requestInResponse.RequestUri != null && requestInResponse.RequestUri.Equals("https://UnknownHost.com"))
            {
                throw new HttpRequestException("No such host is known");
            }

            return Task.FromResult(message);
        }
    }
}
