// -----------------------------------------------------------------------
// <copyright file="ServiceCommunications.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Logic.Office
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Extensions;
    using Newtonsoft.Json;
    using Providers;

    /// <summary>
    /// Provides the ability to interact with Office 365 Service Communications API.
    /// </summary>
    public class ServiceCommunications : IServiceCommunications
    {
        /// <summary>
        /// Provides access to core services.
        /// </summary>
        private readonly IBotProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceCommunications" /> class.
        /// </summary>
        /// <param name="provider">Provides access to core services.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="provider"/> is null.
        /// </exception>
        public ServiceCommunications(IBotProvider provider)
        {
            provider.AssertNotNull(nameof(provider));
            this.provider = provider;
        }

        /// <summary>
        /// Gets the current status information for the specified customer.
        /// </summary>
        /// <param name="customerId">Identifier for the customer.</param>
        /// <param name="token">Access token to be utilized in the request.</param>
        /// <returns>A list of <see cref="HealthEvent"/> objects that represent the current health events for the specified customer.</returns>
        public async Task<List<HealthEvent>> GetCurrentStatusAsync(string customerId, string token)
        {
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            HttpResponseMessage response;
            ODataResponse<HealthEvent> odataResponse;
            string content;
            string requestUri;

            try
            {
                startTime = DateTime.Now;

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    requestUri = $"{provider.Configuration.OfficeManagementEndpoint}/api/v1.0/{customerId}/ServiceComms/CurrentStatus";

                    response = await client.GetAsync(requestUri);
                    content = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new CommunicationException(content, response.StatusCode);
                    }

                    odataResponse = JsonConvert.DeserializeObject<ODataResponse<HealthEvent>>(content);

                    // Track the event measurements for analysis.
                    eventMetrics = new Dictionary<string, double>
                    {
                        { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds },
                        { "NumberOfHealthEvents", odataResponse.Value.Count }
                    };

                    // Capture the request for the customer summary for analysis.
                    eventProperties = new Dictionary<string, string>
                    {
                        { "CustomerId", customerId },
                        { "RequestUri", requestUri }
                    };

                    provider.Telemetry.TrackEvent("GetCurrentStatusAsync", eventProperties, eventMetrics);

                    return odataResponse.Value;
                }
            }
            finally
            {
                eventMetrics = null;
                eventProperties = null;
                response = null;
            }
        }
    }
}