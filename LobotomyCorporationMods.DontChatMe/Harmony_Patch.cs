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
            Config = new DontChatMeConfig(TryGetConfigFilePath());
            GameAdapter = new UnityGameAdapter();
            CooldownGate = new CooldownGate(
                () => UnityEngine.Time.realtimeSinceStartup,
                Config.GlobalCooldownSeconds
            );
            IdempotencyCache = new IdempotencyCache(capacity: 32);
            HudState = new HudState(initiallyEnabled: Config.Enabled);
            SettingsState = new SettingsState();

            Transport = new WebSocketTransport(
                webSocketFactory: (uri, sub) => new WebSocketSharpAdapter(uri, sub),
                config: Config,
                onError: ex => Logger?.WriteException(ex),
                version: typeof(Harmony_Patch).Assembly.GetName().Version.ToString(3),
                lastSeenRedemptionIdProvider: () => IdempotencyCache.MostRecentTerminalRedemptionId
            );

            SettingsController = new SettingsController(Config, Transport);

            var executors = BuildExecutors(GameAdapter, Config);

            Dispatcher = new EffectDispatcher(
                config: Config,
                executors: executors,
                cooldownGate: CooldownGate,
                idempotencyCache: IdempotencyCache,
                sendResponse: Transport.SendResponse,
                logger: new DeferredLogger(() => Logger),
                onExecuted: HudState.RecordEffect
            );

            Pump = new RequestPump(
                drainCallback: Dispatcher.Dispatch,
                onOverwrite: displaced =>
                    UnityEngine.Debug.LogWarning(
                        "DontChatMe RequestPump slot overwritten — server dispatched while a prior frame was pending. Displaced redemption: "
                            + displaced.RedemptionId
                    )
            );

            AvailabilityProbe = new AvailabilityProbe(
                executors: executors,
                config: Config,
                send: Transport.SendEffectState,
                now: () => UnityEngine.Time.realtimeSinceStartup
            );

            GamePhaseProbe = new GamePhaseProbe(
                adapter: GameAdapter,
                send: Transport.SendGameState,
                now: () => UnityEngine.Time.realtimeSinceStartup
            );

            Transport.EffectReceived += OnEffectReceived;
            Transport.StateChanged += HudState.SetConnectionState;
            Transport.StateChanged += OnTransportStateChanged;

            // Start connecting immediately so the transport dials in as soon as the mod loads,
            // regardless of which scene is active. The GameManager.Update hook also calls this
            // but only fires once a game day is in progress.
            EnsureTransportStarted();
        }

        public IDontChatMeConfig Config { get; }
        public IGameAdapter GameAdapter { get; }
        public CooldownGate CooldownGate { get; }
        public IdempotencyCache IdempotencyCache { get; }
        public HudState HudState { get; }
        public SettingsState SettingsState { get; }
        public SettingsController SettingsController { get; }
        public WebSocketTransport Transport { get; }
        public EffectDispatcher Dispatcher { get; }
        public RequestPump Pump { get; }
        public AvailabilityProbe AvailabilityProbe { get; }
        public GamePhaseProbe GamePhaseProbe { get; }

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

            if (!Config.Enabled || Config.ServerUrl == null)
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
            // Server sequences strictly — at most one in-flight per game — so the slot is
            // expected to be empty here. If a prior dispatch is still pending, RequestPump
            // logs and overwrites; we never need to refuse work or send back a queue-full.
            Pump.Enqueue(dispatch);
        }

        private void OnTransportStateChanged(ConnectionState state)
        {
            if (state == ConnectionState.Connected)
            {
                AvailabilityProbe.RequestSnapshot();
                GamePhaseProbe.RequestSnapshot();
                // Tick immediately so the snapshot goes out on the socket thread (before the next
                // GameManager.Update fires). This ensures the initial effect_state reaches the
                // chat-side server even when the game is on the intro/title screen.
                AvailabilityProbe.Tick();
                GamePhaseProbe.Tick();
            }
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

            StatusOverlay.Attach(HudState, SettingsState, Config);
            SettingsWindow.Attach(SettingsState, SettingsController, Config);
        }

        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        private static string TryGetConfigFilePath()
        {
            try
            {
                var persistentDataPath = UnityEngine.Application.persistentDataPath;
                if (string.IsNullOrEmpty(persistentDataPath))
                {
                    return null;
                }

                return System.IO.Path.Combine(
                    System.IO.Path.Combine(
                        System.IO.Path.Combine(persistentDataPath, "LobotomyBaseMod"),
                        "DontChatMe"
                    ),
                    "config.cfg"
                );
            }
#pragma warning disable CA1031 // Application.persistentDataPath is unavailable outside a Unity runtime (e.g. in the test runner); treat as no-file-path.
            catch (Exception)
#pragma warning restore CA1031
            {
                return null;
            }
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
