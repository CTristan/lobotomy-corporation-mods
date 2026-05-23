// SPDX-License-Identifier: MIT

#region

using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Interfaces;
using LobotomyCorporationMods.DontChatMe.Models;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Implementations.Effects
{
    /// <summary>
    ///     Shows a chat-side notification in the in-game system log.
    ///     If the dispatch carries a <c>user_display_name</c>, the message reads
    ///     "{name} stopped by from chat."; otherwise it falls back to a generic note.
    /// </summary>
    public sealed class ShowSystemMessageEffect : IEffectExecutor
    {
        public const string GenericMessage = "A viewer stopped by from chat.";

        private readonly IGameAdapter _gameAdapter;

        public ShowSystemMessageEffect(IGameAdapter gameAdapter)
        {
            ThrowHelper.ThrowIfNull(gameAdapter, nameof(gameAdapter));
            _gameAdapter = gameAdapter;
        }

        public string Slug => EffectSlugs.ShowSystemMessage;
        public float CooldownSeconds => 5f;
        public bool IsDanger => false;

        public string Execute(EffectDispatch dispatch)
        {
            ThrowHelper.ThrowIfNull(dispatch, nameof(dispatch));
            if (!_gameAdapter.IsGameReady)
            {
                return StandardErrors.GameStateBlocked;
            }

            _gameAdapter.ShowSystemMessage(BuildMessage(dispatch.UserDisplayName));
            return null;
        }

        public bool IsAvailableNow(out string reason)
        {
            if (!_gameAdapter.IsGameReady)
            {
                reason = StandardErrors.GameStateBlocked;
                return false;
            }

            reason = null;
            return true;
        }

        public static string BuildMessage(string userDisplayName)
        {
            return string.IsNullOrEmpty(userDisplayName)
                ? GenericMessage
                : userDisplayName + " stopped by from chat.";
        }
    }
}
