// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Configuration;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Dispatch;
using LobotomyCorporationMods.DontChatMe.Implementations;
using LobotomyCorporationMods.DontChatMe.Implementations.Effects;
using LobotomyCorporationMods.DontChatMe.Interfaces;
using LobotomyCorporationMods.DontChatMe.Models;
using LobotomyCorporationMods.DontChatMe.Transport;
using LobotomyCorporationMods.DontChatMe.UiComponents;

#endregion

namespace LobotomyCorporationMods.DontChatMe
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase<Harmony_Patch>
    {
        public static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        private int _transportStarted;
        private int _overlayAttached;

        public Harmony_Patch() { }

        private Harmony_Patch(bool initialize)
            : base(initialize)
        {
            Config = new DontChatMeConfig();
            GameAdapter = new UnityGameAdapter();
            CooldownGate = new CooldownGate(
                () => UnityEngine.Time.realtimeSinceStartup,
                Config.GlobalCooldownSeconds
            );
            IdempotencyCache = new IdempotencyCache(capacity: 1024);
            HudState = new HudState(initiallyEnabled: Config.Enabled);

            Transport = new WebSocketTransport(
                webSocketFactory: uri => new WebSocketSharpAdapter(uri),
                config: Config,
                onError: ex => Logger?.WriteException(ex),
                version: typeof(Harmony_Patch).Assembly.GetName().Version.ToString(3)
            );

            Dispatcher = new EffectDispatcher(
                config: Config,
                executors: BuildExecutors(GameAdapter, Config),
                cooldownGate: CooldownGate,
                idempotencyCache: IdempotencyCache,
                sendReply: Transport.SendReply,
                logger: new DeferredLogger(() => Logger),
                onExecuted: HudState.RecordEffect
            );

            Pump = new RequestPump(
                capacity: Config.MaxInFlight,
                maxPerTick: 4,
                drainCallback: Dispatcher.Dispatch
            );

            Transport.EffectReceived += OnEffectReceived;
            Transport.StateChanged += HudState.SetConnectionState;
        }

        public IDontChatMeConfig Config { get; }
        public IGameAdapter GameAdapter { get; }
        public CooldownGate CooldownGate { get; }
        public IdempotencyCache IdempotencyCache { get; }
        public HudState HudState { get; }
        public WebSocketTransport Transport { get; }
        public EffectDispatcher Dispatcher { get; }
        public RequestPump Pump { get; }

        /// <summary>
        ///     Lazily opens the WebSocket on the first tick after the game enters play.
        ///     Idempotent and safe to call from the Harmony postfix every frame.
        /// </summary>
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public void EnsureTransportStarted()
        {
            if (Interlocked.Exchange(ref _transportStarted, 1) == 1)
            {
                return;
            }

            if (!Config.Enabled)
            {
                return;
            }

            // Connect on a background thread so the first game-tick that triggers us doesn't
            // block waiting for the WebSocket handshake.
            var thread = new Thread(() =>
            {
                try
                {
                    Transport.Start();
                }
#pragma warning disable CA1031 // Connect-time exceptions are surfaced via WriteException and the reconnect loop; we mustn't take down the worker thread.
                catch (Exception ex)
#pragma warning restore CA1031
                {
                    Logger.WriteException(ex);
                }
            })
            {
                IsBackground = true,
                Name = "DontChatMe.InitialConnect",
            };
            thread.Start();
        }

        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        private void OnEffectReceived(EffectDispatch dispatch)
        {
            if (Pump.TryEnqueue(dispatch))
            {
                return;
            }

            Transport.SendReply(EffectReply.Failed(dispatch.RedemptionId, ErrorTags.Overloaded));
        }

        /// <summary>
        ///     Lazily attaches the IMGUI overlay GameObject on the first game tick. Idempotent.
        /// </summary>
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public void EnsureOverlayAttached()
        {
            if (Interlocked.Exchange(ref _overlayAttached, 1) == 1)
            {
                return;
            }

            StatusOverlay.Attach(HudState);
        }

        private static IEffectExecutor[] BuildExecutors(
            IGameAdapter adapter,
            IDontChatMeConfig config
        )
        {
            return new IEffectExecutor[]
            {
                new RandomMeltdownEffect(adapter),
                new KillRandomAgentEffect(adapter),
                new RandomAgentPanicEffect(adapter),
                new AddEnergyEffect(adapter, config),
                new RemoveEnergyEffect(adapter, config),
                new AddMoneyEffect(adapter, config),
                new ShowSystemMessageEffect(adapter),
                new SetGameSpeedEffect(adapter),
                new EscapeRandomCreatureEffect(adapter),
            };
        }
    }
}
