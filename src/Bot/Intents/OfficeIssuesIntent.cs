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
    using Logic;
    using Logic.Office;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
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
        /// <param name="service">Provides access to core services;.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> is null.
        /// or
        /// <paramref name="message"/> is null.
        /// or
        /// <paramref name="result"/> is null.
        /// or 
        /// <paramref name="service"/> is null.
        /// </exception>
        public async Task ExecuteAsync(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result, IBotService service)
        {
            AuthenticationToken token;
            CustomerPrincipal principal;
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            IMessageActivity response;
            List<HealthEvent> healthEvents;

            context.AssertNotNull(nameof(context));
            message.AssertNotNull(nameof(message));
            result.AssertNotNull(nameof(result));
            service.AssertNotNull(nameof(service));

            try
            {
                startTime = DateTime.Now;

                principal = await context.GetCustomerPrincipalAsync(service);

                if (principal.CustomerId.Equals(service.Configuration.PartnerCenterApplicationTenantId, StringComparison.CurrentCultureIgnoreCase))
                {
                    principal.AssertValidCustomerContext(Resources.SelectCustomerFirst);

                    token = await service.TokenManagement.GetAppOnlyTokenAsync(
                        $"{service.Configuration.ActiveDirectoryEndpoint}/{principal.Operation.CustomerId}",
                        service.Configuration.OfficeManagementEndpoint);

                    healthEvents = await service.ServiceCommunications.GetCurrentStatusAsync(principal.Operation.CustomerId, token.Token);
                }
                else
                {
                    token = await service.TokenManagement.GetAppOnlyTokenAsync(
                        $"{service.Configuration.ActiveDirectoryEndpoint}/{principal.CustomerId}",
                        service.Configuration.OfficeManagementEndpoint);

                    healthEvents = await service.ServiceCommunications.GetCurrentStatusAsync(principal.CustomerId, token.Token);
                }

                response = context.MakeMessage();
                response.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                response.Attachments = healthEvents.Select(e => e.ToAttachment()).ToList();

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

                service.Telemetry.TrackEvent("OfficeIssues/Execute", eventProperties, eventMetrics);
            }
            catch (CommunicationException ex)
            {
                response = context.MakeMessage();
                response.Text = Resources.ErrorMessage;

                service.Telemetry.TrackException(ex);

                await context.PostAsync(response);
            }
            finally
            {
                eventMetrics = null;
                eventProperties = null;
                principal = null;
                response = null;
            }
        }
    }
}