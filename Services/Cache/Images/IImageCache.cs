using System.Drawing;

using VkDiskCore.DataBase;

namespace VkDiskCore.Services.Cache.Images
{
    public delegate void ImageLoadedHandler(DbImage image);

    /// <summary>
    /// Image cache should download and cache images
    /// </summary>
    public interface IImageCache
    {
        event ImageLoadedHandler ImageLoaded;

        /// <summary>
        /// Initialize service. Should be run at application startup
        /// </summary>
        void Init();

        /// <summary>
        /// Notice service, that app has been stopped
        /// </summary>
        void Stop();

        /// <summary>
        /// Get image by link
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        Bitmap GetImage(string link);

        /// <summary>
        /// Get image by peer id
        /// </summary>
        /// <param name="peerId"></param>
        /// <returns></returns>
        Bitmap GetImage(long peerId);
    }
}