using Bastet.Database.Model;
using System;
using System.Threading.Tasks;

namespace Bastet.Backends.Http
{
    public abstract class BaseHttpBackend
        : IBackend
    {
        public async Task<IBackendResponse> Request(Device device, BackendRequest request)
        {
            throw new NotImplementedException();
        }

        public abstract Task<BackendDescription> Describe(Device device);

        protected static Task<BackendDescription> FetchWellKnownCore(Uri baseUri)
        {
            throw new NotImplementedException("Fetch .well-known/core");
            throw new NotImplementedException("Turn this into a BackendDescription");
        }

        protected static Task<BackendDescription> FetchSwagger(Uri baseUri, string swaggerPath)
        {
            throw new NotImplementedException("Fetch swaggerPath");
            throw new NotImplementedException("Turn this into a BackendDescription");
        }


        public abstract void Setup(Database.Database database);
    }
}
