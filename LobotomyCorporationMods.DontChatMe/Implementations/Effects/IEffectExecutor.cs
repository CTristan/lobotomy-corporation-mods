// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.DontChatMe.Models;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Implementations.Effects
{
    /// <summary>
    ///     Contract every effect implements.
    ///     Lifecycle is: the dispatcher looks the effect up by <see cref="Slug" />, runs gate checks,
    ///     then calls <see cref="Execute" />. The executor returns <c>null</c> on success or one of
    ///     the strings in <see cref="Constants.ErrorTags" /> on failure (e.g. precondition not met).
    /// </summary>
    public interface IEffectExecutor
    {
        /// <summary>Wire identifier the chat-side server uses for this effect.</summary>
        string Slug { get; }

        /// <summary>Per-slug minimum gap between firings. <c>0</c> means no per-slug cooldown.</summary>
        float CooldownSeconds { get; }

        /// <summary>
        ///     When <c>true</c>, the effect is gated by the <c>DangerEffectsEnabled</c> config flag.
        ///     Use for effects that can swing a run, like releasing a creature.
        /// </summary>
        bool IsDanger { get; }

        /// <summary>
        ///     Runs the effect on the Unity main thread.
        ///     Returns <c>null</c> on success, or one of the <see cref="Constants.ErrorTags" /> strings.
        /// </summary>
        string Execute(EffectDispatch dispatch);
    }
}
