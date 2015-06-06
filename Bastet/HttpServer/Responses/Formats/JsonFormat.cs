using System.Collections.Generic;
using Nancy;
using Nancy.Responses;

namespace Bastet.HttpServer.Responses.Formats
{
    public class JsonFormat
        : IFormat
    {
        public string Name
        {
            get
            {
                return "json";
            }
        }

        public Response Respond(object model, IEnumerable<ISerializer> serializers)
        {
            return new JsonResponse(model, this.FindSerializer(serializers));
        }
    }
}