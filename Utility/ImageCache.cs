using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VkDiskCore.DataBase;

namespace VkDiskCore.Utility
{
    public static class ImageCache
    {
        private static bool _inited;
        private static bool _dataAdded;

        private static List<long> _itemsToLoad;

        public delegate void ImageLoadedHandler(long id);
        public static event ImageLoadedHandler ImageLoaded;

        public static void Init()
        {
            if (_inited) return;
            _inited = true;

            _itemsToLoad = new List<long>();

            Peers.CollectionChanged += CollectionUpdatedHandler;

            if (!Directory.Exists(VkDisk.ImageCacheDir))
                Directory.CreateDirectory(VkDisk.ImageCacheDir);

            Task.Factory.StartNew(DoLoad);
        }

        public static void Stop()
        {
            _inited = false;
        }

        public static Bitmap GetImage(string link)
        {
            var collection = MainDb.GetCollection<DbImage>();

            var item = collection.FindOne(o => o.Link == link);
            if (item == null) return null;

            item.AccesDate = DateTime.Now;

            collection.Update(item);

            var path = VkDisk.ImageCacheDir + item.Name;
            return new Bitmap(path);
        }

        public static Bitmap GetImage(long peerId)
        {
            var collection = MainDb.GetCollection<DbImage>();

            var item = collection.FindOne(o => o.PeerId == peerId);
            if (item == null) return null;

            item.AccesDate = DateTime.Now;

            collection.Update(item);

            var path = VkDisk.ImageCacheDir + item.Name;
            return new Bitmap(path);
        }

        private static void CollectionUpdatedHandler(IEnumerable<long> peers)
        {
            foreach (var peer in peers)
                _itemsToLoad.Add(peer);

            _dataAdded = true;
        }

        private static void DoLoad()
        {
            var startCollection = MainDb.GetCollection<DbImage>();

            var toRemove = startCollection.FindAll().Where(item => item.AccesDate.AddDays(7) < DateTime.Now).ToList();
            foreach (var dbImage in toRemove)
                startCollection.Delete(dbImage.Id);

            startCollection = null;
            toRemove = null;

            while (_inited)
            {
                if (_itemsToLoad.Count == 0)
                {
                    while (!_dataAdded)
                        Thread.Sleep(1000);

                    _dataAdded = false;
                }
                else
                {
                    var item = _itemsToLoad[0];
                    _itemsToLoad.RemoveAt(0);

                    var collection = MainDb.GetCollection<DbImage>();

                    if (collection.Exists(o => o.PeerId == item)) continue;

                    var link = Peers.GetImageLink(item);
                    if (link == null) continue;

                    if (collection.FindOne(o => o.Link == link) != null) continue;

                    var dbImage = new DbImage
                    {
                        AccesDate = DateTime.Now,
                        Link = link,
                        PeerId = item,
                        Name = link.Split('?')[0].Split('/').Last()
                    };

                    using (var client = new HttpClient())
                    {
                        var resp = client.GetAsync(link).Result;
                        var bitmap = (Bitmap)Image.FromStream(resp.Content.ReadAsStreamAsync().Result);
                        bitmap.Save($"{VkDisk.ImageCacheDir}{dbImage.Name}");
                    }

                    collection.Insert(dbImage);

                    ImageLoaded?.Invoke(item);
                }

                Thread.Sleep(100);
            }
        }
    }
}
