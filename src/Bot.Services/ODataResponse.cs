// -----------------------------------------------------------------------
// <copyright file="ODataResponse.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Services
{
    using System.Collections.Generic; 

    internal class ODataResponse<T> where T: class 
    {
        public List<T> Value { get; set; }
    }
}