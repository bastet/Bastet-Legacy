using Bastet.Database.Model;
using CoAP;
using CoAP.Http;
using CoAP.Proxy;
using Nancy;
using Nancy.LightningCache.Extensions;
using Nancy.Security;
using ServiceStack.OrmLite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Request = Nancy.Request;
using Response = CoAP.Response;

namespace Bastet.HttpServer.Modules
{
    public class DevicesProxyModule
        : NancyModule
    {
        public const string PATH = ApiMain.PATH + "/devices/{id}/proxy";

        private readonly IDbConnection _connection;

        public DevicesProxyModule(IDbConnection connection)
            : base(PATH)
        {
            _connection = connection;

            //this.RequiresHttps();

            Get["/.well-known/core", runAsync: true] = ProxyWellKnownCore;

            Get["/", runAsync: true] = Get["/{endpoint*}", runAsync: true] = Proxy;
            Put["/", runAsync: true] = Put["/{endpoint*}", runAsync: true] = Proxy;
            Post["/", runAsync: true] = Post["/{endpoint*}", runAsync: true] = Proxy;
            Delete["/", runAsync: true] = Delete["/{endpoint*}", runAsync: true] = Proxy;
        }

        private Task<dynamic> ProxyWellKnownCore(dynamic parameters, CancellationToken ct)
        {
            return Task.Factory.StartNew(() => {
                if (Request.Headers.ContentType.ToLowerInvariant() == "application/link-format")
                    return Proxy(parameters, ct);

                this.RequiresAuthentication();
                this.RequiresAnyClaim(new[] {"superuser", "device-proxy-all", string.Format("device-proxy-{0}", (int) parameters.id)});

                var device = _connection.SingleById<Device>((int) parameters.id);
                if (device == null)
                    return Negotiate
                        .WithStatusCode(HttpStatusCode.NotFound)
                        .WithModel(new {Error = string.Format("Unknown Device ID ({0})", parameters.id)});

                //Proxy request, and fail as necessary
                var response = Proxy(new CoapDotNetHttpRequest(Request, device.Url + "/.well-known/core"), new Uri("coap://" + device.Url + "/.well-known/core"));
                if (response == null)
                    return Negotiate.WithStatusCode(HttpStatusCode.GatewayTimeout);
                if (response.StatusCode != 200)
                    return (HttpStatusCode) response.StatusCode;

                //Read body
                response.OutputStream.Seek(0, SeekOrigin.Begin);
                var r = new StreamReader(response.OutputStream);

                //Serialize link format
                var c = new LinkFormatParser.LinkCollection(r.ReadToEnd());

                //Return link format model and let content negotiation serialize into correct format
                return Negotiate
                    .WithModel(c.ToArray())
                    .WithHeaders(response.Headers.Where(a => a.Item1.ToLowerInvariant() != "content-type").ToArray());
            }, ct);
        }

        private Task<dynamic> Proxy(dynamic parameters, CancellationToken ct)
        {
            return Task<dynamic>.Factory.StartNew(() =>
            {
                this.RequiresAuthentication();
                this.RequiresAnyClaim(new[] { "superuser", "device-proxy-all", string.Format("device-proxy-{0}", (int)parameters.id) });

                var device = _connection.SingleById<Device>((int) parameters.id);
                if (device == null)
                    return Negotiate
                        .WithStatusCode(HttpStatusCode.NotFound)
                        .WithModel(new {Error = string.Format("Unknown Device ID ({0})", parameters.id)});

                var httpResponse = Proxy(new CoapDotNetHttpRequest(Request, device.Url + "/" + (string) parameters.endpoint), new Uri("coap://" + device.Url + "/" + (string) parameters.endpoint));
                if (httpResponse == null)
                    return HttpStatusCode.GatewayTimeout;

                //Send HTTP response
                httpResponse.OutputStream.Position = 0;
                return Response
                    .FromStream(httpResponse.OutputStream, httpResponse.ContentType)
                    .WithHeaders(httpResponse.Headers.ToArray())
                    .WithStatusCode(httpResponse.StatusCode)
                    .AsCacheable(DateTime.Now + TimeSpan.FromSeconds(httpResponse.MaxAge));
            }, ct);
        }

        private CoapDotNetHttpResponse Proxy(CoapDotNetHttpRequest request, Uri coapUri)
        {
            var coapRequest = HttpTranslator.GetCoapRequest(request, Request.Url.SiteBase, true);
            coapRequest.URI = coapUri;

            //Setup response handler
            Response response = null;
            EventHandler<ResponseEventArgs> responseHandler = null;
            responseHandler = (_, e) =>
            {
                response = e.Response;
                coapRequest.Respond -= responseHandler;
            };
            coapRequest.Respond += responseHandler;

            //send request
            coapRequest.Send();

            DateTime start = DateTime.Now;
            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            while (response == null && (DateTime.Now - start).TotalSeconds < 30)
                Thread.Sleep(1);

            if (response == null)
                return null;

            //Turn COAP response into HTTP response
            var httpResponse = new CoapDotNetHttpResponse { MaxAge = response.MaxAge };
            HttpTranslator.GetHttpResponse(request, response, httpResponse);

            return httpResponse;
        }

        private class CoapDotNetHttpRequest
            : IHttpRequest
        {
            private readonly Request _request;
            private readonly string _url;

            public CoapDotNetHttpRequest(Request request, string url)
            {
                _request = request;
                _url = url;
            }

            public string Url
            {
                get { return _url; }
            }

            public string RequestUri
            {
                get { return _url; }
            }

            public string QueryString
            {
                get { return _request.Url.Query; }
            }

            public string Method
            {
                get { return _request.Method; }
            }

            public NameValueCollection Headers
            {
                get
                {
                    NameValueCollection nvc = new NameValueCollection();
                    foreach (var requestHeader in _request.Headers.SelectMany(a => a.Value.Select(v => new {Key = a.Key, Value = v})))
                        nvc.Add(requestHeader.Key, requestHeader.Value);
                    return nvc;
                }
            }

            public Stream InputStream
            {
                get { return _request.Body; }
            }

            public string Host
            {
                get { throw new NotImplementedException(); }
            }

            public string UserAgent
            {
                get { throw new NotImplementedException(); }
            }

            public string GetParameter(string name)
            {
                throw new NotImplementedException();
            }

            public string[] GetParameters(string name)
            {
                throw new NotImplementedException();
            }

            public object this[object key]
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }

        private class CoapDotNetHttpResponse
            : IHttpResponse
        {
            private readonly ConcurrentDictionary<string, string> _headers = new ConcurrentDictionary<string, string>();

            public IEnumerable<Tuple<string, string>> Headers
            {
                get
                {
                    return _headers.Select(a => new Tuple<string, string>(a.Key, a.Value));
                }
            }

            public string ContentType
            {
                get
                {
                    return GetHeader("content-type");
                }
            }

            public void AppendHeader(string name, string value)
            {
                _headers.AddOrUpdate(name, value, (_, __) => value);
            }

            public string GetHeader(string name)
            {
                string value;
                _headers.TryGetValue(name, out value);
                return value;
            }

            private readonly MemoryStream _stream = new MemoryStream();

            public MemoryStream OutputStream
            {
                get
                {
                    return _stream;
                }
            }

            public int StatusCode { get; set; }

            public string StatusDescription { get; set; }

            Stream IHttpResponse.OutputStream
            {
                get
                {
                    return _stream;
                }
            }

            public long MaxAge { get; set; }
        }
    }
}
