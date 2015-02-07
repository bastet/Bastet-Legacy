using System;
using Nancy;
using Nancy.Hosting.Self;
using ServiceStack.Data;

namespace Bastet.HttpServer
{
    public class HttpServer
    {
        private readonly ushort _httpPort;
        private readonly IDbConnectionFactory _connectionFactory;

        private static NancyHost _host;

        public HttpServer(ushort httpPort, IDbConnectionFactory connectionFactory)
        {
            _httpPort = httpPort;
            _connectionFactory = connectionFactory;
        }

        public void Start()
        {
            var uri = new Uri("http://localhost:" + _httpPort);
            var config = new HostConfiguration
            {
                RewriteLocalhost = false
            };

            var bootstrapper = new Bootstrapper(_connectionFactory);

            _host = new NancyHost(bootstrapper, config, uri);
            _host.Start();

            Console.WriteLine(string.Format("Running HTTP Server on {0}", uri));
        }

        public void Shutdown()
        {
            if (_host != null)
            {
                _host.Stop();
                _host.Dispose();
                _host = null;
            }
        }
    }
}
