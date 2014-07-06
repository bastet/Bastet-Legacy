using Nancy;

namespace Bastet.HttpServer.Modules
{
    public class MainModule
        : NancyModule
    {
        public MainModule()
        {
            Get["/"] = ListRoutes;
        }

        private dynamic ListRoutes(dynamic parameters)
        {
            return new
            {
                devices = Request.Url.SiteBase + DevicesModule.PATH,
                sensors = Request.Url.SiteBase + SensorsModule.PATH
            };
        }
    }
}
