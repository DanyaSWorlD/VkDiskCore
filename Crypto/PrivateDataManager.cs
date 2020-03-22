using System;
using LiteDB;
using VkDiskCore.DataBase;

namespace VkDiskCore.Crypto
{
    public class PrivateDataManager
    {
        /// <summary>
        /// Сохранить логин и пароль в базе данных
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        public static void SaveLoginPass(string login, string password)
        {
            using (var db = GetDb)
            {
                var users = db.GetCollection<DbUser>();

                var user = users.FindOne(o => o.Id > -1);

                if (user == null)
                {
                    user = new DbUser();
                    users.Insert(user);
                }

                user.Login = login;
                user.Password = StringCipher.Encrypt(password, login);
                users.Update(user);
            }
        }

        public static void SaveToken(string token, DateTime? assigned = null, int? expire = null)
        {
            using (var db = GetDb)
            {
                var users = db.GetCollection<DbUser>();
                var user = users.FindOne(Query.All());

                if (user == null) return;

                user.UserId = User.UserId;
                user.TokenAssigned = assigned ?? DateTime.Now;
                user.TokenExpire = expire ?? 3600 * 24 * 90;

                user.Token = StringCipher.Encrypt(token, user.UserId);

                users.Update(user);
            }
        }

        public static void SaveUserId()
        {
            using (var db = GetDb)
            {
                var users = db.GetCollection<DbUser>();
                var user = users.FindOne(Query.All());

                if (user == null) return;

                user.UserId = User.UserId;

                users.Update(user);
            }
        }

        [Obsolete("Logical wrong")]
        public static void FillUser()
        {
            User.UserId = UserId;
            User.Token = Token;
            User.TokenAssigned = TokenAssigned ?? DateTime.MinValue;
            User.TokenExpire = TokenExpire ?? 0;
        }

        public static string Login => GetUser()?.Login;

        public static string Password => GetUser() == null ? null : StringCipher.Decrypt(GetUser()?.Password, Login);

        //[Obsolete("Logical wrong")]
        public static string Token => GetUser() == null ? null : StringCipher.Decrypt(GetUser().Token, UserId);

        public static string UserId => GetUser()?.UserId;

        //[Obsolete("Logical wrong")]
        public static int? TokenExpire => GetUser()?.TokenExpire;

        //[Obsolete("Logical wrong")]
        public static DateTime? TokenAssigned => GetUser()?.TokenAssigned;


        private static DbUser GetUser()
        {
            using (var db = GetDb)
                return db.GetCollection<DbUser>().FindOne(Query.All());

        }

        private static LiteDatabase GetDb => DbProvider.GetDb;
    }
}
