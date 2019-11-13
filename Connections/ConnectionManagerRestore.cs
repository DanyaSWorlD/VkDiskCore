using System;
using System.Collections.Generic;
using System.IO;
using VkDiskCore.Connections.Executors;
using VkDiskCore.Connections.Util;

namespace VkDiskCore.Connections
{
    public static partial class ConnectionManager
    {
        public static void RestoreDownloadConnection(DownloadInfo info)
        {
            try
            {

                var file = $"{info.Folder}\\{info.Name}";
                if (!File.Exists(file)) return;
                using (var s = File.OpenWrite(file))
                {
                    if (s.Length != info.TotalLoad) return;

                    info.LoadState = LoadState.Starting;

                    if (!info.IsVkd)
                    {
                        var executor = new DownloadExecutor();
                        executor.ProgressChanged += info.ProgressChangedHandler;
                        info.LoadStop += () => { executor.Stop = true; };
                        info.LoadState = LoadState.Loading;
                        executor.Download(s, info.Src, info.TotalLoad);
                        info.LoadState = LoadState.Finished;
                        return;
                    }

                    var links = new List<string>();
                    using (var ms = new MemoryStream())
                    {
                        new DownloadExecutor().Download(ms, info.Src);
                        using (var sr = new StreamReader(ms))
                            while (!sr.EndOfStream)
                                links.Add(sr.ReadLine());
                    }

                    var part = (int)(info.TotalLoad / PartSize);

                    var vkdEx = new DownloadExecutor();
                    vkdEx.ProgressChanged += info.ProgressChangedHandler;
                    info.LoadStop += () => { vkdEx.Stop = true; };
                    info.LoadState = LoadState.Loading;
                    vkdEx.Download(s, links[part], info.TotalLoad % PartSize);

                    for (var i = part + 1; i < links.Count; i++)
                    {
                        vkdEx = new DownloadExecutor();
                        vkdEx.ProgressChanged += info.ProgressChangedHandler;
                        info.LoadStop += () => { vkdEx.Stop = true; };
                        vkdEx.Download(s, links[i]);
                    }

                    info.LoadState = LoadState.Finished;
                }
            }
            catch (Exception)
            {
                info.LoadState = LoadState.Error;
            }
        }
    }
}
