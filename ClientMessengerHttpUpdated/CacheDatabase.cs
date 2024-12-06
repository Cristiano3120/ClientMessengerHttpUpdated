using ClientMessengerHttpUpdated.LocalDatabaseClasses;
using LiteDB;

namespace ClientMessengerHttpUpdated
{
    internal static class CacheDatabase
    {
        private const string _pathToCacheDB =
            @"C:\Users\Crist\source\repos\ClientMessengerHttpUpdatedUpdated\ClientMessengerHttpUpdatedUpdated\NeededFiles\CachedData.db";

        internal static void SaveRelationships(List<Relationship> relationships)
        {
            using (var conn = new LiteDatabase(_pathToCacheDB))
            {
                ILiteCollection<Relationship> relationshipsCollection = conn.GetCollection<Relationship>("Relationships");
                relationshipsCollection.InsertBulk(relationships);
            }
        }

        internal static void SaveChats(List<Chat> chats)
        {
            using (var conn = new LiteDatabase(_pathToCacheDB))
            {
                ILiteCollection<Chat> chatCollection = conn.GetCollection<Chat>("Chats");
                chatCollection.InsertBulk(chats);
            }
        }

        internal static void UpdateChats(List<Chat> chats)
        {
            using var conn = new LiteDatabase(_pathToCacheDB);
            ILiteCollection<Chat> chatCollection = conn.GetCollection<Chat>("Chats");

            foreach (Chat chat in chats)
            {
                chatCollection.Update(chat);
            }
        }


        internal static (List<Relationship> relationships, List<Chat> chats) GetData()
        {
            using var conn = new LiteDatabase(_pathToCacheDB);
            ILiteCollection<Relationship> relationshipCollection = conn.GetCollection<Relationship>("Relationships");
            var relationships = relationshipCollection.FindAll().ToList();

            ILiteCollection<Chat> chatsCollection = conn.GetCollection<Chat>("Chats");
            var chats = chatsCollection.FindAll().ToList();

            return (relationships, chats);
        }
    }
}
