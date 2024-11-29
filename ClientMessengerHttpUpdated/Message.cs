namespace ClientMessengerHttpUpdated
{
    internal sealed class Message
    {
        public required string Sender { get; set; }
        public required string Content { get; set; }
        public required DateTime Time { get; set; }

    }
}
