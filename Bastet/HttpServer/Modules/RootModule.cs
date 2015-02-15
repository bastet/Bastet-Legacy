using System.Linq;
using Bastet.Database.Model;
using Nancy;
using System.Data;
using ServiceStack;
using ServiceStack.OrmLite;

namespace Bastet.HttpServer.Modules
{
    public class RootModule
        : NancyModule
    {
        private readonly IDbConnection _connection;

        public RootModule(IDbConnection connection)
        {
            _connection = connection;

            Get["/"] = GetRoot;
        }

        private dynamic GetRoot(dynamic parameters)
        {
            var setup = GetSetupProgress();
            if (!setup.IsComplete())
                return View["Setup", setup];

            return View["Root"];
        }

        private SetupProgress GetSetupProgress()
        {
            return new SetupProgress(_connection);
        }

        public class SetupProgress
        {
            public bool SetAdminPassword { get; private set; }

            public bool CreateUserAccount { get; private set; }

            public bool AddedFirstDevice { get; private set; }

            public SetupProgress(bool setAdminPassword, bool createUserAccount, bool addedFirstDevice)
            {
                SetAdminPassword = setAdminPassword;
                CreateUserAccount = createUserAccount;
                AddedFirstDevice = addedFirstDevice;
            }

            public SetupProgress(IDbConnection connection)
            {
                //Check if there is an admin account with a non null password
                SetAdminPassword = !connection.Select<User>(a => a.Username == Database.Database.DEFAULT_ADMINISTRATOR_USERNAME).Any(a => a.ComputeSaltedHash(Database.Database.DEFAULT_ADMINISTRATOR_PASSWORD).SlowEquals(a.PasswordHash));

                //Check if there is any user account not called "admin"
                CreateUserAccount = connection.Count<User>(a => a.Username != Database.Database.DEFAULT_ADMINISTRATOR_USERNAME) > 0;

                //Check if there are any devices
                AddedFirstDevice = connection.Count<Device>() > 0;
            }

            public bool IsComplete()
            {
                return SetAdminPassword
                    && CreateUserAccount
                    && AddedFirstDevice;
            }
        }
    }
}
