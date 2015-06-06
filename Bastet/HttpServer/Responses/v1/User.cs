using Bastet.HttpServer.Modules;
using Nancy;
using ProtoBuf;
using System.Collections.Generic;

namespace Bastet.HttpServer.Responses.v1
{
    public class UserProcessor
        : BaseVersioningProcessor<User, UserV1>
    {
        public UserProcessor(IEnumerable<ISerializer> serializers)
            : base(serializers, 1)
        {
        }

        protected override UserV1 CreateVersionedType(User model)
        {
            return new UserV1(model);
        }
    }

    [ProtoContract]
    public class UserV1
    {
        [ProtoMember(1)]
        public string[] claims { get; set; }

        [ProtoMember(2)]
        public string username { get; set; }

        [ProtoMember(3)]
        public string id { get; set; }

        public UserV1(User user)
        {
            claims = user.Claims;
            username = user.Username;
            id = string.Format("{0}/{1}", UsersModule.PATH, user.Username);
        }
    }
}
