using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Bastet.Database.Model;
using MoreLinq;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.LightningCache.Extensions;
using Nancy.Routing;
using Nancy.Security;
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

            //Default format to JSON (very low priority)
            ApplicationPipelines.BeforeRequest.AddItemToStartOfPipeline(x => {
                x.Request.Headers.Accept = x.Request.Headers.Accept.Concat(new Tuple<string, decimal>("application/json", 0.01m));
                return null;
            });

            //Make sure this is being accessed over a secure connection
            var httpsRedirect = SecurityHooks.RequiresHttps(redirect: false);
            ApplicationPipelines.BeforeRequest.AddItemToEndOfPipeline(x => {
                if (!IsSecure(x))
                    return httpsRedirect(x);
                return null;
            });
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/", @"Static"));
            base.ConfigureConventions(nancyConventions);
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
                if (ctx.Request.Query.SessionKey.HasValue)
                    sessionKey = (string)ctx.Request.Query.sessionkey;
                else if (!ctx.Request.Cookies.TryGetValue("Bastet_Session_Key", out sessionKey))
                    return null;

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

        private bool IsSecure(NancyContext context)
        {
            //If we're directly access via a secure scheme everything is ok
            if (context.Request.Url.IsSecure)
                return true;

            //We might be directly accessed via an insecure scheme because a reverse proxy did https termination already, check if a header has been set indicating this
            if (context.Request.Headers.Keys.Contains("X-Forwarded-Protocol") && context.Request.Headers["X-Forwarded-Protocol"].Contains("https"))
                return true;
            if (context.Request.Headers.Keys.Contains("X-Forwarded-Proto") && context.Request.Headers["X-Forwarded-Proto"].Contains("https"))
                return true;

#if DEBUG
            var frame = new StackTrace(true).GetFrame(0);
            Console.WriteLine("Skipping HTTPS check (DEBUG MODE) {0} Ln{1}", frame.GetFileName(), frame.GetFileLineNumber());
            return true;
#else
            return false;
#endif
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
