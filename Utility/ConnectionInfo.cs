

using System.Collections.Generic;
using System.Linq;

namespace VkDiskCore.Utility
{
    /// <summary>
    /// Connection information
    /// <seealso cref="VkDiskFileInfo"/>
    /// </summary>
    public class ConnectionInfo
    {
        private const int maxHistory = 10;
        private List<long> _history;

        public ConnectionInfo()
        {
            _history = new List<long>(maxHistory);
        }

        public int Id;
        /// <summary>
        /// is download or upload<seealso cref="VkDiskFileInfo"/>
        /// </summary>
        public bool IsDownload;
        public FileType FileType;
        public ConnectionStatus Status;
        public long ByteSize;
        public long ByteProgress;
        public float PercentProgress;
        /// <summary>
        /// имя файла вида filename.type
        /// </summary>
        public string Name;
        /// <summary>
        /// url адрес файла
        /// </summary>
        public string Src;

        public long ByteSpeed
        {
            get => _history.Sum() / _history.Count;
            set
            {
                if (_history.Count >= maxHistory)
                    _history.RemoveAt(0);

                _history.Add(value);
            }
        }
    }

    /// <summary>
    /// тип файла
    /// </summary>
    public enum FileType
    {
        Single,
        Partial,
        Folder
    }

    /// <summary>
    /// статус подключения
    /// </summary>
    public enum ConnectionStatus
    {
        /// <summary>
        /// Сбор информации
        /// </summary>
        CheckInfo,
        /// <summary>
        /// загрузка (отправка)
        /// </summary>
        Loading,
        /// <summary>
        /// завершено
        /// </summary>
        Ended,
        /// <summary>
        /// ошибка
        /// </summary>
        Error
    }
}
