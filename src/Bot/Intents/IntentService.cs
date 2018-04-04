// -----------------------------------------------------------------------
// <copyright file="IntentService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Intents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Extensions;
    using Providers;

    /// <summary>
    /// Provides access to the supported intents.
    /// </summary>
    /// <seealso cref="IIntentService" />
    public class IntentService : IIntentService
    {
        /// <summary>
        /// Provides access to core core services.
        /// </summary>
        private readonly IBotProvider provider;
        
        /// <summary>
        /// Provides a collection of supported intents.
        /// </summary>
        private Dictionary<string, IIntent> intents;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntentService"/> class.
        /// </summary>
        /// <param name="provider">Provides access to core services.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="provider"/> is null.
        /// </exception>
        public IntentService(IBotProvider provider)
        {
            provider.AssertNotNull(nameof(provider));
            this.provider = provider;
        }

        /// <summary>
        /// Gets the supported intents.
        /// </summary>
        public IDictionary<string, IIntent> Intents => this.intents;

        /// <summary>
        /// Initializes the intent service.
        /// </summary>
        public void Initialize()
        {
            IEnumerable<Type> intentTypes;
            IIntent intent;

            try
            {
                intents = new Dictionary<string, IIntent>();
                intentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && t.GetInterface("IIntent") != null);

                foreach (Type t in intentTypes)
                {
                    intent = Activator.CreateInstance(t) as IIntent;

                    if (intent == null)
                    {
                        continue;
                    }

                    intents.Add(intent.Name, intent);
                    provider.Telemetry.TrackTrace($"Initialized {intent.Name} intent.");
                }
            }
            finally
            {
                intentTypes = null;
            }
        }
    }
}