// SPDX-License-Identifier: MIT

namespace Hemocode.DebugPanel.Interfaces
{
    public interface IJsonParser
    {
        T FromJson<T>(string json);
    }
}
