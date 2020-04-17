using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VkDiskCore.Connections.Util;

namespace VkDiskCore
{
    public class Settings : ViewModelBase
    {
        private bool autoRetryUpload;

        private int autoRetryMaxCount;

        public bool AutoRetryUpload
        {
            get => autoRetryUpload;
            set
            {
                autoRetryUpload = value;
                NotifyPropertyChanged();
            }
        }

        public int AutoRetryMaxCount
        {
            get => autoRetryMaxCount;
            set
            {
                autoRetryMaxCount = value;
                NotifyPropertyChanged();
            }
        }
    }
}
