// created 24.04.2019

using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using VkDiskCore.Connections.Executors;
using VkDiskCore.Connections.Util;
using VkDiskCore.Utility;
using Exception = System.Exception;

namespace VkDiskCore.Connections
{
    /// <summary>
    /// Мэнеджер загрузок и отправок документов вк
    /// </summary>
    public static partial class ConnectionManager
    {
        #region Constants

        public static int PartSize = 198 * 1024 * 1024; // 198 Mb
        public static int MB = 1024 * 1024; // 1MB

        #endregion


        #region Fields

        private static readonly List<DownloadInfo> Downloads = new List<DownloadInfo>();
        private static readonly List<UploadInfo> Uploads = new List<UploadInfo>();

        private static readonly object DownloadLocker = new object();

        public delegate void DownloadUpdatedHandler(DownloadInfo l);

        public delegate void UploadUpdatedHanlder(UploadInfo i);

        public static event DownloadUpdatedHandler DownloadItemCollectionUpdated;

        public static event UploadUpdatedHanlder UploadItemCollectionUpdated;

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
                var links = GetVkdHeader(download.Src).ToList();

                download.TotalSize = GetFileSize(links);

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

            var upload = new UploadInfo(path, name);

            AddToUploads(upload);

            var vkd = !FileAllowed(upload.Ext);

            var executor = new UploadExecutor();
            executor.ProgressChanged += upload.ProgressChangedHandler;
            upload.LoadStop += () => { executor.Stop = true; };

            using (var stream = File.OpenRead(path))
            {
                upload.TotalSize = stream.Length;
                upload.LoadState = LoadState.Loading;

                vkd = vkd || upload.TotalSize > PartSize;
                if (!vkd)
                {
                    try
                    {
                        executor.Upload(stream, name, stream.Length);
                        upload.LoadState = LoadState.Finished;
                        return;
                    }
                    catch (Exception)
                    {
                        upload.LoadState = LoadState.Error;
                        Task.Factory.StartNew(() => Upload(path));
                    }
                }

                try
                {
                    upload.IsVkd = true;

                    while (stream.Position < stream.Length)
                    {
                        var partSize = stream.Position + PartSize > stream.Length
                                           ? stream.Length - stream.Position
                                           : PartSize;

                        var link = executor.Upload(
                            stream,
                            $"{name.WithNoExtensions()}.{upload.Links.Count}.vkdpart",
                            partSize);

                        upload.Links.Add(link);
                    }

                    var sb = new StringBuilder();

                    foreach (var url in upload.Links)
                        sb.Append(url).Append(Environment.NewLine);

                    using (var ms = new MemoryStream(Encoding.Default.GetBytes(sb.ToString())))
                        new UploadExecutor().Upload(ms, $"{name}.vkd", ms.Length);

                    upload.LoadState = LoadState.Finished;
                }
                catch
                {
                    upload.LoadState = LoadState.Error;
                    Task.Factory.StartNew(() => RestoreUploadConnection(upload));
                }
            }
        }
    }
}