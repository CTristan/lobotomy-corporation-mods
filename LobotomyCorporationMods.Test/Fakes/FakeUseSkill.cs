using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.Test.Fakes
{
    internal sealed class FakeUseSkill : UseSkill
    {
        public FakeUseSkill([NotNull] string giftName, long agentId, int numberOfSuccesses)
        {
            // Calling one of these constructors throws an exception, so we need to create an instance without
            // calling the constructor.
            agent = (AgentModel)FormatterServices.GetSafeUninitializedObject(typeof(AgentModel));
            agent.instanceId = agentId;
            targetCreature = (CreatureModel)FormatterServices.GetSafeUninitializedObject(typeof(CreatureModel));
            targetCreature.metaInfo = new CreatureTypeInfo
            {
                equipMakeInfos = new List<CreatureEquipmentMakeInfo> { new FakeCreatureEquipmentMakeInfo(giftName) }
            };
            successCount = numberOfSuccesses;
        }
    }
}
