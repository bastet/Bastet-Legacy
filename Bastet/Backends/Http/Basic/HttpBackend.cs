using System;
using System.Threading.Tasks;
using Bastet.Database.Model;
using ServiceStack.OrmLite;

namespace Bastet.Backends.Http.Basic
{
    public class HttpBackend
        : BaseHttpBackend
    {
        public override Task<BackendDescription> Describe(Device device)
        {
            throw new NotImplementedException();
        }

        public override void Setup(Database.Database database)
        {
            using (var db = database.ConnectionFactory.Open())
                db.CreateTableIfNotExists<BasicHttpModel>();
        }
    }
}
