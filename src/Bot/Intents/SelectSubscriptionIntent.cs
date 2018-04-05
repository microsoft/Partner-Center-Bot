// -----------------------------------------------------------------------
// <copyright file="SelectSubscriptionIntent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Intents
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using PartnerCenter.Models.Subscriptions;
    using Security;
    using Providers;

    /// <summary>
    /// Processes the request to select a specific subscription.
    /// </summary>
    /// <seealso cref="IIntent" />
    public class SelectSubscriptionIntent : IIntent
    {
        /// <summary>
        /// Gets the message to be displayed when help has been requested.
        /// </summary>
        public string HelpMessage => string.Empty;

        /// <summary>
        /// Gets the name of the intent.
        /// </summary>
        public string Name => IntentConstants.SelectSubscription;

        /// <summary>
        /// Gets the permissions required to perform the operation represented by this intent.
        /// </summary>
        public UserRoles Permissions => UserRoles.Partner | UserRoles.AdminAgents;

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
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            EntityRecommendation indentifierEntity;
            IMessageActivity response;
            Subscription subscription;
            string subscriptionId = string.Empty;

            context.AssertNotNull(nameof(context));
            message.AssertNotNull(nameof(message));
            result.AssertNotNull(nameof(result));
            provider.AssertNotNull(nameof(provider));

            try
            {
                startTime = DateTime.Now;
                response = context.MakeMessage();

                principal = await context.GetCustomerPrincipalAsync(provider).ConfigureAwait(false);

                if (result.TryFindEntity("identifier", out indentifierEntity))
                {
                    subscriptionId = indentifierEntity.Entity.Replace(" ", string.Empty);
                    principal.Operation.SubscriptionId = subscriptionId;

                    subscription = await provider.PartnerOperations.GetSubscriptionAsync(principal).ConfigureAwait(false);

                    if (subscription == null)
                    {
                        response.Text = Resources.UnableToLocateSubscription;
                    }
                    else
                    {
                        context.StoreCustomerPrincipal(principal);
                    }
                }

                if (string.IsNullOrEmpty(subscriptionId))
                {
                    response.Text = Resources.UnableToLocateSubscription;
                }
                else
                {
                    response.Text = $"{Resources.SubscriptionContext} {subscriptionId}";
                }

                await context.PostAsync(response).ConfigureAwait(false);

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "ChannelId", context.Activity.ChannelId },
                    { "SubscriptionId", subscriptionId },
                    { "PrincipalCustomerId", principal.CustomerId },
                    { "LocalTimeStamp", context.Activity.LocalTimestamp.ToString() },
                    { "UserId", principal.ObjectId }
                };

                provider.Telemetry.TrackEvent("SelectSubscription/ExecuteAsync", eventProperties, eventMetrics);
            }
            finally
            {
                indentifierEntity = null;
                eventMetrics = null;
                eventProperties = null;
                message = null;
                subscription = null;
            }
        }
    }
}