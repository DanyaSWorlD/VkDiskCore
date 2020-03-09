using LiteDB;
using System.Collections.Generic;
using System.Linq;
using VkDiskCore.DataBase.Model;

namespace VkDiskCore.DataBase
{
    public static class MainDb
    {
        public static LiteDatabase GetDb => DbProvider.GetDb;

        public static LiteCollection<T> GetCollection<T>()
        {
            return GetDb.GetCollection<T>();
        }

        public static List<Document> GetDocuments()
        {
            var collection = GetCollection<Document>();

            if (collection?.Count() > 0)
            {
                var col = collection.Find(o => o.UserId.Equals(VkDisk.VkApi.UserId ?? 0)).ToList(); //litedb 3x
                //var col = collection.Find(o => o.UserId == (VkDisk.VkApi.UserId ?? 0)).ToList(); //litedb 5
                col.Sort((a, b) => b.Date.CompareTo(a.Date));
                return col;
            }

            return new List<Document>();
        }

        public static void DeleteDocuments(IEnumerable<long> docs)
        {
            var liteColl = GetCollection<Document>();

            foreach (var doc in docs)
            {
                var id = liteColl.FindOne(o => o.DocumentId == doc && o.UserId.Equals(VkDisk.VkApi.UserId))
                    .Id;
                liteColl.Delete(id);
            }
        }

        public static void AddDocuments(IEnumerable<VkNet.Model.Attachments.Document> docs)
        {
            var liteColl = GetCollection<Document>();

            foreach (var doc in docs)
            {
                liteColl.Insert(Document.FromVkNetDocument(doc));
            }
        }
    }
}