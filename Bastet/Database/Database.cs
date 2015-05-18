using Bastet.Database.Model;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Bastet.Database
{
    public class Database
    {
        public const string DEFAULT_ADMINISTRATOR_USERNAME = "Administrator";
        public const string DEFAULT_ADMINISTRATOR_PASSWORD = "password";

        private readonly IDbConnectionFactory _connectionFactory;
        public IDbConnectionFactory ConnectionFactory
        {
            get { return _connectionFactory; }
        }

        /// <summary>
        /// Set to true to recreate the database (losing all data)
        /// </summary>
        /// <param name="clean"></param>
        /// <param name="connectionString"></param>
        public Database(bool clean = false, string connectionString = null)
        {
            _connectionFactory = new OrmLiteConnectionFactory(connectionString, SqliteDialect.Provider);
            using (var db = _connectionFactory.Open())
            {
                var models = new[]
                {
                    typeof(Device),
                    typeof(Sensor),
                    typeof(StringReading),
                    typeof(DecimalReading),
                    typeof(BlobReading),

                    typeof(User),
                    typeof(Session),
                    typeof(Claim),
                };

                if (clean)
                {
                    foreach (var model in models)
                        db.CreateTable(true, model);

                    using (var transaction = db.OpenTransaction())
                    {
                        //Create admin user with null password
                        var admin = new User(DEFAULT_ADMINISTRATOR_USERNAME, DEFAULT_ADMINISTRATOR_PASSWORD);
                        db.Save(admin);

                        //Create some sensible claims for an admin user
                        string[] claims = {
                            "superuser"
                        };
                        foreach (var claim in claims)
                            db.Save(new Claim(admin, claim));

                        transaction.Commit();
                    }
                }
                else
                {
                    foreach (var model in models)
                        db.CreateTableIfNotExists(model);
                }
            }
        }
    }
}
