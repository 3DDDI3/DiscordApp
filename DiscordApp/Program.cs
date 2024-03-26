using Discord;
using Discord.Addons.Hosting;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using DiscordApp.Modules;
using DiscordApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VkNet;
using VkNet.AudioBypassService.Extensions;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Utils;

namespace DiscordApp
{
    class Program
    {
        static async Task Main()
        {

#pragma warning disable CA1416 // Проверка совместимости платформы     
            string path = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) path = $@"{Directory.GetCurrentDirectory()}/appsettings.json";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) path = $@"{Directory.GetCurrentDirectory()}\appsettings.json";

            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    var configuration = new ConfigurationBuilder()
                        //.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(path, false, true)
                        .Build();
                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureDiscordHost((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Debug,
                        AlwaysDownloadUsers = false,
                        MessageCacheSize = 200,
                        GatewayIntents = GatewayIntents.All
                    };
                    config.Token = context.Configuration["Token"];
                })
                .UseInteractionService((context, config) =>
                {
                    config.LogLevel = LogSeverity.Info;
                    config.UseCompiledLambda = true;
                })
                .UseCommandService((context, config) =>
                {
                    config.CaseSensitiveCommands = false;
                    config.LogLevel = LogSeverity.Debug;
                    config.DefaultRunMode = RunMode.Sync;
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddHostedService<CommandHandler>()
                        .AddHostedService<InteractionHandler>();
                })
                .UseConsoleLifetime();
#pragma warning restore CA1416 // Проверка совместимости платформы
            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}
