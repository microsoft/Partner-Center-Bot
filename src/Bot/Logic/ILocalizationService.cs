// -----------------------------------------------------------------------
// <copyright file="ILocalizationService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Logic
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents the localization service used by the portal.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets the country ISO2 code.
        /// </summary>
        string CountryIso2Code { get; }

        /// <summary>
        /// Gets the locale for the application.
        /// </summary>
        string Locale { get; }

        /// <summary>
        /// Initializes the localization service.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task InitializeAsync();
    }
}