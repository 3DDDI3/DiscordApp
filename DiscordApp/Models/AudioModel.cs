using DiscordApp.Helper;
using System;

namespace DiscordApp.Models
{
    /// <summary>
    /// Класс, содержащий информацию об Audio
    /// </summary>
    public class AudioModel
    {
        /// <summary>
        /// Исполнитель
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Продолжительность
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Время, на котором закончился трек
        /// </summary>
        public TimeSpan Time { get; set; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Ссылка на видео (P.S. если ModuleType = youtube)
        /// </summary>
        public string ExternalUrl { get; set; }

        /// <summary>
        /// Тип модуля (YT|VK|YM)
        /// </summary>
        public ModuleType ModuleType { get; set; }
    }
}
