using Nancy.Security;

namespace Bastet.HttpServer.Responses
{
    public class Session
    {
        public string SessionKey { get; set; }

        public IUserIdentity User { get; set; }

        public Session(Identity user, string sessionKey)
        {
            User = user;
            SessionKey = sessionKey;
        }
    }
}
