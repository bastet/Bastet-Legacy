using Bastet.Database.Model;
using System.Collections.Generic;
using System.Linq;

namespace Bastet.HttpServer.Responses
{
    public class ClaimsList
    {
        public Claim[] Claims { get; set; }

        public ClaimsList(IEnumerable<Claim> claims)
        {
            Claims = claims.ToArray();
        }
    }
}
