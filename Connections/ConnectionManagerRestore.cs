using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VkDiskCore.Connections.Executors;
using VkDiskCore.Connections.Util;
using VkDiskCore.Utility;

namespace VkDiskCore.Connections
{
    public static partial class ConnectionManager
    {
        /// <summary>
        /// resume failed download connection
        /// </summary>
        /// <param name="info"></param>
        public static void RestoreDownloadConnection(DownloadInfo info)
        {
            try
            {
                // standard files loads as is with default extension
                var file = $"{info.Folder}\\{info.Name}";

                // but if file is .vkd it loads into a .part file, then changes it's extension to real one
                if (info.IsVkd)
                    file = file.WithNoExtensions() + ".part";

                // if no file we're looking for exists then return
                if (!File.Exists(file)) return;

                // if all is ok, open file for write
                using (var s = File.OpenWrite(file))
                {
                    // if file length not equal to downloaded length that means file been changed and we wont correctly resume downloading it 
                    if (s.Length != info.TotalLoad) return;

                    info.LoadState = LoadState.Starting;

                    if (!info.IsVkd)
                    {
                        DownloadPartOrFile(new DownloadExecutor(), info, s, info.Src, totalLoadOffset: info.TotalLoad, onlyOne: true);
                        return;
                    }

                    // load links to file parts
                    var links = GetVkdHeader(info.Src).ToList();

                    var part = (int)(info.TotalLoad / PartSize);

                    info.LoadState = LoadState.Loading;

                    DownloadPartOrFile(new DownloadExecutor(), info, s, links[part], part * PartSize, info.TotalLoad % PartSize);

                    for (var i = part + 1; i < links.Count; i++)
                        DownloadPartOrFile(new DownloadExecutor(), info, s, links[i], i * PartSize);

                    File.Move(file, $"{info.Folder}\\{info.Name}");
                    info.LoadState = LoadState.Finished;
                }
            }
            catch (Exception)
            {
                info.LoadState = LoadState.Error;
            }
        }

        /// <summary>
        /// Continue failed upload
        /// </summary>
        /// <param name="info"></param>
        public static void RestoreUploadConnection(UploadInfo info, int restoreAttempts = 0)
        {

            if (restoreAttempts >= VkDisk.VkDiskSettings.AutoRetryMaxCount)
                return;

            if (!info.IsVkd)
            {
                Task.Factory.StartNew(() => Upload(info.Path));
                return;
            }

            info.LoadState = LoadState.Starting;

            try
            {
                if (info.Links.Count * PartSize < info.TotalSize)
                    using (var stream = File.OpenRead(info.Path))
                    {
                        info.TotalLoad = info.Links.Count * PartSize;
                        stream.Position = info.TotalLoad;
                        info.LoadState = LoadState.Loading;

                        var executor = new UploadExecutor();
                        executor.ProgressChanged += info.ProgressChangedHandler;
                        info.LoadStop += () => { executor.Stop = true; };

                        while (stream.Position < stream.Length)
                        {
                            var partSize = stream.Position + PartSize > stream.Length
                                               ? stream.Length - stream.Position
                                               : PartSize;

                            var link = executor.Upload(
                                stream,
                                $"{info.Name.WithNoExtensions()}.{info.Links.Count}.vkdpart",
                                partSize);

                            info.Links.Add(link);
                        }
                    }

                var header = string.Join(Environment.NewLine, info.Links);

                using (var ms = new MemoryStream(Encoding.Default.GetBytes(header)))
                    new UploadExecutor().Upload(ms, $"{info.Name}.vkd", ms.Length);

                info.LoadState = LoadState.Finished;
            }
            catch
            {
                info.LoadState = LoadState.Error;
                if (VkDisk.VkDiskSettings.AutoRetryUpload)
                    Task.Factory.StartNew(() => RestoreUploadConnection(info, restoreAttempts++));
            }
        }
    }
}
