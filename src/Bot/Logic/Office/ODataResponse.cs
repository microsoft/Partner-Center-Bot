// -----------------------------------------------------------------------
// <copyright file="ODataResponse.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Logic.Office
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a response from an OData endpoint.
    /// </summary>
    /// <typeparam name="T">Type returned from the API.</typeparam>
    internal class ODataResponse<T>
    {
        /// <summary>
        /// Gets or sets the value returned from the API.
        /// </summary>
        public List<T> Value { get; set; }
    }
}