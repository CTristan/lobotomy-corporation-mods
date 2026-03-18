// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Extensions;
using Hemocode.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

namespace Hemocode.Common.Implementations.Adapters.BaseClasses
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class TextureTestAdapter<T> : TestAdapter<T>, ITextureTestAdapter<T> where T : Texture
    {
        protected TextureTestAdapter([NotNull] T gameObject) : base(gameObject)
        {
        }

        public int Width =>
            GameObjectInternal.width;

        public int Height =>
            GameObjectInternal.height;

        public override T GameObject
        {
            get =>
                !GameObjectInternal.IsUnityNull() ? GameObjectInternal : null;
            set =>
                GameObjectInternal = value;
        }
    }
}
