using System;
using Bastet.Database.Model;
using CommandLine;
using CommandLine.Text;
using NHibernate.Criterion;
using NHibernate.Linq;
using Ninject;

namespace Bastet
{
    internal class Program
    {
        private readonly Database.Database _db;
        private readonly HttpServer.HttpServer _server;
        private readonly IKernel _kernel;

        public Program(Options options)
        {
            _kernel = new StandardKernel();

            _db = new Database.Database(options.CleanDatabase);
            _kernel.Bind<Database.Database>().ToConstant(_db);

            _server = new HttpServer.HttpServer(options.HttpPort);
            _kernel.Bind<HttpServer.HttpServer>().ToConstant(_server);
        }

        public void Run()
        {
            _server.Start(_kernel);

            using (var sess = _db.SessionFactory.OpenSession())
            {

                //Create a Product...
                var device = new Device
                {
                    Url = "http://google.co.uk"
                };

                //And save it to the database
                sess.Save(device);
                sess.Flush();

                // Note that we do not use the table name specified
                // in the mapping, but the class name, which is a nice
                // abstraction that comes with NHibernate
                var q = sess.CreateCriteria<Device>()
                            .Add(Restrictions.NotEqProperty("Url", "Id"));
                var list = q.List<Device>();

                var devices = from d in sess.QueryOver<Device>()
                              where d.Url != null
                              select d;

                // List all the entries' names
                devices.List().ForEach(p => Console.WriteLine(p.Id));
            }

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
        public bool CleanDatabase { get; set; }

        [Option('h', "http", Required = true, HelpText = "The port to host the HTTP server on")]
        public ushort HttpPort { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this);
        }
    }
}
