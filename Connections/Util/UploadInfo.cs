using System.Collections.Generic;

namespace VkDiskCore.Connections.Util
{
    /// <summary>
    /// Информация о загружаемом файле
    /// </summary>
    public class UploadInfo : BaseLoadInfo
    {
        public UploadInfo()
        {
            Links = new List<string>();
        }

        public UploadInfo(string path, string name) : this()
        {
            Path = path;
            Name = name;
        }

        /// <summary>
        /// Ссылки на загруженные части
        /// </summary>
        public List<string> Links { get; set; }

        /// <summary>
        /// путь к файлу
        /// </summary>
        public string Path { get; set; }
    }
}
