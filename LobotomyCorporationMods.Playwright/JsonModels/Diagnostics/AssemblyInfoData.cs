// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;

using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Models.Diagnostics;

#endregion

namespace LobotomyCorporationMods.Playwright.JsonModels.Diagnostics
{
    [Serializable]
    public sealed class AssemblyInfoData
    {
        public string name;
        public string version;
        public string location;
        public bool isHarmonyRelated;

        public static AssemblyInfoData FromModel(AssemblyInfo model)
        {
            ThrowHelper.ThrowIfNull(model);

            return new AssemblyInfoData
            {
                name = model.Name,
                version = model.Version,
                location = model.Location,
                isHarmonyRelated = model.IsHarmonyRelated,
            };
        }

        public static AssemblyInfoData[] FromModels(IList<AssemblyInfo> models)
        {
            ThrowHelper.ThrowIfNull(models);

            var result = new AssemblyInfoData[models.Count];
            for (var i = 0; i < models.Count; i++)
            {
                result[i] = FromModel(models[i]);
            }

            return result;
        }
    }
}
