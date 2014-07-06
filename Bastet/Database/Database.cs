using System;
using System.Data;
using System.Data.Linq;
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
        public Database(bool clean = false, string adminUsername = null)
        {
            _connectionFactory = new OrmLiteConnectionFactory("Data Source=bastet.sqlite;Version=3;New=True;", SqliteDialect.Provider);
            using (IDbConnection db = _connectionFactory.Open())
            {
                Type[] models = new Type[]
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

                    if (adminUsername != null)
                    {
                        using (var transaction = db.OpenTransaction())
                        {
                            var admin = db.SingleWhere<User>("Username", adminUsername);
                            if (admin == null)
                                Console.WriteLine("Admin user {0} not found", adminUsername);
                            else
                            {
                                var claim = db.Where<Claim>(new { UserId = admin.Id, Name = "create-claim" });
                                if (claim == null)
                                    db.Save<Claim>(new Claim(admin, "create-claim"));
                            }

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
