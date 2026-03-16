// SPDX-License-Identifier: MIT

#region

using System.Reflection;
using AwesomeAssertions;
using LobotomyCorporationMods.Playwright.Events;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Playwright.Test.Events
{
    public sealed class EventDataFactoryTests
    {
        private sealed class FakeAgent
        {
            public int instanceId { get; set; }
            public string name { get; set; }
            public float hp { get; set; }
            public float maxHp { get; set; }
            public float mental { get; set; }
            public float maxMental { get; set; }
            public string currentSefira { get; set; }
            public int panicLevel { get; set; }
            public int level { get; set; }
            public bool isDead { get; set; }
            public int workCount { get; set; }
        }

        private sealed class FakeCreature
        {
            public int instanceId { get; set; }
            public string name { get; set; }
            public int metaId { get; set; }
            public string riskLevel { get; set; }
            public bool isSuppressed { get; set; }
        }

        private sealed class FakeGift
        {
            public int instanceId { get; set; }
            public string name { get; set; }
            public int metaId { get; set; }
        }

        private static object GetProperty(object obj, string name)
        {
            return obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance)?.GetValue(obj);
        }

        [Fact]
        public void CreateAgentDeadEvent_extracts_properties_from_agent()
        {
            var agent = new FakeAgent
            {
                instanceId = 42,
                name = "TestAgent",
                hp = 0,
                maxHp = 100,
                mental = 50,
                maxMental = 80,
                currentSefira = "MALKUTH"
            };

            var result = EventDataFactory.CreateAgentDeadEvent(agent);

            GetProperty(result, "agentId").Should().Be(42);
            GetProperty(result, "agentName").Should().Be("TestAgent");
            GetProperty(result, "hp").Should().Be(0f);
            GetProperty(result, "maxHp").Should().Be(100f);
            GetProperty(result, "mental").Should().Be(50f);
            GetProperty(result, "maxMental").Should().Be(80f);
            GetProperty(result, "currentSefira").Should().Be("MALKUTH");
        }

        [Fact]
        public void CreateAgentDeadEvent_returns_defaults_for_null_agent()
        {
            var result = EventDataFactory.CreateAgentDeadEvent(null);

            GetProperty(result, "agentId").Should().Be(-1);
            GetProperty(result, "agentName").Should().Be("Unknown");
        }

        [Fact]
        public void CreateAgentPanicEvent_extracts_properties_from_agent()
        {
            var agent = new FakeAgent
            {
                instanceId = 10,
                name = "PanicAgent",
                mental = 0,
                maxMental = 100,
                panicLevel = 3
            };

            var result = EventDataFactory.CreateAgentPanicEvent(agent);

            GetProperty(result, "agentId").Should().Be(10);
            GetProperty(result, "agentName").Should().Be("PanicAgent");
            GetProperty(result, "mental").Should().Be(0f);
            GetProperty(result, "maxMental").Should().Be(100f);
            GetProperty(result, "panicLevel").Should().Be(3);
        }

        [Fact]
        public void CreateAgentPanicReturnEvent_extracts_properties_from_agent()
        {
            var agent = new FakeAgent { instanceId = 7, name = "ReturnAgent", mental = 50, maxMental = 100 };

            var result = EventDataFactory.CreateAgentPanicReturnEvent(agent);

            GetProperty(result, "agentId").Should().Be(7);
            GetProperty(result, "agentName").Should().Be("ReturnAgent");
        }

        [Fact]
        public void CreateAgentPromoteEvent_extracts_properties_from_agent()
        {
            var agent = new FakeAgent { instanceId = 5, name = "PromoteAgent", level = 3 };

            var result = EventDataFactory.CreateAgentPromoteEvent(agent);

            GetProperty(result, "agentId").Should().Be(5);
            GetProperty(result, "agentName").Should().Be("PromoteAgent");
            GetProperty(result, "level").Should().Be(3);
        }

        [Fact]
        public void CreateDeployAgentEvent_extracts_properties_from_unit()
        {
            var agent = new FakeAgent { instanceId = 12, name = "DeployAgent", currentSefira = "YESOD" };

            var result = EventDataFactory.CreateDeployAgentEvent(agent);

            GetProperty(result, "agentId").Should().Be(12);
            GetProperty(result, "agentName").Should().Be("DeployAgent");
            GetProperty(result, "currentSefira").Should().Be("YESOD");
        }

        [Fact]
        public void CreateRemoveAgentEvent_extracts_properties_from_unit()
        {
            var agent = new FakeAgent { instanceId = 15, name = "RemoveAgent", isDead = true };

            var result = EventDataFactory.CreateRemoveAgentEvent(agent);

            GetProperty(result, "agentId").Should().Be(15);
            GetProperty(result, "agentName").Should().Be("RemoveAgent");
            GetProperty(result, "wasDead").Should().Be(true);
        }

        [Fact]
        public void CreateWorkStartEvent_extracts_properties_from_creature()
        {
            var creature = new FakeCreature
            {
                instanceId = 20,
                name = "TestCreature",
                metaId = 100,
                riskLevel = "WAW"
            };

            var result = EventDataFactory.CreateWorkStartEvent(creature);

            GetProperty(result, "creatureId").Should().Be(20);
            GetProperty(result, "creatureName").Should().Be("TestCreature");
            GetProperty(result, "metadataId").Should().Be(100);
            GetProperty(result, "riskLevel").Should().Be("WAW");
        }

        [Fact]
        public void CreateWorkEndReportEvent_extracts_properties_from_agent()
        {
            var agent = new FakeAgent
            {
                instanceId = 30,
                name = "WorkAgent",
                hp = 80,
                maxHp = 100,
                mental = 60,
                maxMental = 100,
                workCount = 5
            };

            var result = EventDataFactory.CreateWorkEndReportEvent(agent);

            GetProperty(result, "agentId").Should().Be(30);
            GetProperty(result, "agentName").Should().Be("WorkAgent");
            GetProperty(result, "hp").Should().Be(80f);
            GetProperty(result, "workCount").Should().Be(5);
        }

        [Fact]
        public void CreateAddCreatureEvent_extracts_properties_from_creature()
        {
            var creature = new FakeCreature
            {
                instanceId = 50,
                name = "NewCreature",
                metaId = 200,
                riskLevel = "ALEPH"
            };

            var result = EventDataFactory.CreateAddCreatureEvent(creature);

            GetProperty(result, "creatureId").Should().Be(50);
            GetProperty(result, "creatureName").Should().Be("NewCreature");
            GetProperty(result, "metadataId").Should().Be(200);
            GetProperty(result, "riskLevel").Should().Be("ALEPH");
        }

        [Fact]
        public void CreateRemoveCreatureEvent_extracts_properties_from_creature()
        {
            var creature = new FakeCreature { instanceId = 60, name = "RemovedCreature", isSuppressed = true };

            var result = EventDataFactory.CreateRemoveCreatureEvent(creature);

            GetProperty(result, "creatureId").Should().Be(60);
            GetProperty(result, "creatureName").Should().Be("RemovedCreature");
            GetProperty(result, "wasSuppressed").Should().Be(true);
        }

        [Fact]
        public void CreateCreatureSuppressedEvent_extracts_properties_from_creature()
        {
            var creature = new FakeCreature { instanceId = 70, name = "Suppressed", metaId = 300 };

            var result = EventDataFactory.CreateCreatureSuppressedEvent(creature);

            GetProperty(result, "creatureId").Should().Be(70);
            GetProperty(result, "creatureName").Should().Be("Suppressed");
            GetProperty(result, "metadataId").Should().Be(300);
        }

        [Fact]
        public void CreateEscapeCreatureEvent_extracts_properties_from_creature()
        {
            var creature = new FakeCreature
            {
                instanceId = 80,
                name = "Escaped",
                metaId = 400,
                riskLevel = "HE"
            };

            var result = EventDataFactory.CreateEscapeCreatureEvent(creature);

            GetProperty(result, "creatureId").Should().Be(80);
            GetProperty(result, "creatureName").Should().Be("Escaped");
            GetProperty(result, "metadataId").Should().Be(400);
            GetProperty(result, "riskLevel").Should().Be("HE");
        }

        [Fact]
        public void CreateGetEGOgiftEvent_extracts_properties_from_gift()
        {
            var gift = new FakeGift { instanceId = 90, name = "TestGift", metaId = 500 };

            var result = EventDataFactory.CreateGetEGOgiftEvent(gift);

            GetProperty(result, "giftId").Should().Be(90);
            GetProperty(result, "giftName").Should().Be("TestGift");
            GetProperty(result, "metadataId").Should().Be(500);
        }

        [Fact]
        public void CreateWorkCoolTimeEndEvent_returns_empty_object()
        {
            EventDataFactory.CreateWorkCoolTimeEndEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateReleaseWorkEvent_returns_empty_object()
        {
            EventDataFactory.CreateReleaseWorkEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateCreatureObserveLevelEvent_returns_empty_object()
        {
            EventDataFactory.CreateCreatureObserveLevelEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateOrdealStartedEvent_returns_empty_object()
        {
            EventDataFactory.CreateOrdealStartedEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateOrdealActivatedEvent_returns_empty_object()
        {
            EventDataFactory.CreateOrdealActivatedEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateEmergencyLevelChangedEvent_returns_empty_object()
        {
            EventDataFactory.CreateEmergencyLevelChangedEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateOrdealEndEvent_returns_empty_object()
        {
            EventDataFactory.CreateOrdealEndEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateStageStartEvent_returns_empty_object()
        {
            EventDataFactory.CreateStageStartEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateStageEndEvent_returns_empty_object()
        {
            EventDataFactory.CreateStageEndEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateNextDayEvent_returns_empty_object()
        {
            EventDataFactory.CreateNextDayEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateFailStageEvent_returns_empty_object()
        {
            EventDataFactory.CreateFailStageEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateChangeGiftEvent_returns_empty_object()
        {
            EventDataFactory.CreateChangeGiftEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateMakeEquipmentEvent_returns_empty_object()
        {
            EventDataFactory.CreateMakeEquipmentEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateUpdateEnergyEvent_returns_empty_object()
        {
            EventDataFactory.CreateUpdateEnergyEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateSefiraEnabledEvent_returns_empty_object()
        {
            EventDataFactory.CreateSefiraEnabledEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateSefiraDisabledEvent_returns_empty_object()
        {
            EventDataFactory.CreateSefiraDisabledEvent().Should().NotBeNull();
        }

        [Fact]
        public void CreateAgentPanicEvent_returns_defaults_for_null_agent()
        {
            var result = EventDataFactory.CreateAgentPanicEvent(null);

            GetProperty(result, "agentId").Should().Be(-1);
            GetProperty(result, "agentName").Should().Be("Unknown");
        }

        [Fact]
        public void CreateWorkStartEvent_returns_defaults_for_null_creature()
        {
            var result = EventDataFactory.CreateWorkStartEvent(null);

            GetProperty(result, "creatureId").Should().Be(-1);
            GetProperty(result, "creatureName").Should().Be("Unknown");
        }

        [Fact]
        public void CreateRemoveCreatureEvent_returns_defaults_for_null_creature()
        {
            var result = EventDataFactory.CreateRemoveCreatureEvent(null);

            GetProperty(result, "creatureId").Should().Be(-1);
            GetProperty(result, "wasSuppressed").Should().Be(false);
        }
    }
}
