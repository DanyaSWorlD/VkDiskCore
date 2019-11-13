namespace VkDiskCore.Utility
{

    public class FileSize
    {
        public static string ToString(long bytes)
        {
            float bLength = 0;
            string[] names = { "байт", "КБ", "МБ", "ГБ" };
            var i = 0;

            while (i < names.Length - 1 && bytes > 1024)
            {
                bLength = bytes / (float)1024;
                bytes /= 1024;
                i++;
            }

            return $"{(int)bLength}.{(int)((bLength - (int)bLength) * 10)}{names[i]}";
        }
    }
}
