using System.Linq;
using Bastet.HttpServer.Modules;
using Nancy;
using ProtoBuf;
using System.Collections.Generic;

namespace Bastet.HttpServer.Responses.v1
{
    public class ClaimListProcessor
        : BaseVersioningProcessor<ClaimsList, ClaimsListV1>
    {
        public ClaimListProcessor(IEnumerable<ISerializer> serializers)
            : base(serializers, 1)
        {
        }

        protected override ClaimsListV1 CreateVersionedType(ClaimsList model)
        {
            return new ClaimsListV1(model);
        }
    }

    [ProtoContract]
    public class ClaimsListV1
    {
        [ProtoMember(1)]
        public string[] claims { get; set; }

        public ClaimsListV1(ClaimsList list)
        {
            claims = list.Claims.Select(a => a.Name).ToArray();
        }
    }
}
