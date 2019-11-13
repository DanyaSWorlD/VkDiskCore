using System;

namespace VkDiskCore.DataBase
{
    public class DbUser
    {
        // id в базе данных
        public int Id { get; set; }

        // vk id
        public string UserId { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string Token { get; set; }

        public DateTime TokenAssigned { get; set; }

        public int TokenExpire { get; set; }
    }
}
