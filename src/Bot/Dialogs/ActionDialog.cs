// -----------------------------------------------------------------------
// <copyright file="ActionDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Dialogs
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using Security;
    using Providers;

    /// <summary>
    /// Dialog that handles communication with the user.
    /// </summary>
    /// <seealso>
    ///     <cref>Microsoft.Bot.Builder.Dialogs.LuisDialog{string}</cref>
    /// </seealso>
    [Serializable]
    public class ActionDialog : LuisDialog<string>
    {
        /// <summary>
        /// Provides access to core application services.
        /// </summary>
        private readonly IBotProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionDialog"/> class.
        /// </summary>
        /// <param name="provider">Provides access to core application services.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="provider"/> is null.
        /// </exception>
        public ActionDialog(IBotProvider provider) :
            base(new LuisService(new LuisModelAttribute(provider.Configuration.LuisAppId, provider.Configuration.LuisApiKey.ToUnsecureString())))
        {
            provider.AssertNotNull(nameof(provider));
            this.provider = provider;
        }

        /// <summary>
        /// Processes the request for help.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task HelpAsync(IDialogContext context)
        {
            CustomerPrincipal principal;
            IMessageActivity message;
            StringBuilder builder;

            context.AssertNotNull(nameof(context));

            try
            {
                message = context.MakeMessage();

                principal = await context.GetCustomerPrincipalAsync(provider);

                if (principal == null)
                {
                    message.Text = Resources.NotAuthenticatedHelpMessage;
                }
                else
                {
                    builder = new StringBuilder();
                    builder.AppendLine($"{Resources.HelpMessage}\n\n");

                    principal.AvailableIntents.Where(x => !string.IsNullOrEmpty(x.Value.HelpMessage)).Aggregate(
                        builder, (sb, pair) => sb.AppendLine($"* {pair.Value.HelpMessage}\n"));

                    message.Text = builder.ToString();
                }

                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
            finally
            {
                builder = null;
                message = null;
                principal = null;
            }
        }

        /// <summary>
        /// Routes intents to the appropriate handler.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="message">The message from the authenticated user.</param>
        /// <param name="result">Result from the Language Understanding Intelligent Service(LUIS).</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task RouteIntentAsync(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            CustomerPrincipal principal;
            string key;

            context.AssertNotNull(nameof(context));
            result.AssertNotNull(nameof(result));

            try
            {
                key = result.TopScoringIntent.Intent.ToCamelCase();

                principal = await context.GetCustomerPrincipalAsync(provider);

                if (principal == null)
                {
                    await HelpAsync(context);
                    return;
                }

                if (principal.AvailableIntents.ContainsKey(key))
                {
                    await principal.AvailableIntents[key]
                        .ExecuteAsync(context, message, result, provider);
                }
                else
                {
                    await HelpAsync(context);
                }
            }
            finally
            {
                principal = null;
            }
        }

        /// <summary>
        /// Processes messages received by the bot from the user.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="item">The message from the conversation.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            IMessageActivity message;

            try
            {
                message = await item;

                if (message.Text.Equals(Resources.Login, StringComparison.CurrentCultureIgnoreCase))
                {
                    await context.Forward(
                        new AuthDialog(provider, message),
                        ResumeAfterAuth,
                        message,
                        CancellationToken.None);
                }
                else if (message.Text.Equals(Resources.Help, StringComparison.CurrentCultureIgnoreCase))
                {
                    await HelpAsync(context);
                }
                else
                {
                    await base.MessageReceived(context, item);
                }
            }
            finally
            {
                message = null;
            }
        }

        /// <summary>
        /// Resumes the conversation once the authentication process has completed.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="result">The result returned from </param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> is null.
        /// or
        /// <paramref name="result"/> is null.
        /// </exception>
        private async Task ResumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        {
            context.AssertNotNull(nameof(context));
            result.AssertNotNull(nameof(result));

            string message = await result;
            await context.PostAsync(message);

            await HelpAsync(context);
        }
    }
}