using System;
using System.Drawing;
using System.Net;

using VkNet.Utils.AntiCaptcha;

namespace VkDiskCore.Utility
{
    public class VkNetCaptchaSolver : ICaptchaSolver
    {
        public string Solve(string url)
        {
            var image = (Bitmap)Image.FromStream(WebRequest.Create(url).GetResponse().GetResponseStream() ?? throw new InvalidOperationException());
            return VkDisk.SolveCaptcha(image);
        }

        public void CaptchaIsFalse()
        {
            throw new NotImplementedException();
        }
    }
}
