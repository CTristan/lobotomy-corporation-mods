// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace LobotomyPlaywright.Queries
{
    public class SefiraData
    {
        public string Name { get; set; }
        public string SefiraEnum { get; set; }
        public bool IsOpen { get; set; }
        public int OpenLevel { get; set; }
        public List<long> AgentIds { get; set; }
        public List<long> CreatureIds { get; set; }
        public int OfficerCount { get; set; }
    }
}
