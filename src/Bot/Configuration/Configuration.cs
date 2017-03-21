// -----------------------------------------------------------------------
// <copyright file="Configuration.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using Logic;

    /// <summary>
    /// Encapsulates configurations utilized by the bot. 
    /// </summary>
    public class Configuration : IConfiguration
    {
        /// <summary>
        /// Provides access to core services.
        /// </summary>
        private readonly IBotService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration" /> class.
        /// </summary>
        /// <param name="service">Provides access to core services.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="service"/> is null.
        /// </exception>
        public Configuration(IBotService service)
        {
            service.AssertNotNull(nameof(service));

            this.service = service;
        }

        /// <summary>
        /// Gets the Azure Active Directory endpoint.
        /// </summary>
        public string ActiveDirectoryEndpoint => ConfigurationManager.AppSettings["ActiveDirectoryEndpoint"];

        /// <summary>
        /// Gets the Azure AD application identifier.
        /// </summary>
        public string ApplicationId => ConfigurationManager.AppSettings["ApplicationId"];

        /// <summary>
        /// Gets the Azure AD application secret.
        /// </summary>
        public string ApplicationSecret => this.GetConfigurationValue("ApplicationSecret");

        /// <summary>
        /// Gets the Azure AD application tenant identifier.
        /// </summary>
        public string ApplicationTenantId => ConfigurationManager.AppSettings["ApplicationTenantId"];

        /// <summary>
        /// Gets the Microsoft Graph endpoint.
        /// </summary>
        public string GraphEndpoint => ConfigurationManager.AppSettings["MicrosoftGraphEndpoint"];

        /// <summary>
        /// Gets the Application Insights instrumentation key.
        /// </summary>
        public string InstrumentationKey => ConfigurationManager.AppSettings["InstrumentationKey"];

        /// <summary>
        /// Gets the LUIS application identifier.
        /// </summary>
        public string LuisAppId => ConfigurationManager.AppSettings["LuisAppId"];

        /// <summary>
        /// Gets the LUIS API key.
        /// </summary>
        public string LuisApiKey => this.GetConfigurationValue("LuisApiKey");

        /// <summary>
        /// Gets the Microsoft application identifier.
        /// </summary>
        public string MicrosoftAppId => ConfigurationManager.AppSettings["MicrosoftAppId"];

        /// <summary>
        /// Gets the Microsoft application password.
        /// </summary>
        public string MicrosoftAppPassword => this.GetConfigurationValue("MicrosoftAppPassword");

        /// <summary>
        /// Gets the Partner Center API endpoint.
        /// </summary>
        public string PartnerCenterEndpoint => ConfigurationManager.AppSettings["PartnerCenterEndpoint"];

        /// <summary>
        /// Gets the Partner Center application identifier value.
        /// </summary>
        public string PartnerCenterApplicationId => ConfigurationManager.AppSettings["PartnerCenterApplicationId"];

        /// <summary>
        /// Gets the Partner Center application secret value.
        /// </summary>
        public string PartnerCenterApplicationSecret => this.GetConfigurationValue("PartnerCenterApplicationSecret");

        /// <summary>
        /// Gets the Partner Center application tenant identifier.
        /// </summary>
        public string PartnerCenterApplicationTenantId => ConfigurationManager.AppSettings["PartnerCenterApplicationTenantId"];

        /// <summary>
        /// Gets the question and answer knowledgebase identifier.
        /// </summary>
        public string QnAKnowledgebaseId => ConfigurationManager.AppSettings["QnAKnowledgebaseId"];

        /// <summary>
        /// Gets question and answer subscription subscription key.
        /// </summary>
        public string QnASubscriptionKey => this.GetConfigurationValue("QnASubscriptionKey");

        /// <summary>
        /// Gets the Redis Cache connection string.
        /// </summary>
        public string RedisCacheConnectionString => this.GetConfigurationValue("RedisCacheConnectionString");

        /// <summary>
        /// Gets the vault application certificate thumbprint.
        /// </summary>
        public string VaultApplicationCertThumbprint => ConfigurationManager.AppSettings["VaultApplicationCertThumbprint"];

        /// <summary>
        /// Gets the vault application tenant identifier.
        /// </summary>
        public string VaultApplicationId => ConfigurationManager.AppSettings["VaultApplicationId"];

        /// <summary>
        /// Gets the vault application tenant identifier.
        /// </summary>
        public string VaultApplicationTenantId => ConfigurationManager.AppSettings["VaultApplicationTenantId"];

        /// <summary>
        /// Gets the base address for the vault.
        /// </summary>
        public string VaultBaseAddress => ConfigurationManager.AppSettings["VaultBaseAddress"];

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="identifier">Identifier of the resource being requested.</param>
        /// <returns>A string represented the value of the configuration.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="identifier"/> is empty or null.
        /// </exception>
        private string GetConfigurationValue(string identifier)
        {
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            string value;

            identifier.AssertNotNull(nameof(identifier));

            try
            {
                startTime = DateTime.Now;

                value = this.service.Vault.Get(identifier);

                if (string.IsNullOrEmpty(value))
                {
                    value = ConfigurationManager.AppSettings[identifier];
                }

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "Identifier", identifier }
                };

                this.service.Telemetry.TrackEvent("Configuration/GetConfigurationValue", eventProperties, eventMetrics);

                return value;
            }
            finally
            {
                eventMetrics = null;
                eventProperties = null;
            }
        }
    }
}