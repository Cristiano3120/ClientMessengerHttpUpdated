namespace ClientMessengerHttpUpdated
{
    internal sealed class Chat
    {
        public required List<string> Participants { get; set; }
        public required List<Message> Messages { get; set; }
    }
}
