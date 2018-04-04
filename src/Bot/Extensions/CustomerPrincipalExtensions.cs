// -----------------------------------------------------------------------
// <copyright file="CustomerPrincipalExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Extensions
{
    using System;
    using Security;

    public static class CustomerPrincipalExtensions
    {
        /// <summary>
        /// Ensures the given <see cref="CustomerPrincipal"/> has a valid customer context.
        /// </summary>
        /// <param name="principalToValidate">An instance of <see cref="CustomerPrincipal"/> to validate.</param>
        /// <param name="message">The message to report in the exception.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="principalToValidate"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="principalToValidate"/> does not contain a valid customer identifier.
        /// </exception>
        public static void AssertValidCustomerContext(this CustomerPrincipal principalToValidate, string message)
        {
            principalToValidate.AssertNotNull(nameof(principalToValidate));

            if (string.IsNullOrEmpty(principalToValidate.Operation.CustomerId))
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}