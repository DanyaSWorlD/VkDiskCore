namespace VkDiskCore.Connections.Util
{
    /// <summary>
    /// Описание загрузки
    /// </summary>
    public class DownloadInfo : BaseLoadInfo
    {
        private string folder;
        private string src;

        private bool isVkd;

        public DownloadInfo(string src, string folder)
        {
            this.src = src;
            this.folder = folder;
        }

        /// <summary>
        /// Папка загрузки
        /// </summary>
        public string Folder
        {
            get => folder;
            set => SetField(ref folder, value);
        }

        /// <summary>
        /// Адрес загрузки
        /// </summary>
        public string Src
        {
            get => src;
            set => SetField(ref src, value);
        }

        public bool IsVkd
        {
            get => isVkd;
            set => SetField(ref isVkd, value);
        }
    }
}