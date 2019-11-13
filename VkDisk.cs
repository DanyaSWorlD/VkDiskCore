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

        public static async Task Auth(string login, string pass)
        {
            await new Auth.Auth(login, pass).LoginAsync();
        }

        public static async Task<bool> TryAuthCached()
        {
            return await VkDiskCore.Auth.Auth.TryLoginAsync();
        }

        public static void Init(int appId)
        {
            var services = new ServiceCollection();
            services.AddAudioBypass();
            VkApi = new Va(services);

            ImageCache.Init();

            ApplicationId = (ulong)appId;
        }

        public static void Stop()
        {
            ImageCache.Stop();
        }

        public static string SolveCapcha(Bitmap capcha)
        {
            return CapchaSolver?.Invoke(capcha);
        }
    }
}
