// -----------------------------------------------------------------------
// <copyright file="QuestionIntent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Intents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Dialogs;
    using Extensions;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using Security;
    using Providers; 

    /// <summary>
    /// Processes the question intent.
    /// </summary>
    /// <seealso cref="IIntent" />
    public class QuestionIntent : IIntent
    {
        /// <summary>
        /// Gets the message to be displayed when help has been requested.
        /// </summary>
        public string HelpMessage => Resources.QuestionHelpMessage;

        /// <summary>
        /// Gets the name of the intent.
        /// </summary>
        public string Name => IntentConstants.Question;

        /// <summary>
        /// Gets the permissions required to perform the operation represented by this intent.
        /// </summary>
        public UserRoles Permissions => UserRoles.Any;

        /// <summary>
        /// Performs the operation represented by this intent.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="message">The message from the authenticated user.</param>
        /// <param name="result">The result from Language Understanding cognitive service.</param>
        /// <param name="provider">Provides access to core services;.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="context"/> is null.
        /// or
        /// <paramref name="message"/> is null.
        /// or
        /// <paramref name="result"/> is null.
        /// or 
        /// <paramref name="provider"/> is null.
        /// </exception>
        public async Task ExecuteAsync(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result, IBotProvider provider)
        {
            IMessageActivity messageActivity;

            context.AssertNotNull(nameof(context));
            message.AssertNotNull(nameof(message));
            result.AssertNotNull(nameof(result));
            provider.AssertNotNull(nameof(provider));

            try
            {
                messageActivity = await message;

                await context.Forward(
                    new QuestionDialog(provider),
                    ResumeAfterQnA,
                    messageActivity,
                    CancellationToken.None);
            }
            finally
            {
                messageActivity = null;
            }
        }

        /// <summary>
        /// Resumes the conversation once the question has been answered.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="result">The result returned from </param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        private static async Task ResumeAfterQnA(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            IMessageActivity messageActivity;

            try
            {
                messageActivity = await result;
                await context.PostAsync(messageActivity);
            }
            finally
            {
                messageActivity = null;
            }
        }
    }
}