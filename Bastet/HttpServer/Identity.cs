using System.Collections.Generic;
using System.Data;
using System.Linq;
using Bastet.Database.Model;
using Nancy.Security;
using ServiceStack.OrmLite;

namespace Bastet.HttpServer
{
    public class Identity
        : IUserIdentity
    {
        public string UserName
        {
            get { return User.Username; }
        }

        public IEnumerable<string> Claims
        {
            get;
            private set;
        }

        public User User { get; private set; }
        public Session Session { get; set; }

        public Identity(IEnumerable<Claim> claims, User user, Session session)
        {
            User = user;
            Session = session;

            Claims = claims.Select(c => c.Name).ToArray();
        }

        public static IEnumerable<Claim> GetClaims(User user, IDbConnection connection)
        {
            return connection.Select<Claim>(c => c.UserId == user.Id);
        }
    }
}
