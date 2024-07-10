// SPDX-License-Identifier: MIT

using System.Xml.Serialization;
using Customizing;

namespace LobotomyCorporationMods.FreeCustomization.Objects
{
    public sealed class Preset
    {
        [XmlAttribute("AgentName")]
        public string AgentName { get; set; }

        [XmlElement("AgentData")]
        public Appearance AgentData { get; set; }
    }
}
