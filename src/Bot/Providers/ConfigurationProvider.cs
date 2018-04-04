// -----------------------------------------------------------------------
// <copyright file="ConfigurationProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Providers
{
    using System.Configuration;
    using System.Security;
    using System.Threading.Tasks;
    using Extensions;

    /// <summary>
    /// Encapsulates configurations utilized by the bot. 
    /// </summary>
    public class ConfigurationProvider : IConfigurationProvider
    {
        /// <summary>
        /// Provides access to core services.
        /// </summary>
        private readonly IBotProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationProvider" /> class.
        /// </summary>
        /// <param name="provider">Provides access to core services.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="provider"/> is null.
        /// </exception>
        public ConfigurationProvider(IBotProvider provider)
        {
            provider.AssertNotNull(nameof(provider));
            this.provider = provider;
        }

        /// <summary>
        /// Gets the Azure Active Directory endpoint.
        /// </summary>
        public string ActiveDirectoryEndpoint { get; private set; }

        /// <summary>
        /// Gets the Azure AD application identifier.
        /// </summary>
        public string ApplicationId { get; private set; }

        /// <summary>
        /// Gets the Azure AD application secret.
        /// </summary>
        public SecureString ApplicationSecret { get; private set; }

        /// <summary>
        /// Gets the Azure AD application tenant identifier.
        /// </summary>
        public string ApplicationTenantId { get; private set; }

        /// <summary>
        /// Gets the access key for the instance of Cosmos DB.
        /// </summary>
        public SecureString CosmosDbAccessKey { get; private set; }

        /// <summary>
        /// Gets the Azure Cosmos DB endpoint address.
        /// </summary>
        public string CosmosDbEndpoint { get; private set; }

        /// <summary>
        /// Gets the Microsoft Graph endpoint.
        /// </summary>
        public string GraphEndpoint { get; private set; }

        /// <summary>
        /// Gets the Application Insights instrumentation key.
        /// </summary>
        public string InstrumentationKey { get; private set; }

        /// <summary>
        /// Gets the endpoint address for the instance of Azure Key Vault.
        /// </summary>
        public string KeyVaultEndpoint { get; private set; }

        /// <summary>
        /// Gets the LUIS API key.
        /// </summary>
        public SecureString LuisApiKey { get; private set; }

        /// <summary>
        /// Gets the LUIS application identifier.
        /// </summary>
        public string LuisAppId { get; private set; }

        /// <summary>
        /// Gets the Microsoft application identifier.
        /// </summary>
        public string MicrosoftAppId { get; private set; }

        /// <summary>
        /// Gets the Microsoft application password.
        /// </summary>
        public SecureString MicrosoftAppPassword { get; private set; }

        /// <summary>
        /// Gets the Office 365 management endpoint address.
        /// </summary>
        public string OfficeManagementEndpoint { get; private set; }

        /// <summary>
        /// Gets the Partner Center account identifier.
        /// </summary>
        public string PartnerCenterAccountId { get; private set; }

        /// <summary>
        /// Gets the Partner Center API endpoint.
        /// </summary>
        public string PartnerCenterEndpoint { get; private set; }

        /// <summary>
        /// Gets the Partner Center application identifier value.
        /// </summary>
        public string PartnerCenterApplicationId { get; private set; }

        /// <summary>
        /// Gets the Partner Center application secret value.
        /// </summary>
        public SecureString PartnerCenterApplicationSecret { get; private set; }

        /// <summary>
        /// Gets the question and answer knowledgebase identifier.
        /// </summary>
        public string QnAKnowledgebaseId { get; private set; }

        /// <summary>
        /// Gets question and answer subscription subscription key.
        /// </summary>
        public SecureString QnASubscriptionKey { get; private set; }

        /// <summary>
        /// Gets the Redis Cache connection string.
        /// </summary>
        public SecureString RedisCacheConnectionString { get; private set; }

        /// <summary>
        /// Performs the necessary initialization operations.
        /// </summary>
        /// <returns>An instance of the <see cref="Task"/> class that represents the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            ActiveDirectoryEndpoint = ConfigurationManager.AppSettings["ActiveDirectoryEndpoint"];
            ApplicationId = ConfigurationManager.AppSettings["ApplicationId"];
            ApplicationTenantId = ConfigurationManager.AppSettings["ApplicationTenantId"];
            CosmosDbEndpoint = ConfigurationManager.AppSettings["CosmosDbEndpoint"];
            GraphEndpoint = ConfigurationManager.AppSettings["MicrosoftGraphEndpoint"];
            InstrumentationKey = ConfigurationManager.AppSettings["InstrumentationKey"];
            KeyVaultEndpoint = ConfigurationManager.AppSettings["KeyVaultEndpoint"];
            LuisAppId = ConfigurationManager.AppSettings["LuisAppId"];
            MicrosoftAppId = ConfigurationManager.AppSettings["MicrosoftAppId"];
            OfficeManagementEndpoint = ConfigurationManager.AppSettings["OfficeManagementEndpoint"];
            PartnerCenterAccountId = ConfigurationManager.AppSettings["PartnerCenter.AccountId"];
            PartnerCenterEndpoint = ConfigurationManager.AppSettings["PartnerCenterEndpoint"];
            PartnerCenterApplicationId = ConfigurationManager.AppSettings["PartnerCenter.ApplicationId"];
            QnAKnowledgebaseId = ConfigurationManager.AppSettings["QnAKnowledgebaseId"];

            ApplicationSecret = await provider.Vault.GetAsync("ApplicationSecret").ConfigureAwait(false);
            CosmosDbAccessKey = await provider.Vault.GetAsync("CosmosDbAccessKey").ConfigureAwait(false);
            LuisApiKey = await provider.Vault.GetAsync("LuisApiKey").ConfigureAwait(false);
            MicrosoftAppPassword = await provider.Vault.GetAsync("MicrosoftAppPassword").ConfigureAwait(false);
            PartnerCenterApplicationSecret = await provider.Vault.GetAsync("PartnerCenterApplicationSecret").ConfigureAwait(false);
            QnASubscriptionKey = await provider.Vault.GetAsync("QnASubscriptionKey").ConfigureAwait(false);
            RedisCacheConnectionString = await provider.Vault.GetAsync("RedisCacheConnectionString").ConfigureAwait(false);
        }
    }
}