using System;

namespace VkDiskCore.DataBase.Model
{
    public class Document
    {
        /// <summary>
        /// Идентификатор записи
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Пользователь, которому принадлежит файл
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// id документа
        /// </summary>
        public long DocumentId { get; set; }

        /// <summary>
        /// Имя дркумента
        /// </summary>
        public string Name { get; set; }

        public string Ext { get; set; }

        /// <summary>
        /// Ссылка на документ
        /// </summary>
        public string Src { get; set; }

        /// <summary>
        /// Размер документа
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Дата добавления файла
        /// </summary>
        public DateTime Date { get; set; }

        public VkNet.Model.Attachments.Document ToVkNetDocument()
        {
            return new VkNet.Model.Attachments.Document()
            {
                Id = DocumentId,
                Title = Name,
                Ext = Ext,
                Size = Size,
                Date = Date,
                Uri = Src
            };
        }

        public static Document FromVkNetDocument(VkNet.Model.Attachments.Document doc)
        {
            return new Document()
            {
                UserId = VkDisk.VkApi.UserId ?? 0,
                DocumentId = doc.Id ?? 0,
                Name = doc.Title,
                Ext = doc.Ext,
                Size = doc.Size ?? 0,
                Src = doc.Uri,
                Date = doc.Date ?? DateTime.Now
            };
        }
    }
}
