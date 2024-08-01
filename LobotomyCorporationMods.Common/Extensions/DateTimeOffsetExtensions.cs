// SPDX-License-Identifier: MIT

#region

using System;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;

#endregion

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class DateTimeOffsetExtensions
    {
        [NotNull]
        internal static string FormatDateTimeOffset(this DateTimeOffset dateTimeOffset)
        {
            var sb = new StringBuilder(8);
            sb.Append(dateTimeOffset.Year.ToString("0000", CultureInfo.InvariantCulture));
            sb.Append(dateTimeOffset.Month.ToString("00", CultureInfo.InvariantCulture));
            sb.Append(dateTimeOffset.Day.ToString("00", CultureInfo.InvariantCulture));

            return sb.ToString();
        }
    }
}
