using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using VkDiskCore.Connections.Util;

namespace VkDiskCore.Connections.Models
{
    [Serializable]
    public class Vkd
    {
        public Vkd()
        {
            InnerVkds = new List<Vkd>();
        }
        
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("owner_id")]
        public long? OwnerId { get; set; }

        [JsonProperty("size")]
        public long? Size { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public VkdTypes? Type { get; set; }

        /// <summary>
        /// Type of the file, for example .txt
        /// </summary>
        [JsonProperty("filetype")]
        public string Filetype { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("inner_vkd")]
        public List<Vkd> InnerVkds { get; set; }
    }
}
