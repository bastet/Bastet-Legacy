using System;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using Bastet.Database.Model;
using CoAP.Http;
using CoAP.Proxy;
using Nancy;
using ServiceStack.OrmLite;

namespace Bastet.HttpServer.Modules
{
    public class DevicesProxyModule
        : NancyModule
    {
        public const string PATH = "/devices/{id}";

        private readonly IDbConnection _connection;
        //private HttpTranslator

        public DevicesProxyModule(IDbConnection connection)
            : base(PATH)
        {
            _connection = connection;

            Get["/{endpoint*}"] = ProxyGet;
            Put["/{endpoint*}"] = ProxyPut;
            Post["/{endpoint*}"] = ProxyPost;
            Delete["/{endpoint*}"] = ProxyDelete;
        }

        private dynamic ProxyGet(dynamic parameters)
        {
            var device = _connection.SingleById<Device>((int)parameters.id);
            if (device == null)
                return Negotiate
                    .WithStatusCode(HttpStatusCode.NotFound)
                    .WithModel(new { Error = string.Format("Unknown Device ID ({0})", parameters.id) });

            var coapRequest = HttpTranslator.GetCoapRequest(new CoapDotNetHttpRequest(Request, device.Url + "/" + (string)parameters.endpoint), Request.Url.SiteBase, true);

            throw new NotImplementedException();
        }

        private dynamic ProxyPut(dynamic parameters)
        {
            throw new NotImplementedException();
        }

        private dynamic ProxyPost(dynamic parameters)
        {
            throw new NotImplementedException();
        }

        private dynamic ProxyDelete(dynamic parameters)
        {
            throw new NotImplementedException();
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
                    foreach (var requestHeader in _request.Headers.SelectMany(a => a.Value.Select(v => new { Key = a.Key, Value = v })))
                        nvc.Add(requestHeader.Key, requestHeader.Value);
                    return nvc;
                }
            }

            public System.IO.Stream InputStream
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
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
