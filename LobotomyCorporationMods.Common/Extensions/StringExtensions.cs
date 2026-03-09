// SPDX-License-Identifier: MIT

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class StringExtensions
    {
        [NotNull]
        public static string GetLocalized(this string localizeTextId)
        {
            const string MissingLocalizationText = LocalizeTextDataModel.Failed;

            string localizedText = LocalizeTextDataModel.instance.GetText(localizeTextId);

            if (!localizedText.Equals(MissingLocalizationText, StringComparison.OrdinalIgnoreCase))
            {
                return localizedText;
            }

            _ = DefaultLocalizedValues.TryGetDefaultLocalizedValue(localizeTextId, out localizedText);

            return localizedText ?? MissingLocalizationText;
        }
    }
}
