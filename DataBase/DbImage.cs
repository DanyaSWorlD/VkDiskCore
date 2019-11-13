using System;

namespace VkDiskCore.DataBase
{
    public class DbImage
    {
        // id in db
        public int Id { get; set; }

        // peerid
        public long? PeerId { get; set; }

        // image link
        public string Link { get; set; }

        // name
        public string Name { get; set; }

        // last date of accesing to image. If this parameter more than 1 week it will be deleted
        public DateTime AccesDate { get; set; }
    }
}
