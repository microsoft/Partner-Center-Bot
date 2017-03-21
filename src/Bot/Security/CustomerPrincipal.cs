// -----------------------------------------------------------------------
// <copyright file="CustomerPrincipal.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Security
{
    using System;
    using System.Collections.Generic;
    using Intents;
    using Logic;
    using Models;

    /// <summary>
    /// Encapsulates relevant information about the authenticated user.
    /// </summary>
    [Serializable]
    public class CustomerPrincipal
    {
        /// <summary>
        /// Encapsulates contextual information regarding the operations the authenticated user is performing.
        /// </summary>
        private OperationContext operationContext; 

        /// <summary>
        /// Gets or sets the access token for the authenticated user.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the available intents for the authenticated user.
        /// </summary>
        public Dictionary<string, IIntent> AvailableIntents { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier for the authenticated user.
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the point in time in which the Access Token returned in the AccessToken
        /// property ceases to be valid. This value is calculated based on current UTC time
        /// measured locally and the value expiresIn received from the service.
        /// </summary>
        public DateTimeOffset ExpiresOn { get; set; }

        /// <summary>
        /// Gets or sets the name for the authenticated user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the object identifier for the authenticated user.
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the contextual information regarding the operations the authenticated user is performing.
        /// </summary>
        public OperationContext Operation
        {
            get { return this.operationContext ?? (this.operationContext = new OperationContext()); }
            set { this.operationContext = value; }
        }

        /// <summary>
        /// Gets or sets the directory roles assigned to the authenticated user.
        /// </summary>
        public List<RoleModel> Roles { get; set; }
    }
}