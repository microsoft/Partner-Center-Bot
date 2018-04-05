// -----------------------------------------------------------------------
// <copyright file="ServiceCommunications.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;
    using Newtonsoft.Json;
    using Rest;

    public class ServiceCommunications : ServiceClient<ServiceCommunications>, IServiceCommuncations
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceCommunications" /> class.
        /// </summary>
        /// <param name="credentials">Credentials used when accessing resources.</param>
        /// <param name="handlers">List of handlers from top to bottom (outer handler is the first in the list)</param>
        public ServiceCommunications(ServiceClientCredentials credentials, params DelegatingHandler[] handlers) : base(handlers)
        {
            Credentials = credentials;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceCommunications" /> class.
        /// </summary>
        /// <param name="endpoint">Address of the resource being accessed.</param>
        /// <param name="credentials">Credentials used when accessing resources.</param>
        /// <param name="handlers">List of handlers from top to bottom (outer handler is the first in the list)</param>
        public ServiceCommunications(Uri endpoint, ServiceClientCredentials credentials, params DelegatingHandler[] handlers) : base(handlers)
        {
            Credentials = credentials;
            Endpoint = endpoint;
        }

        /// <summary>
        /// Gets or sets the credentials used when accessing resources.
        /// </summary>
        public ServiceClientCredentials Credentials { get; private set; }

        /// <summary>
        /// Gets or sets the address of the resource being accessed.
        /// </summary>
        public Uri Endpoint { get; private set; }

        public async Task<List<OfficeHealthEvent>> GetHealthEventsAsync(string customerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            HttpResponseMessage response = null;
            string content;

            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(Endpoint, $"/api/v1.0/{customerId}/ServiceComms/CurrentStatus")))
                {
                    await Credentials.ProcessHttpRequestAsync(request, cancellationToken).ConfigureAwait(false);

                    response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                    content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<ODataResponse<OfficeHealthEvent>>(content).Value;
                }
            }
            finally
            {
                response = null;
            }
        }
    }
}