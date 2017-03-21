// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Logic
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using PartnerCenter.Models.Subscriptions;
    using Security;

    /// <summary>
    /// Provides useful methods used for retrieving, transforming, validating objects.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Gets the customer principal from the private bot data associated with the user.
        /// </summary>
        /// <param name="context">The context for the bot.</param>
        /// <param name="service">Provides access to core application services.</param>
        /// <returns>An instance of <see cref="CustomerPrincipal"/> that represents the authenticated user.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> is null.
        /// or
        /// <paramref name="service"/> is null.
        /// </exception>
        public static async Task<CustomerPrincipal> GetCustomerPrincipalAsync(this IBotContext context, IBotService service)
        {
            CustomerPrincipal principal = null;
            AuthenticationResult authResult;

            context.AssertNotNull(nameof(context));
            service.AssertNotNull(nameof(service));

            try
            {
                if (context.PrivateConversationData.TryGetValue(BotConstants.CustomerPrincipalKey, out principal))
                {
                    if (principal.ExpiresOn < DateTime.UtcNow)
                    {
                        authResult = await service.TokenManagement.AcquireTokenSilentAsync(
                            $"{service.Configuration.ActiveDirectoryEndpoint}/{principal.CustomerId}",
                            service.Configuration.GraphEndpoint,
                            new UserIdentifier(principal.ObjectId, UserIdentifierType.UniqueId));

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
        /// Gets the text from the description attribute.
        /// </summary>
        /// <param name="value">The enumeration value associated with the attribute.</param>
        /// <returns>A <see cref="string"/> containing the text from the description attribute.</returns>
        public static string GetDescription(this Enum value)
        {
            DescriptionAttribute attribute;

            try
            {
                attribute = value.GetType()
                    .GetField(value.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .SingleOrDefault() as DescriptionAttribute;

                return attribute == null ? value.ToString() : attribute.Description;
            }
            finally
            {
                attribute = null;
            }
        }

        /// <summary>
        /// Transforms an instance of <see cref="Subscription"/> into an instance of <see cref="Attachment"/>
        /// </summary>
        /// <param name="subscription">An instance of <see cref="Subscription"/> to be transformed.</param>
        /// <returns>An instance <see cref="Attachment"/> that represents the subscription.</returns>
        public static Attachment ToAttachment(this Subscription subscription)
        {
            string imageUrl;
            string offerName = subscription.OfferName.ToLower();

            if (offerName.Contains("azure") || offerName.Contains("active directory"))
            {
                imageUrl = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Host}:{HttpContext.Current.Request.Url.Port}/content/images/azure-logo.png";
            }
            else if (offerName.Contains("office") || offerName.Contains("365"))
            {
                imageUrl = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Host}:{HttpContext.Current.Request.Url.Port}/content/images/office-logo.png";
            }
            else
            {
                imageUrl = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Host}:{HttpContext.Current.Request.Url.Port}/content/images/microsoft-logo.png";
            }

            HeroCard card = new HeroCard
            {
                Buttons = new List<CardAction>
                {
                    new CardAction
                    {
                        Title = Resources.SelectCaptial,
                        Type = ActionTypes.PostBack,
                        Value = $"select subscription {subscription.Id}"
                    }
                },
                Images = new List<CardImage>
                {
                    new CardImage
                    {
                        Url = imageUrl
                    }
                },
                Subtitle = subscription.Id,
                Title = subscription.FriendlyName
            };

            return card.ToAttachment();
        }

        /// <summary>
        /// Converts the value to camel case. 
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>A string in camel case notation.</returns>
        public static string ToCamelCase(this string value)
        {
            return value.Substring(0, 1).ToLower().Insert(1, value.Substring(1));
        }

        /// <summary>
        /// Ensures that a string is not empty.
        /// </summary>
        /// <param name="nonEmptyString">The string to validate.</param>
        /// <param name="caption">The name to report in the exception.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="nonEmptyString"/> is empty or null.
        /// </exception>
        public static void AssertNotEmpty(this string nonEmptyString, string caption)
        {
            if (string.IsNullOrWhiteSpace(nonEmptyString))
            {
                throw new ArgumentException($"{caption ?? "string"} is not set");
            }
        }

        /// <summary>
        /// Ensures that a given object is not null. Throws an exception otherwise.
        /// </summary>
        /// <param name="objectToValidate">The object we are validating.</param>
        /// <param name="caption">The name to report in the exception.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="objectToValidate"/> is null.
        /// </exception>
        public static void AssertNotNull(this object objectToValidate, string caption)
        {
            if (objectToValidate == null)
            {
                throw new ArgumentNullException(caption);
            }
        }

        /// <summary>
        /// Ensures the given <see cref="CustomerPrincipal"/> has a valid customer context.
        /// </summary>
        /// <param name="principalToValidate">An instance of <see cref="CustomerPrincipal"/> to validate.</param>
        /// <param name="message">The message to report in the exception.</param>
        public static void AssertValidCustomerContext(this CustomerPrincipal principalToValidate, string message)
        {
            principalToValidate.AssertNotNull(nameof(principalToValidate));

            if (string.IsNullOrEmpty(principalToValidate.Operation.CustomerId))
            {
                throw new InvalidOperationException(message);
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