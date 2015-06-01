using System;
using System.Collections.Generic;
using System.Linq;
using Bastet.HttpServer.Responses.Formats;
using Nancy;
using Nancy.Responses.Negotiation;

namespace Bastet.HttpServer.Responses
{
    public abstract class BaseVersioningProcessor<T, V>
        : IResponseProcessor
    {
        private static readonly IFormat[] _formats = {
            new JsonFormat(),
            new ProtobufFormat(),
            //"link-format",
        };

        private readonly string[] _mediaTypes;
        private readonly IEnumerable<ISerializer> _serializers;

        protected BaseVersioningProcessor(IEnumerable<ISerializer> serializers, int version)
        {
            _mediaTypes = MediaTypeName.NamesFor<T>(version, _formats).ToArray();
            _serializers = serializers;
        }

        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            var modelResult = model is T ? MatchResult.ExactMatch : MatchResult.NoMatch;

            //See if we can match any of the acceptable media types
            MatchResult result = MatchResult.NoMatch;
            if (_mediaTypes.Any(mediaType => new MediaRange(mediaType).MatchesWithParameters(requestedMediaRange)))
                result = MatchResult.ExactMatch;

            return new ProcessorMatch {
                ModelResult = modelResult,
                RequestedContentTypeResult = result
            };
        }

        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get { yield break; }
        }

        public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            var v1RouteList = CreateVersionedType((T)model);

            var r = requestedMediaRange.Subtype.ToString();
            var formatter = _formats.First(f => r.Contains(f.Name));
            return formatter.Respond(v1RouteList, _serializers);
        }

        protected abstract V CreateVersionedType(T model);
    }
}
