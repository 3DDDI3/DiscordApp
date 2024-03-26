using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordApp.Helper
{
    public class QueryHelper
    {
        /// <summary>
        /// Проверка корректности ввода треков
        /// Допустимые символы цифры, разделенные запятой или диапозон, разделенный тире (пример: 9,1,10-20)
        /// </summary>
        /// <param name="query">Входящий запрос</param>
        /// <param name="Context">SocketUserMessage</param>
        /// <returns>Сообщение или null</returns>
        public void CheckDigitsInQuery(string[] query)
        {
            /**
            * Вывод сообщения если пользователь ввел некорректные данные
            */
            if (query.Length == 0) 
               throw new Exception("Некорректный ввод. Попробуйте еще");
            if (!Regex.IsMatch(query[0], @"^(((([\d]+-[\d]+,?)+)|((\d+,?)+))|(((\d+,?)+)|(\d+-\d+,?)+)|(\d+,?))+$"))
                throw new Exception("Некорректный ввод. Попробуйте еще");
        }   

        /// <summary>
        /// Проверка входной строки
        /// </summary>
        /// <param name="query">Входная строка</param>
        /// <returns></returns>
        public void CheckStringInQuery(string[] query, bool isSingleCommand = false)
        {
            if (isSingleCommand & query.Length>0)
                throw new Exception("Некорректный ввод. Попробуйте еще");
            if (!isSingleCommand & query.Length == 0)
                throw new Exception("Некорректный ввод. Попробуйте еще");
        }

        /// <summary>
        /// Получение списка номеров тайтлов которые нужно воспроизвести
        /// </summary>
        /// <param name="str">Входящая строка</param>
        /// <returns>Список номеров треков</returns>
        public List<int> GetTrackIndexesFromQuery(string[] str, int TrackCount)
        {
            string[] Query = Regex.Split(str[0], ",");
            List<int> SubQuery = new List<int>();
            List<string> _SubQuery;
            for (int i = 0; i < Query.Length; i++)
            {
                _SubQuery = new List<string>();
                _SubQuery.AddRange(Regex.Split(Query[i], "-"));
                _SubQuery = _SubQuery.OrderBy(x => int.Parse(x.ToString())).ToList();
                if (_SubQuery.Count > 1)    
                {
                    SubQuery.Add(int.Parse(_SubQuery[0]));
                    SubQuery.Add(int.Parse(_SubQuery[1]));
                    for (int j = int.Parse(_SubQuery[0]) + 1; j < int.Parse(_SubQuery[1]); j++)
                        SubQuery.Add(j);
                }
                else SubQuery.Add(int.Parse(_SubQuery[0]));
            }
            SubQuery = SubQuery.Select(x => int.Parse(x.ToString()) - 1).Distinct().OrderBy(x => x).ToList();
            if (TrackCount < SubQuery.Max()) 
                throw new Exception("Номер трека не может быть больше чем размер плейлиста");
            return SubQuery;
        } 
    }
}
