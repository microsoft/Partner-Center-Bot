// -----------------------------------------------------------------------
// <copyright file="BaseApiController.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Controllers
{
    using System.Web.Http;
    using Extensions;
    using Providers;    

    /// <summary>
    /// Base class for all API controllers.
    /// </summary>
    /// <seealso cref="ApiController" />
    public abstract class BaseApiController : ApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseApiController"/> class.
        /// </summary>
        /// <param name="provider">Provides access to core application services.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="provider"/> is null.
        /// </exception>
        protected BaseApiController(IBotProvider provider)
        {
            provider.AssertNotNull(nameof(provider));
            Provider = provider;
        }

        /// <summary>
        /// Gets a reference to the bot service.
        /// </summary>
        protected IBotProvider Provider { get; private set; }
    }
}