using Nancy;
using Nancy.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace Bastet.HttpServer.Csv
{
    public class CsvSerializer : ISerializer
    {
        public bool CanSerialize(string contentType)
        {
            return IsCsvType(contentType);
        }

        public IEnumerable<string> Extensions
        {
            get { yield return "csv"; }
        }

        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            using (var writer = new StreamWriter(new UnclosableStreamWrapper(outputStream)))
            {
                ServiceStack.Text.CsvSerializer.SerializeToWriter(model, writer);
            }
        }

        private static bool IsCsvType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return false;

            var contentMimeType = contentType.Split(';')[0];

            return contentMimeType.Equals("text/csv", StringComparison.InvariantCultureIgnoreCase) ||
                   contentMimeType.StartsWith("text/csv", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
