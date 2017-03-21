// -----------------------------------------------------------------------
// <copyright file="BotConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Logic
{
    /// <summary>
    /// Defines various constant value utilized by the bot.
    /// </summary>
    internal static class BotConstants
    {
        /// <summary>
        /// Key utilized to access the authentication token stored in the user data.
        /// </summary>
        public const string AuthTokenKey = "AuthToken";

        /// <summary>
        /// Authentication endpoint to be utilized when requesting access tokens.
        /// </summary>
        public const string AuthorityEndpoint = "common";

        /// <summary>
        /// Key utilized to access the botId value.
        /// </summary>
        public const string BotIdKey = "botId";

        /// <summary>
        /// Path to be utilized when constructing the callback URI.
        /// </summary>
        public const string CallbackPath = "api/oauthcallback";

        /// <summary>
        /// Key utilized to access the channelId value.
        /// </summary>
        public const string ChannelIdKey = "channelId";

        /// <summary>
        /// Key utilized to access the conversationId value.
        /// </summary>
        public const string ConversationIdKey = "conversationId";

        /// <summary>
        /// Key utilized to manage the <see cref="Security.CustomerPrincipal"/> object stored in the user data.
        /// </summary>
        public const string CustomerPrincipalKey = "CustomerPrincipal";

        /// <summary>
        /// Key utilized to access the locale value.
        /// </summary>
        public const string LocaleKey = "locale";

        /// <summary>
        /// Key utilized to access the Partner Center app only authentication token.
        /// </summary>
        public const string PartnerCenterAppOnlyKey = "Resource::PartnerCenter::AppOnly";

        /// <summary>
        /// Key utilized to access the serviceUrl value.
        /// </summary>
        public const string ServiceUrlKey = "serviceUrl";

        /// <summary>
        /// Key utilizes to access the unique identifier value.
        /// </summary>
        public const string UniqueIdentifierKey = "uniqueId";

        /// <summary>
        /// Key utilized to access the userId value.
        /// </summary>
        public const string UserIdKey = "userId";
    }
}