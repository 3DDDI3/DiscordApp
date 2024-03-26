    using Discord;
using Discord.Commands;
using DiscordApp.Modules;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model;

namespace DiscordApp.Helper
{
    public class EmbedHelper
    {
        private SocketCommandContext context;
        public EmbedHelper() { }
        public EmbedHelper(SocketCommandContext context) => this.context = context;

        /// <summary>
        /// Получение Embed объекта
        /// </summary>
        /// <param name="AudioObject"></param>
        /// <returns></returns>
        public Embed GetEmbedAudioObject(Player Player, string title = null)
        {
            var user = context.Client.CurrentUser;
            EmbedFieldBuilder[] embedBuilder = new EmbedFieldBuilder[Player.Tracks.Count + 1];
            for (int i = 0; i < Player.Tracks.Count; i++)
            {
                embedBuilder[i] = new EmbedFieldBuilder(){
                    Name = "\u200B",
                    Value = String.Format("`[{0}]` - **{1} {2}** `[{3}]`", i + 1, Player.Tracks[i].Artist, Player.Tracks[i].Title, Player.Tracks[i].Duration)
                };
            }
            embedBuilder[embedBuilder.Length - 1] = new EmbedFieldBuilder()
            {
                Name = "\u200B",
                Value = $":warning: Это сообщение будет удалено через 10 с."
            };
            title = title == null ? "Выберите один из предложенных треков" : $"**{title}**";
                return new EmbedBuilder()
                .WithFields(embedBuilder)
                .WithTitle(title)
                .WithColor(Color.LightGrey)
                .WithFooter(new EmbedFooterBuilder() { IconUrl = user.GetAvatarUrl(), Text = $"{user.Username}" })
                .WithTimestamp(DateTimeOffset.Now)
                .Build();
        }

        /// <summary>
        /// Получение текущего трека
        /// </summary>
        /// <param name="Player"></param>
        /// <returns></returns>
        public Embed GetEmbedTrack(Player Player)
        {
            return new EmbedBuilder()
                .WithTitle("Сейчас играет:")
                .WithDescription(Player.Track.ExternalUrl)
                .Build();
        }

        /// <summary>
        /// Обертка сообщения в объект Embed
        /// </summary>
        /// <param name="title">Заголовок Embed</param>
        /// <param name="isSuccesRequest">Статус ответа(пололожительный / отрицательный)</param>
        /// <param name="isTrackPlayed">Время жизни сообщения (true - 10с. | false - до конца трека)</param>
        /// <returns></returns>
        public Embed GetEmbedMessage(string title, bool isSuccesRequest = true, bool isTrackPlayed = true)
        {
            var user = context.Client.CurrentUser;
            EmbedBuilder embed;
            if (isSuccesRequest)
                embed = new EmbedBuilder().WithTitle($":white_check_mark: {title}");
            else
                embed = new EmbedBuilder().WithTitle($":x: {title}");

            embed
                .WithColor(Color.LightGrey)
                .WithFooter(new EmbedFooterBuilder() { IconUrl = user.GetAvatarUrl(), Text = $"{user.Username}" })
                .WithTimestamp(DateTimeOffset.Now);
            embed = isTrackPlayed ? embed.WithDescription($":warning: Это сообщение будет удалено через 10 с.") : embed.WithDescription($":warning: Это сообщение будет удалено по окончанию трека");
            return embed.Build();
        }
    }
}
