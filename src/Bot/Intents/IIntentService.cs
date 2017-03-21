// -----------------------------------------------------------------------
// <copyright file="IIntentService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Intents
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the service that will provide access to the supported intents.
    /// </summary>
    public interface IIntentService
    {
        /// <summary>
        /// Gets the supported intents.
        /// </summary>
        IDictionary<string, IIntent> Intents { get; }

        /// <summary>
        /// Initializes the intent service.
        /// </summary>
        void Initialize();
    }
}