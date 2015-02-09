using System.Data;
using Nancy;

namespace Bastet.HttpServer.Modules
{
    public class RootModule
        : NancyModule
    {
        private readonly IDbConnection _connection;

        public RootModule(IDbConnection connection)
        {
            _connection = connection;

            Get["/"] = GetRoot;
        }

        private dynamic GetRoot(dynamic parameters)
        {
            var setup = GetSetupProgress();
            if (setup.HasValue)
                return View["Setup", setup];
            else
                return View["Root"];
        }

        private SetupProgress? GetSetupProgress()
        {
            return null;
        }

        public struct SetupProgress
        {

        }
    }
}
