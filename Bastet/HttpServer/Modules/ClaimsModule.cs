using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bastet.Database.Model;
using Nancy;
using Nancy.Security;
using ServiceStack.OrmLite;

namespace Bastet.HttpServer.Modules
{
    public class ClaimsModule
        : NancyModule
    {
        public const string PATH = "/users/{username}/claims";

        private readonly IDbConnection _connection;

        public ClaimsModule(IDbConnection connection)
            :base(PATH)
        {
            _connection = connection;

            //this.RequiresHttps();

            Get["/", runAsync: true] = ListClaims;
            Post["/", runAsync: true] = CreateClaim;
            Delete["/", runAsync: true] = DeleteClaim;
        }

        private static dynamic SerializeClaim(Claim claim)
        {
            return claim.Name;
        }

        private Task<dynamic> ListClaims(dynamic parameters, CancellationToken ct)
        {
            return Task<dynamic>.Factory.StartNew(() =>
            {
                this.RequiresAuthentication();

                //Anyone can list their own claims
                if (Context.CurrentUser.UserName != (string)parameters.username)
                    this.RequiresClaims(new[] { "list-claims" });

                return Identity.GetClaims(((Identity)Context.CurrentUser).User, _connection).Select(SerializeClaim).ToArray();
            });
        }

        private Task<dynamic> CreateClaim(dynamic parameters, CancellationToken ct)
        {
            return Task<dynamic>.Factory.StartNew(() =>
            {
                this.RequiresAuthentication();
                this.RequiresClaims(new[] { "create-claim" });

                using (var transaction = _connection.OpenTransaction())
                {
                    //Get the user we're giving a claim to
                    var username = (string) parameters.username;
                    var user = _connection.SingleWhere<User>("Username", username);
                    if (user == null)
                    {
                        return Negotiate
                            .WithModel(new {Error = "No Such User Exists"})
                            .WithStatusCode(HttpStatusCode.NotFound);
                    }

                    //Create the claim
                    using (var reader = new StreamReader(Request.Body))
                    {
                        var claim = new Claim(user, reader.ReadToEnd());
                        _connection.Save(claim);
                    }

                    transaction.Commit();
                }

                return Identity.GetClaims(((Identity)Context.CurrentUser).User, _connection).Select(SerializeClaim).ToArray();
            });
        }

        private Task<dynamic> DeleteClaim(dynamic parameters, CancellationToken ct)
        {
            return Task<dynamic>.Factory.StartNew(() =>
            {
                this.RequiresAuthentication();
                this.RequiresClaims(new[] { "delete-claim" });

                using (var transaction = _connection.OpenTransaction())
                {
                    //Get the user we're taking a claim from
                    var username = (string)parameters.username;
                    var user = _connection.SingleWhere<User>("Username", username);
                    if (user == null)
                    {
                        return Negotiate
                            .WithModel(new {Error = "No Such User Exists"})
                            .WithStatusCode(HttpStatusCode.NotFound);
                    }

                    //Get the claim
                    using (var reader = new StreamReader(Request.Body))
                    {
                        var claimName = reader.ReadToEnd();
                        var claim = _connection.Where<Claim>(new {Name = claimName, UserId = user.Id}).SingleOrDefault();
                        _connection.Delete(claim);
                    }

                    transaction.Commit();
                }

                return Identity.GetClaims(((Identity)Context.CurrentUser).User, _connection).Select(SerializeClaim).ToArray();
            });
        }
    }
}
