using Nancy;

namespace Bastet.HttpServer
{
    public class NancyRootPath
        : IRootPathProvider
    {
        public string GetRootPath()
        {
            return "HttpServer";
        }
    }
}
