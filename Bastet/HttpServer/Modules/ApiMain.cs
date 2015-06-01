using Nancy;

namespace Bastet.HttpServer.Modules
{
    public class ApiMain
        : NancyModule
    {
        public const string PATH = "/api";

        public ApiMain()
        {
            Get[PATH] = ListRoutes;
        }

        private dynamic ListRoutes(dynamic parameters)
        {
            return new
            {
                Devices = ModuleHelpers.CreateUrl(Request, DevicesModule.PATH),
                Authentication = ModuleHelpers.CreateUrl(Request, AuthenticationModule.PATH),
                Users = ModuleHelpers.CreateUrl(Request, UsersModule.PATH)
            };
        }
    }
}
