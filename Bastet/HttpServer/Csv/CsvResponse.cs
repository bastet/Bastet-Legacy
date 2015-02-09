using Nancy;
using System;
using System.IO;

namespace Bastet.HttpServer.Csv
{
    public class CsvResponse<T>
        : Response
    {
        public CsvResponse(T model, ISerializer serializer)
        {
            if (serializer == null)
            {
                throw new InvalidOperationException("CSV Serializer not set");
            }

            Contents = Serialize(model, serializer);
            ContentType = "text/csv";
            StatusCode = HttpStatusCode.OK;
        }

        private static Action<Stream> Serialize(T model, ISerializer serializer)
        {
            return stream => serializer.Serialize("text/csv", model, stream);
        }
    }

    public class CsvResponse : CsvResponse<object>
    {
        public CsvResponse(object model, ISerializer serializer)
            : base(model, serializer)
        {
        }
    }
}
