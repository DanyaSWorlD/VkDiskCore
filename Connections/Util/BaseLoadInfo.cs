using System.Collections.Generic;
using System.Linq;
using VkDiskCore.Utility;

namespace VkDiskCore.Connections.Util
{
    public class BaseLoadInfo : ViewModelBase
    {
        protected const int SpeedsCount = 50;
        protected readonly object Locker = new object();
        protected readonly List<long> Speeds;

        private LoadState loadState;

        private int id;

        private long totalLoad;
        private long totalSize;

        private string name;
        private string ext;

        private bool isVkd;

        public BaseLoadInfo()
        {
            Speeds = new List<long>(SpeedsCount);
            loadState = LoadState.Starting;
        }

        public delegate void LoadStopHandler();

        public event LoadStopHandler LoadStop;

        /// <summary>
        /// Количество загруженных байт
        /// </summary>
        public long TotalLoad
        {
            get => totalLoad;
            set => SetField(ref totalLoad, value);
        }

        /// <summary>
        /// Полный размер файла
        /// </summary>
        public long TotalSize
        {
            get => totalSize;
            set => SetField(ref totalSize, value);
        }

        /// <summary>
        /// Id в списке
        /// </summary>
        public int Id
        {
            get => id;
            set => SetField(ref id, value);
        }

        /// <summary>
        /// Bytes Per Second
        /// </summary>
        public long Bps
        {
            get
            {
                lock (Locker)
                {
                    var totalSpeeds = Speeds.Sum();
                    return totalSpeeds == 0 ? 0 : totalSpeeds / Speeds.Count;
                }
            }
        }

        /// <summary>
        /// Имя файла (полностью)
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                SetField(ref name, value);
                Ext = name.Extension();
            }
        }

        /// <summary>
        /// Расширение файла
        /// </summary>
        public string Ext
        {
            get => ext;
            set => SetField(ref ext, value);
        }

        /// <summary>
        /// Состояние загрузки
        /// </summary>
        public LoadState LoadState
        {
            get => loadState;
            set => SetField(ref loadState, value);
        }

        /// <summary>
        /// Является ли файл файлом вкд
        /// </summary>
        public bool IsVkd
        {
            get => isVkd;
            set => SetField(ref isVkd, value);
        }

        public void Stop()
        {
            LoadStop?.Invoke();
        }

        public void ProgressChangedHandler(long byteProgress, long elapsedMs)
        {
            lock (Locker)
                if (Speeds.Count >= SpeedsCount)
                    Speeds.RemoveAt(0);

            var bytes = byteProgress - TotalLoad;

            TotalLoad = byteProgress;

            lock (Locker)
                Speeds.Add(BytesPerSecond(bytes, elapsedMs));

            NotifyPropertyChanged($"BPS");
        }

        protected long BytesPerSecond(long bytes, long ellapsedMs)
        {
            return bytes == 0 ? 0 : bytes / (ellapsedMs == 0 ? 1 : ellapsedMs) * 1000;
        }
    }
}
