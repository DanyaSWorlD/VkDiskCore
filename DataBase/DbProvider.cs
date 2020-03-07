using System;
using System.Threading;
using LiteDB;

namespace VkDiskCore.DataBase
{
    public static class DbProvider
    {
        private static readonly object Locker = new object();

        public static LiteDatabase GetDb
        {
            get
            {
                lock (Locker)
                {
                    try
                    {
                        return new LiteDatabase(VkDisk.LiteDbConnectionString);
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(100);
                        return GetDb;
                    }

                }
            }
        }
    }
}