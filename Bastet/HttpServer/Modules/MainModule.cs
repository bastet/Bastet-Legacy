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
            var url = Request.Url;
            //TODO:: Once https is working comment this in!
            //url.Scheme = "https";

            return new
            {
                Devices = Request.Url.SiteBase + DevicesModule.PATH,
                Authentication = url.SiteBase + AuthenticationModule.PATH
            };
        }
    }
}
