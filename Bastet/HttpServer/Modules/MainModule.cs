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
            var root = Request.Url;

            return new
            {
                devices = root + DevicesModule.PATH,
                sensors = root + SensorsModule.PATH
            };
        }
    }
}
