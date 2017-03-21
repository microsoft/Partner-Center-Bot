// -----------------------------------------------------------------------
// <copyright file="ITokenManagement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Security
{
    using System;
    using System.Threading.Tasks;
    using IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// Represents a management interface for retrieving access tokens.
    /// </summary>
    public interface ITokenManagement
    {
        /// <summary>
        /// Acquires an access token without asking for user credential.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the client requesting the token.</param>
        /// <param name="objectUserId">Identifier of the user that is requesting the token.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represents the access token.</returns>
        Task<AuthenticationResult> AcquireTokenSilentAsync(string authority, string resource, UserIdentifier objectUserId);

        /// <summary>
        /// Gets an access token from the authority using app only authentication.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represented the access token.</returns>
        Task<AuthenticationToken> GetAppOnlyTokenAsync(string authority, string resource);

        /// <summary>
        /// Gets an access token from the authority using app only authentication.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="scope">Permissions the requested token will need.</param>
        /// <returns>A string that represented the access token.</returns>
        Task<string> GetAppOnlyTokenAsync(string authority, string resource, string scope);

        /// <summary>
        /// Gets an access token from the authority using app + user authentication.
        /// </summary>
        /// <param name="princiapl">Security principal for the calling user.</param>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represented the access token.</returns>
        Task<AuthenticationToken> GetAppPlusUserTokenAsync(CustomerPrincipal princiapl, string authority, string resource);

        /// <summary>
        /// Gets the URL of the authorization endpoint including the query parameters.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="extraQueryParameters">Data that will be appended as is to the query string in the HTTP authentication request to the authority.</param>
        /// <returns>URL of the authorization endpoint including the query parameters.</returns>
        Task<string> GetAuthorizationRequestUrlAsync(string authority, Uri redirectUri, string resource, string extraQueryParameters);

        /// <summary>
        /// Gets an instance of <see cref="IPartnerCredentials"/> used to access the Partner Center API.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <returns>
        /// An instance of <see cref="IPartnerCredentials" /> that represents the access token.
        /// </returns>
        Task<IPartnerCredentials> GetPartnerCenterAppOnlyCredentialsAsync(string authority);

        /// <summary>
        /// Gets an instance of <see cref="IPartnerCredentials"/> used to access the Partner Center API.
        /// </summary>
        /// <param name="principal">Security principal for the calling user.</param>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <returns>
        /// An instance of <see cref="IPartnerCredentials" /> that represents the access token.
        /// </returns>
        Task<IPartnerCredentials> GetPartnerCenterAppPlusUserCredentialsAsync(CustomerPrincipal principal, string authority);

        /// <summary>
        /// Gets an access token utilizing an authorization code. 
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="code">Authorization code received from the service authorization endpoint.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="redirectUri">Redirect URI used for obtain the authorization code.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represented the access token.</returns>
        Task<AuthenticationResult> GetTokenByAuthorizationCodeAsync(string authority, string code, string resource, Uri redirectUri);
    }
}