// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes.ValidCodeCoverageExceptionAttributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.Common.Implementations
{
    /// <inheritdoc />
    /// <summary>Represents a dictionary with Unity adapters for testing game objects that performs additional checks to see if the underlying Unity object was destroyed by the engine.</summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <typeparam name="TOther">The type of the other object being adapted.</typeparam>
    /// <remarks>Unity game objects have a different lifecycle of normal objects. This means we need to do a separate Unity null check to make sure that Unity hasn't destroyed the object.</remarks>
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    [Serializable]
    public class UnityAdapterDictionary<TKey, TValue, TOther> : Dictionary<TKey, TValue> where TValue : ITestAdapter<TOther> where TOther : class
    {
        public UnityAdapterDictionary()
        {
        }

        protected UnityAdapterDictionary(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public new bool ContainsKey([NotNull] TKey key)
        {
            return TryGetValue(key, out _);
        }

        public new bool TryGetValue([NotNull] TKey key,
            [CanBeNull] out TValue value)
        {
            var success = base.TryGetValue(key, out value);

            return success && value.GameObject != null;
        }
    }
}
