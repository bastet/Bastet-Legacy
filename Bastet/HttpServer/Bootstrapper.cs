using System;
using System.Data;
using MoreLinq;
using Nancy;
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
                return NancyInternalConfiguration.WithOverrides(c => c.Serializers.Insert(0, typeof(JsonNetSerializer)));
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

        protected override void ConfigureRequestContainer(IKernel container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            var factory = container.Get<IDbConnectionFactory>();
            container.Bind<IDbConnection>().ToMethod(c => factory.Open()).InSingletonScope();
        }
    }
}
