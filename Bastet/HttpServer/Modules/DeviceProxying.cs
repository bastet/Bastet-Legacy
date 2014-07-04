using System;
using System.Data;
using Bastet.Database.Model;
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
    }
}
