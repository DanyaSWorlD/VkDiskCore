using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using VkDiskCore.DataBase;
using VkDiskCore.PeersData;

namespace VkDiskCore.Services.Cache.Images
{
    public class ImageCache : IImageCache
    {
        private readonly List<DbImage> toLoad;
        private readonly object locker;

        private bool isInitiated;
        private bool isRunning;

        public ImageCache()
        {
            toLoad = new List<DbImage>();
            locker = new object();
        }

        public event ImageLoadedHandler ImageLoaded;

        public void Init()
        {
            lock (locker)
            {
                if (isInitiated) return;
                isInitiated = true;
            }

            Peers.CollectionChanged += CollectionUpdatedHandler;

            if (!Directory.Exists(VkDisk.ImageCacheDir))
                Directory.CreateDirectory(VkDisk.ImageCacheDir);

            Task.Factory.StartNew(Clean);
        }

        public void Stop()
        {
            isInitiated = false;
        }

        public Bitmap GetImage(string link)
        {
            if (string.IsNullOrEmpty(link)) return null;
            var image = new DbImage { Link = link };
            return GetImage(image);
        }

        public Bitmap GetImage(long peerId)
        {
            var image = new DbImage { PeerId = peerId };
            return GetImage(image);
        }

        private Bitmap GetImage(DbImage image)
        {
            var dbImage = GetFromDatabase(image);

            if (dbImage == null)
            {
                AddToDownload(image);
                return null;
            }

            UpdateLastAccessDate(dbImage);

            return DbImageToBitmap(dbImage);
        }

        private void AddToDownload(DbImage image)
        {
            toLoad.Add(image);

            if (!isRunning)
                Run();
        }

        private DbImage GetFromDatabase(DbImage image)
        {
            var connection = MainDb.GetCollection<DbImage>();

            if (string.IsNullOrEmpty(image.Link))
                return connection.FindOne(o => o.Id == image.Id);

            if (image.PeerId == null)
                return connection.FindOne(o => o.Link == image.Link);

            return connection.FindOne(o => o.Link == image.Link && o.Id == image.Id);
        }

        private Bitmap DbImageToBitmap(DbImage image) => new Bitmap(GetImageCachePath(image));

        private void LoadImage(DbImage image)
        {
            using (var client = new HttpClient())
            {
                var resp = client.GetAsync(image.Link).Result;
                var bitmap = (Bitmap)Image.FromStream(resp.Content.ReadAsStreamAsync().Result);
                bitmap.Save(GetImageCachePath(image));
            }
        }

        private void UpdateLastAccessDate(DbImage image)
        {
            image.AccesDate = DateTime.Now;
            MainDb.GetDb.GetCollection<DbImage>().Update(image);
        }

        private void CollectionUpdatedHandler(IEnumerable<long> peers)
        {
            foreach (var peer in peers)
                AddToDownload(new DbImage { PeerId = peer });
        }

        private void Clean()
        {
            var startCollection = MainDb.GetCollection<DbImage>();

            var toRemove = startCollection.FindAll().Where(item => item.AccesDate.AddDays(7) < DateTime.Now).ToList();

            foreach (var image in toRemove)
            {
                File.Delete(GetImageCachePath(image));
                startCollection.Delete(image.Id);
            }
        }

        private void Run()
        {
            while (isInitiated)
            {
                if (toLoad.Count == 0)
                {
                    isRunning = false;
                    return;
                }

                isRunning = true;

                var item = toLoad[0];
                toLoad.Remove(item);

                var collection = MainDb.GetDb.GetCollection<DbImage>();

                if (collection.Exists(o => o.PeerId == item.PeerId)) continue;

                if (string.IsNullOrEmpty(item.Link))
                    item.Link = Peers.GetImageLink(item.PeerId ?? 0);

                if (item.Link == null) continue;

                if (collection.FindOne(o => o.Link == item.Link) != null) continue;

                item.AccesDate = DateTime.Now;
                item.Name = item.Link.Split('?')[0].Split('/').Last();

                LoadImage(item);

                collection.Insert(item);

                ImageLoaded?.Invoke(item);
            }
        }

        private string GetImageCachePath(DbImage image) => $"{VkDisk.ImageCacheDir}{image.Name}";
    }
}