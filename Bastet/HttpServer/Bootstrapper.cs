using System;
using System.Data;
using Bastet.Database.Model;
using MoreLinq;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Nancy.Serialization.JsonNet;
using Newtonsoft.Json;
using Ninject;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Bastet.HttpServer
{
    public class Bootstrapper
        : NinjectNancyBootstrapper
    {
        private readonly IKernel _kernel;

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(x => x.Serializers.Insert(0, typeof(JsonNetSerializer)));
            }
        }

        public Bootstrapper(IKernel kernel)
        {
            _kernel = kernel;

            StaticConfiguration.DisableErrorTraces = false;
            StaticConfiguration.EnableRequestTracing = true;

            //Cross origin resource sharing
            ApplicationPipelines.AfterRequest.AddItemToEndOfPipeline(x => x.Response.WithHeader("Access-Control-Allow-Origin", "*"));
            ApplicationPipelines.AfterRequest.AddItemToEndOfPipeline(x => x.Response.WithHeader("Access-Control-Allow-Methods", "DELETE, GET, HEAD, POST, PUT, OPTIONS, PATCH"));
            ApplicationPipelines.AfterRequest.AddItemToEndOfPipeline(x => x.Response.WithHeader("Access-Control-Allow-Headers", "Content-Type"));
            ApplicationPipelines.AfterRequest.AddItemToEndOfPipeline(x => x.Response.WithHeader("Accept", "application/json"));

            //Default format to JSON
            ApplicationPipelines.BeforeRequest.AddItemToStartOfPipeline(x =>
            {
                x.Request.Headers.Accept = x.Request.Headers.Accept.Concat(new Tuple<string, decimal>("application/json", 1.05m));
                return null;
            });
        }

        protected override IKernel GetApplicationContainer()
        {
            _kernel.Load<FactoryModule>();
            return _kernel;
        }

        protected override void ConfigureApplicationContainer(IKernel container)
        {
            base.ConfigureApplicationContainer(container);

            container.Rebind<JsonSerializer>().To<CustomJsonSerializer>();
        }

        protected override void ApplicationStartup(IKernel container, IPipelines pipelines)
        {
            Nancy.Json.JsonSettings.MaxJsonLength = int.MaxValue;

            base.ApplicationStartup(container, pipelines);
        }

        protected override void RequestStartup(IKernel container, IPipelines pipelines, NancyContext context)
        {
            StatelessAuthentication.Enable(pipelines, new StatelessAuthenticationConfiguration(ctx =>
            {
                if (!ctx.Request.Query.sessionkey.HasValue)
                    return null;

                var connection = container.Get<IDbConnection>();

                var key = (string)ctx.Request.Query.sessionkey;
                var session = connection.SingleWhere<Session>("SessionKey", key);
                if (session == null)
                    return null;
                var user = connection.SingleById<User>(session.UserId);
                if (user == null)
                    return null;

                return new Identity(Identity.GetClaims(user, connection), user, session);
            }));
        }

        protected override void ConfigureRequestContainer(IKernel container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            var factory = container.Get<IDbConnectionFactory>();
            container.Bind<IDbConnection>().ToMethod(c => factory.Open()).InSingletonScope();
        }
    }
}
