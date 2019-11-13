using System.Net;

using VkDiskCore.Utility;

namespace VkDiskCore.Connections.Util
{
    public class Stuff
    {
        public static string GetFileName(string url)
        {
            var r = (HttpWebRequest)WebRequest.Create(url);

            using (var re = r.GetResponse())
                return re.ResponseUri.Segments[re.ResponseUri.Segments.Length - 1].ToNormalName();
        }
    }
}
