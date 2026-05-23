// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Implementations.Effects;
using LobotomyCorporationMods.Test.ModTests.DontChatMeTests.Fakes;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.EffectTests
{
    /// <summary>
    ///     Asserts each effect surfaces its slug, cooldown, and danger flag — these are the wire
    ///     contract the dispatcher uses to route, throttle, and gate.
    /// </summary>
    public sealed class EffectMetadataTests : DontChatMeModTests
    {
        [Fact]
        public void All_effects_expose_their_slug_cooldown_and_danger_flag()
        {
            var adapter = new FakeGameAdapter();
            var config = new FakeConfig();

            new RandomMeltdownEffect(adapter).Slug.Should().Be(EffectSlugs.RandomMeltdown);
            new RandomMeltdownEffect(adapter).CooldownSeconds.Should().BeGreaterThan(0f);
            new RandomMeltdownEffect(adapter).IsDanger.Should().BeFalse();

            new KillRandomAgentEffect(adapter).Slug.Should().Be(EffectSlugs.KillRandomAgent);
            new KillRandomAgentEffect(adapter).CooldownSeconds.Should().BeGreaterThan(0f);
            new KillRandomAgentEffect(adapter).IsDanger.Should().BeFalse();

            new RandomAgentPanicEffect(adapter).Slug.Should().Be(EffectSlugs.RandomAgentPanic);
            new RandomAgentPanicEffect(adapter).CooldownSeconds.Should().BeGreaterThan(0f);
            new RandomAgentPanicEffect(adapter).IsDanger.Should().BeFalse();

            new AddEnergyEffect(adapter, config).Slug.Should().Be(EffectSlugs.AddEnergy);
            new AddEnergyEffect(adapter, config).CooldownSeconds.Should().BeGreaterThan(0f);
            new AddEnergyEffect(adapter, config).IsDanger.Should().BeFalse();

            new RemoveEnergyEffect(adapter, config).Slug.Should().Be(EffectSlugs.RemoveEnergy);
            new RemoveEnergyEffect(adapter, config).CooldownSeconds.Should().BeGreaterThan(0f);
            new RemoveEnergyEffect(adapter, config).IsDanger.Should().BeFalse();

            new AddMoneyEffect(adapter, config).Slug.Should().Be(EffectSlugs.AddMoney);
            new AddMoneyEffect(adapter, config).CooldownSeconds.Should().BeGreaterThan(0f);
            new AddMoneyEffect(adapter, config).IsDanger.Should().BeFalse();

            new ShowSystemMessageEffect(adapter).Slug.Should().Be(EffectSlugs.ShowSystemMessage);
            new ShowSystemMessageEffect(adapter).CooldownSeconds.Should().BeGreaterThan(0f);
            new ShowSystemMessageEffect(adapter).IsDanger.Should().BeFalse();

            new SetGameSpeedEffect(adapter).Slug.Should().Be(EffectSlugs.SetGameSpeed);
            new SetGameSpeedEffect(adapter).CooldownSeconds.Should().BeGreaterThan(0f);
            new SetGameSpeedEffect(adapter).IsDanger.Should().BeFalse();

            new EscapeRandomCreatureEffect(adapter)
                .Slug.Should()
                .Be(EffectSlugs.EscapeRandomCreature);
            new EscapeRandomCreatureEffect(adapter).CooldownSeconds.Should().BeGreaterThan(0f);
            new EscapeRandomCreatureEffect(adapter).IsDanger.Should().BeTrue();
        }
    }
}
