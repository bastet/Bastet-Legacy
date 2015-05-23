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
        /// <param name="connectionString"></param>
        public Database(string connectionString = null)
        {
            _connectionFactory = new OrmLiteConnectionFactory(connectionString, SqliteDialect.Provider);
            using (var db = _connectionFactory.Open())
            {
                db.CreateTableIfNotExists<Device>();
                db.CreateTableIfNotExists<Session>();
                db.CreateTableIfNotExists<Claim>();

                if (!db.TableExists<User>())
                {
                    db.CreateTable<User>();

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
            }
        }
    }
}
