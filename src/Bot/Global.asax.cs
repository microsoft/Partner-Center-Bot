// -----------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot
{
    using System;
    using System.Reflection;
    using System.Web.Http;
    using Autofac;
    using Autofac.Integration.WebApi;
    using Providers;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Builder.Azure;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using System.Threading.Tasks;
    using Extensions;

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
            RegisterContainer();
            GlobalConfiguration.Configure(WebApiConfig.Register);

            using (ILifetimeScope scope = Container.BeginLifetimeScope())
            {
                IBotProvider provider = scope.Resolve<IBotProvider>();

                Task.Run(() => provider.InitializeAsync()).Wait();

                ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey =
                    provider.Configuration.InstrumentationKey;

                Conversation.UpdateContainer(
                    builder =>
                    {
                        builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));

                        DocumentDbBotDataStore store = new DocumentDbBotDataStore(
                            new Uri(provider.Configuration.CosmosDbEndpoint),
                            provider.Configuration.CosmosDbAccessKey.ToUnsecureString());

                        builder.Register(c =>
                        {
                            return new MicrosoftAppCredentials(
                                provider.Configuration.MicrosoftAppId,
                                provider.Configuration.MicrosoftAppPassword.ToUnsecureString());
                        }).SingleInstance();

                        builder.Register(c => store)
                            .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                            .AsSelf()
                            .SingleInstance();

                        builder.Register(c => new CachingBotDataStore(store, CachingBotDataStoreConsistencyPolicy.ETagBasedConsistency))
                            .As<IBotDataStore<BotData>>()
                            .AsSelf()
                            .InstancePerLifetimeScope();
                    });
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception;
            IBotProvider provider;

            try
            {
                using (ILifetimeScope scope = Container.BeginLifetimeScope())
                {
                    exception = Server.GetLastError();
                    provider = scope.Resolve<IBotProvider>();

                    provider.Telemetry.TrackException(exception);
                }
            }
            finally
            {
                exception = null;
                provider = null;
            }
        }

        /// <summary>
        /// Registers the container for the web application.
        /// </summary>
        private void RegisterContainer()
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<BotProvider>().As<IBotProvider>().AsImplementedInterfaces();

            Container = builder.Build();
        }
    }
}