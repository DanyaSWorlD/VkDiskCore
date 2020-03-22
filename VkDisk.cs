using System;

using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using VkDiskCore.Categories;
using VkDiskCore.Utility;

using VkNet.AudioBypassService.Extensions;
using VkNet.NLog.Extensions.Logging;
using VkNet.NLog.Extensions.Logging.Extensions;

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

            // uncomment, if need logging:
            // AddLogger(services);
            // --------------------
            // init vk api
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

            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            NLog.LogManager.Shutdown();
        }

        public static string SolveCaptcha(Bitmap captcha)
        {
            return CapchaSolver?.Invoke(captcha);
        }

        private static void AddLogger(ServiceCollection services)
        {
            // Регистрация логгера
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddNLog(new NLogProviderOptions
                    {
                        CaptureMessageProperties = true,
                        CaptureMessageTemplates = true
                    });
                });
            NLog.LogManager.LoadConfiguration("nlog.config");
        }
    }
}
