// SPDX-License-Identifier: MIT

using System;
using Avalonia;

namespace RetargetHarmony.Installer
{
    /// <summary>
    /// Entry point for the RetargetHarmony Installer application.
    /// </summary>
    public sealed class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            _ = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        /// <summary>
        /// Builds the Avalonia application configuration.
        /// </summary>
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
        }
    }
}
