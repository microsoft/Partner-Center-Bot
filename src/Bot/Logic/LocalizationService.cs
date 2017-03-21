// -----------------------------------------------------------------------
// <copyright file="LocalizationService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Logic
{
    using System.Globalization;
    using System.Threading.Tasks;
    using PartnerCenter.Models.CountryValidationRules;
    using PartnerCenter.Models.Partners;

    /// <summary>
    /// Provides localization for the bot.
    /// </summary>
    /// <seealso cref="ILocalizationService" />
    public class LocalizationService : ILocalizationService
    {
        /// <summary>
        /// Provides access to core application services.
        /// </summary>
        private readonly IBotService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationService"/> class.
        /// </summary>
        /// <param name="service">Provides access to core application services.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="service"/> is null.
        /// </exception>
        public LocalizationService(IBotService service)
        {
            service.AssertNotNull(nameof(service));

            this.service = service;
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
                businessProfile = await this.service.PartnerOperations.GetLegalBusinessProfileAsync();

                this.CountryIso2Code = businessProfile.Address.Country;

                try
                {
                    // Obtain the country validation rules for the configured partner.
                    countryValidationRules = await this.service.PartnerOperations.GetCountryValidationRulesAsync(this.CountryIso2Code);

                    this.Locale = countryValidationRules.DefaultCulture;
                }
                catch
                {
                    // Default the region to en-US.
                    this.Locale = "en-US";
                }

                // Set the culture to partner locale.
                Resources.Culture = new CultureInfo(this.Locale);
            }
            finally
            {
                businessProfile = null;
                countryValidationRules = null;
            }
        }
    }
}