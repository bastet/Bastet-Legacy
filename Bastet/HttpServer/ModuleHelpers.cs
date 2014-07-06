using System;
using System.Data;
using System.IO;
using Nancy;
using ServiceStack.OrmLite;

namespace Bastet.HttpServer
{
    internal static class ModuleHelpers
    {
        public static T Delete<T>(IDbConnection connection, long id)
            where T : class
        {
            var dbItem = connection.SingleById<T>(id);

            if (dbItem == null)
                return null;

            using (var transaction = connection.BeginTransaction())
            {
                connection.Delete(dbItem);
                transaction.Commit();
            }

            return dbItem;
        }

        public static string CreateUrl(Request request, params string[] parts)
        {
            var url = new Url(new Uri(new Uri(request.Url.SiteBase), Path.Combine(parts)).ToString());

            if (request.Query.sessionkey.HasValue)
                url.Query = "sessionkey=" + (string)request.Query.sessionkey;

            return url.ToString();
        }
    }
}
