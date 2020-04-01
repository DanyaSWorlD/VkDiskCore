using System;
using System.Collections.Generic;
using System.IO;
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
                        var executor = new DownloadExecutor();
                        executor.ProgressChanged += info.ProgressChangedHandler;
                        info.LoadStop += () => { executor.Stop = true; };
                        info.LoadState = LoadState.Loading;
                        executor.Download(s, info.Src, info.TotalLoad);
                        info.LoadState = LoadState.Finished;
                        return;
                    }

                    // todo remove this code duplication
                    // load links to file parts
                    var links = new List<string>();
                    using (var ms = new MemoryStream())
                    {
                        new DownloadExecutor().Download(ms, info.Src);
                        ms.Position = 0;
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
