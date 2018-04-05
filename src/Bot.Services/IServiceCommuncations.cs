// -----------------------------------------------------------------------
// <copyright file="IServiceCommunications.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Rest;
    using Models;

    public interface IServiceCommuncations
    {
        /// <summary>
        /// Gets or sets the credentials used when accessing resources.
        /// </summary>
        ServiceClientCredentials Credentials { get; }

        /// <summary>
        /// Gets or sets the address of the resource being accessed.
        /// </summary>
        Uri Endpoint { get; }

        Task<List<OfficeHealthEvent>> GetHealthEventsAsync(string customerId, CancellationToken cancellationToken = default(CancellationToken));
    }
}