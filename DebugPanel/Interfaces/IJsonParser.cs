// SPDX-License-Identifier: MIT

namespace DebugPanel.Interfaces
{
    public interface IJsonParser
    {
        T FromJson<T>(string json);
    }
}
