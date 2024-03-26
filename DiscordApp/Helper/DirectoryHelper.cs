using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeDLSharp.Metadata;

namespace DiscordApp.Helper
{
    /// <summary>
    /// Класс для работы с папками
    /// </summary>
    public class DirectoryHelper
    {
        /// <summary>
        /// Список файлов
        /// </summary>
        private List<FileInfo> files;

        private List<DirectoryInfo> directories;

        /// <summary>
        /// Папка
        /// </summary>
        private DirectoryInfo dir;

        public DirectoryInfo Dir
        {
            get => dir;
            set => dir = value;
        }
        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="dir">Папка</param>
        public DirectoryHelper(DirectoryInfo dir) { 
            files = new List<FileInfo>();
            directories = new List<DirectoryInfo>();
            this.dir = dir;
        }

       /// <summary>
       /// Ассинхронное удаление файлов (Если files не пустой, то будут удалены файлы все, кроме перечисленных в files)
       /// </summary>
       /// <param name="files">Список файлов, которые не нужно удалять</param>
        public void DeleteFilesAsync(List<string> files)
        {
            if (files.Count > 0)
            {
                foreach (var file in dir.GetFiles())
                {
                    if (!files.Contains(file.Name)) this.files.Add(file);
                }
            }
            else
            {
                this.files = dir.GetFiles().ToList();
            }
            this.files.AsParallel().Select(f => f).ForAll(f => f.Delete());
            Task.Delay(100).Wait();
        }

        /// <summary>
        /// Асинхронное удаление директорий
        /// </summary>
        /// <param name="directories">Список директорий</param>
        public void DeleteDirectoriesAsync(List<string> directories)
        {
            foreach (var directory in dir.GetDirectories())
            {
                if (!directories.Contains(directory.Name)) this.directories.Add(directory);
            }
            this.directories.AsParallel().Select(f => f).ForAll(f => Directory.Delete(f.FullName, true));
            Task.Delay(100).Wait();
        }

        /// <summary>
        /// Проверка существования файла
        /// </summary>
        /// <param name="FileName">Название файла</param>
        /// <returns></returns>
        public bool IsFileExist(string FileName)
        {
            if (dir.GetFiles(FileName).ToList().Count() > 0) return true;
            else return false;
        }
    }
}
