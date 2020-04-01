// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

/*
 * Copyright © 2016-2019 by DAQUGA studios
 * Author - Даниил Миренский
 * Creation - 13.12.2016
 * Refactored - 27.04.2019
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace VkDiskCore.Connections.Executors
{
    public class DownloadExecutor
    {
        public const int Kb = 1024;  // kilobyte
        public const int Mb = Kb * 1024;  // megabyte

        public delegate void ProgressChangedHandler(long downloadedSize, long timeMilliSeconds);
        public event ProgressChangedHandler ProgressChanged;

        public bool Stop = false;

        public long Download(Stream writeStream, string url, long readStreamStart = 0)
        {
            using (var client = new HttpClient())
            using (var stream = client.GetStreamAsync(url).Result)
            {
                int c;
                var total = readStreamStart;
                var buffer = new byte[Mb];

                Skip(stream, (int)total);
                writeStream.Position = total;

                var watch = new Stopwatch();
                watch.Start();

                while ((c = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writeStream.Write(buffer, 0, c);
                    total += c;
                    var ms = watch.ElapsedMilliseconds;

                    if (ms <= 100) continue;

                    watch.Restart();
                    ProgressChanged?.Invoke(total, ms);

                    if (Stop)
                        throw new Exception("STOP!!!!!");
                }

                watch.Stop();
                return total;
            }
        }

        private void Skip(Stream s, int n)
        {
            if (s.CanSeek)
                s.Seek(n, SeekOrigin.Current);
            else
            {
                const int bufsize = 65536;
                var buf = new byte[bufsize];
                while (n > 0)
                {
                    n -= s.Read(buf, 0, Math.Min(n, bufsize));

                    if (Stop)
                        throw new Exception("STOP!!!!!");
                }
            }
        }
    }
}