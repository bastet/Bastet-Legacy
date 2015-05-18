using System.Reflection;
using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace Bastet
{
    internal class Program
    {
        private readonly HttpServer.HttpServer _server;

        private readonly bool _daemon = false;

        private Program(Options options)
        {
            _daemon = options.Daemon;

            if (options.CleanStart)
                options.InteractiveSetup();

            Database.Database db = new Database.Database(options.CleanStart, options.ConnectionString);

            _server = new HttpServer.HttpServer(options.HttpPort, db.ConnectionFactory);
        }

        private void Run(Options options)
        {
            _server.Start(options);

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
            // Move into the directory of the exe file, important for unix like environments
            // ReSharper disable once PossibleNullReferenceException
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToString(CultureInfo.InvariantCulture);

            var options = new Options();
            if (options.LoadFromFile() || Parser.Default.ParseArguments(args, options))
            {
                var p = new Program(options);
                p.Run(options);
            }
            else
                HelpText.DefaultParsingErrorsHandler(options, null);
        }
    }

    public class Options
    {
        [Option('s', "setup", Required = false, HelpText = "If set, clear all data from the database on startup")]
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public bool CleanStart { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global

        [Option('h', "http", Required = true, HelpText = "The port to host the HTTP server on")]
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public ushort HttpPort { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global

        [Option('c', "connection", Required = false, HelpText = "A database connection string to use")]
        public string ConnectionString { get; set; }

        [Option('d', "daemonize", Required = false, HelpText = "If set, this process will run as a daemon")]
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public bool Daemon { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global

        [Option('l', "bindlocalhost", Required = false, DefaultValue = false, HelpText = "Indicates if the localhost URL should be bound")]
        public bool BindLocalhost { get; set; }

        [Option('n', "bindname", Required = false, DefaultValue = false, HelpText = "Indicates if the machine name URL should be bound")]
        public bool BindName { get; set; }

        [Option('i', "bindips", Required = false, DefaultValue = false, HelpText = "Indicates if all the machine IPs should be bound")]
        public bool BindIps { get; set; }

        [Option('b', "bind", Required = false, DefaultValue = null, HelpText = "Indicates if all the machine IPs should be bound")]
        public string ExplicitBind { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this);
        }

        #region interactive setup
        public void InteractiveSetup()
        {
            if (ConnectionString == null)
                ConnectionString = InteractiveConnectionString();
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
                else switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        highlighted = highlighted - 1;
                        if (highlighted == -1)
                            highlighted = lines.Length - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        highlighted = (highlighted + 1) % lines.Length;
                        break;
                    case ConsoleKey.Enter: {
                        var str = lines[highlighted].Specific();
                        Console.WriteLine(" - Using Configured Connection String, Next Time Run With:");
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\t--connection='{0}'", str);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine();
                        return str;
                    }
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
        public bool LoadFromFile()
        {
            if (!File.Exists("config.json"))
            {
                Console.WriteLine("Could not find expected options file at path config.json");
                return false;
            }

            var json = JObject.Parse(File.ReadAllText("config.json"));

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

            return true;
        }
        #endregion
    }
}
