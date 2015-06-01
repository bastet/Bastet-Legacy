using System.Collections.Generic;
using System.Globalization;
using Bastet.HttpServer.Responses.Formats;

namespace Bastet.HttpServer.Responses
{
    public static class MediaTypeName
    {
        public static IEnumerable<string> NamesFor<T>(int version, IFormat[] formats)
        {
            var name = typeof(T).Name.ToLowerInvariant();
            var v = version.ToString(CultureInfo.InvariantCulture);

            foreach (var format in formats)
                yield return string.Format("application/prs.bastet.{0}+{1}; version={2}", name, format.Name, v);
        }
    }
}
