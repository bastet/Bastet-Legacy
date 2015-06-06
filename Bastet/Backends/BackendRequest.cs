using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bastet.Backends
{
    public class BackendRequest
    {
        /// <summary>
        /// Url to send this request to
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Method to use for this request
        /// </summary>
        public string Method { get; private set; }

        /// <summary>
        /// Headers to include with this request
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Headers { get; private set; }

        /// <summary>
        /// Body to send with this request
        /// </summary>
        public Stream Body { get; private set; }

        public BackendRequest(string url, string method, IEnumerable<KeyValuePair<string, string>> headers, Stream body)
        {
            Url = url;
            Method = method;
            Headers = headers.ToArray();
            Body = body;
        }
    }
}
