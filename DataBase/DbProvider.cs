using System;
using System.Threading;
using LiteDB;
using System.IO;

namespace VkDiskCore.DataBase
{
    public static class DbProvider
    {
        private static readonly object Locker = new object();

        public static LiteDatabase GetDb
        {
            get
            {
                // figth the file is busy exception
                lock (Locker)
                {
                    try
                    {
                        return new LiteDatabase(VkDisk.LiteDbConnectionString);
                    }
                    catch (IOException e)
                    {
                        Thread.Sleep(100);
                        Console.WriteLine(e.Message);
                        return GetDb;
                    }
                }
            }
        }
    }
}