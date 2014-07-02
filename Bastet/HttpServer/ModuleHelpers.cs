using System;
using Nancy;
using Nancy.Responses.Negotiation;

namespace Bastet.HttpServer
{
    internal static class ModuleHelpers
    {
        public static dynamic Delete<T>(Database.Database db, Negotiator negotiate, string idString)
            where T : class
        {
            using (var session = db.SessionFactory.OpenSession())
            {
                Guid id;
                if (!Guid.TryParse(idString, out id))
                {
                    return negotiate
                        .WithModel(new { Error = "Cannot parse GUID" })
                        .WithStatusCode(HttpStatusCode.BadRequest);
                }

                var dbItem = session.Get<T>(id);

                if (dbItem == null)
                    return HttpStatusCode.NoContent;

                using (var transaction = session.BeginTransaction())
                {
                    session.Delete(dbItem);
                    transaction.Commit();
                }

                return dbItem;
            }
        }
    }
}
