// // SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Enums
{
    public enum CreatureConsoleCommandIds
    {
        None = 0,

        /*
         * Does nothing
         * AddCreatureFeeling = 0,
         */

        /*
         * Does nothing
         * SubCreatureFeeling = 1,
         */

        /*
         * Does nothing
         * SetCreatureObservable = 2,
         */

        /// <summary>Deals red damage to the abnormality. Usage: [long Abnormality ID] [float amount]</summary>
        TakePhysicalDamage = 3,

        /// <summary>Adds unique PE-Boxes for the abnormality. Usage: [long Abnormality ID] [float amount]</summary>
        AddCumlativeCube = 4,

        /// <summary>
        ///     Reduces the abnormality's Qliphoth counter by the specified amount. Usage: [long Abnormality ID] [float
        ///     amount]
        /// </summary>
        QliportCounterReduce = 5,

        /// <summary>Suppresses all breaching abnormalities. Does not affect minions or ordeals.</summary>
        SuppressAll = 6
    }
}
