// SPDX-License-Identifier: MIT

#region

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class StringExtensions
    {
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
