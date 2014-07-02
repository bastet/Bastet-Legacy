using System;
using System.Linq;
using Bastet.Database.Model;
using NHibernate;
using Nancy;
using Nancy.ModelBinding;

namespace Bastet.HttpServer.Modules
{
    public class SensorsModule
        : NancyModule
    {
        private readonly ISession _session;

        public const string PATH = "/sensors";

        public SensorsModule(ISession session)
            : base(PATH)
        {
            _session = session;

            Get["/"] = ListSensors;
            Post["/"] = CreateSensor;

            Get["/{id}"] = SensorDetails;
            Delete["/{id}"] = DeleteSensor;
        }

        private dynamic ListSensors(dynamic parameters)
        {
                var url = Request.Url;

                return _session
                    .QueryOver<Sensor>()
                    .List()
                    .Select(a => url + "/" + a.Id);
        }

        private dynamic CreateSensor(dynamic parameters)
        {
            var sensor = this.Bind<Sensor>("Id");

            var id = sensor.Device.Id;
            sensor.Device = _session.Get<Device>(id);

            if (sensor.Device == null)
            {
                return Negotiate
                    .WithModel(new { Error = string.Format("Cannot Find Device with Id {0}", id) })
                    .WithStatusCode(HttpStatusCode.BadRequest);
            }

            _session.Save(sensor);

            return sensor;
        }

        private dynamic SensorDetails(dynamic parameters)
        {
            Guid id;
            if (!Guid.TryParse((string)parameters.id, out id))
            {
                return Negotiate
                    .WithModel(new { Error = "Cannot parse GUID" })
                    .WithStatusCode(HttpStatusCode.BadRequest);
            }

            var sensor = _session.Load<Sensor>(id);
            if (sensor == null)
                return HttpStatusCode.NotFound;
            else
            {
                NHibernateUtil.Initialize(sensor);
                return sensor;
            }
        }

        private dynamic DeleteSensor(dynamic parameters)
        {
            throw new NotImplementedException();
            //return ModuleHelpers.Delete<Sensor>(_session, Negotiate, (string)parameters.id);
        }
    }
}
