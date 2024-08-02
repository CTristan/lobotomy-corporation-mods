// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using UnityEngine;
using WorkerSprite;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class AppearanceUIExtensions
    {
        /// <summary>Loads all the panic mouth sprites into the list of battle mouth sprites so that they can be used for customization.</summary>
        /// <param name="appearanceUI">The AppearanceUI instance.</param>
        public static void LoadPanicMouthSpritesIntoBattleMouths([NotNull] this AppearanceUI appearanceUI)
        {
            Guard.Against.Null(appearanceUI, nameof(appearanceUI));

            var mouthBattleSprites = new List<Sprite>();
            mouthBattleSprites.AddRange(appearanceUI.mouth_Battle.SpriteList);
            var basicData = WorkerSpriteManager.instance.basicData;
            if (basicData.GetData(BasicSpriteRegion.MOUTH_PANIC, out var mouthPanicSprites))
            {
                var allMouthPanicSprites = mouthPanicSprites.GetAllSprites();
                mouthBattleSprites.AddRange(allMouthPanicSprites);
            }

            appearanceUI.mouth_Battle.Init(mouthBattleSprites);
        }
    }
}
