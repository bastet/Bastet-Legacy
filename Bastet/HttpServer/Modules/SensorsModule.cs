using System.Data;
using System.Globalization;
using System.Linq;
using Bastet.Database.Model;
using Nancy;
using Nancy.ModelBinding;
using ServiceStack.OrmLite;

namespace Bastet.HttpServer.Modules
{
    public class SensorsModule
        : NancyModule
    {
        private readonly IDbConnection _connection;

        public const string PATH = DevicesModule.PATH + "/{deviceid}/sensors";

        public SensorsModule(IDbConnection connection)
            : base(PATH)
        {
            _connection = connection;

            //this.RequiresHttps();

            Get["/"] = ListSensors;
            Post["/"] = CreateSensor;

            Delete["/{id}"] = DeleteSensor;
        }

        private object SerializeSensor(Sensor sensor)
        {
            if (sensor == null)
                return null;

            return new
            {
                Id = sensor.Id,
                Device = Request.Url.SiteBase + DevicesModule.PATH + "/" + sensor.DeviceId,
                Readings = (Request.Url.SiteBase + ReadingsModule.PATH)
                    .Replace("{sensorid}", sensor.Id.ToString(CultureInfo.InvariantCulture))
                    .Replace("{deviceid}", sensor.DeviceId.ToString(CultureInfo.InvariantCulture))
            };
        }

        private dynamic ListSensors(dynamic parameters)
        {
            long deviceId = (long)parameters.deviceid;

            return _connection
                .Select<Sensor>(a => a.DeviceId == deviceId)
                .Select(SerializeSensor)
                .ToArray();
        }

        private dynamic CreateSensor(dynamic parameters)
        {
            var sensor = this.Bind<Sensor>("Id");

            using (var transaction = _connection.OpenTransaction())
            {
                //Ensure this device actually exists
                if (_connection.SingleById<Device>(sensor.DeviceId) == null)
                {
                    return Negotiate
                        .WithModel(new { Error = string.Format("Cannot Find Device with Id {0}", sensor.DeviceId) })
                        .WithStatusCode(HttpStatusCode.BadRequest);
                }

                _connection.Save(sensor);
                transaction.Commit();
            }

            return SerializeSensor(sensor);
        }

        private dynamic DeleteSensor(dynamic parameters)
        {
            return SerializeSensor(ModuleHelpers.Delete<Sensor>(_connection, (int)parameters.id)) ?? HttpStatusCode.NoContent;
        }
    }
}
