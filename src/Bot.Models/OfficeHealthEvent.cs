// -----------------------------------------------------------------------
// <copyright file="OfficeHealthEvent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Models
{
    using System; 

    public class OfficeHealthEvent
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the event status. 
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the status time.
        /// </summary>
        public DateTime StatusTime { get; set; }

        /// <summary>
        /// Gets or sets the workload.
        /// </summary>
        public string Workload { get; set; }

        /// <summary>
        /// Gets or sets the display name for the workload.
        /// </summary>
        public string WorkloadDisplayName { get; set; }
    }
}