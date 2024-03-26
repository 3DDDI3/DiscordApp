using Discord;
using DiscordApp.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Yandex.Music.Api.Models.Common;
using Yandex.Music.Api.Models.Search;

namespace DiscordApp.Helper
{
    internal class YMHelper
    {
        private YandexMusicApi ym;
        private AuthStorage authStorage;
        public YMHelper()
        {
            JsonHelper json = new JsonHelper();
            JObject jObject = json.GetDataFromInitFile();
            ym = new YandexMusicApi();
            authStorage = new AuthStorage();
            ym.User.Authorize(authStorage, (string)jObject["YMToken"]);
        }

        /// <summary>
        /// Получение треков с Yandex Music 
        /// </summary>
        /// <param name="query">Запрос</param>
        /// <param name="count">Кол-во возвращаемых треков</param>
        /// <returns></returns>
        public List<AudioModel> GetResponseObject(string query, byte count = 3)
        {
            List<AudioModel> Audio = new List<AudioModel>();
            YResponse<YSearch> YResponse = ym.Search.Search(authStorage, query, YSearchType.Track);
            if (YResponse.Result.Tracks.Total < count) count = byte.Parse(YResponse.Result.Tracks.Total.ToString());
            for (byte i = 0; i < count; i++)
            {
                TimeSpan ts = TimeSpan.FromMilliseconds(YResponse.Result.Tracks.Results[i].DurationMs);
                Audio.Add(
                    new AudioModel()
                    {
                        Artist = String.Join(",", YResponse.Result.Tracks.Results[i].Artists.Select(x => x.Name).ToArray()),
                        Duration = new TimeSpan(ts.Hours, ts.Minutes, ts.Seconds),
                        Title = YResponse.Result.Tracks.Results[i].Title,
                        Url = ym.Track.GetFileLink(authStorage, YResponse.Result.Tracks.Results[i].Id),
                        ModuleType = ModuleType.YandexMusic
                    }
                );
            }
            return Audio;
        }
    }
}
