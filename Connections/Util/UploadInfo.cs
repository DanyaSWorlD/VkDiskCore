using System.Collections.Generic;

namespace VkDiskCore.Connections.Util
{
    /// <summary>
    /// Информация о загружаемом файле
    /// </summary>
    public class UploadInfo : BaseLoadInfo
    {
        public UploadInfo(string path, string name)
        {
            Path = path;
            Name = name;
        }

        /// <summary>
        /// путь к файлу
        /// </summary>
        public string Path { get; set; }
    }
}
