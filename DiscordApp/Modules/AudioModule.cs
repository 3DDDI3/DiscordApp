using Discord;
using Discord.Audio;
using Discord.Commands;
using DiscordApp.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VkNet.Model;

namespace DiscordApp.Modules
{
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        public static Player Player = new Player();
        private JsonHelper json = new JsonHelper();
        private IVoiceState VC;
        public static IAudioClient client;

        /// <summary>
        /// Удаление сообщений
        /// </summary>
        /// <param name="str">Кол-во сообщений, которые нужно удалить</param>
        /// <returns></returns>
        [Command("delete", RunMode = RunMode.Async)]
        [Alias("del", "d")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task DeleteCommandAsync(params String[] query)
        {

            QueryHelper queryHelper = new QueryHelper();
            try
            {
                queryHelper.CheckStringInQuery(query);
                var MessageChannel = Context.Channel as ITextChannel;
                IEnumerable<IMessage> Messages = await MessageChannel.GetMessagesAsync(int.Parse(query[0])).FlattenAsync();
                foreach (var msg in Messages)
                {
                    await msg.Channel.DeleteMessageAsync(msg.Id);
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
                await Context.Channel.SendMessageAsync(embed: new EmbedHelper(Context).GetEmbedMessage($"Удаленно {Messages.Count()} сообщений(-я)."));
            }
            catch(Exception ex)
            {
                var trace = new StackTrace(ex, true);
                await Context.Channel.SendMessageAsync(embed: new EmbedHelper(Context).GetEmbedMessage(ex.Message));
            }
        }

        /// <summary>
        /// Запуст VK плеера
        /// </summary>
        /// <param name="Query">Запрос на поиск аудиозаписи</param> 
        /// <returns></returns>
        [Command("vkplaytrack", RunMode = RunMode.Async)]
        [Alias("vkp","vk")]
        public async Task VkPlayCommand(params String[] query)
        {
            QueryHelper queryHelper = new QueryHelper();
            try
            {
                queryHelper.CheckStringInQuery(query);
                VKHelper vk = new VKHelper();
                vk.Authorize();
                Player.Tracks = vk.SearchAudioRecords(String.Join(" ", query));
                EmbedHelper embedHelper = new EmbedHelper(Context);
                await Context.Channel.SendMessageAsync(embed: embedHelper.GetEmbedAudioObject(Player));
                Player.ModuleType = ModuleType.VKMusic;
            }
            catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync(embed: new EmbedHelper().GetEmbedMessage(ex.Message, false));
            } 
        }

        /// <summary>
        /// Добавление трека с youtube в плейлист 
        /// </summary>
        /// <param name="query">Входная строка с дискорда</param>
        /// <returns></returns>
        [Command("ytplay", RunMode = RunMode.Async)]
        [Alias("yt")]
        public async Task YoutubeTrackAdd(params String[] query)
        {
            QueryHelper queryHelper = new QueryHelper();
            EmbedHelper embedHelper = new EmbedHelper(Context);
            try
            {
                queryHelper.CheckStringInQuery(query);
                Player.ModuleType = ModuleType.YTMusic;
                Player.Tracks = await Player.GetTracksAudioObject(String.Join(" ", query));
                await Context.Channel.SendMessageAsync(embed: embedHelper.GetEmbedAudioObject(Player));
            }
            catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync(embed: embedHelper.GetEmbedMessage(ex.Message, false));
            }
        }

