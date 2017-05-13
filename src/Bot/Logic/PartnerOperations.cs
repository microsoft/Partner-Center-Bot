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
    using PartnerCenter.Enumerators;
    using PartnerCenter.Exceptions;
    using PartnerCenter.Models;
    using PartnerCenter.Models.CountryValidationRules;
    using PartnerCenter.Models.Customers;
    using PartnerCenter.Models.Partners;
    using PartnerCenter.Models.Subscriptions;
    using RequestContext;
    using Security;

    /// <summary>
    /// Provides the ability to perform various partner operations.
    /// </summary>
    public class PartnerOperations : IPartnerOperations
    {
        /// <summary>
        /// Provides the ability to perform partner operation using app only authentication.
        /// </summary>
        private IAggregatePartner appOperations;

        /// <summary>
        /// Provides access to core services.
        /// </summary>
        private IBotService service;

        /// <summary>
        /// Provides a way to ensure that <see cref="appOperations"/> is only being modified 
        /// by one thread at a time. 
        /// </summary>
        private object appLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="PartnerOperations"/> class.
        /// </summary>
        /// <param name="service">Provides access to core services.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="service"/> is null.
        /// </exception>
        public PartnerOperations(IBotService service)
        {
            service.AssertNotNull(nameof(service));
            this.service = service;
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

                rules = await operations.CountryValidationRules.ByCountry(countryCode).GetAsync();

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

                service.Telemetry.TrackEvent("GetCountryValidationRulesAsync", eventProperties, eventMetrics);

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
            return await GetCustomerAsync(principal, principal.Operation.CustomerId);
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

                if (principal.CustomerId.Equals(service.Configuration.PartnerCenterApplicationTenantId))
                {
                    customer = await operations.Customers.ById(customerId).GetAsync();
                }
                else
                {
                    customer = await operations.Customers.ById(principal.CustomerId).GetAsync();
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

                service.Telemetry.TrackEvent("GetCustomerAsync", eventProperties, eventMetrics);

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

                if (principal.CustomerId.Equals(service.Configuration.PartnerCenterApplicationTenantId))
                {
                    seekCustomers = await operations.Customers.GetAsync();
                    customersEnumerator = operations.Enumerators.Customers.Create(seekCustomers);

                    while (customersEnumerator.HasValue)
                    {
                        customers.AddRange(customersEnumerator.Current.Items);
                        await customersEnumerator.NextAsync();
                    }
                }
                else
                {
                    customer = await operations.Customers.ById(principal.CustomerId).GetAsync();
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

                service.Telemetry.TrackEvent("GetCustomersAsync", eventProperties, eventMetrics);

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
                if (service.Initialized)
                {
                    throw new InvalidOperationException(Resources.ServiceInitializedException);
                }

                startTime = DateTime.Now;
                correlationId = Guid.NewGuid();
                operations = await GetAppOperationsAsync(correlationId);

                profile = await operations.Profiles.LegalBusinessProfile.GetAsync();

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

                service.Telemetry.TrackEvent("GetLegalBusinessProfileAsync", eventProperties, eventMetrics);

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
                        .Subscriptions.ById(principal.Operation.SubscriptionId).GetAsync();
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

                service.Telemetry.TrackEvent("GetSubscriptionAsync", eventProperties, eventMetrics);

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

                if (principal.CustomerId.Equals(service.Configuration.PartnerCenterApplicationTenantId))
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

                service.Telemetry.TrackEvent("GetCustomersAsync", eventProperties, eventMetrics);

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
                IPartnerCredentials credentials = await service.TokenManagement
                    .GetPartnerCenterAppOnlyCredentialsAsync(
                        $"{service.Configuration.ActiveDirectoryEndpoint}/{service.Configuration.PartnerCenterApplicationTenantId}");

                PartnerService.Instance.ApplicationName = BotConstants.ApplicationName;

                lock (appLock)
                {
                    appOperations = PartnerService.Instance.CreatePartnerOperations(credentials);
                }
            }

            return appOperations.With(RequestContextFactory.Instance.Create(correlationId));
        }
    }
}