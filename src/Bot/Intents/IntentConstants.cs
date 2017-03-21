// -----------------------------------------------------------------------
// <copyright file="IntentConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Intents
{
    /// <summary>
    /// Defines constants utilized by LUIS intents.
    /// </summary>
    public static class IntentConstants
    {
        /// <summary>
        /// Name of the customer entity.
        /// </summary>
        public const string CustomerEntity = "Customer";

        /// <summary>
        /// Intent name for the list customers intent.
        /// </summary>
        public const string ListCustomers = "ListCustomers";

        /// <summary>
        /// Intent name for the list customers intent.
        /// </summary>
        public const string ListSubscriptions = "ListSubscriptions";

        /// <summary>
        /// Intent name for the question and answer intent.
        /// </summary>
        public const string Question = "Question";

        /// <summary>
        /// Intent name for the select customer intent.
        /// </summary>
        public const string SelectCustomer = "SelectCustomer";

        /// <summary>
        /// Intent name for the select subscription intent.
        /// </summary>
        public const string SelectSubscription = "SelectSubscription";
    }
}