// SPDX-License-Identifier: MIT

namespace DebugPanel.Common.Interfaces
{
    /// <summary>An adapter to use for testing Unity game objects.</summary>
    /// <typeparam name="T">The type of the game object being adapted.</typeparam>
    public interface ITestAdapter<T>
    {
        T GameObject { get; set; }
    }
}
