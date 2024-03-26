using DiscordApp.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using VkNet.Exception;

namespace DiscordApp.Helper
{
    public class HttpHelper<T> where T : Player
    {
        private T _AudioList;
        private Player obj;

        public T AudioList { get=>_AudioList; set => _AudioList = value; }
        public HttpHelper(T obj)
        {
            this.AudioList = obj;
            this.obj = new Player();
        }

        /// <summary>
        /// Проверка актуальность ссылок в файле Temporary.json
        /// </summary>
        public Task CheckUrlExistingUrlAsync()
        {
            /*
             * Поиск треков с неактуальными ссылками и 
             */
            Player _obj = new Player();
            for (int i = 0; i < AudioList.Tracks.Count; i++)
            {
                /*
                 * Проверка ссылки на актуальность
                 */
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, AudioList.Tracks[i].Url);
                HttpResponseMessage response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode) continue;

                /*
                 * Запись новой данных о треке в файл yt.txt
                 */
                ProcessHellper processHellper = new ProcessHellper();

                if (AudioList.Tracks[i].ModuleType == ModuleType.YTMusic)
                {
                    processHellper.PrintYTAudioToFile(AudioList.Tracks[i]);
                    // Считывание полученных данных из файла yt.txx
                    FileHelper fileHelper = new FileHelper();
                    _obj.Tracks.Add(fileHelper.GetFileData(callback: fileHelper.ReadFromYtFile)[0]);
                }
                if (AudioList.Tracks[i].ModuleType == ModuleType.YandexMusic)
                {
                    _obj.Tracks.Add(new YMHelper().GetResponseObject(AudioList.Tracks[i].Title)[0]);
                }
            }
            AudioList = (T)_obj;
            return Task.CompletedTask;
        }
    }
}