        [Command("ym", RunMode = RunMode.Async)]
        public async Task ym(params String[] query)
        {
            try
            {
                new QueryHelper().CheckStringInQuery(query);
                YMHelper yMHelper = new YMHelper();
                Player.Tracks = yMHelper.GetResponseObject(string.Join(" ", query));
                EmbedHelper embedHelper = new EmbedHelper(Context);
                await Context.Message.ReplyAsync(embed: embedHelper.GetEmbedAudioObject(Player));
                Player.ModuleType = ModuleType.YandexMusic;
            }
            catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync(embed: new EmbedHelper(Context).GetEmbedMessage(ex.Message, false));
            }
        }

        /// <summary>
        /// Отображение текущего трека
        /// </summary>
        /// <returns></returns>
        [Command("nowplaying", RunMode = RunMode.Sync)]
        [Alias("np")]
        public Task ShowNowPlayingTrack()
        {
            EmbedHelper embedHelper = new EmbedHelper(Context);
            Context.Channel.SendMessageAsync($"Сейчас играет:\n{Player.Track.Title}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Проверка ссылок на треки на актуальность (если ссылка неактуальна, то перезаписать на корректную)
        /// P.S. полученные ссылки на треки с Youtube имееют строк годности
        /// Удаление дубликатов треков
        /// </summary>
        /// <returns></returns>
        [Command("update_playlist", RunMode = RunMode.Async)]
        [Alias("upd")]
        private async Task UpdateTracklist(params string[] query) {
            EmbedHelper embedHelper = new EmbedHelper(Context);
            try {
                new QueryHelper().CheckStringInQuery(query, true);
                await Player.UpdateTrackListAsync();
                await Context.Channel.SendMessageAsync(embed: embedHelper.GetEmbedMessage("Данные успешно обновлены."));
            }
            catch (Exception ex) {
                await Context.Channel.SendMessageAsync(embed: embedHelper.GetEmbedMessage(ex.Message, false));
            }
        }

        [Command("yt_add", RunMode = RunMode.Async)]
        [Alias("yta")]
        public Task AddYTTrackToPlaylist(params String[] query)
        {
            Player.ModuleType = ModuleType.YTMusic;
            Player.AddTrackToPlaylist(query, Context).Wait();
            return Task.CompletedTask;
        }

        [Command("ym_add", RunMode = RunMode.Async)]
        [Alias("yma")]
        public Task AddYMTrackToPlaylist(params String[] query)
        {
            Player.ModuleType = ModuleType.YandexMusic;
            Player.AddTrackToPlaylist(query, Context).Wait();
            return Task.CompletedTask;
        }

        [Command("vk_add", RunMode = RunMode.Async)]
        [Alias("vka")]
        public Task AddVKTrackToPlaylist(params String[] query)
        {
            Player.ModuleType = ModuleType.VKMusic;
            Player.AddTrackToPlaylist(query, Context).Wait();
            return Task.CompletedTask;
        }

        [Command("playlist_show", RunMode = RunMode.Async)]
        [Alias("ps")]
        public Task ShowPlaylist(params String[] query)
        {
            try
            {
                new QueryHelper().CheckStringInQuery(query, true);
                Context.Channel.SendMessageAsync(embed: new EmbedHelper(Context).GetEmbedAudioObject(Player.ShowPlaylist(), "Плейлист:"));
            }catch(Exception ex)
            {
                Context.Channel.SendMessageAsync(embed: new EmbedHelper(Context).GetEmbedMessage(ex.Message, false));
            }
            return Task.CompletedTask;
        }

        [Command("playlist_remove", RunMode = RunMode.Async)]
        [Alias("pr")]
        public Task RemoveFromPlaylist(params String[] query)
        {
            try
            {
                Player.RemoveTrackFromPlaylist(Player, query);
            }
            catch(Exception ex)
            {
                Context.Channel.SendMessageAsync(embed: new EmbedHelper(Context).GetEmbedMessage(ex.Message, false));
            }
            return Task.CompletedTask;
        }

        #region Функции плеера
        /// <summary>
        /// Воспроизведение трека
        /// </summary>
        /// <param name="query">Номера выбранных треков</param>
        /// <returns></returns>
        [Command("play", RunMode = RunMode.Async)]
        [Alias("pl")]
        public async Task PlayCommandAsync(params String[] query)
        {
            EmbedHelper embedHelper = new EmbedHelper(Context);

            VC = ((IVoiceState)Context.User);
            if (VC.VoiceChannel == null)
            {
                await Context.Channel.SendMessageAsync(embed: embedHelper.GetEmbedMessage("Чтобы запусть воспроизведение трека нужно быть на аудиоканале", false));
                return;
            }
            try
            {
                QueryHelper queryHelper = new QueryHelper();
                queryHelper.CheckDigitsInQuery(query);
                if (Player.Tracks.Count == 0) Player = json.GetDataFromJson<Player>("Temporary.json");
                List<int> SubQuery = queryHelper.GetTrackIndexesFromQuery(query, Player.Tracks.Count);
                await Player.Play(Context, SubQuery);
            }
            catch(Exception ex)
            {
                await Context.Channel.SendMessageAsync(ex.Message, false); 
            }
            if (!Player.isPlayerPaused) Player = new Player();
        }

        /// <summary>
        /// Переключение на следующий трек
        /// </summary>
        /// <returns></returns>
        [Command("next_track", RunMode = RunMode.Async)]
        [Alias("nt")]
        public async Task SwitchToNextRecord()
        {
            await Player.SwitchToNextRecord();
        }

        /// <summary>
        /// Переключение на предыдущий трек
        /// </summary>
        /// <returns></returns>
        [Command("previous_track", RunMode = RunMode.Async)]
        [Alias("pt")]
        public async Task SwitchToPreviusRecord()
        {
            Player.SelectedIndex--;
            await Player.SwitchToPreviousRecordAsync();
        }

        [Command("pause", RunMode = RunMode.Async)]
        [Alias("p")]
        public async Task PausePlayer()
        {
            Player.isPlayerPaused = true;
            new ProcessHellper().ProcessKill();
            await Context.Channel.SendMessageAsync(embed: new EmbedHelper(Context).GetEmbedMessage("Воспроизведение трека поставлено на паузу", isSuccesRequest: true));
        }

        [Command("resume", RunMode = RunMode.Async)]
        [Alias("r","up")]
        public async Task ResumePlayer()
        {
            Player.isPlayerPaused = false;
            await Context.Channel.SendMessageAsync(embed: new EmbedHelper(Context).GetEmbedMessage("Воспроизведение трека возобновлено", isSuccesRequest: false));
            await Player.Play(Context);
        }

        /// <summary>
        /// Выключение проигрывателя
        /// </summary>
        /// <returns></returns>
        [Command("stop", RunMode = RunMode.Async)]
        [Alias("st")]
        public async Task StopPlayer()
        {
            await Player.StopPlayingTracks();
            EmbedHelper embedHelper = new EmbedHelper(Context);
            await Context.Channel.SendMessageAsync(embed: embedHelper.GetEmbedMessage($"Трек **{Player.Track.Title}** остановлен"));
        }

        [Command("playlist", RunMode = RunMode.Async)]
        public async Task ShowAudioPlaylist()
        {
            await Context.Channel.SendMessageAsync(embed: Player.GetAudioPlaylist());
        }

        /// <summary>
        /// Включение повтора трека
        /// </summary>
        /// <returns></returns>
        [Command("track_loop", RunMode = RunMode.Async)]
        [Alias("tl")]
        public async Task TrackLoop()
        {
            Player.isPlayerLooped = true;
            await Context.Channel.SendMessageAsync(embed: new EmbedHelper(Context).GetEmbedMessage("Повтор трека включен."));
        }

        /// <summary>
        /// Отключение повтора трека
        /// </summary>
        /// <returns></returns>
        [Command("track_unloop", RunMode = RunMode.Async)]
        [Alias("tul")]
        public async Task PlayerUnLoop()
        {
            Player.isPlayerLooped = false;
            await Context.Channel.SendMessageAsync(embed: new EmbedHelper(Context).GetEmbedMessage("Повтор трека выключен."));
        }
        #endregion
    }

}
