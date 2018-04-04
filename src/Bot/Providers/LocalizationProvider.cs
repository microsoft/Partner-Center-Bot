// -----------------------------------------------------------------------
// <copyright file="LocalizationProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Providers
{
    using System.Globalization;
    using System.Threading.Tasks;
    using PartnerCenter.Models.CountryValidationRules;
    using PartnerCenter.Models.Partners;
    using Extensions;

    /// <summary>
    /// Provides localization for the bot.
    /// </summary>
    /// <seealso cref="ILocalizationProvider" />
    public class LocalizationProvider : ILocalizationProvider
    {
        /// <summary>
        /// Provides access to core application services.
        /// </summary>
        private readonly IBotProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationProvider"/> class.
        /// </summary>
        /// <param name="provider">Provides access to core application services.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="provider"/> is null.
        /// </exception>
        public LocalizationProvider(IBotProvider provider)
        {
            provider.AssertNotNull(nameof(provider));
            this.provider = provider;
        }

        /// <summary>
        /// Gets the country ISO2 code.
        /// </summary>
        public string CountryIso2Code { get; private set; }

        /// <summary>
        /// Gets the locale for the application.
        /// </summary>
        public string Locale { get; private set; }

        /// <summary>
        /// Initializes the localization service.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            CountryValidationRules countryValidationRules;
            LegalBusinessProfile businessProfile;

            try
            {
                // Obtain the business profile for the configured partner.
                businessProfile = await provider.PartnerOperations
                    .GetLegalBusinessProfileAsync().ConfigureAwait(false);

                CountryIso2Code = businessProfile.Address.Country;

                try
                {
                    // Obtain the country validation rules for the configured partner.
                    countryValidationRules = await provider.PartnerOperations
                        .GetCountryValidationRulesAsync(CountryIso2Code).ConfigureAwait(false);

                    Locale = countryValidationRules.DefaultCulture;
                }
                catch
                {
                    // Default the region to en-US.
                    Locale = "en-US";
                }

                // Set the culture to partner locale.
                Resources.Culture = new CultureInfo(Locale);
            }
            finally
            {
                businessProfile = null;
                countryValidationRules = null;
            }
        }
    }
}