// SPDX-License-Identifier: MIT

#region

using System;
using FluentAssertions;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using UnityEngine;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.CommonTests.AdapterTests
{
    /// <summary>
    ///     Since adapter classes are wrappers for Unity system calls, testing the methods will always fail. These are mostly
    ///     for code coverage.
    /// </summary>
    public sealed class AnimationScriptAdapterTests
    {
        [Fact]
        public void Able_to_unbox_animation_script_to_original_type()
        {
            var animationScript = TestExtensions.CreateBeautyBeastAnim();
            var adapter = new AnimationScriptAdapter(animationScript);

            var result = adapter.UnpackScriptAsType<BeautyBeastAnim>();

            result.Should().BeSameAs(animationScript);
        }

        [Fact]
        public void Valid_BeautyBeastAnim_does_not_error()
        {
            var animationScript = TestExtensions.CreateBeautyBeastAnim();
            var adapter = new AnimationScriptAdapter(animationScript);

            Action action = () => _ = adapter.BeautyAndTheBeastState;

            action.ShouldNotThrow();
        }

        [Fact]
        public void Invalid_BeautyBeastAnim_throws_non_Unity_exception()
        {
            var animationScript = TestExtensions.CreateCreatureAnimScript();
            var adapter = new AnimationScriptAdapter(animationScript);

            Action action = () => _ = adapter.BeautyAndTheBeastState;

            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void Valid_YggdrasilAnim_with_flowers_returns_number_of_flowers()
        {
            var flowers = new[] { TestExtensions.CreateGameObject() };
            var animationScript = TestExtensions.CreateYggdrasilAnim(flowers);
            var mockGameObjectAdapter = new Mock<IGameObjectAdapter>();
            mockGameObjectAdapter.Setup(static objectAdapter => objectAdapter.GameObjectIsActive(It.IsAny<GameObject>())).Returns(true);
            var adapter = new AnimationScriptAdapter(animationScript, mockGameObjectAdapter.Object);

            var result = adapter.ParasiteTreeNumberOfFlowers;

            result.Should().Be(1);
        }

        [Fact]
        public void Valid_YggdrasilAnim_with_no_flowers_does_not_throw_exception()
        {
            var animationScript = TestExtensions.CreateYggdrasilAnim();
            var adapter = new AnimationScriptAdapter(animationScript);

            Action action = () => _ = adapter.ParasiteTreeNumberOfFlowers;

            action.ShouldNotThrow();
        }

        [Fact]
        public void Invalid_YggdrasilAnim_throws_non_Unity_exception_when_checking_flowers()
        {
            var animationScript = TestExtensions.CreateCreatureAnimScript();
            var adapter = new AnimationScriptAdapter(animationScript);

            Action action = () => _ = adapter.ParasiteTreeNumberOfFlowers;

            action.ShouldThrow<InvalidOperationException>();
        }
    }
}
