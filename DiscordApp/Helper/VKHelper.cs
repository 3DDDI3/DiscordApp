using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordApp.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkNet;
using VkNet.AudioBypassService.Extensions;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace DiscordApp.Helper
{
    public class VKHelper
    {
        private readonly ServiceCollection Service;
        private readonly VkApi Api;
        public VkCollection<Audio> Audios { get; set; }
        public VKHelper()
        {
            Service = new ServiceCollection();
            Service.AddAudioBypass();
            Api = new VkApi(Service);
        }

        /// <summary>
        /// Авторизация в ВК
        /// </summary>
        public void Authorize()
        {
            JsonHelper json = new JsonHelper();
            JObject jObject = json.GetDataFromInitFile();
            Api.Authorize(new ApiAuthParams() { AccessToken = (string)jObject["VKToken"] });
        }

        /// <summary>
        /// Поиск аудиозаписей
        /// </summary>
        /// <param name="Query">Запрос на поиск</param>
        /// <param name="LimitRecords">Ограничение на кол-во аудиозаписей</param>
        public List<AudioModel> SearchAudioRecords(string Query, int LimitRecords = 3)
        {
            var obj = Api.Audio.Search(new AudioSearchParams() { Query = Query, Count = LimitRecords });
            if (obj.Count < 3) LimitRecords = obj.Count;
            List<AudioModel> result = new List<AudioModel>();
            for (int i = 0; i < LimitRecords; i++)
            {
                result.Add(
                    new AudioModel() { 
                        Artist = obj[i].Artist, 
                        Duration = TimeSpan.FromSeconds(obj[i].Duration), 
                        Title = obj[i].Title,
                        Url = obj[i].Url.AbsoluteUri,
                        ModuleType=ModuleType.VKMusic
                    }
                );
            }
            return result;
        }
    }
}
