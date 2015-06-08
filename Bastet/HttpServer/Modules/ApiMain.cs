using Bastet.HttpServer.Responses;
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

        private static dynamic ListRoutes(dynamic parameters)
        {
            return new MainRouteList
            {
                Devices = DevicesModule.PATH,
                Authentication = AuthenticationModule.PATH,
                Users = UsersModule.PATH
            };
        }
    }
}
