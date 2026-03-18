// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Implementations.Adapters.BaseClasses;
using Hemocode.Common.Interfaces.Adapters;
using UnityEngine;

namespace Hemocode.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class Texture2dTestAdapter : TextureTestAdapter<Texture2D>, ITexture2dTestAdapter
    {
        private const int DefaultTextureSize = 2;

        internal Texture2dTestAdapter() : this(new Texture2D(DefaultTextureSize, DefaultTextureSize))
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global
        internal Texture2dTestAdapter([NotNull] Texture2D gameObject) : base(gameObject)
        {
        }

        public bool LoadImage(byte[] data)
        {
            return GameObjectInternal.LoadImage(data);
        }
    }
}
