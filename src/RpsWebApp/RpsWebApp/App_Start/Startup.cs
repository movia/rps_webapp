using Microsoft.Owin;
using Owin;
using System.Web.Http;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RpsPositionWebApp.DataProviders.Rps;
using System.Web.Hosting;
using System.Threading.Tasks;
using System.Threading;
using Common.Logging;
using Microsoft.Owin.BuilderProperties;
using System.Net.Http;
using RpsPositionWebApp.Data;
using Microsoft.AspNet.SignalR;

namespace RpsPositionWebApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var log = LogManager.GetLogger<Startup>();
            log.Info("Application is starting using OWIN.");

            log.Info("Injecting SignalR Middleware.");
            var hubConfiguration = new HubConfiguration();
            hubConfiguration.EnableDetailedErrors = true;
            app.MapSignalR(hubConfiguration);

            var httpConfiguration = new HttpConfiguration();

            httpConfiguration.Formatters.Clear();
            httpConfiguration.Formatters.Add(new JsonMediaTypeFormatter());

            httpConfiguration.Formatters.JsonFormatter.SerializerSettings =
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

            httpConfiguration.MapHttpAttributeRoutes();

            httpConfiguration.Routes.MapHttpRoute(
                name: "WsiProxy",
                routeTemplate: "wsi/{*path}",
                handler: HttpClientFactory.CreatePipeline(
                    innerHandler: new HttpClientHandler(), // will never get here if proxy is doing its job
                    handlers: new DelegatingHandler[] { new WsiProxyHandler() }
                  ),
                defaults: new { path = RouteParameter.Optional },
                constraints: null
            );

            log.Info("Injecting WebApi Middleware.");
            app.UseWebApi(httpConfiguration);

            var properties = new AppProperties(app.Properties);
            CancellationToken token = properties.OnAppDisposing;

            log.Info("Starting Background Services.");

            var rpsAdapter = new RpsAdapter() { CancellationToken = token };
            DataProviderManager.Instance.PositionAdapters.Add(rpsAdapter);
            DataProviderManager.Instance.Startup(token);

            if (token != CancellationToken.None)
            {
                token.Register(() =>
                {
                    log.Info("Application is stopping.");
                });
            }
        }
    }
}