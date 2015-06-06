using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Bastet.Backends
{
    public sealed class BackendResponse
        : IDisposable
    {
        /// <summary>
        /// Response status
        /// </summary>
        public HttpStatusCode Status { get; private set; }

        /// <summary>
        /// Headers of this response
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Headers { get; private set; }

        /// <summary>
        /// Body of this response
        /// </summary>
        public Stream Body { get; private set; }

        public BackendResponse(HttpStatusCode status, IEnumerable<KeyValuePair<string, string>> headers, Stream body)
        {
            Status = status;
            Headers = headers.ToArray();
            Body = body;
        }

        ~BackendResponse()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Body != null)
                    Body.Dispose();
            }
        }
    }
}
