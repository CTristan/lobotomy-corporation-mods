// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using LobotomyCorporation.Mods.Common;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Dispatch
{
    /// <summary>
    ///     Throttles effect execution two ways: a global minimum gap between any two effects, and a
    ///     per-slug minimum gap between successive firings of the same effect.
    ///     Time is read through a function so tests advance time without sleeping.
    /// </summary>
    public sealed class CooldownGate
    {
        private const float NeverFired = float.MinValue;

        private readonly Func<float> _now;
        private readonly float _globalCooldownSeconds;
        private readonly Dictionary<string, float> _perSlugLastFiredAt =
            new Dictionary<string, float>();
        private float _lastAnyEffectAt = NeverFired;

        public CooldownGate(Func<float> now, float globalCooldownSeconds)
        {
            ThrowHelper.ThrowIfNull(now, nameof(now));
            _now = now;
            _globalCooldownSeconds = globalCooldownSeconds;
        }

        public bool IsOnCooldown(string slug, float perSlugCooldownSeconds)
        {
            ThrowHelper.ThrowIfNull(slug, nameof(slug));
            var now = _now();

            if (
                _globalCooldownSeconds > 0f
                && _lastAnyEffectAt > NeverFired
                && now - _lastAnyEffectAt < _globalCooldownSeconds
            )
            {
                return true;
            }

            float lastForSlug;
            if (
                perSlugCooldownSeconds > 0f
                && _perSlugLastFiredAt.TryGetValue(slug, out lastForSlug)
                && now - lastForSlug < perSlugCooldownSeconds
            )
            {
                return true;
            }

            return false;
        }

        public void Mark(string slug)
        {
            ThrowHelper.ThrowIfNull(slug, nameof(slug));
            var now = _now();
            _lastAnyEffectAt = now;
            _perSlugLastFiredAt[slug] = now;
        }
    }
}
