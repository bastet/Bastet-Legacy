using System;
using System.Data;
using System.Globalization;
using System.Linq;
using Bastet.Database.Model;
using Nancy;
using Nancy.ModelBinding;
using ServiceStack.OrmLite;

namespace Bastet.HttpServer.Modules
{
    public class ReadingsModule
        : NancyModule
    {
        public const string PATH = SensorsModule.PATH + "/{sensorid}/readings";

        private readonly IDbConnection _connection;

        public ReadingsModule(IDbConnection connection)
            :base(PATH)
        {
            _connection = connection;

            Get["/"] = ListAllReadings;

            Get["/strings"] = ListReadings<StringReading, string>;
            Post["/strings"] = PostReading<StringReading, string>;

            Get["/blobs"] = ListReadings<BlobReading, byte[]>;
            Post["/blobs"] = PostReading<BlobReading, byte[]>;

            Get["/decimals"] = ListReadings<DecimalReading, decimal>;
            Post["/decimals"] = PostReading<DecimalReading, decimal>;
        }

        private object SerializeReading<V>(IReading<V> reading)
        {
            if (reading == null)
                return null;

            return new
            {
                Id = reading.Id,
                Timestamp = reading.Timestamp,
                Sensor = ModuleHelpers.CreateUrl(Request, SensorsModule.PATH, reading.SensorId.ToString(CultureInfo.InvariantCulture)),
                Value = reading.Value
            };
        }

        private dynamic ListAllReadings(dynamic parameters)
        {
            return new
            {
                strings = Request.Url + "/strings",
                blob = Request.Url + "/blobs",
                decimals = Request.Url + "/decimals"
            };
        }

        private dynamic ListReadings<R, V>(dynamic parameters) where R : IReading<V>
        {
            var sensorId = (int)parameters.sensorid;

            DateTime before = DateTime.MaxValue;
            DateTime after = DateTime.MinValue;
            if (Request.Query.ContainsKey("before"))
                before = (DateTime)Request.Query.before;
            if (Request.Query.ContainsKey("after"))
                after = (DateTime)Request.Query.after;

            return _connection
                .Select<R>(r => r.SensorId == sensorId && r.Timestamp < before && r.Timestamp > after)
                .Cast<IReading<V>>()
                .Select(SerializeReading<V>);
        }

        private dynamic PostReading<R, V>(dynamic parameters) where R : IReading<V>
        {
            var sensorId = (int)parameters.sensorid;

            //Ensure this sensor actually exists
            if (_connection.SingleById<Sensor>(sensorId) == null)
            {
                return Negotiate
                    .WithModel(new {Error = string.Format("Cannot Find Sensor with Id {0}", sensorId)})
                    .WithStatusCode(HttpStatusCode.BadRequest);
            }

            //Copy reading data from body and URL
            R reading = this.Bind<R>("Id", "SensorId");
            reading.SensorId = sensorId;

            //Save reading in database
            using (var transaction = _connection.OpenTransaction())
            {
                //Ensure this device actually exists
                if (_connection.SingleById<Sensor>(reading.SensorId) == null)
                {
                    return Negotiate
                        .WithModel(new { Error = string.Format("Cannot Find Sensor with Id {0}", reading.SensorId) })
                        .WithStatusCode(HttpStatusCode.BadRequest);
                }

                _connection.Save(reading);
                transaction.Commit();
            }

            return SerializeReading<V>(reading);
        }
    }
}
