using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

        public void Start(Options options)
        {
            var bootstrapper = new Bootstrapper(_connectionFactory);

            var binds = Binds(options.BindLocalhost, options.BindName, options.BindIps, options.ExplicitBind).ToArray();
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

        private IEnumerable<Uri> Binds(bool localhost, bool name, bool ips, string bind)
        {
            if (localhost)
                yield return Localhost();

            if (name)
                yield return MachineName();

            if (ips)
                foreach (var uri in LocalIp())
                    yield return uri;

            if (!string.IsNullOrWhiteSpace(bind))
                yield return new Uri(bind);
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
                      .Where(a => !IPAddress.IsLoopback(a))
                      .Select(a => ToUri(a, _httpPort))
                      .Where(a => a != null);
        }

        private static Uri ToUri(IPAddress addr, ushort port)
        {
            switch (addr.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    return new Url(string.Format("http://{0}:{1}", addr, port));
                case AddressFamily.InterNetworkV6:
                    return new Url(string.Format("http://[{0}]:{1}", addr, port));
            }

            return null;
        }
    }
}
