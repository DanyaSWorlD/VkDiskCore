/*
 * Copyright © 2016 by DAQUGA studios
 * Author - Даниил Миренский
 * Created - 18.12.2016
 */

using System;
using System.Data.SqlTypes;

namespace VkDiskCore
{
    public static class User
    {
        private static string _token;
        private static string _userId;
        private static int _tokenExpire;
        private static DateTime _tokenAssigned;

        public static string Token
        {
            get { return _token; }
            set { _token = value; if (_userId != null) Changed?.Invoke(); }
        }

        public static string UserId
        {
            get { return _userId; }
            set { _userId = value; if (_userId != null) Changed?.Invoke(); }
        }

        public static int TokenExpire
        {
            get => _tokenExpire;
            set
            {
                _tokenExpire = value;
                TokenAssigned = DateTime.Now;
            }
        }

        public static DateTime TokenAssigned
        {
            get => _tokenAssigned;
            set => _tokenAssigned = value;
        }

        public delegate void UserInfoHandler();
        public static event UserInfoHandler Changed;
    }
}
