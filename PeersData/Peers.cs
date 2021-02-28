using System.Collections.Generic;
using System.Linq;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;

namespace VkDiskCore.PeersData
{
    public static class Peers
    {
        private static readonly List<VkPeerInfo> _peers = new List<VkPeerInfo>();

        public delegate void CollectionChangedHandler(IEnumerable<long> peers);
        public static event CollectionChangedHandler CollectionChanged;

        public static void Add(IEnumerable<ConversationAndLastMessage> conversations)
        {
            var usersIds = new List<long>();
            var groupsIds = new List<string>();
            var chatsIds = new List<long>();

            foreach (var message in conversations)
            {
                if (message.Conversation.Peer.Type == ConversationPeerType.User)
                    usersIds.Add(message.Conversation.Peer.Id);

                if (message.Conversation.Peer.Type == ConversationPeerType.Group)
                    groupsIds.Add(message.Conversation.Peer.LocalId.ToString());

                if (message.Conversation.Peer.Type == ConversationPeerType.Chat)
                    chatsIds.Add(message.Conversation.Peer.LocalId);
            }

            if (usersIds.Count > 0)
            {
                var usersResult = VkDisk.VkApi.Users.Get(usersIds, ProfileFields.Photo100);
                Add(usersResult);
            }

            if (groupsIds.Count > 0)
            {
                var groupsResult = VkDisk.VkApi.Groups.GetById(groupsIds, "", GroupsFields.Description);
                Add(groupsResult);

            }

            if (chatsIds.Count > 0)
            {
                var chatsResult = VkDisk.VkApi.Messages.GetChat(chatsIds);
                Add(chatsResult);
            }
        }

        public static void Add(IEnumerable<VkNet.Model.User> users)
        {
            if (users == null) return;

            var usersArray = users as VkNet.Model.User[] ?? users.ToArray();

            if (!usersArray.Any()) return;

            foreach (var user in usersArray)
                TryAdd(new VkPeerInfo
                {
                    Id = user.Id,
                    ImageLink = user.Photo100.AbsoluteUri,
                    Title = $"{user.FirstName} {user.LastName}"
                });

            CollectionChanged?.Invoke(usersArray.Select(o => o.Id));
        }

        public static void Add(IEnumerable<Group> groups)
        {
            if (groups == null) return;

            var groupsArray = groups as Group[] ?? groups.ToArray();

            if (!groupsArray.Any()) return;

            foreach (var @group in groupsArray)
                TryAdd(new VkPeerInfo
                {
                    Id = @group.Id,
                    ImageLink = @group.Photo100.AbsoluteUri,
                    Title = @group.Name
                });

            CollectionChanged?.Invoke(groupsArray.Select(o => o.Id));
        }

        public static void Add(IEnumerable<Chat> chats)
        {
            if (chats == null) return;

            var chatsArray = chats as Chat[] ?? chats.ToArray();

            if (!chatsArray.Any()) return;

            foreach (var chat in chatsArray)
                TryAdd(new VkPeerInfo
                {
                    Id = chat.Id,
                    ImageLink = chat.Photo100,
                    Title = chat.Title
                });

            CollectionChanged?.Invoke(chatsArray.Select(o => o.Id));
        }

        public static void Add(ConversationResult o)
        {
            Add(o.Groups);
            Add(o.Profiles);
        }

        public static string GetTitle(long? id)
        {
            return _peers.FirstOrDefault(o => o.Id == id)?.Title;
        }

        public static string GetImageLink(long id)
        {
            return _peers.FirstOrDefault(o => o.Id == id)?.ImageLink;
        }

        private static void TryAdd(VkPeerInfo info)
        {
            if (_peers.All(o => o.Id != info.Id))
                _peers.Add(info);
        }
    }
}
