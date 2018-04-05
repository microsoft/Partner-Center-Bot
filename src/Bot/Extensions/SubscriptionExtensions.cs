// -----------------------------------------------------------------------
// <copyright file="ListSubscriptionsIntent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Extensions
{
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.Bot.Connector;
    using PartnerCenter.Models.Subscriptions;

    public static class SubscriptionExtensions
    {
        /// <summary>
        /// Transforms an instance of <see cref="Subscription"/> into an instance of <see cref="Attachment"/>
        /// </summary>
        /// <param name="subscription">An instance of <see cref="Subscription"/> to be transformed.</param>
        /// <returns>An instance <see cref="Attachment"/> that represents the subscription.</returns>
        public static Attachment ToAttachment(this Subscription subscription)
        {
            string offerName = subscription.OfferName.ToLower(CultureInfo.CurrentCulture);

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
                Subtitle = subscription.Id,
                Title = subscription.FriendlyName
            };

            return card.ToAttachment();
        }
    }
}