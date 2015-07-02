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
        private UriBuilder baseUri = new UriBuilder("https://wsilb.moviatrafik.dk/");

        private HttpClient _HttpClient = new HttpClient();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //Console.WriteLine(new HttpMessageContent(request).ReadAsStringAsync().Result);


            var newRequest = CreateNewRequest(request);

            var t = _HttpClient.SendAsync(newRequest);

            await t;

            if (t.IsCompleted)
            {
                try
                {
                    var response = CreateNewResponse(t.Result);
                    //Console.WriteLine("--->");
                    //Console.WriteLine(new HttpMessageContent(response).ReadAsStringAsync().Result);
                    return response;
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
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
            var mediatype = (httpContent.Headers != null && httpContent.Headers.ContentType != null) ? httpContent.Headers.ContentType.MediaType : null;

            return new StringContent(httpContent.ReadAsStringAsync().Result, Encoding.UTF8, mediatype);
        }
    }


}