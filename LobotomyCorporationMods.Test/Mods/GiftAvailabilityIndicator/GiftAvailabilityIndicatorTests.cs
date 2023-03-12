// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.GiftAvailabilityIndicator;
using LobotomyCorporationMods.Test.Extensions;
using Moq;

#endregion

namespace LobotomyCorporationMods.Test.Mods.GiftAvailabilityIndicator
{
    public class GiftAvailabilityIndicatorTests
    {
        protected GiftAvailabilityIndicatorTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);
        }

        protected static Mock<IGameObjectAdapter> GetMockImageGameObject()
        {
            var imageGameObject = new Mock<IGameObjectAdapter>();
            var transform = new Mock<ITransformAdapter>();
            var componentAdapter = new Mock<IComponentAdapter>();

            imageGameObject.Setup(static adapter => adapter.Transform).Returns(transform.Object);
            imageGameObject.Setup(static adapter => adapter.AddComponentAdapter<TooltipMouseOver>()).Returns(componentAdapter.Object);
            transform.Setup(static adapter => adapter.ParentAdapter).Returns(transform.Object);
            componentAdapter.Setup(static adapter => adapter.GameObjectAdapter).Returns(imageGameObject.Object);

            return imageGameObject;
        }
    }
}
