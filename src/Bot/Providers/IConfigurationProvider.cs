// -----------------------------------------------------------------------
// <copyright file="IConfigurationProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Providers
{
    using System.Security;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents the ability to reference various configurations. 
    /// </summary>
    public interface IConfigurationProvider
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
        SecureString ApplicationSecret { get; }

        /// <summary>
        /// Gets the Azure AD application tenant identifier.
        /// </summary>
        string ApplicationTenantId { get; }

        /// <summary>
        /// Gets the access key for the instance of Cosmos Db.
        /// </summary>
        SecureString CosmosDbAccessKey { get; }

        /// <summary>
        /// Gets the Azure Cosmos DB endpoint address.
        /// </summary>
        string CosmosDbEndpoint { get; }

        /// <summary>
        /// Gets the Microsoft Graph endpoint.
        /// </summary>
        string GraphEndpoint { get; }

        /// <summary>
        /// Gets the Application Insights instrumentation key.
        /// </summary>
        string InstrumentationKey { get; }

        /// <summary>
        /// Gets the endpoint address for the instance of Azure Key Vault.
        /// </summary>
        string KeyVaultEndpoint { get; }

        /// <summary>
        /// Gets the LUIS API key.
        /// </summary>
        SecureString LuisApiKey { get; }

        /// <summary>
        /// Gets the LUIS application identifier.
        /// </summary>
        string LuisAppId { get; }

        /// <summary>
        /// Gets the Microsoft application identifier.
        /// </summary>
        string MicrosoftAppId { get; }

        /// <summary>
        /// Gets the Microsoft application password.
        /// </summary>
        SecureString MicrosoftAppPassword { get; }

        /// <summary>
        /// Gets the Office 365 management endpoint address.
        /// </summary>
        string OfficeManagementEndpoint { get; }

        /// <summary>
        /// Gets the Partner Center account identifier. 
        /// </summary>
        string PartnerCenterAccountId { get; }

        /// <summary>
        /// Gets the Partner Center application identifier value.
        /// </summary>
        string PartnerCenterApplicationId { get; }

        /// <summary>
        /// Gets the Partner Center application secret value.
        /// </summary>
        SecureString PartnerCenterApplicationSecret { get; }

        /// <summary>
        /// Gets the Partner Center API endpoint.
        /// </summary>
        string PartnerCenterEndpoint { get; }

        /// <summary>
        /// Gets the question and answer knowledgebase identifier.
        /// </summary>
        string QnAKnowledgebaseId { get; }

        /// <summary>
        /// Gets question and answer subscription subscription key.
        /// </summary>
        SecureString QnASubscriptionKey { get; }

        /// <summary>
        /// Gets the Redis Cache connection string.
        /// </summary>
        SecureString RedisCacheConnectionString { get; }

        /// <summary>
        /// Performs the necessary initialization operations.
        /// </summary>
        /// <returns>An instance of the <see cref="Task"/> class that represents the asynchronous operation.</returns>
        Task InitializeAsync();
    }
}