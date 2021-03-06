﻿/*
 * Copyright © 2016 by DAQUGA studios
 * Author - Даниил Миренский
 * Created - 18.12.2016
 * Refactored 08.05.2019
 */

using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using VkNet.Model.Attachments;

namespace VkDiskCore.Connections.Executors
{
    public class UploadExecutor
    {
        public const int Kb = 1024;  // kilobyte
        public const int Mb = Kb * 1024;  // megabyte

        public delegate void ProgressChangedHandler(long downloadedSize, long timeMilliSeconds);
        public event ProgressChangedHandler ProgressChanged;

        public bool Stop;

        public string Upload(Stream from, string name, long size)
        {
            var uploadUrl = VkDisk.VkApi.Docs.GetUploadServer().UploadUrl;
            var boundary = "----------" + DateTime.Now.Ticks.ToString("x");
            var request = (HttpWebRequest)WebRequest.Create(uploadUrl);

            PrepareRequest(request, boundary);

            using (var s = request.GetRequestStream())
            {
                var h = Encoding.UTF8.GetBytes($"\r\n--{boundary}\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{name}\"\r\nContent-Type: application/octet-stream\r\n\r\n");
                s.WriteTimeout = 3600000;
                s.Write(h, 0, h.Length);

                var w = new Stopwatch();
                var b = new byte[Mb];
                var total = 0;
                int cur;

                w.Start();
                while (total < size && (cur = from.Read(b, 0, b.Length)) > 0)
                {
                    s.Write(b, 0, cur);
                    total += cur;

                    var ms = w.ElapsedMilliseconds;

                    if (ms <= 100) continue;

                    ProgressChanged?.Invoke(total, ms);
                    w.Restart();

                    if (Stop)
                        throw new Exception("STOP!!!!!");
                }

                var f = Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");
                s.Write(f, 0, f.Length);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var s = response.GetResponseStream())
            {
                if (s == null)
                    return null;

                using (var reader = new StreamReader(s))
                {
                    var resp = reader.ReadToEnd();

                    var file = (string)JObject.Parse(resp).SelectToken("file");

                    return TrySaveDoc(name, file);
                }
            }
        }

        private string TrySaveDoc(string name, string file, string capcha = null, long sid = -1)
        {
            try
            {
                return capcha == null
                    ? (VkDisk.VkApi.Docs.Save(title: name, file: file)[0].Instance as Document).Uri
                    : (VkDisk.VkApi.Docs.Save(title: name, file: file, captchaSid: sid, captchaKey: capcha)[0].Instance as Document).Uri;
            }
            catch (VkNet.Exception.CaptchaNeededException cne)
            {
                var image = (Bitmap)Image.FromStream(WebRequest.Create(cne.Img).GetResponse().GetResponseStream() ?? throw new InvalidOperationException());
                var cap = VkDisk.SolveCapcha(image);
                return TrySaveDoc(name, file, cap, (long)cne.Sid);
            }
        }

        private void PrepareRequest(HttpWebRequest request, string boundary)
        {
            request.Method = "POST";
            request.ContentType = $"multipart/form-data; boundary={boundary}";
            request.AllowWriteStreamBuffering = false;
            request.SendChunked = true;
            request.Timeout = 3600000;
            request.KeepAlive = true;
        }
    }
}