using Bastet.Database.Model;
using ServiceStack.DataAnnotations;

namespace Bastet.Backends.Http.Basic
{
    public class BasicHttpModel
    {
        /// <summary>
        /// The ID of the user this session is for
        /// </summary>
        [PrimaryKey]
        public long DeviceId { get; set; }

        /// <summary>
        /// The Base Url of this http API
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Path (in the API) to get a swagger document from
        /// </summary>
        public string SwaggerPath { get; set; }

        public BasicHttpModel(Device device, string url, string swaggerPath)
        {
            DeviceId = device.Id;
            Url = url;
            SwaggerPath = swaggerPath;
        }
    }
}
