using DiscordApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using VkNet.Model.Attachments;

namespace DiscordApp.Helper
{
    internal class JsonHelper
    {
        private string path;
        public JsonHelper()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) path = $@"{Directory.GetCurrentDirectory()}\";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) path = $@"{Directory.GetCurrentDirectory()}/";
        }
        /// <summary>
        /// Запись в JSON файл результата поиска аудиозаписей и плейлиста
        /// </summary>
        /// <param name="obj">Аудиозаписи</param>
        /// <param name="playlist">Плейлист</param>
        public void WriteToJsonAsync<T>(string Filename, T AM)
        {
            string json = JsonConvert.SerializeObject(AM, Formatting.Indented);
            File.WriteAllText($@"{Directory.GetCurrentDirectory()}\{Filename}", json);
        }

        /// <summary>
        /// Получение данных с json файал
        /// </summary>
        /// <typeparam name="T">T class</typeparam>
        /// <param name="FileName">Название файла</param>
        /// <param name="AM">T obect</param>
        /// <returns>T object</returns>
        public T GetDataFromJson<T>(string FileName, T AM = null) where T : class
        {
            string JsonObject = File.ReadAllText($"{path}{FileName}");
            AM = (T)JsonConvert.DeserializeObject(JsonObject, typeof(T));
            return AM;
        }

        /// <summary>
        /// Получение содержимого конфигурационого файла
        /// </summary>
        /// <returns></returns>
        public JObject GetDataFromInitFile()
        {
            string json = File.ReadAllText($"{path}appsettings.json");
            return JsonConvert.DeserializeObject<JObject>(json);
        }

        /// <summary>
        /// Запись в конфигурационный файл 
        /// </summary>
        /// <param name="obj">Объект</param>
        public void WriteDataToInitFile(JObject obj)
        {
            File.WriteAllText($"{path}appsettings.json", JsonConvert.SerializeObject(obj, Formatting.Indented));
        }

    }
}
