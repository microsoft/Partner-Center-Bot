// -----------------------------------------------------------------------
// <copyright file="BotContextExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Extensions
{
    using System; 
    using System.Threading.Tasks;
    using IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Bot.Builder.Dialogs;
    using Logic;
    using Providers;
    using Security;
    
    internal static class BotContextExtensions
    {
        /// <summary>
        /// Gets the customer principal from the private bot data associated with the user.
        /// </summary>
        /// <param name="context">The context for the bot.</param>
        /// <param name="provider">Provides access to core application services.</param>
        /// <returns>An instance of <see cref="CustomerPrincipal"/> that represents the authenticated user.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> is null.
        /// or
        /// <paramref name="provider"/> is null.
        /// </exception>
        public static async Task<CustomerPrincipal> GetCustomerPrincipalAsync(this IBotContext context, IBotProvider provider)
        {
            CustomerPrincipal principal = null;
            AuthenticationResult authResult;

            context.AssertNotNull(nameof(context));
            provider.AssertNotNull(nameof(provider));

            try
            {
                if (context.PrivateConversationData.TryGetValue(BotConstants.CustomerPrincipalKey, out principal))
                {
                    if (principal.ExpiresOn < DateTime.UtcNow)
                    {
                        authResult = await provider.AccessToken.AcquireTokenSilentAsync(
                            $"{provider.Configuration.ActiveDirectoryEndpoint}/{principal.CustomerId}",
                            provider.Configuration.GraphEndpoint,
                            provider.Configuration.ApplicationId,
                            new UserIdentifier(principal.ObjectId, UserIdentifierType.UniqueId)).ConfigureAwait(false);

                        principal.AccessToken = authResult.AccessToken;
                        principal.ExpiresOn = authResult.ExpiresOn;

                        context.StoreCustomerPrincipal(principal);
                    }

                    return principal;
                }

                return null;
            }
            finally
            {
                authResult = null;
            }
        }

        /// <summary>
        /// Stores an instance of <see cref="CustomerPrincipal"/> in the private bot data associated with the user.
        /// </summary>
        /// <param name="context">The context for the bot.</param>
        /// <param name="principal">An instance of <see cref="CustomerPrincipal"/> associated with the authenticated user.</param>
        public static void StoreCustomerPrincipal(this IBotContext context, CustomerPrincipal principal)
        {
            context.AssertNotNull(nameof(context));
            principal.AssertNotNull(nameof(principal));

            context.PrivateConversationData.SetValue(BotConstants.CustomerPrincipalKey, principal);
        }
    }
}