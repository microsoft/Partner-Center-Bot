// -----------------------------------------------------------------------
// <copyright file="OperationContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Logic
{
    using System;

    /// <summary>
    /// Encapsulates relevant information about operations an authenticated user is performing. 
    /// </summary>
    [Serializable]
    public class OperationContext
    {
        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the subscription identifier.
        /// </summary>
        public string SubscriptionId { get; set; }
    }
}