using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using DiscordApp.Helper;
using DiscordApp.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.Utils;

namespace DiscordApp
{
    /// <summary>
    /// Проигрыватель
    /// </summary>
    public class Player
    {
        private IAudioClient client;
        private SocketCommandContext context;
        private ProcessHellper ProcessHelper;
        private ulong vc;

        /// <summary>
        /// Тип модуля (VK|YM|YT )
        /// </summary>
        public ModuleType ModuleType { get; set; }

        /// <summary>
        /// Флаг, показывающий, что трек переключен на следующий | предыдущий
        /// </summary>
        public bool isTrackSwitchedToNextOrPrevious { get; set; } = false;

        /// <summary>
        /// Флаг, показывающий что плеер остановлен
        /// </summary>
        public bool isPlayerStopped { get; set; }

        /// <summary>
        /// Флаг, показывающий, что плеер зациклен
        /// </summary>
        public bool isPlayerLooped { get; set; }

        /// <summary>
        /// Флаг, показывающий, что плеер поставлен на паузу
        /// </summary>
        public bool isPlayerPaused { get; set; }

        /// <summary>
        /// Список выбранных треков
        /// </summary>
        public List<int> SelectedTrackIndex { get; set; }

        /// <summary>
        /// Индекс текущего трека
        /// </summary>
        public int SelectedIndex { get; set; } = 0;

        /// <summary>
        /// Текущий трек
        /// </summary>
        public AudioModel Track { get; set; }

        /// <summary>
        /// Список треков
        /// </summary>
        public List<AudioModel> Tracks { get; set; }

        /// <summary>
        /// Объект с перечнем конфигурационных данных
        /// </summary>
        private JObject config;

        public Player()
        {
            this.SelectedTrackIndex = new List<int>();
            this.Tracks = new List<AudioModel>();
            config = new JsonHelper().GetDataFromInitFile();
        }
        public Player(List<AudioModel> tracks, List<int> selectedTrackIndex)
        {
            SelectedTrackIndex = selectedTrackIndex;
            Tracks = tracks;
            Track = Tracks[SelectedIndex];
        }

        /// <summary>
        /// Воспроизведение треков
        /// </summary>
        /// <param name="context"></param>
        /// <param name="SelectedTrackIndex"></param>
        /// <returns></returns>
        public async Task Play(SocketCommandContext context, List<int> SelectedTrackIndex = null)
        {
            AudioStream discord = null;
            this.context = context;
            if (SelectedTrackIndex != null) {
                this.SelectedTrackIndex = SelectedTrackIndex;
                Track = Tracks[SelectedTrackIndex[SelectedIndex]];
            }
            DirectoryHelper directoryHelper = new DirectoryHelper(new DirectoryInfo($@"{config["Path"]}{config["Delimiter"]}{config["AudioDir"]}"));
            IVoiceState VC = context.Message.Author as IVoiceState;
            try
            {
                ProcessHelper = new ProcessHellper();
                List<Task> tasks = new List<Task>();
                while (this.SelectedIndex < this.SelectedTrackIndex.Count)
                {
                    if (client != null && !isPlayerLooped) await VC.VoiceChannel.DisconnectAsync();
                    if (!directoryHelper.IsFileExist($"{Tracks[this.SelectedTrackIndex[SelectedIndex]].Title}.mp3"))
                        await ProcessHelper.DownloadAudioRecord(this.Tracks[this.SelectedTrackIndex[SelectedIndex]]);
                    if (this.isTrackSwitchedToNextOrPrevious && SelectedIndex > 0) 
                        this.isTrackSwitchedToNextOrPrevious = false;
                    if (this.isPlayerStopped) {
                        await VC.VoiceChannel.DisconnectAsync();
                        return;
                    }
                    if (this.isPlayerPaused) {
                        await VC.VoiceChannel.DisconnectAsync();
                        return;
                    }
                    client = await VC.VoiceChannel.ConnectAsync();
                    vc = VC.VoiceChannel.Id;
                    client.ClientDisconnected += ClientDisconnected;
                    await client.SetSpeakingAsync(true);
                    Track = Tracks[this.SelectedTrackIndex[SelectedIndex]];
                    var lastMessage = await context.Channel.GetMessagesAsync(1).FlattenAsync();
                    RestUserMessage botMessage = null;
                    if (lastMessage.Where(x => Regex.IsMatch(x.Content, "Воспроизведение трека возобновлено")).Count() == 0) {
                        botMessage  = await context.Channel.SendMessageAsync(embed: new EmbedHelper(context).GetEmbedMessage(String.Format("Трек **{0} {1}** `[{2}]` запущен", Track.Artist, Track.Title, Track.Duration), isTrackPlayed: false));
                    }
                    await ProcessHelper.PlayTrack(Track);
                    Stream output = ProcessHelper.Process.StandardOutput.BaseStream;
                    discord = client.CreatePCMStream(AudioApplication.Mixed);
                    await output.CopyToAsync(discord);
                    await client.SetSpeakingAsync(false);
                    if (VC.VoiceChannel != null) await VC.VoiceChannel.DisconnectAsync();
                    if (botMessage != null) await context.Channel.DeleteMessageAsync(botMessage.Id);
                    if (!isPlayerLooped) SelectedIndex++;
                    else Track.Time = new TimeSpan();
                    if (isPlayerPaused) SelectedIndex--;
                }
            }
            catch (Exception ex)
            {
                if (discord != null) await discord.FlushAsync();
            }
        }

