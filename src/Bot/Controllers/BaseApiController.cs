// -----------------------------------------------------------------------
// <copyright file="BaseApiController.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Controllers
{
    using System.Web.Http;
    using Logic;

    /// <summary>
    /// Base class for all API controllers.
    /// </summary>
    /// <seealso cref="ApiController" />
    public abstract class BaseApiController : ApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseApiController"/> class.
        /// </summary>
        /// <param name="service">Provides access to core application services.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="service"/> is null.
        /// </exception>
        protected BaseApiController(IBotService service)
        {
            service.AssertNotNull(nameof(service));

            this.Service = service;
        }

        /// <summary>
        /// Gets a reference to the bot service.
        /// </summary>
        protected IBotService Service { get; private set; }
    }
}