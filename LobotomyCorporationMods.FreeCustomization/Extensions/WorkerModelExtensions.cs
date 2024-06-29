// SPDX-License-Identifier: MIT

#region

using Customizing;
using JetBrains.Annotations;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Extensions
{
    internal static class WorkerModelExtensions
    {
        [NotNull]
        internal static Appearance GetAppearanceData([NotNull] this WorkerModel workerModel)
        {
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
                EyeColor = workerModel.spriteData.EyeColor,
            };
        }

        internal static void SetAppearanceData([NotNull] this WorkerModel workerModel,
            [NotNull] Appearance appearance)
        {
            workerModel.spriteData = appearance.spriteSet;
            workerModel.spriteData.BattleEyeBrow = appearance.Eye_Battle;
            workerModel.spriteData.FrontHair = appearance.FrontHair;
            workerModel.spriteData.RearHair = appearance.RearHair;
            workerModel.spriteData.EyeBrow = appearance.Eyebrow_Def;
            workerModel.spriteData.PanicEyeBrow = appearance.Eyebrow_Panic;
            workerModel.spriteData.BattleEyeBrow = appearance.Eye_Battle;
            workerModel.spriteData.Eye = appearance.Eye_Def;
            workerModel.spriteData.EyePanic = appearance.Eye_Panic;
            workerModel.spriteData.EyeDead = appearance.Eye_Dead;
            workerModel.spriteData.Mouth = appearance.Mouth_Def;
            workerModel.spriteData.BattleMouth = appearance.Mouth_Battle;
            workerModel.spriteData.HairColor = appearance.HairColor;
            workerModel.spriteData.EyeColor = appearance.EyeColor;
        }
    }
}
