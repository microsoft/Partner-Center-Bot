// -----------------------------------------------------------------------
// <copyright file="OAuthCallbackController.cs" company="Microsoft">
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
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using Autofac;
    using Exceptions;
    using IdentityModel.Clients.ActiveDirectory;
    using Logic;
    using Microsoft.Bot.Builder.ConnectorEx;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Models;
    using Security;

    /// <summary>
    /// Manages callbacks from the authentication endpoint.
    /// </summary>
    /// <seealso cref="BaseApiController" />
    public class OAuthCallbackController : BaseApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthCallbackController"/> class.
        /// </summary>
        /// <param name="service">Provides access to core services.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="service"/> is null.
        /// </exception>
        public OAuthCallbackController(IBotService service) : base(service)
        {
        }

        /// <summary>
        /// Processes the callback from the authentication endpoint.
        /// </summary>
        /// <param name="code">The authorization code that was requested.</param>
        /// <param name="state">State of the user who authenticated.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> token to observe.</param>
        /// <returns>A HTTP status code indicating whether the callback was processed successfully or not.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="code"/> is empty or null.
        /// or
        /// <paramref name="state"/> is empty or null. 
        /// </exception>
        [HttpGet]
        [Route("api/OAuthCallback")]
        public async Task<HttpResponseMessage> OAuthCallbackAsync([FromUri]string code, [FromUri]string state, CancellationToken cancellationToken)
        {
            Activity message;
            Address address;
            ConversationReference conversationReference;
            CustomerPrincipal principal = null;
            DateTime startTime;
            Dictionary<string, double> eventMeasurements;
            Dictionary<string, string> eventProperties;
            Dictionary<string, string> stateData;
            IBotData botData;
            HttpResponseMessage response;

            code.AssertNotEmpty(nameof(code));
            state.AssertNotEmpty(nameof(state));

            try
            {
                startTime = DateTime.Now;
                stateData = UrlToken.Decode<Dictionary<string, string>>(state);

                address = new Address(
                    stateData[BotConstants.BotIdKey],
                    stateData[BotConstants.ChannelIdKey],
                    stateData[BotConstants.UserIdKey],
                    stateData[BotConstants.ConversationIdKey],
                    stateData[BotConstants.ServiceUrlKey]);

                conversationReference = address.ToConversationReference();

                message = conversationReference.GetPostToBotMessage();

                using (ILifetimeScope scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    botData = scope.Resolve<IBotData>();
                    await botData.LoadAsync(cancellationToken);

                    if (!Validate(botData, stateData))
                    {
                        return Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            new InvalidOperationException(Resources.InvalidAuthenticationException));
                    }

                    principal = await GetCustomerPrincipalAsync(code);

                    if (principal == null)
                    {
                        message.Text = Resources.NoRelationshipException;
                        await Conversation.ResumeAsync(conversationReference, message, cancellationToken);

                        return Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            new InvalidOperationException(Resources.NoRelationshipException));
                    }

                    botData.PrivateConversationData.SetValue(BotConstants.CustomerPrincipalKey, principal);

                    await botData.FlushAsync(cancellationToken);
                    await Conversation.ResumeAsync(conversationReference, message, cancellationToken);

                    // Capture the request for the customer summary for analysis.
                    eventProperties = new Dictionary<string, string>
                    {
                        { "CustomerId", principal.CustomerId },
                        { "Name", principal.Name },
                        { "ObjectId", principal.ObjectId }
                    };

                    // Track the event measurements for analysis.
                    eventMeasurements = new Dictionary<string, double>
                    {
                        { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds },
                        { "NumberOfIntents", principal.AvailableIntents.Count },
                        { "NumberOfRoles", principal.Roles.Count }
                    };

                    Service.Telemetry.TrackEvent("api/OAuthCallback", eventProperties, eventMeasurements);

                    response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent(Resources.SuccessfulAuthentication);
                }
            }
            catch (Exception ex)
            {
                Service.Telemetry.TrackException(ex);
                response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            finally
            {
                address = null;
                botData = null;
                conversationReference = null;
                eventMeasurements = null;
                eventProperties = null;
                message = null;
                principal = null;
            }

            return response;
        }

        /// <summary>
        /// Obtain an instance of <see cref="CustomerPrincipal"/> that represents the authenticated user.
        /// </summary>
        /// <param name="code">The authorization code that was requested.</param>
        /// <returns>An instance of <see cref="CustomerPrincipal"/> that represents the authenticated user.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="code"/> is empty or null.
        /// </exception>
        private async Task<CustomerPrincipal> GetCustomerPrincipalAsync(string code)
        {
            AuthenticationResult authResult;
            CustomerPrincipal principal;
            IGraphClient client;
            List<RoleModel> roles;
            Uri redirectUri;

            code.AssertNotEmpty(nameof(code));

            try
            {
                redirectUri =
                    new Uri($"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Host}:{HttpContext.Current.Request.Url.Port}/{BotConstants.CallbackPath}");

                authResult = await Service.TokenManagement.GetTokenByAuthorizationCodeAsync(
                    $"{Service.Configuration.ActiveDirectoryEndpoint}/{BotConstants.AuthorityEndpoint}",
                    code,
                    Service.Configuration.GraphEndpoint,
                    redirectUri);

                client = new GraphClient(Service, authResult.TenantId);

                roles = await client.GetDirectoryRolesAsync(authResult.UserInfo.UniqueId);

                principal = new CustomerPrincipal
                {
                    AccessToken = authResult.AccessToken,
                    AvailableIntents = (from intent in Service.Intent.Intents
                                        let roleList = Permissions.GetRoles(intent.Value.Permissions)
                                        from r in roleList
                                        where roles.SingleOrDefault(x => x.DisplayName.Equals(r)) != null
                                        select intent).Distinct().ToDictionary(intent => intent.Key, intent => intent.Value),
                    CustomerId = authResult.TenantId,
                    ExpiresOn = authResult.ExpiresOn,
                    Name = authResult.UserInfo.GivenName,
                    ObjectId = authResult.UserInfo.UniqueId,
                    Roles = roles
                };

                if (!Service.Configuration.ApplicationTenantId.Equals(
                    authResult.TenantId,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    await Service.PartnerOperations.GetCustomerAsync(principal, authResult.TenantId);
                }

                return principal;
            }
            catch (PartnerException ex)
            {
                if (ex.ErrorCategory != PartnerErrorCategory.NotFound)
                {
                    throw;
                }

                return null;
            }
            finally
            {
                authResult = null;
                roles = null;
            }
        }

        /// <summary>
        /// Ensures that the request is valid.
        /// </summary>
        /// <param name="botData">Private bot data</param>
        /// <param name="stateData">Data extracted from the state parameter.</param>
        /// <returns><c>true</c> if the request is valid; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="botData"/> is null.
        /// or 
        /// <paramref name="stateData"/> is null.
        /// </exception>
        private bool Validate(IBotData botData, IDictionary<string, string> stateData)
        {
            string uniqueId;

            botData.AssertNotNull(nameof(botData));
            stateData.AssertNotNull(nameof(stateData));

            if (botData.PrivateConversationData.TryGetValue(BotConstants.UniqueIdentifierKey, out uniqueId))
            {
                if (!uniqueId.Equals(stateData[BotConstants.UniqueIdentifierKey], StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return stateData[BotConstants.UniqueIdentifierKey].Equals(uniqueId, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}