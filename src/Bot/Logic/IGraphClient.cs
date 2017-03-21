// -----------------------------------------------------------------------
// <copyright file="IGraphClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Logic
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    /// <summary>
    /// Represents an object that interacts with Microsoft Graph.
    /// </summary>
    public interface IGraphClient
    {
        /// <summary>
        /// Gets a list of roles assigned to the specified object identifier.
        /// </summary>
        /// <param name="objectId">Object identifier for the object to be checked.</param>
        /// <returns>A list of roles that that are associated with the specified object identifier.</returns>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="objectId"/> is empty or null.
        /// </exception>
        Task<List<RoleModel>> GetDirectoryRolesAsync(string objectId);
    }
}