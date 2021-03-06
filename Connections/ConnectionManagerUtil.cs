﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using VkDiskCore.Connections.Executors;
using VkDiskCore.Connections.Util;
using VkDiskCore.Utility;

namespace VkDiskCore.Connections
{
    public static partial class ConnectionManager
    {
        /// <summary>
        /// Получает имя временного файла для загрузки.
        /// Если файл с таким имененм уже есть - удаляет.
        /// </summary>
        /// <param name="file">Имя файла</param>
        /// <param name="folder">Папка, в которой находится файл</param>
        /// <returns> Полный путь до файла </returns>
        private static string PreparePartFile(string file, string folder)
        {
            var partFile = $"{file.WithNoExtensions()}.part";
            var partFilePath = folder.FixFolder() + partFile;

            if (File.Exists(partFilePath))
                File.Delete(partFilePath);

            return partFilePath;
        }

        /// <summary>
        /// Получить строковое содержимое файла с удаленного сервера
        /// </summary>
        /// <param name="src">url с которого брать файл</param>
        /// <returns> Содержимое файла </returns>
        private static string GetFileContent(string src)
        {
            using (var stream = new MemoryStream())
            {
                new DownloadExecutor().Download(stream, src);
                stream.Position = 0;
                using (var sr = new StreamReader(stream))
                    return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// Возвращает суммарный размер файлов
        /// </summary>
        /// <param name="links"> ссылки на файлы </param>
        /// <returns> общий размер файлов </returns>
        private static long GetFileSize(IEnumerable<string> links)
        {
            return links.Sum(link => GetFileSize(link));
        }

        /// <summary>
        /// Возвращает размер файла по обьекту DownloadInfo
        /// </summary>
        /// <param name="downloadInfo"></param>
        private static void GetFileSize(DownloadInfo downloadInfo)
        {
            var r = (HttpWebRequest)WebRequest.Create(downloadInfo.Src);
            using (var re = r.GetResponse())
                downloadInfo.TotalSize = long.Parse(re.Headers.Get("Content-Length"));
        }

        /// <summary>
        /// Возвращает размер файла по ссылке
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static long GetFileSize(string url)
        {
            var r = (HttpWebRequest)WebRequest.Create(url);
            using (var re = r.GetResponse())
                return long.Parse(re.Headers.Get("Content-Length"));
        }

        /// <summary>
        /// Потокобезопасная функция добавления в список загрузок
        /// </summary>
        /// <param name="downloadInfo">обьект описания загрузки</param>
        /// <returns></returns>
        private static int AddToDownloads(DownloadInfo downloadInfo)
        {
            lock (DownloadLocker)
            {
                Downloads.Add(downloadInfo);
                var id = Downloads.Count - 1;
                downloadInfo.Id = id;
                DownloadItemCollectionUpdated?.Invoke(downloadInfo);
                return id;
            }
        }

        private static int AddToUploads(UploadInfo uploadInfo)
        {
            lock (Uploads)
            {
                   Uploads.Add(uploadInfo);
                   var id = Uploads.Count - 1;
                   uploadInfo.Id = id;
                   UploadItemCollectionUpdated?.Invoke(uploadInfo);
                   return id;
            }
        }

        /// <summary>
        /// Возвращает имя файла, так, чтоб оно не существовало в папке загрузки.
        /// </summary>
        /// <returns> имя файла </returns>
        private static string GetFileName(DownloadInfo downloadInfo)
        {
            var name = downloadInfo.Name ?? Stuff.GetFileName(downloadInfo.Src);

            if (name.Extension() == "vkd")
            {
                downloadInfo.IsVkd = true;
                name = name.WithNoExtensions();
            }

            var num = 0;

            while (File.Exists(downloadInfo.Folder.FixFolder() + NameWithNumber(name, num)))
                num++;

            return NameWithNumber(name, num);
        }

        /// <summary>
        /// Возвращает имя файла в виде name(number).extention
        /// ex: name = file.txt, number = 1. result = file(1).txt
        /// </summary>
        /// <param name="name">Имя файла</param>
        /// <param name="number">Номер</param>
        /// <returns></returns>
        private static string NameWithNumber(string name, int number)
        {
            if (number == 0) return name;

            var nameNoExt = name.WithNoExtensions();
            var ext = name.Extension();

            return $"{nameNoExt}({number}).{ext}";
        }

        private static bool FileAllowed(string extension)
        {
            var extensions = new[]
                                 {
                                     "exe",
                                     "jar",
                                     "bat",
                                     "apk",
                                     "mp3",
                                     "rar",
                                     "zip"
                                 };

            return extensions.All(s => !s.Equals(extension));
        }
    }
}