// -----------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot
{
    using System.Reflection;
    using System.Web.Http;
    using Autofac;
    using Autofac.Integration.WebApi;
    using Logic;

    /// <summary>
    /// Defines the methods and properties that are common to application objects.
    /// </summary>
    public class WebApiApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Gets the container for the application.
        /// </summary>
        internal static IContainer Container { get; private set; }

        /// <summary>
        /// Logic required to start the application.
        /// </summary>
        protected void Application_Start()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<BotService>().As<IBotService>().AsImplementedInterfaces();

            Container = builder.Build();

            using (ILifetimeScope scope = Container.BeginLifetimeScope())
            {
                IBotService service = scope.Resolve<IBotService>();

                ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey =
                    service.Configuration.InstrumentationKey;

                service.InitializeAsync().Wait();
            }

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}