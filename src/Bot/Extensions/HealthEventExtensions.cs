// -----------------------------------------------------------------------
// <copyright file="HealthEventExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Extensions
{
    using Models;
    using Microsoft.Bot.Connector;

    public static class HealthEventExtensions
    {
        /// <summary>
        /// Transforms an instance of <see cref="HealthEvent"/> into an instance of <see cref="Attachment"/>
        /// </summary>
        /// <param name="healthEvent">An instance of <see cref="HealthEvent"/> to be transformed.</param>
        /// <returns>An instance of <see cref="Attachment"/> that represents the health event.</returns>
        public static Attachment ToAttachment(this OfficeHealthEvent healthEvent)
        {
            HeroCard card = new HeroCard
            {
                Subtitle = healthEvent.Status,
                Title = healthEvent.WorkloadDisplayName
            };

            return card.ToAttachment();
        }
    }
}