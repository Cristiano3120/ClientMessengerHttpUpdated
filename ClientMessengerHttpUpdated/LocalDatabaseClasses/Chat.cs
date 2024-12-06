using LiteDB;

namespace ClientMessengerHttpUpdated.LocalDatabaseClasses
{
    internal sealed class Chat
    {
        [BsonId]
        public required string ID { get; set; }
        public required string Name { get; set; }
        public required List<string> Participants { get; set; }
        public required List<Message> Messages { get; set; }
    }
}
