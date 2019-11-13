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

        private LoadState _loadState;

        private int _id;

        private long _totalLoad;
        private long _totalSize;

        private string _name;
        private string _ext;

        public delegate void LoadStopHandler();
        public event LoadStopHandler LoadStop;

        public BaseLoadInfo()
        {
            Speeds = new List<long>(SpeedsCount);
            _loadState = LoadState.Starting;
        }

        /// <summary>
        /// Количество загруженных байт
        /// </summary>
        public long TotalLoad
        {
            get => _totalLoad;
            set => SetField(ref _totalLoad, value);
        }

        /// <summary>
        /// Полный размер файла
        /// </summary>
        public long TotalSize
        {
            get => _totalSize;
            set => SetField(ref _totalSize, value);
        }

        /// <summary>
        /// Id в списке
        /// </summary>
        public int Id
        {
            get => _id;
            set => SetField(ref _id, value);
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
            get => _name;
            set
            {
                SetField(ref _name, value);
                Ext = _name.Extension();
            }
        }

        /// <summary>
        /// Расширение файла
        /// </summary>
        public string Ext
        {
            get => _ext;
            set => SetField(ref _ext, value);
        }

        /// <summary>
        /// Состояние загрузки
        /// </summary>
        public LoadState LoadState
        {
            get => _loadState;
            set => SetField(ref _loadState, value);
        }

        public void Stop()
        {
            LoadStop?.Invoke();
        }

        public void ProgressChangedHandler(long byteProgress, long ellapsedMs)
        {
            lock (Locker)
                if (Speeds.Count >= SpeedsCount)
                    Speeds.RemoveAt(0);

            var bytes = byteProgress - TotalLoad;

            TotalLoad = byteProgress;

            lock (Locker)
                Speeds.Add(BytesPerSecond(bytes, ellapsedMs));

            NotifyPropertyChanged($"BPS");
        }

        protected long BytesPerSecond(long bytes, long ellapsedMs)
        {
            return bytes == 0 ? 0 : bytes / (ellapsedMs == 0 ? 1 : ellapsedMs) * 1000;
        }
    }
}
