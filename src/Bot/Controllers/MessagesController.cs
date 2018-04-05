// -----------------------------------------------------------------------
// <copyright file="MessagesController.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Dialogs;
    using Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Providers;
    using Security;

    /// <summary>
    /// Provides the ability to handle messages.
    /// </summary>
    /// <seealso cref="BaseApiController" />
    [CustomBotAuthentication]
    [RoutePrefix("api/messages")]
    public class MessagesController : BaseApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagesController"/> class.
        /// </summary>
        /// <param name="provider">Provides access to core services.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="provider"/> is null.
        /// </exception>
        public MessagesController(IBotProvider provider) : base(provider)
        {
        }

        /// <summary>
        /// Processes messages received from a user. 
        /// </summary>
        /// <param name="activity">Represents the message received from a user.</param>
        /// <returns>A HTTP status code that reflects whether the request was successful or not.</returns>
        [HttpPost]
        [Route("")]
        public async Task<HttpResponseMessage> PostAsync([FromBody]Activity activity)
        {
            ConnectorClient client;
            DateTime startTime;
            Dictionary<string, double> eventMeasurements;
            Dictionary<string, string> eventProperties;

            try
            {
                startTime = DateTime.Now;

                if (activity.Type == ActivityTypes.ConversationUpdate)
                {
                    if (activity.MembersAdded.Any(o => o.Id == activity.Recipient.Id))
                    {
                        client = new ConnectorClient(
                            new Uri(activity.ServiceUrl),
                            new MicrosoftAppCredentials(
                                Provider.Configuration.MicrosoftAppId,
                                Provider.Configuration.MicrosoftAppPassword.ToUnsecureString()));

                        await client.Conversations.ReplyToActivityAsync(activity.CreateReply(Resources.Welcome)).ConfigureAwait(false);
                    }
                }

                if (activity.Type == ActivityTypes.Message)
                {
                    await Conversation.SendAsync(activity, () => new ActionDialog(Provider)).ConfigureAwait(false);
                }

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "ChannelId", activity.ChannelId },
                    { "Locale", activity.Locale },
                    { "ActivityType", activity.Type }
                };

                // Track the event measurements for analysis.
                eventMeasurements = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                Provider.Telemetry.TrackEvent("api/messages", eventProperties, eventMeasurements);

                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                Provider.Telemetry.TrackException(ex);
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            finally
            {
                client = null;
                eventMeasurements = null;
                eventProperties = null;
            }
        }
    }
}