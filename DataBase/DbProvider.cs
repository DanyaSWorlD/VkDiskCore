using System;
using System.Threading;
using LiteDB;
using System.IO;

namespace VkDiskCore.DataBase
{
    public static class DbProvider
    {
        private static readonly object Locker = new object();

        public static LiteDatabase GetDb => new LiteDatabase(VkDisk.LiteDbConnectionString);
    }
}