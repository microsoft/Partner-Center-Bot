// -----------------------------------------------------------------------
// <copyright file="BotProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Providers
{
    using System;
    using System.Threading.Tasks;
    using Intents;
    using Logic;
    using Telemetry;

    /// <summary>
    /// Provides access to core services.
    /// </summary>
    /// <seealso cref="IBotProvider" />
    [Serializable]
    public class BotProvider : IBotProvider
    {
        /// <summary>
        /// Provides the ability to manage access tokens.
        /// </summary>
        private static IAccessTokenProvider accessToken;

        /// <summary>
        /// Provides the ability to cache often used objects. 
        /// </summary>
        private static ICacheProvider cache;

        /// <summary>
        /// Provides the ability to access various configurations.
        /// </summary>
        private static IConfigurationProvider configuration;

        /// <summary>
        /// A flag indicating whether or the service has been initialized. 
        /// </summary>
        private static bool initialized = false;

        /// <summary>
        /// Provides access to the supported intents.
        /// </summary>
        private static IIntentService intent;

        /// <summary>
        /// Provides localization functionality for the bot.
        /// </summary>
        private static ILocalizationProvider localization;

        /// <summary>
        /// Provides the ability to perform various partner operations.
        /// </summary>
        private static IPartnerOperations partnerOperations;
        
        /// <summary>
        /// Provides the ability to track telemetry data.
        /// </summary>
        private static ITelemetryProvider telemetry;

        /// <summary>
        /// Provides the ability to retrieve and store data in a secure vault.
        /// </summary>
        private static IVaultProvider vault;

        /// <summary>
        /// Gets the a reference to the token management service.
        /// </summary>
        public IAccessTokenProvider AccessToken => accessToken ?? (accessToken = new AccessTokenProvider(this));

        /// <summary>
        /// Gets the service that provides caching functionality.
        /// </summary>
        public ICacheProvider Cache => cache ?? (cache = new RedisCacheProvider(this));

        /// <summary>
        /// Gets a reference to the available configurations.
        /// </summary>
        public IConfigurationProvider Configuration => configuration ?? (configuration = new ConfigurationProvider(this));

        /// <summary>
        /// Gets a flag indicating whether or the service has been initialized.
        /// </summary>
        public bool Initialized => initialized;

        /// <summary>
        /// Gets the service that provides access to the supported intents.
        /// </summary>
        public IIntentService Intent => intent;

        /// <summary>
        /// Gets the service that provide localization functionality.
        /// </summary>
        public ILocalizationProvider Localization => localization;

        /// <summary>
        /// Gets a reference to the partner operations.
        /// </summary>
        public IPartnerOperations PartnerOperations => partnerOperations ?? (partnerOperations = new PartnerOperations(this));

        /// <summary>
        /// Gets the telemetry service reference.
        /// </summary>
        public ITelemetryProvider Telemetry
        {
            get
            {
                if (telemetry != null)
                {
                    return telemetry;
                }

                if (string.IsNullOrEmpty(Configuration.InstrumentationKey))
                {
                    telemetry = new EmptyTelemetryProvider();
                }
                else
                {
                    telemetry = new ApplicationInsightsTelemetryProvider();
                }

                return telemetry;
            }
        }

        /// <summary>
        /// Gets a reference to the vault service.
        /// </summary>
        public IVaultProvider Vault => vault ?? (vault = new KeyVaultProvider(this));

        /// <summary>
        /// Initializes the application core services.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        public async Task InitializeAsync()
        {
            intent = new IntentService(this);
            localization = new LocalizationProvider(this);

            intent.Initialize();

            await Configuration.InitializeAsync().ConfigureAwait(false);
            await localization.InitializeAsync().ConfigureAwait(false);
        }
    }
}