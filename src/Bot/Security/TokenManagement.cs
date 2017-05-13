// -----------------------------------------------------------------------
// <copyright file="TokenManagement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Security
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Cache;
    using Extensions;
    using IdentityModel.Clients.ActiveDirectory;
    using Logic;
    using Models;

    /// <summary>
    /// Provides the ability to manage access tokens.
    /// </summary>
    /// <seealso cref="ITokenManagement" />
    public class TokenManagement : ITokenManagement
    {
        /// <summary>
        /// Type of the assertion representing the user when performing app + user authentication.
        /// </summary>
        private const string AssertionType = "urn:ietf:params:oauth:grant-type:jwt-bearer";

        /// <summary>
        /// Provides access to core services.
        /// </summary>
        private readonly IBotService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenManagement"/> class.
        /// </summary>
        /// <param name="service">Provides access to core services.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="service"/> is null.
        /// </exception>
        public TokenManagement(IBotService service)
        {
            service.AssertNotNull(nameof(service));
            this.service = service;
        }

        /// <summary>
        /// Acquires an access token without asking for user credential.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the client requesting the token.</param>
        /// <param name="objectUserId">Identifier of the user that is requesting the token.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represents the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="objectUserId"/> is null.
        /// </exception>
        public async Task<AuthenticationResult> AcquireTokenSilentAsync(string authority, string resource, UserIdentifier objectUserId)
        {
            AuthenticationContext authContext;
            AuthenticationResult authResult;

            authority.AssertNotEmpty(nameof(authority));
            resource.AssertNotEmpty(nameof(resource));
            objectUserId.AssertNotNull(nameof(objectUserId));

            try
            {
                authContext = new AuthenticationContext(authority);

                authResult = await authContext.AcquireTokenSilentAsync(
                    resource,
                    service.Configuration.ApplicationId,
                    objectUserId);

                return authResult;
            }
            finally
            {
                authContext = null;
            }
        }

        /// <summary>
        /// Gets an access token from the authority using app only authentication.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represented the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        public async Task<AuthenticationToken> GetAppOnlyTokenAsync(string authority, string resource)
        {
            AuthenticationContext authContext;
            AuthenticationResult authResult;
            DistributedTokenCache tokenCache;

            authority.AssertNotEmpty(nameof(authority));
            resource.AssertNotEmpty(nameof(resource));

            try
            {
                if (service.Cache.IsEnabled)
                {
                    tokenCache = new DistributedTokenCache(service, resource, $"AppOnly::{resource}");
                    authContext = new AuthenticationContext(authority, tokenCache);
                }
                else
                {
                    authContext = new AuthenticationContext(authority);
                }

                authResult = await authContext.AcquireTokenAsync(
                    resource,
                    new ClientCredential(
                        service.Configuration.ApplicationId,
                        service.Configuration.ApplicationSecret));

                return new AuthenticationToken(authResult.AccessToken, authResult.ExpiresOn);
            }
            finally
            {
                authContext = null;
                authResult = null;
                tokenCache = null;
            }
        }

        /// <summary>
        /// Gets an access token from the authority using app only authentication.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="scope">Permissions the requested token will need.</param>
        /// <returns>A string that represented the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        public async Task<string> GetAppOnlyTokenAsync(string authority, string resource, string scope)
        {
            AuthenticationContext authContext;
            AuthenticationResult authResult;
            X509Certificate2 certificate;

            authority.AssertNotEmpty(nameof(authority));
            resource.AssertNotEmpty(nameof(resource));

            try
            {
                authContext = new AuthenticationContext(authority);

                certificate = FindCertificateByThumbprint(service.Configuration.VaultApplicationCertThumbprint);

                authResult = await authContext.AcquireTokenAsync(
                    resource,
                    new ClientAssertionCertificate(
                        service.Configuration.VaultApplicationId,
                        certificate));

                return authResult.AccessToken;
            }
            finally
            {
                authContext = null;
                authResult = null;
                certificate = null;
            }
        }

        /// <summary>
        /// Gets an access token from the authority using app + user authentication.
        /// </summary>
        /// <param name="principal">Security principal for the calling user.</param>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represented the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="principal"/> is null.
        /// </exception>
        public async Task<AuthenticationToken> GetAppPlusUserTokenAsync(CustomerPrincipal principal, string authority, string resource)
        {
            AuthenticationContext authContext;
            AuthenticationResult authResult;
            DistributedTokenCache tokenCache;
            string key;

            authority.AssertNotEmpty(nameof(authority));
            principal.AssertNotNull(nameof(principal));
            resource.AssertNotEmpty(nameof(resource));

            try
            {
                key = $"AppPlusUser::{resource}::{principal.ObjectId}";

                if (service.Cache.IsEnabled)
                {
                    tokenCache = new DistributedTokenCache(service, resource, key);
                    authContext = new AuthenticationContext(authority, tokenCache);
                }
                else
                {
                    authContext = new AuthenticationContext(authority);
                }

                try
                {
                    authResult = await authContext.AcquireTokenAsync(
                        resource,
                        new ClientCredential(
                            service.Configuration.ApplicationId,
                            service.Configuration.ApplicationSecret),
                        new UserAssertion(principal.AccessToken, AssertionType));
                }
                catch (AdalServiceException ex)
                {
                    if (ex.ErrorCode.Equals("AADSTS70002", StringComparison.CurrentCultureIgnoreCase))
                    {
                        await service.Cache.DeleteAsync(CacheDatabaseType.Authentication, key);

                        authResult = await authContext.AcquireTokenAsync(
                            resource,
                            new ClientCredential(
                                service.Configuration.ApplicationId,
                                service.Configuration.ApplicationSecret),
                            new UserAssertion(principal.AccessToken, AssertionType));
                    }
                    else
                    {
                        throw;
                    }
                }

                return new AuthenticationToken(authResult.AccessToken, authResult.ExpiresOn);
            }
            finally
            {
                authContext = null;
                authResult = null;
                tokenCache = null;
            }
        }

        /// <summary>
        /// Gets the URL of the authorization endpoint including the query parameters.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="extraQueryParameters">Data that will be appended as is to the query string in the HTTP authentication request to the authority.</param>
        /// <returns>URL of the authorization endpoint including the query parameters.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or 
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="redirectUri"/> is null.
        /// </exception>
        public async Task<string> GetAuthorizationRequestUrlAsync(string authority, Uri redirectUri, string resource, string extraQueryParameters)
        {
            AuthenticationContext authContext;
            Uri authUri;

            authority.AssertNotEmpty(nameof(authority));
            resource.AssertNotEmpty(nameof(resource));
            redirectUri.AssertNotNull(nameof(redirectUri));

            try
            {
                authContext = new AuthenticationContext(authority);

                authUri = await authContext.GetAuthorizationRequestUrlAsync(
                    resource,
                    service.Configuration.ApplicationId,
                    redirectUri,
                    UserIdentifier.AnyUser,
                    extraQueryParameters);

                return authUri.ToString();
            }
            finally
            {
                authContext = null;
            }
        }

        /// <summary>
        /// Gets an instance of <see cref="IPartnerCredentials"/> used to access the Partner Center Managed API.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <returns>
        /// An instance of <see cref="IPartnerCredentials" /> that represents the access token.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// </exception>
        /// <remarks>This function will use app only authentication to obtain the credentials.</remarks>
        public async Task<IPartnerCredentials> GetPartnerCenterAppOnlyCredentialsAsync(string authority)
        {
            authority.AssertNotEmpty(nameof(authority));

            // Attempt to obtain the Partner Center token from the cache.
            IPartnerCredentials credentials =
                 await service.Cache.FetchAsync<PartnerCenterTokenModel>(
                     CacheDatabaseType.Authentication, BotConstants.PartnerCenterAppOnlyKey);

            if (credentials != null && !credentials.IsExpired())
            {
                return credentials;
            }

            // The access token has expired, so a new one must be requested.
            credentials = await PartnerCredentials.Instance.GenerateByApplicationCredentialsAsync(
                service.Configuration.PartnerCenterApplicationId,
                service.Configuration.PartnerCenterApplicationSecret,
                service.Configuration.PartnerCenterApplicationTenantId);

            await service.Cache.StoreAsync(
                CacheDatabaseType.Authentication, BotConstants.PartnerCenterAppOnlyKey, credentials);

            return credentials;
        }

        /// <summary>
        /// Gets an instance of <see cref="IPartnerCredentials"/> used to access the Partner Center API.
        /// </summary>
        /// <param name="principal">Security principal for the calling user.</param>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <returns>
        /// An instance of <see cref="IPartnerCredentials" /> that represents the access token.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="principal"/> is null.
        /// </exception>
        public async Task<IPartnerCredentials> GetPartnerCenterAppPlusUserCredentialsAsync(CustomerPrincipal principal, string authority)
        {
            authority.AssertNotEmpty(nameof(authority));
            principal.AssertNotNull(nameof(principal));

            string key = $"AppPlusUser::PartnerCenter::{principal.ObjectId}";

            IPartnerCredentials credentials =
                 await service.Cache.FetchAsync<PartnerCenterTokenModel>(
                     CacheDatabaseType.Authentication, key);

            if (credentials != null && !credentials.IsExpired())
            {
                return credentials;
            }

            AuthenticationToken token = await GetAppPlusUserTokenAsync(
                principal,
                 authority,
                 service.Configuration.PartnerCenterEndpoint);

            credentials = await PartnerCredentials.Instance.GenerateByUserCredentialsAsync(
                service.Configuration.PartnerCenterApplicationId, token);

            await service.Cache.StoreAsync(
               CacheDatabaseType.Authentication, key, credentials);

            return credentials;
        }

        /// <summary>
        /// Gets an access token utilizing an authorization code. 
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="code">Authorization code received from the service authorization endpoint.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="redirectUri">Redirect URI used for obtain the authorization code.</param>
        /// <returns>An instance of <see cref="AuthenticationResult"/> that represented the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="code"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="redirectUri"/> is null.
        /// </exception>
        public async Task<AuthenticationResult> GetTokenByAuthorizationCodeAsync(string authority, string code, string resource, Uri redirectUri)
        {
            AuthenticationContext authContext;

            authority.AssertNotEmpty(nameof(authority));
            code.AssertNotEmpty(nameof(code));
            redirectUri.AssertNotNull(nameof(redirectUri));
            resource.AssertNotEmpty(nameof(resource));

            try
            {
                authContext = new AuthenticationContext(authority);

                return await authContext.AcquireTokenByAuthorizationCodeAsync(
                    code,
                    redirectUri,
                    new ClientCredential(
                        service.Configuration.ApplicationId,
                        service.Configuration.ApplicationSecret),
                    resource);
            }
            finally
            {
                authContext = null;
            }
        }

        /// <summary>
        /// Locates a certificate by thumbprint.
        /// </summary>
        /// <param name="thumbprint">Thumbprint of the certificate to be located.</param>
        /// <returns>An instance of <see cref="X509Certificate2"/> that represents the certificate.</returns>
        private static X509Certificate2 FindCertificateByThumbprint(string thumbprint)
        {
            X509Store store = null;
            X509Certificate2Collection col;

            thumbprint.AssertNotNull(nameof(thumbprint));

            try
            {
                store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);

                col = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                return col.Count == 0 ? null : col[0];
            }
            finally
            {
                col = null;
                store?.Close();
                store = null;
            }
        }
    }
}