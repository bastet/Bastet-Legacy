using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json.Linq;
using Ninject;
using ServiceStack.Data;

namespace Bastet
{
    internal class Program
    {
        private readonly Database.Database _db;
        private readonly HttpServer.HttpServer _server;
        private readonly IKernel _kernel;

        private readonly bool _daemon = false;

        private Program(Options options)
        {
            _daemon = options.Daemon;

            _kernel = new StandardKernel();

            if (options.CleanStart)
                options.InteractiveSetup();

            _db = new Database.Database(options.CleanStart, options.AdminUsername, options.AdminPassword, options.ConnectionString);
            _kernel.Bind<Database.Database>().ToConstant(_db);
            _kernel.Bind<IDbConnectionFactory>().ToMethod(c => _db.ConnectionFactory);

            _server = new HttpServer.HttpServer(options.HttpPort);
            _kernel.Bind<HttpServer.HttpServer>().ToConstant(_server);
        }

        private void Run()
        {
            _server.Start(_kernel);

            //Under mono if you deamonize a process a Console.ReadLine will cause an EOF 
            //so we need to block another way
            if (_daemon)
            {
                while (true)
                    Thread.Sleep(10000);
            }
            else
            {
                Console.WriteLine("Press Any Key To Exit");
                Console.ReadKey();
            }
        }

        public static void Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                options.LoadFromFile();

                var p = new Program(options);
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
        [Option('o', "options", Required = false, HelpText = "Path to a file of options (overrides commandline arguments)")]
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public string OptionsPath { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper restore MemberCanBePrivate.Global

        [Option('s', "setup", Required = false, HelpText = "If set, clear all data from the database on startup")]
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public bool CleanStart { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global

        [Option('h', "http", Required = true, HelpText = "The port to host the HTTP server on")]
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public ushort HttpPort { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global

        [Option('a', "admin", Required = false,
        HelpText = "The username of the admin user (only applied in conjunction with --setup)")]
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public string AdminUsername { get; set; }

        // ReSharper restore UnusedAutoPropertyAccessor.Global

        [Option('p', "password", Required = false,
        HelpText = "The password of the admin user (only applied in conjunction with --setup)")]
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public string AdminPassword { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global

        [Option('c', "connection", Required = false, HelpText = "A database connection string to use")]
        public string ConnectionString { get; set; }

        [Option('d', "daemonize", Required = false, HelpText = "If set, this process will run as a daemon")]
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public bool Daemon { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this);
        }

        #region interactive setup
        public void InteractiveSetup()
        {
            if (AdminUsername == null)
            {
                Console.WriteLine("Enter username for Administrator account:");
                Console.Write("> ");
                AdminUsername = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(AdminUsername))
                    AdminUsername = null;
            }

            if (AdminPassword == null)
            {
                Console.WriteLine("Enter password for Administrator account:");
                Console.Write("> ");
                AdminPassword = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(AdminPassword))
                    AdminPassword = null;
            }

            if (ConnectionString == null)
            {
                ConnectionString = InteractiveConnectionString();
            }
        }

        private static string InteractiveConnectionString()
        {
            Console.WriteLine("Which type of database do you want to use?");
            var lines = new[]
            {
                new { Name = "sqlite", Specific = (Func<string>)ConfigSqlite },
                new { Name = "sqlite (in memory)", Specific = new Func<string>(() => "Data Source=:memory:;Version=3;New=True;") },
                new { Name = "other (enter complete connection string)", Specific = (Func<string>)ReadConnectionString }
            };

            var posX = Console.CursorLeft;
            var posY = Console.CursorTop;

            int highlighted = 0;
            while (true)
            {
                Console.SetCursorPosition(posX, posY);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (i == highlighted)
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine((i + 1) + ". > " + lines[i].Name);
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine((i + 1) + ".   " + lines[i].Name);
                    }
                }

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;

                Console.CursorVisible = false;
                var key = Console.ReadKey();
                if (Char.IsNumber(key.KeyChar))
                {
                    var num = int.Parse(key.KeyChar.ToString(CultureInfo.InvariantCulture));
                    highlighted = (num - 1) < lines.Length ? (num - 1) : highlighted;
                }
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    highlighted = highlighted - 1;
                    if (highlighted == -1)
                        highlighted = lines.Length - 1;
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    highlighted = (highlighted + 1) % lines.Length;
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    var str = lines[highlighted].Specific();
                    Console.WriteLine(" - Using Configured Connection String, Next Time Run With:");
                    Console.WriteLine("--connection='{0}'", str);
                    return str;
                }
            }
        }

        private static string ConfigSqlite()
        {
            Console.WriteLine("Where Should The Database be Stored (Enter a path)?");
            Console.Write("> ");
            var path = Console.ReadLine();
            return string.Format("Data Source={0};Version=3;", path);
        }

        private static string ReadConnectionString()
        {
            Console.WriteLine("Enter Connection String:");
            Console.Write("> ");
            return Console.ReadLine();
        }
        #endregion

        #region load from file
        public void LoadFromFile()
        {
            if (OptionsPath == null)
                return;

            if (!File.Exists(OptionsPath))
            {
                Console.WriteLine("Could not find expected options file at path {0}", OptionsPath);
                return;
            }

            var json = JObject.Parse(File.ReadAllText(OptionsPath));

            //Select properties marked with the OptionAttribute
            var properties = GetType()
                .GetProperties()
                .Select(p => new { prop = p, attr = (OptionAttribute)p.GetCustomAttributes(typeof(OptionAttribute), true).SingleOrDefault() })
                .Where(p => p.attr != null);

            //Find a matching string in the json document
            foreach (var property in properties)
            {
                JToken value;
                if (json.TryGetValue(property.attr.LongName, out value))
                    property.prop.SetValue(this, value.ToObject(property.prop.PropertyType), null);
            }
        }
        #endregion
    }
}
