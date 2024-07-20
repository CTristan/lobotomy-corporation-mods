// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces.UiComponents
{
    public interface IUiImage : IUiComponent
    {
        void SetImageFromFile(string imagePath);

        void SetSize(float width,
            float height);
    }
}
