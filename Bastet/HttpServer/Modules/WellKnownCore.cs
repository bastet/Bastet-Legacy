using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bastet.Database.Model;
using CoAP;
using Nancy;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Nancy.LightningCache;
using Nancy.Security;
using RestSharp;
using ServiceStack.OrmLite;
using HttpStatusCode = System.Net.HttpStatusCode;
using Method = RestSharp.Method;
using Response = Nancy.Response;

namespace Bastet.HttpServer.Modules
{
    public class WellKnownCore
        : NancyModule
    {
        public const string PATH = ".well-known/core";

        private readonly IDbConnection _connection;

        public WellKnownCore(IDbConnection connection)
            :base(PATH)
        {
            _connection = connection;

            Get["/", runAsync: true] = GetCore;
        }

        private Task<dynamic> GetCore(dynamic parameters, CancellationToken ct)
        {
            return Task<dynamic>.Factory.StartNew(() => {

                this.RequiresAuthentication();
                //this.RequiresHttps();

                var user = Context.CurrentUser as Identity;
                if (user == null)
                    return Negotiate.WithStatusCode(Nancy.HttpStatusCode.Unauthorized);

                List<string> responses = new List<string>();

                var client = new RestClient(Request.Url.SiteBase);
                foreach (var device in _connection.Select<Device>())
                {
                    var request = new RestRequest(DevicesProxyModule.PATH + "/.well-known/core", Method.GET);
                    request.AddUrlSegment("id", device.Id.ToString(CultureInfo.InvariantCulture));
                    request.AddParameter("SessionKey", user.Session.SessionKey);

                    var resp = client.Execute(request);

                    if (resp.StatusCode == HttpStatusCode.OK)
                        responses.Add(resp.Content);
                }

                var r = (Response)string.Join(",", responses);
                r.ContentType = "application/link-format";
                return r;
            }, ct);
        }
    }
}
