using System;
using System.Data;
using System.Text;
using Bastet.Database.Model;
using MoreLinq;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.LightningCache.Extensions;
using Nancy.Routing;
using Nancy.Serialization.JsonNet;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Bastet.HttpServer
{
    public class Bootstrapper
        : DefaultNancyBootstrapper
    {
        private readonly IDbConnectionFactory _connectionFactory;

        protected override IRootPathProvider RootPathProvider
        {
            get { return new NancyRootPath(); }
        }

        public Bootstrapper(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;

            StaticConfiguration.DisableErrorTraces = false;
            StaticConfiguration.EnableRequestTracing = true;

            //Cross origin resource sharing
            ApplicationPipelines.AfterRequest.AddItemToEndOfPipeline(x => x.Response.WithHeader("Access-Control-Allow-Origin", "*"));
            ApplicationPipelines.AfterRequest.AddItemToEndOfPipeline(x => x.Response.WithHeader("Access-Control-Allow-Methods", "DELETE, GET, HEAD, POST, PUT, OPTIONS, PATCH"));
            ApplicationPipelines.AfterRequest.AddItemToEndOfPipeline(x => x.Response.WithHeader("Access-Control-Allow-Headers", "Content-Type"));
            ApplicationPipelines.AfterRequest.AddItemToEndOfPipeline(x => x.Response.WithHeader("Accept", "application/json"));

            ApplicationPipelines.BeforeRequest.AddItemToStartOfPipeline(x =>
            {
                //Default format to JSON
                x.Request.Headers.Accept = x.Request.Headers.Accept.Concat(new Tuple<string, decimal>("application/json", 1.05m));

                return null;
            });
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register<IDbConnectionFactory>(_connectionFactory);
            container.Register<JsonSerializer>((a, b) => new CustomJsonSerializer());
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            Nancy.Json.JsonSettings.MaxJsonLength = int.MaxValue;

            base.ApplicationStartup(container, pipelines);

            this.EnableLightningCache(container.Resolve<IRouteResolver>(), ApplicationPipelines, new UrlHashKeyGenerator());
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            StatelessAuthentication.Enable(pipelines, new StatelessAuthenticationConfiguration(ctx =>
            {
                string sessionKey;
                if (!ctx.Request.Cookies.TryGetValue("Bastet_Session_Key", out sessionKey))
                {
                    if (ctx.Request.Query.sessionkey.HasValue)
                        sessionKey = (string)ctx.Request.Query.sessionkey;
                    else
                        return null;
                }

                var connection = container.Resolve<IDbConnection>();

                var session = connection.SingleWhere<Session>("SessionKey", sessionKey);
                if (session == null)
                    return null;
                var user = connection.SingleById<User>(session.UserId);
                if (user == null)
                    return null;

                return new Identity(Identity.GetClaims(user, connection), user, session);
            }));
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            var factory = container.Resolve<IDbConnectionFactory>();
            container.Register<IDbConnection>(factory.Open());
        }
    }

    public class UrlHashKeyGenerator : Nancy.LightningCache.CacheKey.ICacheKeyGenerator
    {
        public string Get(Request request)
        {
            using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                var hash = md5.ComputeHash(Encoding.UTF32.GetBytes(request.Url.ToString()));
                return Convert.ToBase64String(hash);
            }
        }
    }
}
