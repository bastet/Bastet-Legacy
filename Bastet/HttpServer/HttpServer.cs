using System;
using Nancy.Hosting.Self;
using Ninject;

namespace Bastet.HttpServer
{
    public class HttpServer
    {
        private readonly ushort _httpPort;

        private static NancyHost _host;

        public HttpServer(ushort httpPort)
        {
            _httpPort = httpPort;
        }

        public void Start(IKernel kernel)
        {
            var uri = new Uri("http://localhost:" + _httpPort);
            var config = new HostConfiguration
            {
                RewriteLocalhost = false,
            };

            var bootstrapper = new Bootstrapper(kernel);

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
