// created 24.04.2019

using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using VkDiskCore.Connections.Executors;
using VkDiskCore.Connections.Models;
using VkDiskCore.Connections.Util;
using VkDiskCore.Errors;
using VkDiskCore.Utility;

using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams.Stories;

using Exception = System.Exception;

namespace VkDiskCore.Connections
{
    /// <summary>
    /// Мэнеджер загрузок и отправок документов вк
    /// </summary>
    public static partial class ConnectionManager
    {
        #region Fields

        private static readonly List<DownloadInfo> Downloads = new List<DownloadInfo>();
        private static readonly List<UploadInfo> Uploads = new List<UploadInfo>();

        private static readonly object DownloadLocker = new object();

        public delegate void DownloadUpdatedHandler(DownloadInfo l);

        public delegate void UploadUpdatedHanlder(UploadInfo i);

        public static event DownloadUpdatedHandler DownloadItemCollectionUpdated;

        public static event UploadUpdatedHanlder UploadItemCollectionUpdated;

        #endregion

        #region Constants

        public static long PartSize => (long)2 * 1024 * 1024 * 1024; //2 GB

        public static int MB => 1024 * 1024; // 1MB

        public static List<DownloadInfo> GetDownloads => Downloads;

        public static List<UploadInfo> GetUploads => Uploads;

        #endregion


        public static void DownloadAsync(string src, string folder)
        {
            Task.Factory.StartNew(() => Download(src, folder));
        }

        public static void UploadAsync(string path)
        {
            Task.Factory.StartNew(() => Upload(path));
        }

        public static void Download(string src, string folder)
        {
            // если идет загрузка этого файла - выходим.
            lock (DownloadLocker)
                if (Downloads.Any(o => o.Src == src && o.Folder == folder && o.LoadState < LoadState.Finished))
                    return;

            var download = new DownloadInfo(src, folder);
            AddToDownloads(download);

            download.Name = GetFileName(download);

            if (!download.IsVkd)
            {
                GetFileSize(download);

                try
                {
                    using (var s = File.Create(folder.FixFolder() + download.Name))
                        DownloadPartOrFile(new DownloadExecutor(), download, s, download.Src, onlyOne: true);
                }
                catch (Exception)
                {
                    download.LoadState = LoadState.Error;
                }
            }
            else
            {
                download = GetVkdHeader(download);
                download.TotalSize = download.Vkd.Size ?? GetFileSize(download.Links);

                var links = download.Vkd == null ? download.Links : GetVkdLinks(download);

                var partFile = PreparePartFile(download.Name, download.Folder);

                download.LoadState = LoadState.Loading;

                try
                {
                    using (var stream = File.Create(partFile))
                        foreach (var link in links)
                            DownloadPartOrFile(new DownloadExecutor(), download, stream, link, download.TotalLoad);

                    File.Move(partFile, download.Folder.FixFolder() + GetFileName(download));
                    download.LoadState = LoadState.Finished;
                }
                catch (Exception)
                {
                    download.LoadState = LoadState.Error;
                }
            }
        }

        public static void Upload(string path)
        {
            var name = path.Split('\\').Last();
            var uploadInfo = new UploadInfo(path, name);

            AddToUploads(uploadInfo);

            var executor = new UploadExecutor();

            uploadInfo.LoadState = LoadState.Starting;
            uploadInfo.LoadStop += () => { executor.Stop = true; };

            var documents = new List<Document>();

            using (var stream = File.OpenRead(path))
            {
                uploadInfo.TotalSize = stream.Length;
                uploadInfo.IsVkd = !FileAllowed(uploadInfo.Ext) || uploadInfo.TotalSize > PartSize;

                if (uploadInfo.IsVkd)
                    executor.ProgressChanged += (downloadedSize, time)
                        => uploadInfo.ProgressChangedHandler(downloadedSize + (PartSize * uploadInfo.Links.Count), time);
                else
                    executor.ProgressChanged += uploadInfo.ProgressChangedHandler;

                uploadInfo.Vkd = new Vkd
                {
                    Type = VkdTypes.VkdFile,
                    Name = uploadInfo.Name,
                    Size = uploadInfo.TotalSize,
                    Filetype = uploadInfo.Ext
                };

                var retryCount = 0;
                while (stream.Position < stream.Length || retryCount < VkDisk.VkDiskSettings.AutoRetryMaxCount)
                {
                    try
                    {
                        uploadInfo.LoadState = LoadState.Loading;

                        var partSize = stream.Position + PartSize > stream.Length
                                           ? stream.Length - stream.Position
                                           : PartSize;

                        var localName = uploadInfo.IsVkd
                                            ? NameWithNumber(
                                                $"{uploadInfo.Name.WithNoExtensions()}.vkdpart",
                                                uploadInfo.Links.Count)
                                            : uploadInfo.Name;

                        var document = executor.Upload(stream, localName, partSize);

                        if (!uploadInfo.IsVkd)
                        {
                            uploadInfo.LoadState = LoadState.Finished;
                            return;
                        }

                        uploadInfo.Vkd.InnerVkds.Add(new Vkd
                        {
                            Filetype = "vkdpart",
                            Id = document.Id,
                            OwnerId = document.OwnerId,
                            Link = document.Uri,
                            Size = document.Size,
                            Type = VkdTypes.File,
                            Name = localName
                        });
                    }
                    catch (Exception e)
                    {
                        uploadInfo.LoadState = LoadState.Error;

                        VkDisk.HandleException(new FileUploadingException("Во время отправки файла произошла одна или несколько ошибок", e));

                        var uploadPosition = uploadInfo.Links.Count * (long)PartSize;
                        if (uploadPosition != stream.Position)
                            stream.Position = uploadPosition;

                        retryCount++;
                    }
                }

                using (var ms = new MemoryStream(Encoding.Default.GetBytes(JsonConvert.SerializeObject(uploadInfo.Vkd))))
                    new UploadExecutor().Upload(ms, $"{name}.vkd", ms.Length);

                uploadInfo.LoadState = LoadState.Finished;
            }
        }
    }
}