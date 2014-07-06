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
        public Database(bool clean = false)
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
                    typeof(BlobReading)
                };

                if (clean)
                {
                    foreach (var model in models)
                        db.CreateTable(true, model);
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
