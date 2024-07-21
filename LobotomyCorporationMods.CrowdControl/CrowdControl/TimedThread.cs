// SPDX-License-Identifier: MIT

using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace LobotomyCorporationMods.CrowdControl.CrowdControl
{
    public class TimedThread
    {
        private static readonly Collection<TimedThread> s_threads = new Collection<TimedThread>();
        private int Duration;
        private int Id { get; }
        private int Remaining { get; set; }
        private bool Paused { get; set; }

        public static void AddTime(int duration)
        {
            try
            {
                lock (s_threads)
                {
                    foreach (var thread in s_threads)
                    {
                        Interlocked.Add(ref thread.Duration, duration + 5);
                        if (thread.Paused)
                        {
                            continue;
                        }

                        var time = thread.Remaining;
                        new TimedResponse(thread.Id, time, CrowdControlResponseStatus.STATUS_PAUSE).Send(CrowdControlClient.Socket);
                        thread.Paused = true;
                    }
                }
            }
            catch (Exception e)
            {
                Harmony_Patch.Instance.Logger.LogException(e);
                throw;
            }
        }

        public static void UnPause()
        {
            try
            {
                lock (s_threads)
                {
                    foreach (var thread in s_threads)
                    {
                        if (!thread.Paused)
                        {
                            continue;
                        }

                        var time = thread.Remaining;
                        new TimedResponse(thread.Id, time, CrowdControlResponseStatus.STATUS_RESUME).Send(CrowdControlClient.Socket);
                        thread.Paused = false;
                    }
                }
            }
            catch (Exception e)
            {
                Harmony_Patch.Instance.Logger.LogException(e);
                throw;
            }
        }

        public static void TickTime(int duration)
        {
            try
            {
                lock (s_threads)
                {
                    foreach (var thread in s_threads)
                    {
                        var time = thread.Remaining;
                        time -= duration;
                        if (time < 0)
                        {
                            time = 0;
                        }

                        thread.Remaining = time;
                    }
                }
            }
            catch (Exception e)
            {
                Harmony_Patch.Instance.Logger.LogException(e);
                throw;
            }
        }
    }
}
