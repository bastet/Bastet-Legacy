using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bastet.Database.Model;
using Nancy;
using Nancy.Cookies;
using Nancy.Security;
using ServiceStack.OrmLite;

namespace Bastet.HttpServer.Modules
{
    public class AuthenticationModule
        : NancyModule
    {
        public const string PATH = ApiMain.PATH + "/authentication";

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
                string userName;
                string password;

                // First, accept auth value from HTTP basic auth
                if (Request.Headers.Authorization.Any())
                {
                    // https://en.wikipedia.org/wiki/Basic_access_authentication

                    var headerValue = Request.Headers.Authorization.Split(' ');
                    if (!headerValue[0].Equals("basic", StringComparison.InvariantCultureIgnoreCase))
                        throw new NotSupportedException("Authorization type must be 'basic'");
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(headerValue[1]));

                    var splitIndex = decoded.IndexOf(':');
                    userName = decoded.Substring(0, splitIndex);
                    password = decoded.Substring(splitIndex + 1, decoded.Length - decoded.IndexOf(':') - 1);
                }
                else
                {
                    //If no basic auth data was supplied, pull data from query string or form
                    userName = (string)Request.Query.UserName ?? (string)Request.Form.UserName;
                    password = (string)Request.Query.Password ?? (string)Request.Form.Password;
                }

                using (var transaction = _connection.OpenTransaction())
                {
                    //Find the user with the given name (and correct password)
                    var userIdentity = ValidateUser(userName, password);
                    if (userIdentity == null)
                    {
                        return Negotiate
                            .WithModel(new {Error = "Incorrect Username Or Password"})
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

                    return Negotiate
                        .WithCookie(CreateCookie(session.SessionKey))
                        .WithModel(new
                        {
                            SessionKey = session.SessionKey,
                        });
                }
            }, ct);
        }

        private Task<dynamic> GetAuth(dynamic parameters, CancellationToken ct)
        {
            return Task<dynamic>.Factory.StartNew(() =>
            {
                this.RequiresAuthentication();

                return Negotiate
                    .WithCookie(CreateCookie(((Identity) Context.CurrentUser).Session.SessionKey))
                    .WithModel(new
                    {
                        User = ModuleHelpers.CreateUrl(Request, UsersModule.PATH, Uri.EscapeUriString(Context.CurrentUser.UserName)),
                        Claims = Context.CurrentUser.Claims.ToArray()
                    });
            }, ct);
        }

        private Task<dynamic> DeleteAuth(dynamic parameters, CancellationToken ct)
        {
            return Task<dynamic>.Factory.StartNew(() =>
            {
                this.RequiresAuthentication();

                ModuleHelpers.Delete<Session>(_connection, ((Identity)Context.CurrentUser).Session.Id);

                return HttpStatusCode.NoContent;
            }, ct);
        }

        private NancyCookie CreateCookie(string key)
        {
            //todo: set secure: true when HTTPs is working
            return new NancyCookie("Bastet_Session_Key", key);//, false, true)
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
