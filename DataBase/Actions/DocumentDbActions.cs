using System;
using System.Collections.Generic;
using System.Linq;
using VkNet.Model.Attachments;

namespace VkDiskCore.DataBase.Actions
{
    public static class DocumentDbActions
    {
        public delegate void DocumentsChangedDelegate(IEnumerable<Document> add, IEnumerable<long> delete);
        public static event DocumentsChangedDelegate DocumentsChanged;

        public static bool Sync()
        {
            var items = MainDb.GetDocuments();

            try
            {
                var toDelete = new List<long>();

                var docs = VkDisk.VkApi.Docs.Get(ownerId: VkDisk.VkApi.UserId).ToList();

                foreach (var item in items)
                {
                    var notFound = true;
                    foreach (var doc in docs)
                    {
                        if (item.DocumentId != doc.Id) continue;

                        notFound = false;
                        docs.Remove(doc);
                        break;
                    }

                    if (notFound)
                        toDelete.Add(item.DocumentId);

                }

                if (toDelete.Count > 0 || docs.Count > 0)
                {

                    MainDb.DeleteDocuments(toDelete);
                    MainDb.AddDocuments(docs);

                    DocumentsChanged?.Invoke(docs, toDelete);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
