// -----------------------------------------------------------------------
// <copyright file="CustomBotAuthentication.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Security
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;
    using Autofac;
    using Logic;
    using Microsoft.Bot.Connector;

    /// <summary>
    /// Provides custom authentication for the bot itself.
    /// </summary>
    /// <seealso cref="BotAuthentication" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomBotAuthentication : BotAuthentication
    {
        /// <summary>
        ///  Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            using (ILifetimeScope scope = WebApiApplication.Container.BeginLifetimeScope())
            {
                IBotService service = scope.Resolve<IBotService>();

                this.MicrosoftAppId = service.Configuration.MicrosoftAppId;
                this.MicrosoftAppPassword = service.Configuration.MicrosoftAppPassword;
            }

            return base.OnActionExecutingAsync(actionContext, cancellationToken);
        }
    }
}