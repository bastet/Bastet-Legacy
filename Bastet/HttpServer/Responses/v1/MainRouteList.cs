using Nancy;
using System.Collections.Generic;
using ProtoBuf;

namespace Bastet.HttpServer.Responses.v1
{
    public class MainRouteListProcessor
        : BaseVersioningProcessor<MainRouteList, MainRouteListV1>
    {        
        public MainRouteListProcessor(IEnumerable<ISerializer> serializers)
            : base(serializers, 1)
        {
        }

        protected override MainRouteListV1 CreateVersionedType(MainRouteList model)
        {
            return new MainRouteListV1(model);
        }
    }

    [ProtoContract]
    public class MainRouteListV1
    {
        [ProtoMember(1)]
        public string Devices { get; set; }

        [ProtoMember(2)]
        public string Authentication { get; set; }

        [ProtoMember(3)]
        public string Users { get; set; }

        public MainRouteListV1(MainRouteList list)
        {
            Devices = list.Devices;
            Authentication = list.Authentication;
            Users = list.Users;
        }
    }
}
