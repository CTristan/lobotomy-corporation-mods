// SPDX-License-Identifier: MIT

using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.FreeCustomization.Extensions
{
    internal static class WorkerModelExtensions
    {
        [NotNull]
        internal static Appearance GetAppearanceData([NotNull] this WorkerModel workerModel)
        {
            Guard.Against.Null(workerModel, nameof(workerModel));

            return new Appearance
            {
                spriteSet = workerModel.spriteData,
                Eyebrow_Battle = workerModel.spriteData.BattleEyeBrow,
                FrontHair = workerModel.spriteData.FrontHair,
                RearHair = workerModel.spriteData.RearHair,
                Eyebrow_Def = workerModel.spriteData.EyeBrow,
                Eyebrow_Panic = workerModel.spriteData.PanicEyeBrow,
                Eye_Battle = workerModel.spriteData.BattleEyeBrow,
                Eye_Def = workerModel.spriteData.Eye,
                Eye_Panic = workerModel.spriteData.EyePanic,
                Eye_Dead = workerModel.spriteData.EyeDead,
                Mouth_Def = workerModel.spriteData.Mouth,
                Mouth_Battle = workerModel.spriteData.BattleMouth,
                HairColor = workerModel.spriteData.HairColor,
                EyeColor = workerModel.spriteData.EyeColor
            };
        }
    }
}
