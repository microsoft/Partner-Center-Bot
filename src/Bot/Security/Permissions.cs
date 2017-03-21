// -----------------------------------------------------------------------
// <copyright file="Permissions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Security
{
    using System.Collections.Generic;
    using Logic;

    /// <summary>
    /// Provides the ability to verify the authenticated user has the appropriate permissions.
    /// </summary>
    public static class Permissions
    {
        /// <summary>
        /// Gets a list of roles that required to perform the operation.
        /// </summary>
        /// <param name="requiredRole">User role required to perform the operation.</param>
        /// <returns>A list of roles that required to perform the operation.</returns>
        public static List<string> GetRoles(UserRoles requiredRole)
        {
            List<string> required = new List<string>();

            if (requiredRole.HasFlag(UserRoles.AdminAgents))
            {
                required.Add(UserRoles.AdminAgents.GetDescription());
            }

            if (requiredRole.HasFlag(UserRoles.BillingAdmin))
            {
                required.Add(UserRoles.BillingAdmin.GetDescription());
            }

            if (requiredRole.HasFlag(UserRoles.GlobalAdmin))
            {
                required.Add(UserRoles.GlobalAdmin.GetDescription());
            }

            if (requiredRole.HasFlag(UserRoles.HelpdeskAgent))
            {
                required.Add(UserRoles.HelpdeskAgent.GetDescription());
            }

            if (requiredRole.HasFlag(UserRoles.User))
            {
                required.Add(UserRoles.User.GetDescription());
            }

            if (requiredRole.HasFlag(UserRoles.UserAdministrator))
            {
                required.Add(UserRoles.UserAdministrator.GetDescription());
            }

            return required;
        }
    }
}