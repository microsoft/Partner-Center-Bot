// -----------------------------------------------------------------------
// <copyright file="PartnerOperations.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cache;
    using Microsoft.Store.PartnerCenter.Extensions;
    using PartnerCenter.Enumerators;
    using PartnerCenter.Exceptions;
    using PartnerCenter.Models;
    using PartnerCenter.Models.CountryValidationRules;
    using PartnerCenter.Models.Customers;
    using PartnerCenter.Models.Partners;
    using PartnerCenter.Models.Subscriptions;
    using Providers;
    using RequestContext;
    using Bot.Extensions;
    using Security;

    /// <summary>
    /// Provides the ability to perform various partner operations.
    /// </summary>
    public class PartnerOperations : IPartnerOperations
    {
        /// <summary>
        /// Name of the application calling the Partner Center Managed API.
        /// </summary>
        private const string ApplicationName = "Partner Center Bot v0.2";

        /// <summary>
        /// Key utilized to retrieve and store Partner Center access tokens. 
        /// </summary>
        private const string PartnerCenterCacheKey = "Resource::PartnerCenter::AppOnly";

        /// <summary>
        /// Provides the ability to perform partner operation using app only authentication.
        /// </summary>
        private IAggregatePartner appOperations;

        /// <summary>
        /// Provides access to core services.
        /// </summary>
        private IBotProvider provider;

        /// <summary>
        /// Provides a way to ensure that <see cref="appOperations"/> is only being modified 
        /// by one thread at a time. 
        /// </summary>
        private object appLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="PartnerOperations"/> class.
        /// </summary>
        /// <param name="provider">Provides access to core services.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="provider"/> is null.
        /// </exception>
        public PartnerOperations(IBotProvider provider)
        {
            provider.AssertNotNull(nameof(provider));
            this.provider = provider;
        }

        /// <summary>
        /// Gets the country validation rules for the specified country.
        /// </summary>
        /// <param name="countryCode">The country ISO2 code.</param>
        /// <returns>
        /// An instance of <see cref="CountryValidationRules"/> that represents the country validation rules for the specified country.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="countryCode"/> is empty or null.
        /// </exception>
        public async Task<CountryValidationRules> GetCountryValidationRulesAsync(string countryCode)
        {
            CountryValidationRules rules;
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            Guid correlationId;
            IPartner operations;

            countryCode.AssertNotEmpty(nameof(countryCode));

            try
            {
                startTime = DateTime.Now;
                correlationId = Guid.NewGuid();
                operations = await GetAppOperationsAsync(correlationId);

                rules = await operations.CountryValidationRules.ByCountry(countryCode).GetAsync().ConfigureAwait(false);

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "Country", countryCode },
                    { "ParternCenterCorrelationId", correlationId.ToString() }
                };

                provider.Telemetry.TrackEvent(nameof(GetCountryValidationRulesAsync), eventProperties, eventMetrics);

                return rules;
            }
            finally
            {
                eventMetrics = null;
                eventProperties = null;
                operations = null;
            }
        }

        /// <summary>
        /// Gets the customer specified in the operation context.
        /// </summary>
        /// <param name="principal">Security principal for the calling user.</param>
        /// <returns>An instance of <see cref="Customer"/> that represents the specified customer.</returns>
        public async Task<Customer> GetCustomerAsync(CustomerPrincipal principal)
        {
            return await GetCustomerAsync(principal, principal.Operation.CustomerId).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the specified customer.
        /// </summary>
        /// <param name="principal">Security principal for the calling user.</param>
        /// <param name="customerId">Identifier for the customer.</param>
        /// <returns>An instance of <see cref="Customer"/> that represents the specified customer.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="customerId"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="principal"/> is null.
        /// </exception>
        public async Task<Customer> GetCustomerAsync(CustomerPrincipal principal, string customerId)
        {
            Customer customer;
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            Guid correlationId;
            IPartner operations;

            customerId.AssertNotEmpty(nameof(customerId));
            principal.AssertNotNull(nameof(principal));

            try
            {
                startTime = DateTime.Now;
                correlationId = Guid.NewGuid();
                operations = await GetAppOperationsAsync(correlationId);

                if (principal.CustomerId.Equals(provider.Configuration.PartnerCenterAccountId))
                {
                    customer = await operations.Customers.ById(customerId).GetAsync().ConfigureAwait(false);
                }
                else
                {
                    customer = await operations.Customers.ById(principal.CustomerId).GetAsync().ConfigureAwait(false);
                }

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "CustomerId", principal.CustomerId },
                    { "Name", principal.Name },
                    { "ParternCenterCorrelationId", correlationId.ToString() }
                };

                provider.Telemetry.TrackEvent(nameof(GetCustomerAsync), eventProperties, eventMetrics);

                return customer;
            }
            finally
            {
                eventMetrics = null;
                eventProperties = null;
                operations = null;
                principal = null;
            }
        }

        /// <summary>
        /// Gets the available customers.
        /// </summary>
        /// <param name="principal">Security principal for the calling user.</param>
        /// <returns>A list of available customers.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="principal"/> is null.
        /// </exception>
        public async Task<List<Customer>> GetCustomersAsync(CustomerPrincipal principal)
        {
            Customer customer;
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            Guid correlationId;
            IPartner operations;
            IResourceCollectionEnumerator<SeekBasedResourceCollection<Customer>> customersEnumerator;
            List<Customer> customers;
            SeekBasedResourceCollection<Customer> seekCustomers;

            principal.AssertNotNull(nameof(principal));

            try
            {
                startTime = DateTime.Now;
                correlationId = Guid.NewGuid();
                operations = await GetAppOperationsAsync(correlationId);

                customers = new List<Customer>();

                if (principal.CustomerId.Equals(provider.Configuration.PartnerCenterAccountId))
                {
                    seekCustomers = await operations.Customers.GetAsync().ConfigureAwait(false);
                    customersEnumerator = operations.Enumerators.Customers.Create(seekCustomers);

                    while (customersEnumerator.HasValue)
                    {
                        customers.AddRange(customersEnumerator.Current.Items);
                        await customersEnumerator.NextAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    customer = await operations.Customers.ById(principal.CustomerId).GetAsync().ConfigureAwait(false);
                    customers.Add(customer);
                }

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds },
                    { "NumberOfCustomers", customers.Count }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "CustomerId", principal.CustomerId },
                    { "Name", principal.Name },
                    { "ParternCenterCorrelationId", correlationId.ToString() }
                };

                provider.Telemetry.TrackEvent(nameof(GetCustomersAsync), eventProperties, eventMetrics);

                return customers;
            }
            finally
            {
                customersEnumerator = null;
                eventMetrics = null;
                eventProperties = null;
                operations = null;
                principal = null;
                seekCustomers = null;
            }
        }

        /// <summary>
        /// Gets the legal business profile for the configured partner.
        /// </summary>
        /// <returns>An instance of <see cref="LegalBusinessProfile"/> that represents the partner's legal business profile.</returns>
        /// <exception cref="InvalidOperationException">
        /// Cannot invoke this function after the bot service has been initialized. 
        /// </exception>
        public async Task<LegalBusinessProfile> GetLegalBusinessProfileAsync()
        {
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            Guid correlationId;
            IPartner operations;
            LegalBusinessProfile profile;

            try
            {
                if (provider.Initialized)
                {
                    throw new InvalidOperationException(Resources.ServiceInitializedException);
                }

                startTime = DateTime.Now;
                correlationId = Guid.NewGuid();
                operations = await GetAppOperationsAsync(correlationId);

                profile = await operations.Profiles.LegalBusinessProfile.GetAsync().ConfigureAwait(false);

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "ParternCenterCorrelationId", correlationId.ToString() }
                };

                provider.Telemetry.TrackEvent(nameof(GetLegalBusinessProfileAsync), eventProperties, eventMetrics);

                return profile;
            }
            finally
            {
                eventMetrics = null;
                eventProperties = null;
                operations = null;
            }
        }

        /// <summary>
        /// Gets the specified subscription.
        /// </summary>
        /// <param name="principal">Security principal for the calling user.</param>
        /// <returns>An instance of <see cref="Subscription"/> that represents the specified subscription.</returns>
        /// <exception cref="ArgumentException">
        /// The operation context from <paramref name="principal"/> does not contain a valid customer identifier.
        /// or
        /// The operation context from <paramref name="principal"/> does not contain a valid subscription identifier.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="principal"/> is null.
        /// </exception>
        public async Task<Subscription> GetSubscriptionAsync(CustomerPrincipal principal)
        {
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            Guid correlationId;
            IPartner operations;
            Subscription subscription = null;

            principal.AssertNotNull(nameof(principal));
            principal.Operation.CustomerId.AssertNotEmpty(nameof(principal.Operation.CustomerId));
            principal.Operation.SubscriptionId.AssertNotEmpty(nameof(principal.Operation.SubscriptionId));

            try
            {
                startTime = DateTime.Now;
                correlationId = Guid.NewGuid();
                operations = await GetAppOperationsAsync(correlationId);

                try
                {
                    subscription = await operations.Customers.ById(principal.Operation.CustomerId)
                        .Subscriptions.ById(principal.Operation.SubscriptionId).GetAsync().ConfigureAwait(false);
                }
                catch (PartnerException ex)
                {
                    if (ex.ErrorCategory != PartnerErrorCategory.NotFound)
                    {
                        throw;
                    }
                }

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "CustomerId", principal.CustomerId },
                    { "ObjectId", principal.ObjectId },
                    { "ParternCenterCorrelationId", correlationId.ToString() }
                };

                provider.Telemetry.TrackEvent("GetSubscriptionAsync", eventProperties, eventMetrics);

                return subscription;
            }
            finally
            {
                eventMetrics = null;
                eventProperties = null;
                operations = null;
            }
        }

        /// <summary>
        /// Gets the available subscriptions.
        /// </summary>
        /// <param name="principal">Security principal for the calling user.</param>
        /// <returns>A list of available subscriptions.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="principal"/> is null.
        /// </exception>
        public async Task<List<Subscription>> GetSubscriptionsAsync(CustomerPrincipal principal)
        {
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            Guid correlationId;
            IPartner operations;
            ResourceCollection<Subscription> subscriptions;

            principal.AssertNotNull(nameof(principal));

            try
            {
                startTime = DateTime.Now;
                correlationId = Guid.NewGuid();
                operations = await GetAppOperationsAsync(correlationId);

                if (principal.CustomerId.Equals(provider.Configuration.PartnerCenterAccountId))
                {
                    principal.AssertValidCustomerContext(Resources.InvalidCustomerContextException);
                    subscriptions = await operations.Customers.ById(principal.Operation.CustomerId).Subscriptions.GetAsync();
                }
                else
                {
                    subscriptions = await operations.Customers.ById(principal.CustomerId).Subscriptions.GetAsync();
                }

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds },
                    { "NumberOfSubscriptions", subscriptions.TotalCount }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "CustomerId", principal.CustomerId },
                    { "Name", principal.Name },
                    { "ParternCenterCorrelationId", correlationId.ToString() }
                };

                provider.Telemetry.TrackEvent("GetCustomersAsync", eventProperties, eventMetrics);

                return new List<Subscription>(subscriptions.Items);
            }
            finally
            {
                eventMetrics = null;
                eventProperties = null;
                operations = null;
                principal = null;
                subscriptions = null;
            }
        }

        /// <summary>
        /// Gets an instance of the partner service that utilizes app only authentication.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for the operation.</param>
        /// <returns>An instance of the partner service.</returns>
        private async Task<IPartner> GetAppOperationsAsync(Guid correlationId)
        {
            if (appOperations == null || appOperations.Credentials.ExpiresAt > DateTime.UtcNow)
            {
                IPartnerCredentials credentials = await GetPartnerCenterCredentialsAsync().ConfigureAwait(false);

                lock (appLock)
                {
                    appOperations = PartnerService.Instance.CreatePartnerOperations(credentials);
                }

                PartnerService.Instance.ApplicationName = ApplicationName;
            }

            // TODO -- Add localization
            // return appOperations.With(RequestContextFactory.Instance.Create(correlationId, service.Localization.Locale));
            return appOperations.With(RequestContextFactory.Instance.Create(correlationId));
        }

        /// <summary>
        /// Gets an instance of <see cref="IPartnerCredentials"/> used to access the Partner Center Managed API.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <returns>
        /// An instance of <see cref="IPartnerCredentials" /> that represents the access token.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// </exception>
        private async Task<IPartnerCredentials> GetPartnerCenterCredentialsAsync()
        {
            // Attempt to obtain the Partner Center token from the cache.
            IPartnerCredentials credentials =
                 await provider.Cache.FetchAsync<Models.PartnerCenterToken>(
                     CacheDatabaseType.Authentication, PartnerCenterCacheKey).ConfigureAwait(false);

            if (credentials != null && !credentials.IsExpired())
            {
                return credentials;
            }

            // The access token has expired, so a new one must be requested.
            credentials = await PartnerCredentials.Instance.GenerateByApplicationCredentialsAsync(
                provider.Configuration.PartnerCenterApplicationId,
                provider.Configuration.PartnerCenterApplicationSecret.ToUnsecureString(),
                provider.Configuration.PartnerCenterAccountId).ConfigureAwait(false);

            await provider.Cache.StoreAsync(CacheDatabaseType.Authentication, PartnerCenterCacheKey, credentials).ConfigureAwait(false);

            return credentials;
        }
    }
}