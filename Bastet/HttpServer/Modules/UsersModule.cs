using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
        }

        private object SerializeUser(User user)
        {
            return new
            {
                Nick = user.Nick,
                Username = user.Username
            };
        }

        private Task<dynamic> ListUsers(dynamic parameters, CancellationToken ct)
        {
            return Task<dynamic>.Factory.StartNew(() =>
            {
                this.RequiresAuthentication();
                this.RequiresClaims(new[] {"list-users"});

                throw new NotImplementedException();
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
    }
}
