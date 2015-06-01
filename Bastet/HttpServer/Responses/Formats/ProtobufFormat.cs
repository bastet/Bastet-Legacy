using Nancy;
using ProtoBuf;
using System.Collections.Generic;

namespace Bastet.HttpServer.Responses.Formats
{
    public class ProtobufFormat
        : IFormat
    {
        public string Name
        {
            get
            {
                return "protobuf";
            }
        }

        public Response Respond(object model, IEnumerable<ISerializer> serializers)
        {
            return new ProtobufResponce(model);
        }
    }

    public class ProtobufResponce : Response
    {
        /// <summary>
        /// Initializes a new instance ofthe <see cref="ProtobufResponce"/> class.
        /// </summary>
        /// <param name="body">Model instance to be serialized as the body.</param>
        public ProtobufResponce(object body)
        {
            Contents = stream => Serializer.Serialize(stream, body);
            ContentType = "application/protobuf";
            StatusCode = HttpStatusCode.OK;
        }

        /// <summary>
        /// Sets the HttpStatusCode property of the current <see cref="ProtobufResponce"/> instance
        /// to the specified value.
        /// </summary>
        /// <param name="httpStatusCode">The new http status code</param>
        /// <returns>The modified <see cref="ProtobufResponce"/> instance</returns>
        public ProtobufResponce WithStatusCode(HttpStatusCode httpStatusCode)
        {
            StatusCode = httpStatusCode;
            return this;
        }
    }
}
