// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace Hemocode.Playwright.Queries
{
    public sealed class SefiraData
    {
        public string Name { get; set; }
        public string SefiraEnum { get; set; }
        public bool IsOpen { get; set; }
        public int OpenLevel { get; set; }
        public ICollection<long> AgentIds { get; set; }
        public ICollection<long> CreatureIds { get; set; }
        public int OfficerCount { get; set; }
    }
}
