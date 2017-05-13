// -----------------------------------------------------------------------
// <copyright file="IServiceCommunications.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Logic.Office
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents the ability to interact with Office 365 Service Communications API.
    /// </summary>
    public interface IServiceCommunications
    {
        /// <summary>
        /// Gets the current status information for the specified customer.
        /// </summary>
        /// <param name="customerId">Identifier for the customer.</param>
        /// <param name="token">Access token to be utilized in the request.</param>
        /// <returns>A list of <see cref="HealthEvent"/> objects that represent the current health events for the specified customer.</returns>
        Task<List<HealthEvent>> GetCurrentStatusAsync(string customerId, string token);
    }
}