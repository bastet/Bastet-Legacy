using System;
using System.Data;
using System.Globalization;
using System.Linq;
using Bastet.Database.Model;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
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

            //this.RequiresHttps();

            Get["/"] = ListDevices;
            Post["/"] = CreateDevice;

            Get["/{id}"] = DeviceDetails;
            Delete["/{id}"] = DeleteDevice;
        }

        private object SerializeDevice(Device device)
        {
            if (device == null)
                return null;

            return new
            {
                Id = device.Id,
                Url = device.Url,
                Proxy = ModuleHelpers.CreateUrl(Request, DevicesProxyModule.PATH.Replace("{id}", device.Id.ToString(CultureInfo.InvariantCulture))),
                Resources = ModuleHelpers.CreateUrl(Request, DevicesProxyModule.PATH.Replace("{id}", device.Id.ToString(CultureInfo.InvariantCulture)), ".well-known/core")
            };
        }

        /// <summary>
        /// Return a list of links to devices
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private dynamic ListDevices(dynamic parameters)
        {
            this.RequiresAuthentication();
            this.RequiresClaims(new[] { "list-devices" });

            return _connection
                .Select<Device>()
                .Select(d => ModuleHelpers.CreateUrl(Request, PATH, d.Id.ToString(CultureInfo.InvariantCulture)))
                .ToArray();
        }

        private dynamic CreateDevice(dynamic parameters)
        {
            this.RequiresAuthentication();
            this.RequiresClaims(new[] { "create-device" });

            var device = this.Bind<Device>("Id");
            _connection.Save(device);

            return SerializeDevice(device);
        }

        /// <summary>
        /// Return details about an individual device
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private dynamic DeviceDetails(dynamic parameters)
        {
            this.RequiresAuthentication();
            this.RequiresAnyClaim(new[] { "device-details-all", string.Format("device-details-{0}", (int)parameters.id) });

            return SerializeDevice(_connection.SingleById<Device>((int)parameters.id)) ?? HttpStatusCode.NotFound;
        }

        private dynamic DeleteDevice(dynamic parameters)
        {
            this.RequiresAuthentication();
            this.RequiresAnyClaim(new[] { "device-delete-all", string.Format("device-delete-{0}", (int)parameters.id) });

            return SerializeDevice(ModuleHelpers.Delete<Device>(_connection, (int)parameters.id)) ?? HttpStatusCode.NoContent;
        }
    }
}
