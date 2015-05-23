using Bastet.Database.Model;
using System.Threading.Tasks;

namespace Bastet.Backends
{
    public interface IBackend
    {
        /// <summary>
        /// Send a request to a backend and return the response
        /// </summary>
        /// <param name="device"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IBackendResponse> Request(Device device, BackendRequest request);

        /// <summary>
        /// Describe the possible interactions with the given backend
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        Task<BackendDescription> Describe(Device device);

        /// <summary>
        /// Setup the database for this backend
        /// </summary>
        /// <param name="database"></param>
        void Setup(Database.Database database);

        //todo: observe
    }
}
