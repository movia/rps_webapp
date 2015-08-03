using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Newtonsoft.Json;

namespace RpsPositionWebApp
{
    /// <summary>
    /// Proxy handler for the Movia Web Service Infrastructure (WSI)
    /// </summary>
    public class WsiProxyHandler : DelegatingHandler
    {
        private readonly UriBuilder baseUri;
        private readonly HttpClient httpClient;

        public WsiProxyHandler()
        {
            baseUri = new UriBuilder("https://wsilb.moviatrafik.dk/");
            httpClient = new HttpClient();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var newRequest = CreateNewRequest(request);

            var t = httpClient.SendAsync(newRequest);

            await t;

            if (t.IsCompleted)
            {
                try
                {
                    var response = CreateNewResponse(t.Result);
                    return response;
                }
                catch (Exception ex)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent(ex.Message) };
                }
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent(t.Exception.Message) };
            }
        }

        private HttpRequestMessage CreateNewRequest(HttpRequestMessage request)
        {
            var newRequest = new HttpRequestMessage();

            var routedata = request.GetRouteData().Values;
            var path = (routedata.ContainsKey("path") ? routedata["path"] : null) ?? string.Empty;

            newRequest.Headers.Clear();
            foreach (var header in request.Headers)
            {
                newRequest.Headers.Add(header.Key, header.Value);
            }

            if (request.Method != HttpMethod.Get && request.Content.Headers.ContentLength != 0)
            {
                newRequest.Content = TranslateContent(request.Content);
            }
            else
            {
                newRequest.Content = null;
            }

            newRequest.Headers.Host = null;
            newRequest.Method = request.Method;

            UriBuilder forwardUri = new UriBuilder(request.RequestUri);
            forwardUri.Scheme = baseUri.Scheme;
            forwardUri.Host = baseUri.Host;
            forwardUri.Port = baseUri.Port;
            forwardUri.Path = baseUri.Path + path;

            newRequest.RequestUri = forwardUri.Uri;

            return newRequest;
        }

        private HttpResponseMessage CreateNewResponse(HttpResponseMessage response)
        {
            response.Content = TranslateContent(response.Content);
            return response;
        }

        private HttpContent TranslateContent(HttpContent httpContent)
        {
            var mediatype = httpContent.Headers?.ContentType?.MediaType;

            return new StringContent(httpContent.ReadAsStringAsync().Result, Encoding.UTF8, mediatype);
        }
    }
}