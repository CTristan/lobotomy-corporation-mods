// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace Hemocode.Playwright.Events
{
    /// <summary>
    /// Subscribes to the game's Notice system and forwards events to subscribed TCP clients.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class NoticeSubscriber
    {
        private readonly List<int> _subscriptionIds;
        private bool _isSubscribed;

        public NoticeSubscriber()
        {
            _subscriptionIds = new List<int>();
            _isSubscribed = false;
        }

        /// <summary>
        /// Subscribe to all relevant game events.
        /// </summary>
        public void SubscribeToAllEvents()
        {
            if (_isSubscribed)
            {
                return;
            }

            try
            {
                // Subscribe to common game events
                // Note: Notice.instance.Observe returns an int subscription ID

                // Agent events
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnAgentDead, OnAgentDead));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnAgentPanic, OnAgentPanic));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnAgentPanicReturn, OnAgentPanicReturn));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnAgentPromote, OnAgentPromote));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.DeployAgent, OnDeployAgent));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.RemoveAgent, OnRemoveAgent));

                // Work events
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnWorkStart, OnWorkStart));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnWorkCoolTimeEnd, OnWorkCoolTimeEnd));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnReleaseWork, OnReleaseWork));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.WorkEndReport, OnWorkEndReport));

                // Creature events
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.AddCreature, OnAddCreature));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.RemoveCreature, OnRemoveCreature));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnCreatureSuppressed, OnCreatureSuppressed));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.EscapeCreature, OnEscapeCreature));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.CreatureObserveLevelAdded, OnCreatureObserveLevelAdded));

                // Ordeal events
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnOrdealStarted, OnOrdealStarted));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnOrdealActivated, OnOrdealActivated));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnEmergencyLevelChanged, OnEmergencyLevelChanged));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OrdealEnd, OnOrdealEnd));

                // Game state events
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnStageStart, OnStageStart));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnStageEnd, OnStageEnd));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnNextDay, OnNextDay));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnFailStage, OnFailStage));

                // E.G.O. events
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnGetEGOgift, OnGetEGOgift));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.OnChangeGift, OnChangeGift));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.MakeEquipment, OnMakeEquipment));

                // Energy events
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.UpdateEnergy, OnUpdateEnergy));

                // Department events
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.SefiraEnabled, OnSefiraEnabled));
                _subscriptionIds.Add(Notice.instance.Observe(NoticeName.SefiraDisabled, OnSefiraDisabled));

                _isSubscribed = true;
            }
            catch (Exception ex)
            {
                Server.TcpServer.LogError($"[LobotomyPlaywright] Failed to subscribe to events: {ex.Message}");
            }
        }

        /// <summary>
        /// Unsubscribe from all events.
        /// </summary>
        public void UnsubscribeFromAllEvents()
        {
            if (!_isSubscribed)
            {
                return;
            }

            try
            {
                // Note: We can't easily unsubscribe without tracking which event each ID corresponds to
                // For now, we'll just clear the subscription IDs and let them be cleaned up on game reload
                _subscriptionIds.Clear();
                _isSubscribed = false;
            }
            catch (Exception ex)
            {
                Server.TcpServer.LogError($"[LobotomyPlaywright] Failed to unsubscribe from events: {ex.Message}");
            }
        }

        private void BroadcastEvent(string eventName, object eventData)
        {
            try
            {
                EventSubscriptionManager.BroadcastEvent(eventName, eventData);
            }
            catch (Exception ex)
            {
                Server.TcpServer.LogError($"[LobotomyPlaywright] Failed to broadcast event {eventName}: {ex.Message}");
            }
        }

        // Agent event handlers

        private void OnAgentDead(params object[] param)
        {
            if (param.Length > 0 && param[0] != null)
            {
                var eventData = EventDataFactory.CreateAgentDeadEvent(param[0]);
                BroadcastEvent(NoticeName.OnAgentDead, eventData);
            }
        }

        private void OnAgentPanic(params object[] param)
        {
            if (param.Length > 0 && param[0] != null)
            {
                var eventData = EventDataFactory.CreateAgentPanicEvent(param[0]);
                BroadcastEvent(NoticeName.OnAgentPanic, eventData);
            }
        }

        private void OnAgentPanicReturn(params object[] param)
        {
            if (param.Length > 0 && param[0] != null)
            {
                var eventData = EventDataFactory.CreateAgentPanicReturnEvent(param[0]);
                BroadcastEvent(NoticeName.OnAgentPanicReturn, eventData);
            }
        }

        private void OnAgentPromote(params object[] param)
        {
            if (param.Length > 0 && param[0] != null)
            {
                var eventData = EventDataFactory.CreateAgentPromoteEvent(param[0]);
                BroadcastEvent(NoticeName.OnAgentPromote, eventData);
            }
        }

        private void OnDeployAgent(params object[] param)
        {
            if (param.Length > 0 && param[0] != null)
            {
                var eventData = EventDataFactory.CreateDeployAgentEvent(param[0]);
                BroadcastEvent(NoticeName.DeployAgent, eventData);
            }
        }

        private void OnRemoveAgent(params object[] param)
        {
            if (param.Length > 0 && param[0] != null)
            {
                var eventData = EventDataFactory.CreateRemoveAgentEvent(param[0]);
                BroadcastEvent(NoticeName.RemoveAgent, eventData);
            }
        }

        // Work event handlers

        private void OnWorkStart(params object[] param)
        {
            if (param.Length > 0 && param[0] != null)
            {
                var eventData = EventDataFactory.CreateWorkStartEvent(param[0]);
                BroadcastEvent(NoticeName.OnWorkStart, eventData);
            }
        }

        private void OnWorkCoolTimeEnd(params object[] param)
        {
            var eventData = EventDataFactory.CreateWorkCoolTimeEndEvent();
            BroadcastEvent(NoticeName.OnWorkCoolTimeEnd, eventData);
        }

        private void OnReleaseWork(params object[] param)
        {
            var eventData = EventDataFactory.CreateReleaseWorkEvent();
            BroadcastEvent(NoticeName.OnReleaseWork, eventData);
        }

        private void OnWorkEndReport(params object[] param)
        {
            if (param.Length > 0 && param[0] != null)
            {
                var eventData = EventDataFactory.CreateWorkEndReportEvent(param[0]);
                BroadcastEvent(NoticeName.WorkEndReport, eventData);
            }
        }

        // Creature event handlers

        private void OnAddCreature(params object[] param)
        {
            if (param.Length > 0 && param[0] != null)
            {
                var eventData = EventDataFactory.CreateAddCreatureEvent(param[0]);
                BroadcastEvent(NoticeName.AddCreature, eventData);
            }
        }

        private void OnRemoveCreature(params object[] param)
        {
            if (param.Length > 0 && param[0] != null)
            {
                var eventData = EventDataFactory.CreateRemoveCreatureEvent(param[0]);
                BroadcastEvent(NoticeName.RemoveCreature, eventData);
            }
        }

        private void OnCreatureSuppressed(params object[] param)
        {
            if (param.Length > 0 && param[0] != null)
            {
                var eventData = EventDataFactory.CreateCreatureSuppressedEvent(param[0]);
                BroadcastEvent(NoticeName.OnCreatureSuppressed, eventData);
            }
        }

        private void OnEscapeCreature(params object[] param)
        {
            if (param.Length > 0 && param[0] != null)
            {
                var eventData = EventDataFactory.CreateEscapeCreatureEvent(param[0]);
                BroadcastEvent(NoticeName.EscapeCreature, eventData);
            }
        }

        private void OnCreatureObserveLevelAdded(params object[] param)
        {
            var eventData = EventDataFactory.CreateCreatureObserveLevelEvent();
            BroadcastEvent(NoticeName.CreatureObserveLevelAdded, eventData);
        }

        // Ordeal event handlers

        private void OnOrdealStarted(params object[] param)
        {
            var eventData = EventDataFactory.CreateOrdealStartedEvent();
            BroadcastEvent(NoticeName.OnOrdealStarted, eventData);
        }

        private void OnOrdealActivated(params object[] param)
        {
            var eventData = EventDataFactory.CreateOrdealActivatedEvent();
            BroadcastEvent(NoticeName.OnOrdealActivated, eventData);
        }

        private void OnEmergencyLevelChanged(params object[] param)
        {
            var eventData = EventDataFactory.CreateEmergencyLevelChangedEvent();
            BroadcastEvent(NoticeName.OnEmergencyLevelChanged, eventData);
        }

        private void OnOrdealEnd(params object[] param)
        {
            var eventData = EventDataFactory.CreateOrdealEndEvent();
            BroadcastEvent(NoticeName.OrdealEnd, eventData);
        }

        // Game state event handlers

        private void OnStageStart(params object[] param)
        {
            var eventData = EventDataFactory.CreateStageStartEvent();
            BroadcastEvent(NoticeName.OnStageStart, eventData);
        }

        private void OnStageEnd(params object[] param)
        {
            var eventData = EventDataFactory.CreateStageEndEvent();
            BroadcastEvent(NoticeName.OnStageEnd, eventData);
        }

        private void OnNextDay(params object[] param)
        {
            var eventData = EventDataFactory.CreateNextDayEvent();
            BroadcastEvent(NoticeName.OnNextDay, eventData);
        }

        private void OnFailStage(params object[] param)
        {
            var eventData = EventDataFactory.CreateFailStageEvent();
            BroadcastEvent(NoticeName.OnFailStage, eventData);
        }

        // E.G.O. event handlers

        private void OnGetEGOgift(params object[] param)
        {
            if (param.Length > 0 && param[0] != null)
            {
                var eventData = EventDataFactory.CreateGetEGOgiftEvent(param[0]);
                BroadcastEvent(NoticeName.OnGetEGOgift, eventData);
            }
        }

        private void OnChangeGift(params object[] param)
        {
            var eventData = EventDataFactory.CreateChangeGiftEvent();
            BroadcastEvent(NoticeName.OnChangeGift, eventData);
        }

        private void OnMakeEquipment(params object[] param)
        {
            var eventData = EventDataFactory.CreateMakeEquipmentEvent();
            BroadcastEvent(NoticeName.MakeEquipment, eventData);
        }

        // Energy event handlers

        private void OnUpdateEnergy(params object[] param)
        {
            var eventData = EventDataFactory.CreateUpdateEnergyEvent();
            BroadcastEvent(NoticeName.UpdateEnergy, eventData);
        }

        // Department event handlers

        private void OnSefiraEnabled(params object[] param)
        {
            var eventData = EventDataFactory.CreateSefiraEnabledEvent();
            BroadcastEvent(NoticeName.SefiraEnabled, eventData);
        }

        private void OnSefiraDisabled(params object[] param)
        {
            var eventData = EventDataFactory.CreateSefiraDisabledEvent();
            BroadcastEvent(NoticeName.SefiraDisabled, eventData);
        }
    }
}
