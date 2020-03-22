using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Threading.Tasks;
using VkDiskCore.Categories;
using VkDiskCore.Utility;
using VkNet.AudioBypassService.Extensions;
using St = VkNet.Enums.Filters.Settings;
using Va = VkNet.VkApi;

namespace VkDiskCore
{
    public static class VkDisk
    {
        public static ulong ApplicationId;

        public static St Settings;

        public delegate string CapchaSolverDelegate(Bitmap capcha);

        public static CapchaSolverDelegate CapchaSolver { get; set; }

        public static DocumentCategory Document { get; } = new DocumentCategory();

        public static Va VkApi { get; private set; }

        public static string LiteDbConnectionString { get; set; } = @"lite.db";

        public static string ImageCacheDir { get; set; } = "imageCache/";

        public static void Init(int appId)
        {
            var services = new ServiceCollection();
            services.AddAudioBypass();
            VkApi = new Va(services)
            {
                CaptchaSolver = new VkNetCaptchaSolver()
            };

            ImageCache.Init();

            ApplicationId = (ulong)appId;
        }

        public static void Stop()
        {
            ImageCache.Stop();
        }

        public static string SolveCaptcha(Bitmap captcha)
        {
            return CapchaSolver?.Invoke(captcha);
        }
    }
}
