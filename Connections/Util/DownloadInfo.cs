namespace VkDiskCore.Connections.Util
{
    /// <summary>
    /// Описание загрузки
    /// </summary>
    public class DownloadInfo : BaseLoadInfo
    {
        private string _folder;
        private string _src;

        private bool _isVkd;

        public DownloadInfo(string src, string folder)
        {
            _src = src;
            _folder = folder;
        }

        /// <summary>
        /// Папка загрузки
        /// </summary>
        public string Folder
        {
            get => _folder;
            set => SetField(ref _folder, value);
        }

        /// <summary>
        /// Адрес загрузки
        /// </summary>
        public string Src
        {
            get => _src;
            set => SetField(ref _src, value);
        }

        public bool IsVkd
        {
            get => _isVkd;
            set => SetField(ref _isVkd, value);
        }
    }
}