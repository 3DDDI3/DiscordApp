using DiscordApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VkNet.Model;
using VkNet.Model.RequestParams.Ads;

namespace DiscordApp.Helper
{
    public class FileHelper
    {
        /// <summary>
        /// Считывание данных с файла
        /// </summary>
        /// <param name="callback">Функция, которая будет овтвечать за чтение данных</param>
        /// <returns>List<AudioObject></returns>
        public List<AudioModel> GetFileData(Func<bool, List<AudioModel>> callback, bool returnFullImage = false)
        {
           return callback(returnFullImage);
        }

        /// <summary>
        /// Чтение данных из файла yt.txt
        /// </summary>
        public Func<bool, List<AudioModel>> ReadFromYtFile = (bool returnFullImage) =>
        {
            List<AudioModel> AudioObjects = new List<AudioModel>();
            try
            {
                using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + @"\yt.txt", FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while (!sr.EndOfStream)
                        {
                            String[] array = sr.ReadLine().Split("~~");
                            {
                                if (returnFullImage)
                                {
                                    AudioObjects.Add(new AudioModel()
                                    {
                                        Duration = TimeSpan.FromSeconds(double.Parse(array[1])),
                                        Title = Regex.Replace(array[0], @"[^\d\w\s]", ""),
                                        ExternalUrl = array[2],
                                        Url = sr.ReadLine(),
                                        ModuleType = ModuleType.YTMusic
                                    });
                                }
                                else
                                {
                                    AudioObjects = new List<AudioModel>()
                                    {
                                        new AudioModel()
                                        {
                                            Duration = TimeSpan.FromSeconds(double.Parse(array[1])),
                                            Title = Regex.Replace(array[0], @"[^\d\w\s]", ""),
                                            ExternalUrl = array[2],
                                            Url = sr.ReadLine(),
                                            ModuleType = ModuleType.YTMusic
                                        }
                                    };
                                }
                            };
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                if (ex.Message.Contains("used by another process")) Task.Delay(100);
            }
            return AudioObjects;
        };
    }
}
