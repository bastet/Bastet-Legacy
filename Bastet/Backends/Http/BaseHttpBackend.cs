using System.Data;
using Bastet.Database.Model;
using System.Threading.Tasks;

namespace Bastet.Backends.Http
{
    public abstract class BaseHttpBackend
        : IBackend
    {
        public abstract Task<BackendResponse> Request(IDbConnection db, Device device, BackendRequest request);

        public abstract Task<BackendDescription> Describe(IDbConnection db, Device device);

        public abstract void Setup(Database.Database database);
    }
}
