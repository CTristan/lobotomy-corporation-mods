// SPDX-License-Identifier: MIT

using JetBrains.Annotations;

namespace LobotomyCorporationMods.Common.Interfaces.UiComponents
{
    public interface IUiImage : IUiComponent
    {
        void SetImageFromFile(string imagePath);

        void SetSize(float width,
            float height);
    }

    public static class IUiImageExtensions
    {
        public static bool IsUnityNull([CanBeNull] this IUiImage uiImage)
        {
            return uiImage == null || uiImage.AnyComponentIsNull();
        }
    }
}
