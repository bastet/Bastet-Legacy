using Nancy;
using Nancy.Responses.Negotiation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bastet.HttpServer.Csv
{
    public class CsvProcessor : IResponseProcessor
    {
        private readonly ISerializer _serializer;

        private static readonly IEnumerable<Tuple<string, MediaRange>> _extensionMappings = new[] {
            new Tuple<string, MediaRange>("csv", new MediaRange("text/csv"))
        };

        public CsvProcessor(IEnumerable<ISerializer> serializers)
        {
            _serializer = serializers.FirstOrDefault(x => x.CanSerialize("text/csv"));
        }

        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get { return _extensionMappings; }
        }

        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            if (requestedMediaRange.Matches("text/csv"))
            {
                return new ProcessorMatch
                {
                    ModelResult = MatchResult.DontCare,
                    RequestedContentTypeResult = MatchResult.ExactMatch
                };
            }

            return new ProcessorMatch
            {
                ModelResult = MatchResult.DontCare,
                RequestedContentTypeResult = MatchResult.NoMatch
            };
        }

        public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            return new CsvResponse(model, _serializer);
        }
    }
}
