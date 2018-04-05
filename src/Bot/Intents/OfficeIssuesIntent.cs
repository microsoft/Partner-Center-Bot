// -----------------------------------------------------------------------
// <copyright file="OfficeIssuesIntent.cs" company="Microsoft">
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
    using IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using Services;
    using Models;
    using Providers;
    using Security;

    /// <summary>
    /// Processes the request to list Office 365 issues. 
    /// </summary>
    /// <seealso cref="IIntent" />
    public class OfficeIssuesIntent : IIntent
    {
        /// <summary>
        /// Gets the message to be displayed when help has been requested.
        /// </summary>
        public string HelpMessage => Resources.OfficeIssuesHelpMessage;

        /// <summary>
        /// Gets the name of the intent.
        /// </summary>
        public string Name => IntentConstants.OfficeIssues;

        /// <summary>
        /// Gets the permissions required to perform the operation represented by this intent.
        /// </summary>
        public UserRoles Permissions => UserRoles.Partner | UserRoles.GlobalAdmin;

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
            CustomerPrincipal principal;
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            IMessageActivity response;
            List<OfficeHealthEvent> healthEvents;
            ServiceCommunications serviceComm;
            string authority;
            string customerId;

            context.AssertNotNull(nameof(context));
            message.AssertNotNull(nameof(message));
            result.AssertNotNull(nameof(result));
            provider.AssertNotNull(nameof(provider));

            try
            {
                startTime = DateTime.Now;

                principal = await context.GetCustomerPrincipalAsync(provider).ConfigureAwait(false);

                if (principal.CustomerId.Equals(provider.Configuration.PartnerCenterAccountId, StringComparison.CurrentCultureIgnoreCase))
                {
                    principal.AssertValidCustomerContext(Resources.SelectCustomerFirst);

                    authority = $"{provider.Configuration.ActiveDirectoryEndpoint}/{principal.Operation.CustomerId}";
                    customerId = principal.Operation.CustomerId;
                }
                else
                {
                    authority = $"{provider.Configuration.ActiveDirectoryEndpoint}/{principal.CustomerId}";
                    customerId = principal.CustomerId;
                }

                serviceComm = new ServiceCommunications(
                    new Uri(provider.Configuration.OfficeManagementEndpoint),
                    new ServiceCredentials(
                        provider.Configuration.ApplicationId,
                        provider.Configuration.ApplicationSecret.ToUnsecureString(),
                        provider.Configuration.OfficeManagementEndpoint,
                        principal.Operation.CustomerId));

                healthEvents = await serviceComm.GetHealthEventsAsync(principal.Operation.CustomerId).ConfigureAwait(false);

                response = context.MakeMessage();
                response.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                response.Attachments = healthEvents.Select(e => e.ToAttachment()).ToList();

                await context.PostAsync(response).ConfigureAwait(false);

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

                provider.Telemetry.TrackEvent("OfficeIssues/Execute", eventProperties, eventMetrics);
            }
            catch (Exception ex)
            {
                response = context.MakeMessage();
                response.Text = Resources.ErrorMessage;

                provider.Telemetry.TrackException(ex);

                await context.PostAsync(response).ConfigureAwait(false);
            }
            finally
            {
                eventMetrics = null;
                eventProperties = null;
                principal = null;
                response = null;
                serviceComm = null;
            }
        }
    }
}