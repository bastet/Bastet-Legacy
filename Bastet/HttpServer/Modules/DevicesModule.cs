using System.Data;
using System.Linq;
using Bastet.Database.Model;
using Nancy;
using Nancy.ModelBinding;
using ServiceStack.OrmLite;

namespace Bastet.HttpServer.Modules
{
    public class DevicesModule
        : NancyModule
    {
        public const string PATH = "/devices";

        private readonly IDbConnection _connection;

        public DevicesModule(IDbConnection connection)
            :base(PATH)
        {
            _connection = connection;

            Get["/"] = ListDevices;
            Post["/"] = CreateDevice;

            Get["/{id}"] = DeviceDetails;
            Delete["/{id}"] = DeleteDevice;
        }

        private static object SerializeDevice(Url baseUrl, Device device)
        {
            if (device == null)
                return null;

            return new
            {
                Id = device.Id,
                Url = device.Url,
                Resources = baseUrl + "/well-known/.core"
            };
        }

        /// <summary>
        /// Return a list of links to devices
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private dynamic ListDevices(dynamic parameters)
        {
            var url = Request.Url;
            return _connection
                .Select<Device>()
                .Select(d => url + "/" + d.Id)
                .ToArray();
        }

        private dynamic CreateDevice(dynamic parameters)
        {
            var device = this.Bind<Device>("Id");
            _connection.Save(device);

            return SerializeDevice(Request.Url, device);
        }

        /// <summary>
        /// Return details about an individual device
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private dynamic DeviceDetails(dynamic parameters)
        {
            return SerializeDevice(Request.Url, _connection.SingleById<Device>((int)parameters.id)) ?? HttpStatusCode.NotFound;
        }

        private dynamic DeleteDevice(dynamic parameters)
        {
            return SerializeDevice(Request.Url, ModuleHelpers.Delete<Device>(_connection, (int)parameters.id)) ?? HttpStatusCode.NoContent;
        }
    }
}
