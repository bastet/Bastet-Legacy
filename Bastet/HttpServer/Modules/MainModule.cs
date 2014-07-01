using Nancy;
using Nancy.Routing;

namespace Bastet.HttpServer.Modules
{
    public class MainModule
        : NancyModule
    {
        public MainModule(IRouteCacheProvider routeCacheProvider)
        {
            Get["/"] = ListRoutes;
        }

        private dynamic ListRoutes(dynamic parameters)
        {
            var root = Request.Url;

            return new
            {
                devices = root + DevicesModule.PATH,
            };
        }
    }
}
