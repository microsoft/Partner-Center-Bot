// -----------------------------------------------------------------------
// <copyright file="IPartnerOperations.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Logic
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using PartnerCenter.Models.CountryValidationRules;
    using PartnerCenter.Models.Customers;
    using PartnerCenter.Models.Partners;
    using PartnerCenter.Models.Subscriptions;
    using Security;

    /// <summary>
    /// Represents the ability to perform various partner operations.
    /// </summary>
    public interface IPartnerOperations
    {
        /// <summary>
        /// Gets the country validation rules for the specified country.
        /// </summary>
        /// <param name="countryCode">The country ISO2 code.</param>
        /// <returns>
        /// An instance of <see cref="CountryValidationRules"/> that represents the country validation rules for the specified country.
        /// </returns>
        Task<CountryValidationRules> GetCountryValidationRulesAsync(string countryCode);

        /// <summary>
        /// Gets the customer specified in the operation context.
        /// </summary>
        /// <param name="principal">Security principal for the calling user.</param>
        /// <returns>An instance of <see cref="Customer"/> that represents the specified customer.</returns>
        Task<Customer> GetCustomerAsync(CustomerPrincipal principal);

        /// <summary>
        /// Gets the specified customer.
        /// </summary>
        /// <param name="principal">Security principal for the calling user.</param>
        /// <param name="customerId">Identifier for the customer.</param>
        /// <returns>An instance of <see cref="Customer"/> that represents the specified customer.</returns>
        Task<Customer> GetCustomerAsync(CustomerPrincipal principal, string customerId);

        /// <summary>
        /// Gets the available customers.
        /// </summary>
        /// <param name="principal">Security principal for the calling user.</param>
        /// <returns>A list of available customers.</returns>
        Task<List<Customer>> GetCustomersAsync(CustomerPrincipal principal);

        /// <summary>
        /// Gets the legal business profile for the configured partner.
        /// </summary>
        /// <returns>An instance of <see cref="LegalBusinessProfile"/> that represents the partner's legal business profile.</returns>
        Task<LegalBusinessProfile> GetLegalBusinessProfileAsync();

        /// <summary>
        /// Gets the available subscriptions.
        /// </summary>
        /// <param name="principal">Security principal for the calling user.</param>
        /// <returns>A list of available subscriptions.</returns>
        Task<List<Subscription>> GetSubscriptionsAsync(CustomerPrincipal principal);
    }
}