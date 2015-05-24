using Bastet.Database.Model;
using System;
using System.Threading.Tasks;

namespace Bastet.Backends.Coap
{
    public class CoapBackend
        : IBackend
    {
        public Task<IBackendResponse> Request(Device device, BackendRequest request)
        {
            throw new NotImplementedException("Send a coap request");
        }

        public Task<BackendDescription> Describe(Device device)
        {
            throw new NotImplementedException("retrieve .well-known/core");
        }

        public void Setup(Database.Database database)
        {
            throw new NotImplementedException();
        }
    }
}
