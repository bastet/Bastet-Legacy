using Nancy.Security;
using System.Linq;

namespace Bastet.HttpServer.Responses
{
    public class User
    {
        public string[] Claims { get; set; }

        public string Username { get; set; }

        public User(IUserIdentity identity)
        {
            Claims = identity.Claims.ToArray();
            Username = identity.UserName;
        }
    }
}
