using System;
using System.Linq;
using System.Text;

namespace VkDiskCore.Utility
{
    public static class Extensions
    {
        public static string makeVKD(this string name, int index)
        {
            return name.Substring(0, name.LastIndexOf('.')) + $".{index}.vkdpart";
        }

        /// <summary>
        /// Возвращает файл без раширения
        /// </summary>
        /// <param name="str">Имя файла (без пути)</param>
        /// <returns> имя файла </returns>
        public static string WithNoExtensions(this string str)
        {
            int id = str.LastIndexOf('.');
            if (id == str.Length - 1) throw new ArgumentException("String may contain dots");
            return str.Substring(0, id);
        }


        /// <summary>
        /// Возвращает расширение файла без точки
        /// </summary>
        /// <param name="str">Имя файла</param>
        /// <returns> расширение файла </returns>
        public static string Extension(this string str)
        {
            return str.Split('.').Last();
        }

        public static string ToNormalName(this string str)
        {
            return str.Replace('_', '.');
        }


        /// <summary>
        /// Исправляет слэши в пути к директории
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static string FixFolder(this string f)
        {
            f = f.Replace('/', '\\');
            return f[f.Length - 1] != '\\' ? f + '\\' : f;
        }

        public static string Ex(this string s, params string[] p)
        {
            var b = new StringBuilder(s);
            foreach (var c in p)
            {
                b.Append(c);
            }
            return b.ToString();
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }
    }
}
