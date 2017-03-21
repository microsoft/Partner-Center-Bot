// -----------------------------------------------------------------------
// <copyright file="IConfiguration.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Configuration
{
    /// <summary>
    /// Represents the ability to reference various configurations. 
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Gets the Azure Active Directory endpoint.
        /// </summary>
        string ActiveDirectoryEndpoint { get; }

        /// <summary>
        /// Gets the Azure AD application identifier.
        /// </summary>
        string ApplicationId { get; }

        /// <summary>
        /// Gets the Azure AD application secret.
        /// </summary>
        string ApplicationSecret { get; }

        /// <summary>
        /// Gets the Azure AD application tenant identifier.
        /// </summary>
        string ApplicationTenantId { get; }

        /// <summary>
        /// Gets the Microsoft Graph endpoint.
        /// </summary>
        string GraphEndpoint { get; }

        /// <summary>
        /// Gets the Application Insights instrumentation key.
        /// </summary>
        string InstrumentationKey { get; }

        /// <summary>
        /// Gets the LUIS application identifier.
        /// </summary>
        string LuisAppId { get; }

        /// <summary>
        /// Gets the LUIS API key.
        /// </summary>
        string LuisApiKey { get; }

        /// <summary>
        /// Gets the Microsoft application identifier.
        /// </summary>
        string MicrosoftAppId { get; }

        /// <summary>
        /// Gets the Microsoft application password.
        /// </summary>
        string MicrosoftAppPassword { get; }

        /// <summary>
        /// Gets the Partner Center API endpoint.
        /// </summary>
        string PartnerCenterEndpoint { get; }

        /// <summary>
        /// Gets the Partner Center application identifier value.
        /// </summary>
        string PartnerCenterApplicationId { get; }

        /// <summary>
        /// Gets the Partner Center application secret value.
        /// </summary>
        string PartnerCenterApplicationSecret { get; }

        /// <summary>
        /// Gets the Partner Center application tenant identifier. 
        /// </summary>
        string PartnerCenterApplicationTenantId { get; }

        /// <summary>
        /// Gets the question and answer knowledgebase identifier.
        /// </summary>
        string QnAKnowledgebaseId { get; }

        /// <summary>
        /// Gets question and answer subscription subscription key.
        /// </summary>
        string QnASubscriptionKey { get; }

        /// <summary>
        /// Gets the Redis Cache connection string.
        /// </summary>
        string RedisCacheConnectionString { get; }

        /// <summary>
        /// Gets the vault application certificate thumbprint.
        /// </summary>
        string VaultApplicationCertThumbprint { get; }

        /// <summary>
        /// Gets the vault application identifier.
        /// </summary>
        string VaultApplicationId { get; }

        /// <summary>
        /// Gets the vault application tenant identifier.
        /// </summary>
        string VaultApplicationTenantId { get; }

        /// <summary>
        /// Gets the base address for the vault.
        /// </summary>
        string VaultBaseAddress { get; }
    }
}