        /// <summary>
        /// Удаление ненужных файлов (аудиозаписей и логов) при выходе запустившего плеер пользователя
        /// </summary>
        /// <param name="arg">id вышедшего пользователя</param>
        /// <returns></returns>
        private async Task ClientDisconnected(ulong arg)
        {
            List<SocketVoiceChannel> channels = context.Guild.VoiceChannels.Where(x => x.Id == vc).ToList();
            List<SocketGuildUser> users = channels[0].ConnectedUsers.ToList();
            if(users.Count == 1 && users[0].IsBot && users[0].Id == 656223704048730125)
            {
                await ProcessHelper.ProcessKill();
                DirectoryHelper directoryHelper = new DirectoryHelper(new DirectoryInfo($@"{config["Path"]}{config["Delimiter"]}{config["m3u8"]}"));
                directoryHelper.DeleteDirectoriesAsync(new List<string>() { "ffmpeg.exe", "m3u8dl.exe", "yt.exe" });
                directoryHelper.Dir = new DirectoryInfo($@"{config["Path"]}{config["Delimiter"]}{config["AudioDir"]}");
                directoryHelper.DeleteFilesAsync(new List<string>());
                await users[0].VoiceChannel.DisconnectAsync();
            }
        }

        /// <summary>
        /// Переключение трека на следующий
        /// </summary>
        /// <returns></returns>
        public async Task SwitchToNextRecord()
        {
            //int _selectedIndex = SelectedIndex;
            //ProcessHellper processHelper = new ProcessHellper();
            //SelectedIndex = _selectedIndex;
            this.SelectedIndex++;
            if (SelectedIndex < this.SelectedTrackIndex.Count)
            {
                this.isTrackSwitchedToNextOrPrevious = true;
                ProcessHellper processHellper = new ProcessHellper();
                await processHellper.ProcessKill();
                await this.Play(context);
            }
        }

        /// <summary>
        /// Переключение трека на предыдущий
        /// </summary>
        /// <returns></returns>
        public async Task SwitchToPreviousRecordAsync()
        {
            int _selectedIndex = --SelectedIndex;
            ProcessHellper processHelper = new ProcessHellper();
            await processHelper.ProcessKill();
            SelectedIndex = _selectedIndex;
            if (SelectedIndex >=0)
            {
                this.SelectedIndex--;
                this.isTrackSwitchedToNextOrPrevious = true;
                ProcessHellper processHellper = new ProcessHellper();
                await processHellper.ProcessKill();
                await this.Play(context);
            }
        }

        /// <summary>
        /// Выключение проигрывателя
        /// </summary>
        /// <returns></returns>
        public async Task StopPlayingTracks()
        {
            ProcessHellper processHellper = new ProcessHellper();
            this.isPlayerStopped = true;
            await processHellper.ProcessKill();
        }

        /// <summary>
        /// Отображение треков, вклюбченнных в плейлист
        /// </summary>
        /// <returns></returns>
        public Embed GetAudioPlaylist()
        {
            JsonHelper json = new JsonHelper();
            Tracks = json.GetDataFromJson<Player>("Temporary.json").Tracks;
            EmbedHelper embedHelper = new EmbedHelper();
            return embedHelper.GetEmbedAudioObject(
                new Player() {
                    Tracks = this.Tracks
                },
                "Треки из плейлиста:"
            );
        }

