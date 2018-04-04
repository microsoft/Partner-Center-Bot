// -----------------------------------------------------------------------
// <copyright file="IAccessTokenProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Providers
{
    using System;
    using System.Threading.Tasks;
    using IdentityModel.Clients.ActiveDirectory;
    using Models;

    /// <summary>
    /// Represents the ability to manage access tokens.
    /// </summary>
    public interface IAccessTokenProvider
    {
        Task<AuthenticationResult> AcquireTokenByAuthorizationCodeAsync(string authority, string code, ApplicationCredential credential, Uri redirectUri, string resource);

        Task<AuthenticationResult> AcquireTokenSilentAsync(string authority, string resource, string clientId, UserIdentifier objectUserId);
        /// <summary>
        /// Gets an access token from the authority.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="credential">The credentials to use for token acquisition.</param>
        /// <returns>An instance of <see cref="AuthenticationResult"/> that represented the access token.</returns>
        Task<AuthenticationResult> GetAccessTokenAsync(string authority, string resource, ApplicationCredential credential);

        /// <summary>
        /// Gets an access token from the authority.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="credential">The credentials to use for token acquisition.</param>
        /// <param name="token">Assertion token representing the user.</param>
        /// <returns>An instance of <see cref="AuthenticationResult"/> that represented the access token.</returns>
        Task<AuthenticationResult> GetAccessTokenAsync(string authority, string resource, ApplicationCredential credential, string token);

        Task<string> GetAuthorizationRequestUrlAsync(string authority, string resource, string clientId, Uri redirectUri, string extraQueryParameters);
    }
}