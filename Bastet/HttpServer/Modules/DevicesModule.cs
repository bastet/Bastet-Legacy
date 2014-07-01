using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bastet.Database.Model;
using Nancy;

namespace Bastet.HttpServer.Modules
{
    public class DevicesModule
        : NancyModule
    {
        public const string PATH = "/devices";

        private readonly Database.Database _db;

        public DevicesModule(Database.Database db)
            :base(PATH)
        {
            _db = db;

            Get["/"] = ListDevices;

            Get["/{id}"] = DeviceDetails;
        }

        /// <summary>
        /// Return a list of links to devices
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private dynamic ListDevices(dynamic parameters)
        {
            using (var session = _db.SessionFactory.OpenSession())
            {
                var url = Request.Url;
                return session
                    .QueryOver<Device>()
                    .List()
                    .Select(a => url + "/" + a.Id);
            }
        }

        /// <summary>
        /// Return details about an individual device
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private dynamic DeviceDetails(dynamic parameters)
        {
            using (var session = _db.SessionFactory.OpenSession())
            {
                Guid id;
                if (!Guid.TryParse((string) parameters.id, out id))
                {
                    return Negotiate
                        .WithModel(new { Error = "Cannot parse GUID" })
                        .WithStatusCode(HttpStatusCode.BadRequest);
                }

                return (dynamic)session.QueryOver<Device>().Where(a => a.Id == id).SingleOrDefault() ?? HttpStatusCode.NotFound;
            }
        }
    }
}
