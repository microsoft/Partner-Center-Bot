// -----------------------------------------------------------------------
// <copyright file="IBotProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Providers
{
    using System.Threading.Tasks;
    using Intents;
    using Logic;
    using Logic.Office;
    using Telemetry;

    /// <summary>
    /// Represents the core service that powers the application.
    /// </summary>
    public interface IBotProvider
    {
        /// <summary>
        /// Gets the a reference to the token management service.
        /// </summary>
        IAccessTokenProvider AccessToken { get; }

        /// <summary>
        /// Gets the service that provides caching functionality.
        /// </summary>
        ICacheProvider Cache { get; }

        /// <summary>
        /// Gets a reference to the available configurations.
        /// </summary>
        IConfigurationProvider Configuration { get; }

        /// <summary>
        /// Gets a value indicating whether or the service has been initialized.
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// Gets the service that provides access to the supported intents.
        /// </summary>
        IIntentService Intent { get; }

        /// <summary>
        /// Gets the service that provides localization functionality.
        /// </summary>
        ILocalizationProvider Localization { get; }

        /// <summary>
        /// Gets a reference to the partner operations.
        /// </summary>
        IPartnerOperations PartnerOperations { get; }

        /// <summary>
        /// Gets a reference t the service communications service.
        /// </summary>
        IServiceCommunications ServiceCommunications { get; }

        /// <summary>
        /// Gets the telemetry service reference.
        /// </summary>
        ITelemetryProvider Telemetry { get; }

        /// <summary>
        /// Gets a reference to the vault service. 
        /// </summary>
        IVaultProvider Vault { get; }

        /// <summary>
        /// Initializes the bot service and all the dependent services.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task InitializeAsync();
    }
}