        /// <summary>
        /// Обновление данных треков в файле
        /// </summary>
        /// <returns></returns>
        public async Task<string> UpdateTrackListAsync()
        {
            JsonHelper json = new JsonHelper();
            HttpHelper<Player> httpHelper = new HttpHelper<Player>(json.GetDataFromJson<Player>("Temporary.json"));
            await httpHelper.CheckUrlExistingUrlAsync();
            Player Player = httpHelper.AudioList;
            if (Player.Tracks.Count == 0) return null;
            Player _AudioModule = new Player();
            _AudioModule = json.GetDataFromJson<Player>("Temporary.json");
            for (int j = 0; j < _AudioModule.Tracks.Count; j++)
            {
                for (int l = 0; l < Player.Tracks.Count; l++)
                {
                    if (_AudioModule.Tracks[j].Title == Player.Tracks[l].Title)
                        _AudioModule.Tracks[j] = Player.Tracks[l];
                }
            }
            _AudioModule.Tracks = _AudioModule.Tracks.Select(x => x).Distinct().ToList();
            json.WriteToJsonAsync("Temporary.json", _AudioModule);
            return $"Изменения успешно сохранены";
        }

        /// <summary>
        /// Получение объекта с треками
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<List<AudioModel>> GetTracksAudioObject(string query)
        { 
            ProcessHellper processHellper = new ProcessHellper();
            if (ModuleType == ModuleType.YTMusic)
            {
                await processHellper.PrintYTAudioToFile(query: query);
                FileHelper fileHelper = new FileHelper();
                return fileHelper.GetFileData(fileHelper.ReadFromYtFile, true);
            }
            if (ModuleType == ModuleType.VKMusic)
            {
                VKHelper vk = new VKHelper();
                vk.Authorize();
                return vk.SearchAudioRecords(query);
            }
            if (ModuleType == ModuleType.YandexMusic)
            {
                YMHelper YMHelper = new YMHelper();
                return YMHelper.GetResponseObject(query);
            }
            return null;
        }

        /// <summary>
        /// Добавление трека в плейлист
        /// </summary>
        /// <param name="query">Запрос</param>
        /// <param name="context">context</param>
        public async Task AddTrackToPlaylist(string[] query, SocketCommandContext context)
        {
            try
            {
                QueryHelper queryHelper = new QueryHelper();
                queryHelper.CheckStringInQuery(query);
                try
                {
                    /*
                    * Добавление трека в файл 
                    */
                    SelectedTrackIndex = queryHelper.GetTrackIndexesFromQuery(query, Tracks.Count);
                    for (int i = Tracks.Count - 1; i >= 0; i--)
                        if (!SelectedTrackIndex.Contains(i)) Tracks.RemoveAt(i);

                    DirectoryHelper directoryHelper = new DirectoryHelper(new DirectoryInfo(config["Path"].ToString()));
                    JsonHelper json = new JsonHelper();
                    if (directoryHelper.IsFileExist("Temporary.json"))
                    {
                        Player _Player = new Player();
                        _Player = json.GetDataFromJson<Player>("Temporary.json");
                        if (_Player != null)
                            Tracks.AddRange(_Player.Tracks);
                        Tracks.Reverse();
                    }
                    json.WriteToJsonAsync("Temporary.json", this);
                    await context.Channel.SendMessageAsync(embed: new EmbedHelper(context).GetEmbedMessage($"Трек **{Tracks[Tracks.Count - 1].Artist ?? null} {Tracks[Tracks.Count - 1].Title}** успешно добавлен в плейлист"));
                }
                catch (Exception)
                {
                    /*
                     * Вывод вариантов найденных треков 
                     */
                    Tracks = await  GetTracksAudioObject(String.Join(" ", query));
                    await context.Channel.SendMessageAsync(embed: new EmbedHelper(context).GetEmbedAudioObject(this));
                }

            }
            catch (Exception ex)
            {
                await context.Channel.SendMessageAsync(embed: new EmbedHelper(context).GetEmbedMessage(ex.Message, false));
            }
        }

        public Player? ShowPlaylist()
        {
            JsonHelper json = new JsonHelper();
            return json.GetDataFromJson<Player>("Temporary.json");
        }

        /// <summary>
        /// Удаление трека из плейлиста
        /// </summary>
        /// <param name="Player"></param>
        /// <param name="query"></param>
        public void RemoveTrackFromPlaylist(Player Player, string[] query)
        {
            QueryHelper queryHelper = new QueryHelper();
            queryHelper.CheckDigitsInQuery(query);
            JsonHelper json = new JsonHelper();
            Player = json.GetDataFromJson<Player>("Temporary.json");
            Player.SelectedTrackIndex = queryHelper.GetTrackIndexesFromQuery(query, Player.Tracks.Count);
            for (int i = Player.SelectedTrackIndex.Count - 1; i >= 0; i--)
                Player.Tracks.RemoveAt(Player.SelectedTrackIndex[i]);
            json.WriteToJsonAsync("Temporary.json", Player);

        }
       
    }
}
