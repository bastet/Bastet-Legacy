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
                Devices = Request.Url.SiteBase + DevicesModule.PATH,
                Authentication = Request.Url.SiteBase + AuthenticationModule.PATH
            };
        }
    }
}
