// -----------------------------------------------------------------------
// <copyright file="AiExceptionLogger.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Logic
{
    using System.Web.Http.ExceptionHandling;
    using Autofac;
    using Providers;

    /// <summary>
    /// Represents an unhandled exception logger.
    /// </summary>
    /// <seealso cref="ExceptionLogger" />
    public class AiExceptionLogger : ExceptionLogger
    {
        /// <summary>
        /// Logs the specified exception.
        /// </summary>
        /// <param name="context">The exception logger context</param>
        public override void Log(ExceptionLoggerContext context)
        {
            using (ILifetimeScope scope = WebApiApplication.Container.BeginLifetimeScope())
            {
                IBotProvider provider = scope.Resolve<IBotProvider>();

                if (context?.Exception != null)
                {
                    provider.Telemetry.TrackException(context.Exception);
                }
            }

            base.Log(context);
        }
    }
}