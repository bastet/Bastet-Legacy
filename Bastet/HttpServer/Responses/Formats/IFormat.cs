using System.Collections.Generic;
using System.Linq;
using Nancy;

namespace Bastet.HttpServer.Responses.Formats
{
    public interface IFormat
    {
        string Name { get; }

        Response Respond(object model, IEnumerable<ISerializer> serializers);
    }

    internal static class FormatExtensions
    {
        public static ISerializer FindSerializer(this IFormat format, IEnumerable<ISerializer> serializers)
        {
            return serializers.First(x => x.CanSerialize(string.Format("application/{0}", format.Name)));
        }
    }
}
