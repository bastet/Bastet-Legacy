using Nancy;
using ProtoBuf;
using System.Collections.Generic;

namespace Bastet.HttpServer.Responses.v1
{
    public class SessionProcessor
        : BaseVersioningProcessor<Session, SessionV1>
    {
        public SessionProcessor(IEnumerable<ISerializer> serializers)
            : base(serializers, 1)
        {
        }

        protected override SessionV1 CreateVersionedType(Session model)
        {
            return new SessionV1(model);
        }
    }

    [ProtoContract]
    public class SessionV1
    {
        public string SessionKey { get; set; }

        public UserV1 User { get; set; }

        public SessionV1(Session key)
        {
            SessionKey = key.SessionKey;
            User = new UserV1(new User(key.User));
        }
    }
}
