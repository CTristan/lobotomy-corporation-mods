// SPDX-License-Identifier: MIT

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class StringExtensions
    {
        [NotNull]
        public static string JsonEscape(this string value)
        {
            Guard.Against.Null(value, nameof(value));

            return value.Replace("\\", @"\\").Replace("\"", "\\\"").Replace("\n", "\\n");
        }

        [NotNull]
        public static string GetLocalized(this string localizeTextId)
        {
            const string MissingLocalizationText = LocalizeTextDataModel.Failed;

            var localizedText = LocalizeTextDataModel.instance.GetText(localizeTextId);

            if (!localizedText.Equals(MissingLocalizationText, StringComparison.OrdinalIgnoreCase))
            {
                return localizedText;
            }

            DefaultLocalizedValues.TryGetDefaultLocalizedValue(localizeTextId, out localizedText);

            return localizedText ?? MissingLocalizationText;
        }
    }
}
