using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordApp.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DiscordApp.Services
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider provider;
        private readonly DiscordSocketClient client;
        private readonly CommandService service;
        private readonly IConfiguration configuration;

        public CommandHandler(DiscordSocketClient client, ILogger<CommandHandler> logger, IServiceProvider provider, CommandService commandService, IConfiguration config) : base(client, logger)
        {
            this.provider = provider;
            this.client = client;
            service = commandService;
            configuration = config;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            this.client.MessageReceived += OnMessageReceived;
            this.service.CommandExecuted += OnCommandExecuted;
            this.client.Connected += Client_Connected; 
            await this.service.AddModulesAsync(Assembly.GetEntryAssembly(), this.provider);
        }

        private Task Client_Connected()
        {
            JsonHelper jsonHelper = new JsonHelper();
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    JObject obj = jsonHelper.GetDataFromInitFile();
                    obj["Delimiter"] = "\\";
                    obj["Path"] = Directory.GetCurrentDirectory();
                    jsonHelper.WriteDataToInitFile(obj);
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    JObject obj = jsonHelper.GetDataFromInitFile();
                    obj["Delimiter"] = "/";
                    obj["Path"] = $"{Regex.Replace(Directory.GetCurrentDirectory(), @"", "")}";
                    jsonHelper.WriteDataToInitFile(obj);
                }
            }
            catch(Exception ex)
            {

            }
            return Task.CompletedTask;
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> CommandInfo, ICommandContext Context, Discord.Commands.IResult Result)
        {
            if (!Result.IsSuccess) await Context.Channel.SendMessageAsync(Result.ErrorReason);
            var messages = await ((ITextChannel)Context.Channel).GetMessagesAsync(10).FlattenAsync();
            messages = messages.Where(x => x.CreatedAt >= Context.Message.CreatedAt && (x.Author == Context.Message.Author || x.Author.Username == client.CurrentUser.Username));
            await Task.Delay(TimeSpan.FromSeconds(10));
            try
            {
                foreach (var message in messages)
                {
                    await Context.Channel.DeleteMessageAsync(message.Id);
                }
            }
            catch(Exception ex)
            {

            }
        }

        private async Task OnMessageReceived(SocketMessage SocketMessage)
        {
            if (!(SocketMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            if (!message.HasStringPrefix(this.configuration["Prefix"], ref argPos) && !message.HasMentionPrefix(this.client.CurrentUser, ref argPos)) return;

            var contex = new SocketCommandContext(this.client, message);
            await this.service.ExecuteAsync(contex, argPos, this.provider);
        }
    }
}
