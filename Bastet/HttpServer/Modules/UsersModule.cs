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
    public class UsersModule
        : NancyModule
    {
        public const string PATH = "/users";

        private readonly IDbConnection _connection;

        public UsersModule(IDbConnection connection)
            :base(PATH)
        {
            _connection = connection;

            //this.RequiresHttps();

            Get["/", runAsync: true] = ListUsers;
            Post["/", runAsync: true] = CreateUser;

            Get["/{username}", runAsync: true] = GetUserDetails;
        }

        private object SerializeUser(User user)
        {
            return new
            {
                Username = user.Username,
                Claims = ModuleHelpers.CreateUrl(Request, ClaimsModule.PATH.Replace("{username}", Uri.EscapeUriString(user.Username)))
            };
        }

        private Task<dynamic> ListUsers(dynamic parameters, CancellationToken ct)
        {
            return Task<dynamic>.Factory.StartNew(() =>
            {
                this.RequiresAuthentication();
                this.RequiresClaims(new[] {"list-users"});

                return _connection
                    .Select<User>()
                    .Select(d => Request.Url.SiteBase + PATH + "/" + Uri.EscapeUriString(d.Username))
                    .ToArray();
            });
        }

        private Task<dynamic> CreateUser(dynamic parameters, CancellationToken ct)
        {
            return Task<dynamic>.Factory.StartNew(() =>
            {
                var userName = (string)(Request.Query.UserName ?? Request.Form.UserName);
                var password = (string)(Request.Query.Password ?? Request.Form.Password);

                using (var transaction = _connection.OpenTransaction())
                {
                    //Find the user with the given name (and correct password)
                    var user = _connection.Select<User>(a => a.Username == userName).SingleOrDefault();
                    if (user != null)
                    {
                        return Negotiate
                            .WithModel(new { Error = "User With This Username Already Exists" })
                            .WithStatusCode(HttpStatusCode.Conflict);
                    }

                    //Create a new user
                    user = new User(userName, password);
                    _connection.Save(user);

                    //Save any changes made
                    transaction.Commit();

                    //Return the user
                    return SerializeUser(user);
                }
            });
        }

        private Task<dynamic> GetUserDetails(dynamic parameters, CancellationToken ct)
        {
            return Task<dynamic>.Factory.StartNew(() =>
            {
                var username = (string)parameters.username;
                var user = _connection.SingleWhere<User>("Username", username);
                if (user == null)
                {
                    return Negotiate
                        .WithModel(new { Error = "No Such User Exists" })
                        .WithStatusCode(HttpStatusCode.NotFound);
                }

                return SerializeUser(user);
            });
        }
    }
}
