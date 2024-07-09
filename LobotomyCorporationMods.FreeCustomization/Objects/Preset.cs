// SPDX-License-Identifier: MIT

using System;
using System.Xml.Serialization;

namespace LobotomyCorporationMods.FreeCustomization.Objects
{
    [Serializable]
    public sealed class Preset
    {
        [XmlAttribute("AgentName")]
        public string AgentName { get; set; }

        [XmlElement("AgentData")]
        public WorkerSprite.WorkerSprite AgentData { get; set; }
    }
}
