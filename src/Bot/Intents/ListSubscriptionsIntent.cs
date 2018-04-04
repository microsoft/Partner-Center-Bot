// -----------------------------------------------------------------------
// <copyright file="ListSubscriptionsIntent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Intents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using PartnerCenter.Models.Customers;
    using PartnerCenter.Models.Subscriptions;
    using Security;
    using Providers;

    /// <summary>
    /// Processes the request to list subscriptions.
    /// </summary>
    /// <seealso cref="IIntent" />
    public class ListSubscriptionsIntent : IIntent
    {
        /// <summary>
        /// Gets the message to be displayed when help has been requested.
        /// </summary>
        public string HelpMessage => Resources.ListSubscriptionssHelpMessage;

        /// <summary>
        /// Gets the name of the intent.
        /// </summary>
        public string Name => IntentConstants.ListSubscriptions;

        /// <summary>
        /// Gets the permissions required to perform the operation represented by this intent.
        /// </summary>
        public UserRoles Permissions => UserRoles.AdminAgents | UserRoles.HelpdeskAgent | UserRoles.GlobalAdmin;

        /// <summary>
        /// Performs the operation represented by this intent.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="message">The message from the authenticated user.</param>
        /// <param name="result">The result from Language Understanding cognitive service.</param>
        /// <param name="provider">Provides access to core services;.</param>
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
            Customer customer = null;
            CustomerPrincipal principal;
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            IMessageActivity response;
            List<Subscription> subscriptions;

            context.AssertNotNull(nameof(context));
            message.AssertNotNull(nameof(message));
            result.AssertNotNull(nameof(result));
            provider.AssertNotNull(nameof(principal));

            try
            {
                startTime = DateTime.Now;

                principal = await context.GetCustomerPrincipalAsync(provider);

                if (principal.CustomerId.Equals(provider.Configuration.PartnerCenterAccountId))
                {
                    customer = await provider.PartnerOperations.GetCustomerAsync(principal);

                    response = context.MakeMessage();
                    response.Text = string.Format(Resources.SubscriptionRequestMessage, customer.CompanyProfile.CompanyName);
                    await context.PostAsync(response);
                }

                subscriptions = await provider.PartnerOperations.GetSubscriptionsAsync(principal);

                response = context.MakeMessage();
                response.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                response.Attachments = subscriptions.Select(s => s.ToAttachment()).ToList();

                await context.PostAsync(response);

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds },
                    { "NumberOfSubscriptions", response.Attachments.Count }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "ChannelId", context.Activity.ChannelId },
                    { "CustomerId", principal.CustomerId },
                    { "LocalTimeStamp", context.Activity.LocalTimestamp.ToString() },
                    { "UserId", principal.ObjectId }
                };

                provider.Telemetry.TrackEvent("ListCustomers/Execute", eventProperties, eventMetrics);
            }
            finally
            {
                customer = null;
                eventMetrics = null;
                eventProperties = null;
                principal = null;
                response = null;
                subscriptions = null;
            }
        }
    }
}