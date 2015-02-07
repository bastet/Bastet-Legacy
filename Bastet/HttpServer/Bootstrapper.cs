using System;
using System.Data;
using Bastet.Database.Model;
using MoreLinq;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
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

        //protected override NancyInternalConfiguration InternalConfiguration
        //{
        //    get
        //    {
        //        return NancyInternalConfiguration.WithOverrides(x => x.Serializers.Insert(0, typeof(JsonNetSerializer)));
        //    }
        //}

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
}
