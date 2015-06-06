using System.Data;
using Bastet.Database.Model;
using System.Threading.Tasks;

namespace Bastet.Backends
{
    public interface IBackend
    {
        /// <summary>
        /// Send a request to a backend and return the response
        /// </summary>
        /// <param name="db"></param>
        /// <param name="device"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BackendResponse> Request(IDbConnection db, Device device, BackendRequest request);

        /// <summary>
        /// Describe the possible interactions with the given backend
        /// </summary>
        /// <param name="db"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        Task<BackendDescription> Describe(IDbConnection db, Device device);

        /// <summary>
        /// Setup the database for this backend
        /// </summary>
        /// <param name="database"></param>
        void Setup(Database.Database database);

        //todo: observe
    }
}
