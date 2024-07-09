// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LobotomyCorporationMods.FreeCustomization.Objects
{
    [Serializable]
    [XmlRoot("Presets")]
    public class PresetList
    {
        [XmlElement("Preset")]
        public ICollection<Preset> Presets { get; } = new List<Preset>();
    }
}
