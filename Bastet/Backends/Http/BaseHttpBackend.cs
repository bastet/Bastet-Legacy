using Bastet.Database.Model;
using System.Threading.Tasks;

namespace Bastet.Backends.Http
{
    public abstract class BaseHttpBackend
        : IBackend
    {
        public abstract Task<IBackendResponse> Request(Device device, BackendRequest request);

        public abstract Task<BackendDescription> Describe(Device device);

        public abstract void Setup(Database.Database database);
    }
}
