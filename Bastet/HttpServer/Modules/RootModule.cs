using Nancy;

namespace Bastet.HttpServer.Modules
{
    public class RootModule
        : NancyModule
    {
        public RootModule()
        {
            Get["/"] = _ => View["Root"];
        }
    }
}
