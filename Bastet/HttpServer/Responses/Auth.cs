
using Nancy.Security;

namespace Bastet.HttpServer.Responses
{
    public class Auth
    {
        public IUserIdentity User { get; set; }
    }
}
