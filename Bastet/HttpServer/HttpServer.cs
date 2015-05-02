using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            var bootstrapper = new Bootstrapper(_connectionFactory);

            var binds = Binds().ToArray();
            _host = new NancyHost(bootstrapper, new HostConfiguration
            {
                RewriteLocalhost = false
            }, binds);
            _host.Start();

            Console.WriteLine(string.Format("Running HTTP Server on {0} IPs: {1}", binds.Length, string.Join<Uri>("\n", binds)));
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

        private IEnumerable<Uri> Binds()
        {
            yield return Localhost();
            yield return MachineName();
            foreach (var uri in LocalIp())
                yield return uri;
        }

        private Uri Localhost()
        {
            return new Uri("http://localhost:" + _httpPort);
        }

        private Uri MachineName()
        {
            return new Uri(string.Format("http://{0}:{1}", Environment.MachineName, _httpPort));
        }

        private IEnumerable<Uri> LocalIp()
        {
            return Dns.GetHostAddresses(Dns.GetHostName())
                      .Where(IPAddress.IsLoopback)
                      .Select(a => new Uri(string.Format("http://{0}:{1}", a, _httpPort)));
        }
    }
}
