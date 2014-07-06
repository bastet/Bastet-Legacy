using System;
using Bastet.HttpServer;
using CommandLine;
using CommandLine.Text;
using Ninject;
using ServiceStack.Data;

namespace Bastet
{
    internal class Program
    {
        private readonly Database.Database _db;
        private readonly HttpServer.HttpServer _server;
        private readonly IKernel _kernel;

        private Program(Options options)
        {
            _kernel = new StandardKernel();

            _db = new Database.Database(options.CleanDatabase, options.AdminUsername);
            _kernel.Bind<Database.Database>().ToConstant(_db);
            _kernel.Bind<IDbConnectionFactory>().ToMethod(c => _db.ConnectionFactory);

            _server = new HttpServer.HttpServer(options.HttpPort);
            _kernel.Bind<HttpServer.HttpServer>().ToConstant(_server);
        }

        private void Run()
        {
            _server.Start(_kernel);

            Console.ReadLine();
        }

        public static void Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                Program p = new Program(options);
                p.Run();
            }
            else
            {
                HelpText.DefaultParsingErrorsHandler(options, null);
            }
        }
    }

    public class Options
    {
        [Option('c', "clean", Required = false, HelpText = "If set, clear all data from the database on startup")]
// ReSharper disable UnusedAutoPropertyAccessor.Global
        public bool CleanDatabase { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global

        [Option('h', "http", Required = true, HelpText = "The port to host the HTTP server on")]
// ReSharper disable UnusedAutoPropertyAccessor.Global
        public ushort HttpPort { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global

        [Option('a', "admin", Required = false, HelpText = "The username of the admin user (only applied in conjunction with --clean)")]
// ReSharper disable UnusedAutoPropertyAccessor.Global
        public string AdminUsername { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this);
        }
    }
}
