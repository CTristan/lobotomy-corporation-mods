// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface IJsonParser
    {
        T FromJson<T>(string json);
    }
}
