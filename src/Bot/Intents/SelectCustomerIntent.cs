// -----------------------------------------------------------------------
// <copyright file="SelectCustomerIntent.cs" company="Microsoft">
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
    using PartnerCenter.Models.Customers;
    using Security;
    using Providers;

    /// <summary>
    /// Processes the request to select a specific customer.
    /// </summary>
    /// <seealso cref="IIntent" />
    public class SelectCustomerIntent : IIntent
    {
        /// <summary>
        /// Gets the message to be displayed when help has been requested.
        /// </summary>
        public string HelpMessage => string.Empty;

        /// <summary>
        /// Gets the name of the intent.
        /// </summary>
        public string Name => IntentConstants.SelectCustomer;

        /// <summary>
        /// Gets the permissions required to perform the operation represented by this intent.
        /// </summary>
        public UserRoles Permissions => UserRoles.AdminAgents | UserRoles.HelpdeskAgent;

        /// <summary>
        /// Performs the operation represented by this intent.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="message">The message from the authenticated user.</param>
        /// <param name="result">The result from Language Understanding cognitive service.</param>
        /// <param name="provider">Provides access to core services.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="System.ArgumentNullException">
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
            Customer customer;
            DateTime startTime;
            Dictionary<string, double> eventMeasurements;
            Dictionary<string, string> eventProperties;
            EntityRecommendation indentifierEntity;
            IMessageActivity response;
            string customerId = string.Empty;

            context.AssertNotNull(nameof(context));
            message.AssertNotNull(nameof(message));
            result.AssertNotNull(nameof(result));
            provider.AssertNotNull(nameof(provider));

            try
            {
                startTime = DateTime.Now;
                response = context.MakeMessage();

                principal = await context.GetCustomerPrincipalAsync(provider);

                if (result.TryFindEntity("identifier", out indentifierEntity))
                {
                    customerId = indentifierEntity.Entity.Replace(" ", string.Empty);
                    principal.Operation.CustomerId = customerId;
                    principal.Operation.SubscriptionId = string.Empty;
                    context.StoreCustomerPrincipal(principal);
                }

                if (string.IsNullOrEmpty(customerId))
                {
                    response.Text = Resources.UnableToLocateCustomer;
                }
                else
                {
                    customer = await provider.PartnerOperations.GetCustomerAsync(principal, customerId);
                    response.Text = $"{Resources.CustomerContext} {customer.CompanyProfile.CompanyName}";
                }

                await context.PostAsync(response);

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "ChannelId", context.Activity.ChannelId },
                    { "CustomerId", customerId },
                    { "PrincipalCustomerId", principal.CustomerId },
                    { "LocalTimeStamp", context.Activity.LocalTimestamp.ToString() },
                    { "UserId", principal.ObjectId }
                };

                // Track the event measurements for analysis.
                eventMeasurements = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                provider.Telemetry.TrackEvent("SelectCustomer/ExecuteAsync", eventProperties, eventMeasurements);
            }
            finally
            {
                customer = null;
                indentifierEntity = null;
                eventMeasurements = null;
                eventProperties = null;
                message = null;
            }
        }
    }
}