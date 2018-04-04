// -----------------------------------------------------------------------
// <copyright file="EnumExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot.Extensions
{
    using System;
    using System.ComponentModel;
    using System.Linq; 

    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the text from the description attribute.
        /// </summary>
        /// <param name="value">The enumeration value associated with the attribute.</param>
        /// <returns>A <see cref="string"/> containing the text from the description attribute.</returns>
        public static string GetDescription(this Enum value)
        {
            DescriptionAttribute attribute;

            try
            {
                attribute = value.GetType()
                    .GetField(value.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .SingleOrDefault() as DescriptionAttribute;

                return attribute == null ? value.ToString() : attribute.Description;
            }
            finally
            {
                attribute = null;
            }
        }
    }
}