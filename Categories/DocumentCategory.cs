﻿using VkDiskCore.Connections;

namespace VkDiskCore.Categories
{
    public class DocumentCategory
    {
        public void Upload(string path)
        {
            ConnectionManager.UploadAsync(path);
        }

        public void Download(string src, string folder)
        {
            ConnectionManager.DownloadAsync(src, folder);
        }
    }
}
