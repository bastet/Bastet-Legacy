using System;
using System.Data;
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
        public const string PATH = "sensors/{sensorid}/readings";

        private readonly IDbConnection _connection;

        public ReadingsModule(IDbConnection connection)
            :base(PATH)
        {
            _connection = connection;

            Get["/"] = ListReadings;
        }

        private object SerializeReading(Reading reading)
        {
            if (reading == null)
                return null;

            return new
            {
                Id = reading.Id,
                Url = reading.Timestamp,
            };
        }

        /// <summary>
        /// Return a list of links to devices
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private dynamic ListReadings(dynamic parameters)
        {
            var sensorId = (int)parameters.sensorid;

            DateTime before = DateTime.MaxValue;
            DateTime after = DateTime.MinValue;
            if (Request.Query.ContainsKey("before"))
                before = (DateTime)Request.Query.before;
            if (Request.Query.ContainsKey("after"))
                after = (DateTime)Request.Query.after;

            return _connection
                .Select<Reading>(r => r.SensorId == sensorId && r.Timestamp < before && r.Timestamp > after)
                .Select(SerializeReading);
        }
    }
}
