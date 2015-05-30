using Nancy;
using System.Data;

namespace Bastet.HttpServer.Modules
{
    public class RootModule
        : NancyModule
    {
        private readonly IDbConnection _connection;

        public RootModule(IDbConnection connection)
        {
            _connection = connection;

            Get["/"] = _ => Response.AsRedirect("index.html");
        }
    }
}
