// // SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Enums
{
    public enum StandardConsoleCommandIds
    {
        None = 0,

        /*
         * Sends a message to the system log.
         * Not needed since it's more efficient to send the message directly.
         * AddSystemLog = 0
         */

        /// <summary>Adds the amount of energy to the day's quota. Usage: [float amount]</summary>
        EnergyFill = 1,

        /*
         * Displays a message through Angela.
         * Not needed since it's more efficient to send the message directly.
         * AngelaDescMake = 2
         */

        /*
         * Does nothing.
         * OpenListWindow = 3
         */

        /// <summary>Adds LOB points to the player. Usage: [int amount]</summary>
        AddMoney = 4,

        /*
         * Opens the inventory screen.
         * Not really useful for us.
         * InventoryOpen = 5
         */

        /*
         * Adds an amount to the emergency value which affects when trumpets show on-screen.
         * Not really useful for us.
         * EmregencyAdd = 6,
         */

        /*
         * Creates a visual damage effect, but has no gameplay value.
         * DamageInvoking = 7,
         */

        /// <summary>Adds a copy of the specified equipment to the EGO list. Usage: [long Equipment ID]</summary>
        MakeEquipment = 8,

        /*
         * Spawns a random ordeal for the set level (0-3), but only if the player is on a day that allows for that level of ordeal.
         * Because of that limitation, we'll instead use our own method that can spawn any level of ordeal on any day.
         * ActivateOrdeal = 9
         */

        /// <summary>Reloads all of the managerial bullets. No parameters.</summary>
        FullAmmo = 10,

        /// <summary>Fills up the Qliphoth Meltdown meter by the given value. Usage: [int amount]</summary>
        InvokeOverload = 11

        /*
         * Invokes a Sephirah Meltdown if the player meets the conditions required.
         * Because of that limitation, we need to use our own method instead.
         * Boss = 12
         */

        /*
         * Displays a message in the style of a Sephirah's line during a Core Suppression.
         * Limited use and no gameplay value.
         * SefiraBossConversation = 13
         */

        /*
         * Adds an abnormality to the "waiting list" of the next abnormalities to be available the next day.
         * Doesn't have an immediate effect and may never happen if the player stops playing before completing the current day.
         * WaitingCreature = 14,
         */

        /*
         * Sets the current mission for a specified Sephirah as completed.
         * Completing a mission doesn't take effect until the next day so the viewer may not even get to see the effect.
         * MissionClear = 15
         */

        /*
         * Fills all empty slots with randomly generated new hires.
         * Cute but ultimately pointless in almost all cases.
         * AllocateAgents = 16
         */

        /*
         * Opens the menu for the Rabbit Team.
         * Just opens the menu and can be closed by the player, so it's not really useful.
         * RabbitProtocol = 17
         */

        /*
         * Gives 8 specific gifts to all agents.
         * Interesting effect but we can do better since the gifts are the same every time.
         * PresentCluster = 18
         */

        /*
         * Clears up all abnormality overloads.
         * Overloads only last for at most 60 seconds, so by the time the viewer purchases it, either the player would have handled it or they would have expired.
         * ClearOverload = 19
         */

        /*
         * Instantly clears the Sephirah Meltdown.
         * These are the boss battles for the game, and you can only complete them once.
         * All this really does is take content away from both the player and the viewers.
         * ClearBoss = 20
         */

        /*
         * Instantly get all researches.
         * Not interesting and just serves to remove content.
         * ResearchAll = 21
         */

        /*
         * Unassigns all agents while in the Deployment screen.
         * Not useful for us.
         * DeallocateAll = 22,
         */

        /*
         * Starts the specified mission.
         * Not useful for us.
         * MissionAdd = 23,
         */
    }
}
