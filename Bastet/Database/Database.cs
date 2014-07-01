using Common.Logging;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Bastet.Database
{
    public class Database
    {
        private static readonly ILog _logger = LogManager.GetCurrentClassLogger();

        private readonly ISessionFactory _sessionFactory;
        public ISessionFactory SessionFactory
        {
            get { return _sessionFactory; }
        }

        /// <summary>
        /// Set to true to recreate the database (losing all data)
        /// </summary>
        /// <param name="clean"></param>
        public Database(bool clean = false)
        {
            _logger.Info("Creating database");

            // Initialize NHibernate
            var cfg = new Configuration();
            cfg.Configure();
            cfg.AddAssembly(typeof(Database).Assembly);

            if (clean)
            {
                _logger.Info("Cleaning database");
                var s = new SchemaExport(cfg);
                s.Create(false, true);
            }
            else
            {
                var s = new SchemaUpdate(cfg);
                s.Execute(false, true);
            }

            // Get ourselves an NHibernate Session factory
            _sessionFactory = cfg.BuildSessionFactory();
        }
    }
}
