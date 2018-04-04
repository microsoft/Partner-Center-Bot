// -----------------------------------------------------------------------
// <copyright file="ListCustomersIntent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Intents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using PartnerCenter.Models.Customers;
    using Security;
    using Providers;

    /// <summary>
    /// Processes the request to list customers.
    /// </summary>
    /// <seealso cref="IIntent" />
    [Serializable]
    public class ListCustomersIntent : IIntent
    {
        /// <summary>
        /// Gets the message to be displayed when help has been requested.
        /// </summary>
        public string HelpMessage => Resources.ListCustomersHelpMessage;

        /// <summary>
        /// Gets the name of the intent.
        /// </summary>
        public string Name => IntentConstants.ListCustomers;

        /// <summary>
        /// Gets the permissions required to perform the operation represented by this intent.
        /// </summary>
        public UserRoles Permissions => UserRoles.Partner;

        /// <summary>
        /// Performs the operation represented by this intent.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="message">The message from the authenticated user.</param>
        /// <param name="result">The result from Language Understanding cognitive service.</param>
        /// <param name="provider">Provides access to core services.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> is null.
        /// or
        /// <paramref name="message"/> is null.
        /// or
        /// <paramref name="result"/> is null.
        /// or 
        /// <paramref name="provider"/> is null.
        /// </exception>
        public async Task ExecuteAsync(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result, IBotProvider provider)
        {
            CustomerPrincipal principal;
            DateTime startTime;
            Dictionary<string, double> eventMeasurements;
            Dictionary<string, string> eventProperties;
            IMessageActivity response;
            List<Customer> customers;

            context.AssertNotNull(nameof(context));
            message.AssertNotNull(nameof(message));
            result.AssertNotNull(nameof(result));
            provider.AssertNotNull(nameof(provider));

            try
            {
                startTime = DateTime.Now;

                principal = await context.GetCustomerPrincipalAsync(provider);

                customers = await provider.PartnerOperations.GetCustomersAsync(principal);

                response = context.MakeMessage();
                response.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                response.Attachments = customers.Select(c => (new ThumbnailCard
                {
                    Buttons = new List<CardAction>
                    {
                        new CardAction
                        {
                            Title = Resources.SelectCaptial,
                            Type = ActionTypes.PostBack,
                            Value = $"select customer {c.Id}"
                        }
                    },
                    Images = new List<CardImage>
                    {
                        new CardImage
                        {
                            Url = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Host}:{HttpContext.Current.Request.Url.Port}/api/images/dynamic?value={HttpUtility.UrlEncode(c.CompanyProfile.CompanyName)}"
                        }
                    },
                    Subtitle = c.CompanyProfile.Domain,
                    Title = c.CompanyProfile.CompanyName
                }).ToAttachment()).ToList();

                await context.PostAsync(response);

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "ChannelId", context.Activity.ChannelId },
                    { "CustomerId", principal.CustomerId },
                    { "LocalTimeStamp", context.Activity.LocalTimestamp.ToString() },
                    { "Locale", response.Locale },
                    { "UserId", principal.ObjectId }
                };

                // Track the event measurements for analysis.
                eventMeasurements = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds },
                    { "NumberOfCustomers", response.Attachments.Count }
                };

                provider.Telemetry.TrackEvent("ListCustomers/Execute", eventProperties, eventMeasurements);
            }
            finally
            {
                customers = null;
                eventMeasurements = null;
                eventProperties = null;
            }
        }
    }
}