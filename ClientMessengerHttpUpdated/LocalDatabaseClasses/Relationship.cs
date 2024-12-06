namespace ClientMessengerHttpUpdated.LocalDatabaseClasses
{
    internal sealed class Relationship()
    {
        public required RelationshipState RelationshipState { get; set; }
        public required User User { get; set; }
    }
}
