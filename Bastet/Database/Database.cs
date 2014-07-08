using Bastet.Database.Model;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Bastet.Database
{
    public class Database
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public IDbConnectionFactory ConnectionFactory
        {
            get { return _connectionFactory; }
        }

        /// <summary>
        /// Set to true to recreate the database (losing all data)
        /// </summary>
        /// <param name="clean"></param>
        /// <param name="adminUsername"></param>
        /// <param name="adminPassword"></param>
        /// <param name="connectionString"></param>
        public Database(bool clean = false, string adminUsername = null, string adminPassword = null, string connectionString = null)
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
                    typeof(Claim)
                };

                if (clean)
                {
                    foreach (var model in models)
                        db.CreateTable(true, model);

                    if (adminUsername != null && adminPassword != null)
                    {
                        using (var transaction = db.OpenTransaction())
                        {
                            //Create admin
                            var admin = new User(adminUsername, adminPassword);
                            db.Save(admin);

                            //Give admin permission to give out new permissions
                            var claim = db.Where<Claim>(new { UserId = admin.Id, Name = "create-claim" });
                            if (claim == null)
                                db.Save(new Claim(admin, "create-claim"));

                            transaction.Commit();
                        }
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
