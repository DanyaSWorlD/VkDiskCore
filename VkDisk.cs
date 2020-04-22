using System;
using System.Drawing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using VkDiskCore.Categories;
using VkDiskCore.Utility;

using VkNet.AudioBypassService.Extensions;
using VkNet.NLog.Extensions.Logging;
using VkNet.NLog.Extensions.Logging.Extensions;

using Va = VkNet.VkApi;

namespace VkDiskCore
{
    using VkNet.Utils.AntiCaptcha;

    public static class VkDisk
    {
        public delegate string CaptchaSolverDelegate(Bitmap captcha);

        public delegate void ExceptionHandled(Exception e);

        public static event ExceptionHandled OnExceptionThrown;

        public static ulong ApplicationId { get; set; }

        public static CaptchaSolverDelegate CaptchaSolver { get; set; }

        public static DocumentCategory Document { get; } = new DocumentCategory();

        public static Settings VkDiskSettings { get; } = new Settings { AutoRetryUpload = true, AutoRetryMaxCount = 5 };

        public static Va VkApi { get; private set; }

        public static string LiteDbConnectionString { get; set; } = @"lite.db";

        public static string ImageCacheDir { get; set; } = "imageCache/";

        public static void Init(int appId, bool logger = false)
        {
            ApplicationId = (ulong)appId;

            var services = new ServiceCollection();
            AddDefaultServices(services, logger);

            VkApi = new Va(services);
        }

        public static void Init(bool logger = false)
        {
            var services = new ServiceCollection();
            services.AddAudioBypass();
            AddDefaultServices(services, logger);

            VkApi = new Va(services);
        }

        public static void Stop()
        {
            ImageCache.Stop();

            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            NLog.LogManager.Shutdown();
        }

        public static string SolveCaptcha(Bitmap captcha) => CaptchaSolver?.Invoke(captcha);

        public static void HandleException(Exception e) => OnExceptionThrown?.Invoke(e);

        private static void AddDefaultServices(ServiceCollection services, bool addLogger)
        {
            services.AddSingleton<ICaptchaSolver, VkNetCaptchaSolver>();

            if (addLogger)
                AddLogger(services);
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