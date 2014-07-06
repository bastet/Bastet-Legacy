using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bastet.Database.Model;
using Nancy;
using Nancy.Security;
using ServiceStack.OrmLite;

namespace Bastet.HttpServer.Modules
{
    public class AuthenticationModule
        : NancyModule
    {
        public const string PATH = "/authentication";

        private readonly IDbConnection _connection;

        public AuthenticationModule(IDbConnection connection)
            :base(PATH)
        {
            _connection = connection;

            //this.RequiresHttps();

            Get["/", runAsync: true] = GetAuth;
            Post["/", runAsync: true] = PostAuth;
            Delete["/", runAsync: true] = DeleteAuth;
        }

        private Task<dynamic> PostAuth(dynamic parameters, CancellationToken ct)
        {
            return Task<dynamic>.Factory.StartNew(() =>
            {
                var userName = (string)(Request.Query.UserName ?? Request.Form.UserName);
                var password = (string)(Request.Query.Password ?? Request.Form.Password);

                using (var transaction = _connection.OpenTransaction())
                {
                    //Find the user with the given name (and correct password)
                    var userIdentity = ValidateUser(userName, password);
                    if (userIdentity == null)
                    {
                        return Negotiate
                            .WithModel(new { Error = "Incorrect Username Or Password" })
                            .WithStatusCode(HttpStatusCode.Unauthorized);
                    }

                    //Create or find a session for this user
                    var session = _connection.Select<Session>(s => s.UserId == userIdentity.User.Id).SingleOrDefault();
                    if (session == null)
                    {
                        session = new Session(userIdentity.User);
                        _connection.Save(session);
                    }

                    //Store session in user identity
                    userIdentity.Session = session;

                    //Save any changes made
                    transaction.Commit();

                    //Return the session key
                    return new
                    {
                        SessionKey = session.SessionKey,
                    };
                }
            });
        }

        private Task<dynamic> GetAuth(dynamic parameters, CancellationToken ct)
        {
            return Task<dynamic>.Factory.StartNew(() =>
            {
                this.RequiresAuthentication();

                var u = Context.CurrentUser;
                return new
                {
                    User = ModuleHelpers.CreateUrl(Request, UsersModule.PATH, Uri.EscapeUriString(u.UserName)),
                    Claims = u.Claims.ToArray()
                };
            });
        }

        private Task<dynamic> DeleteAuth(dynamic parameters, CancellationToken ct)
        {
            return Task<dynamic>.Factory.StartNew(() =>
            {
                this.RequiresAuthentication();

                ModuleHelpers.Delete<Session>(_connection, ((Identity)Context.CurrentUser).Session.Id);

                return HttpStatusCode.NoContent;
            });
        }

        private Identity ValidateUser(string userName, string password)
        {
            if (userName == null || password == null)
                return null;

            var user = _connection.Select<User>(a => a.Username == userName).SingleOrDefault();
            if (user == null)
                return null;

            if (user.ComputeSaltedHash(password).SlowEquals(user.PasswordHash))
                return new Identity(Identity.GetClaims(user, _connection), user, null);
            return null;
        }
    }
}
