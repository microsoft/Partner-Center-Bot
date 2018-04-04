// -----------------------------------------------------------------------
// <copyright file="IIntent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Intents
{
    using System.Threading.Tasks;
    using Logic;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using Security;
    using Providers; 

    /// <summary>
    /// Represents an intent discovered in the conversation.
    /// </summary>
    public interface IIntent
    {
        /// <summary>
        /// Gets the message to be displayed when help has been requested.
        /// </summary>
        string HelpMessage { get; }

        /// <summary>
        /// Gets the name of the intent.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the permissions required to perform the operation represented by this intent.
        /// </summary>
        UserRoles Permissions { get; }

        /// <summary>
        /// Performs the operation represented by this intent.
        /// </summary>
        /// <param name="context">The context of the conversational process.</param>
        /// <param name="message">The message from the authenticated user.</param>
        /// <param name="result">The result from Language Understanding cognitive service.</param>
        /// <param name="provider">Provides access to core services.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ExecuteAsync(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result, IBotProvider provider);
    }
}