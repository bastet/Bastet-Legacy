using Bastet.Backends;
using Bastet.Database.Model;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using ServiceStack.OrmLite;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Bastet.HttpServer.Modules
{
    public class DevicesModule
        : NancyModule
    {
        public const string PATH = ApiMain.PATH + "/devices";

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
                Backend = device.Backend,
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
            this.RequiresAnyClaim(new[] { "superuser", "list-devices" });

            return _connection
                .Select<Device>()
                .Select(SerializeDevice)
                .ToArray();
        }

        private dynamic CreateDevice(dynamic parameters)
        {
            this.RequiresAuthentication();
            this.RequiresAnyClaim(new[] { "superuser", "create-device" });

            var device = this.Bind<Device>("Id");

            var backendType = BackendFactory.BackendType(device.Backend);
            device.Backend = backendType == null ? null : backendType.AssemblyQualifiedName;
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
            this.RequiresAnyClaim(new[] { "superuser", "device-details-all", string.Format("device-details-{0}", (int)parameters.id) });

            return SerializeDevice(_connection.SingleById<Device>((int)parameters.id)) ?? HttpStatusCode.NotFound;
        }

        private dynamic DeleteDevice(dynamic parameters)
        {
            this.RequiresAuthentication();
            this.RequiresAnyClaim(new[] { "superuser", "device-delete-all", string.Format("device-delete-{0}", (int)parameters.id) });

            return SerializeDevice(ModuleHelpers.Delete<Device>(_connection, (int)parameters.id)) ?? HttpStatusCode.NoContent;
        }
    }
}
