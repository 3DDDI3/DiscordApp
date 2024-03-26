using DiscordApp.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using VkNet.Model.Attachments;
using YoutubeDLSharp;

namespace DiscordApp.Helper
{
    public class ProcessHellper
    {
        /// <summary>
        /// Процесс
        /// </summary>
        private Process _Process;

        /// <summary>
        /// Информация дял старта процесса
        /// </summary>
        private ProcessStartInfo ProcessStartInfo;

        /// <summary>
        /// Процесс
        /// </summary>
        public Process Process { get => _Process; set => _Process = value; }

        /// <summary>
        /// Название процесса
        /// </summary>
        private string FileName;

        /// <summary>
        /// Конфигурационный объект
        /// </summary>
        private JObject config;

        public ProcessHellper()
        {
            config = new JsonHelper().GetDataFromInitFile();
        }

        /// <summary>
        /// Скачивание аудиотрека в асинхронном режиме
        /// </summary>
        /// <param name="Audio">Трек</param>
        /// <returns></returns>
        public async Task DownloadAudioRecordAsync(AudioModel Audio)
        {
            /**
             * Если трек взят с YT
             */
            if (Audio.ModuleType == ModuleType.YTMusic)
            {
                var ytdl = new YoutubeDL();
                ytdl.FFmpegPath = @$"{config["Path"]}{config["Delimiter"]}{config["ffmpeg"]}{config["Delimiter"]}ffmpeg";
                ytdl.YoutubeDLPath = @$"{config["Path"]}{config["Delimiter"]}{config["yt"]}{config["Delimiter"]}yt";
                ytdl.OutputFolder = @$"{config["Path"]}{config["Delimiter"]}{config["AudioDir"]}";
                ytdl.OutputFileTemplate = $"{Audio.Title}";
                await ytdl.RunAudioDownload(Audio.ExternalUrl, YoutubeDLSharp.Options.AudioConversionFormat.Mp3);
            }

            /**
             * Если трек взят с YM
             */
            if (Audio.ModuleType == ModuleType.YandexMusic)
            {
                FileName = @$"{config["Path"]}{config["Delimiter"]}{config["ffmpeg"]}\ffmpeg";
                string _Argument = $@"-hide_banner -i ""{Audio.Url}"" ""{config["Path"]}{config["Delimiter"]}{config["AudioDir"]}{config["Delimiter"]}{Audio.Title}.mp3"" ";
                ProcessStartInfo = new ProcessStartInfo()
                {
                    FileName = FileName,
                    Arguments = _Argument,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                _Process = Process.Start(ProcessStartInfo);
            }

            /**
             * Если трек взят с VK
             */
            if(Audio.ModuleType == ModuleType.VKMusic)
            {
                ProcessStartInfo = new ProcessStartInfo()
                {
                    FileName = @$"{config["Path"]}{config["Delimiter"]}{config["m3u8"]}{config["Delimiter"]}m3u8dl",
                    Arguments = $"\"{Audio.Url}\" --enableDelAfterDone --disableDateInfo --saveName \"{Audio.Title}\" --workDir \"{config["Path"]}{config["Delimiter"]}{config["AudioDir"]}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                _Process = Process.Start(ProcessStartInfo);
                _Process.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// Скачивание трека в синронном режиме
        /// </summary>
        /// <param name="Audio">Трек</param>
        /// <returns></returns>
        public async Task DownloadAudioRecord(AudioModel Audio)
        {
            /**
             * Если трек взят с YT
             */
            if (Audio.ModuleType == ModuleType.YTMusic)
            {
                var ytdl = new YoutubeDL();
                ytdl.FFmpegPath = @$"{config["Path"]}{config["Delimiter"]}{config["ffmpeg"]}{config["Delimiter"]}ffmpeg";
                ytdl.YoutubeDLPath = @$"{config["Path"]}{config["Delimiter"]}{config["yt"]}{config["Delimiter"]}yt";
                ytdl.OutputFolder = @$"{config["Path"]}{config["Delimiter"]}{config["AudioDir"]}";
                ytdl.OutputFileTemplate = $"{Audio.Title}";
                var res = await ytdl.RunAudioDownload(Audio.ExternalUrl, YoutubeDLSharp.Options.AudioConversionFormat.Mp3);
            }

            /**
             * Если трек взят с YM
             */
            if (Audio.ModuleType == ModuleType.YandexMusic)
            {
                FileName = @$"{config["Path"]}{config["Delimiter"]}{config["m3u8"]}{config["Delimiter"]}ffmpeg";
                string _Argument = $@"-hide_banner -i ""{Audio.Url}"" ""{config["Path"]}{config["Delimiter"]}{config["AudioDir"]}{config["Delimiter"]}{Audio.Title}.mp3"" ";
                ProcessStartInfo = new ProcessStartInfo()
                {
                    FileName = FileName,
                    Arguments = _Argument,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                _Process = Process.Start(ProcessStartInfo);
                _Process.WaitForExit();
            }

            /**
             * Если трек взят с VK
             */
            if (Audio.ModuleType == ModuleType.VKMusic)
            {
                ProcessStartInfo = new ProcessStartInfo()
                {
                    FileName = @$"{config["Path"]}{config["Delimiter"]}{config["m3u8"]}{config["Delimiter"]}m3u8dl",
                    Arguments = $"\"{Audio.Url}\" --enableDelAfterDone --disableDateInfo --saveName \"{Audio.Title}\" --workDir \"{config["Path"]}{config["Delimiter"]}{config["AudioDir"]}\" ",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                _Process = Process.Start(ProcessStartInfo);
                _Process.WaitForExit();
            }
        }

        /// <summary>
        /// Воспроизведение трека
        /// </summary>
        /// <returns></returns>
        public Task PlayTrack(AudioModel Audio)
        {
            string format = Audio.ModuleType == ModuleType.VKMusic ? "ts" : "mp3";
            ProcessStartInfo = new ProcessStartInfo()
            {
                FileName = @$"{config["Path"]}{config["Delimiter"]}{config["ffmpeg"]}{config["Delimiter"]}ffmpeg",
                Arguments = $@"-hide_banner -ss {Audio.Time} -i ""{config["Path"]}{config["Delimiter"]}{config["AudioDir"]}{config["Delimiter"]}{Audio.Title}.{format}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                //CreateNoWindow = false,
                UseShellExecute = false
            };
           
            _Process = Process.Start(ProcessStartInfo);
            string str = null;

            /**
             * Задача по считыванию продолжительности и времени трека, на котором он остановился 
             */
            Task.Run(() =>
            {
                while ((str = _Process.StandardError.ReadLine()) != null)
                {
                    Debug.WriteLine(str);
                    if (Regex.IsMatch(str, @"(size)|(time)"))
                    {
                        str = Regex.Replace(str, @"(size=\s{1,}\d{1,}kB\s{1}time=)|(.\d{1,}\s{1}bitrate=\d{1,}.\d{1,}kbits/s\s{1}speed=\d{1,}.\d{1,}x\s{1,})", "");
                        try
                        {
                            Audio.Time = TimeSpan.ParseExact(str, @"hh\:mm\:ss", CultureInfo.InvariantCulture);
                        }
                        catch (Exception ex) { }
                    }
                    if (Regex.IsMatch(str, @"Duration"))
                    {
                        str = Regex.Replace(str, @"(\s{1,}Duration:\s{1})|(.\d{1,},\s{1}start:\s{1}\d{1,}.\d{1,},\s{1}bitrate:\s{1}\d{1,}\s{1}kb/s)", "");
                        Audio.Duration = TimeSpan.ParseExact(str, @"hh\:mm\:ss", CultureInfo.InvariantCulture);
                    }
                }
            });
            return Task.CompletedTask;
        }


        /// <summary>
        /// Запись данных о аудио файл
        /// </summary>
        /// <param name="AudioObject">Трек</param>
        /// <param name="IsSoloRecord">Указать true, если нужно искать 3 трека</param>
        /// <returns></returns>
        public Task PrintYTAudioToFile(AudioModel Audio = null, string query = null)
        {
            FileName = @$"{config["Path"]}{config["Delimiter"]}{config["yt"]}{config["Delimiter"]}yt";
            DirectoryHelper directory = new DirectoryHelper(new DirectoryInfo($"{config["Path"]}"));
            directory.DeleteFilesAsync(new List<string>() { "yt.txt"});
            var str = Audio == null ? $"ytsearch3:\"{query}\"" : Audio.ExternalUrl;
            ProcessStartInfo = new ProcessStartInfo
            {
                FileName = FileName,
                Arguments = $@" {str} --default-search ""ytsearch"" --no-download --print-to-file ""%(title)s~~%(duration)s~~%(original_url)s~~%(urls)s"" yt.txt ",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            _Process = Process.Start(ProcessStartInfo);
            _Process.WaitForExit();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Завершение процесса
        /// </summary>
        /// <returns></returns>
        public Task ProcessKill()
        {
            ProcessStartInfo = new ProcessStartInfo()
            {
                FileName = "taskkill",
                Arguments = $"/IM ffmpeg.exe /T /F",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            _Process = Process.Start(ProcessStartInfo);
            _Process.WaitForExit();
            return Task.CompletedTask;
        }
    }
}
