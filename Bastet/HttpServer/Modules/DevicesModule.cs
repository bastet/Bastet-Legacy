﻿using System;
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

        private static object SerializeDevice(Url url, Device device)
        {
            if (device == null)
                return null;

            return new
            {
                Id = device.Id,
                Url = device.Url,
                Proxy = new Uri(new Uri(url.SiteBase), String.Format("devices/{0}/proxy", device.Id)),
                Resources = new Uri(new Uri(url.SiteBase), String.Format("devices/{0}/proxy/.well-known/core", device.Id))
            };
        }

        /// <summary>
        /// Return a list of links to devices
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private dynamic ListDevices(dynamic parameters)
        {
            return _connection
                .Select<Device>()
                .Select(d => Request.Url.SiteBase + PATH + "/" + d.Id)
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
