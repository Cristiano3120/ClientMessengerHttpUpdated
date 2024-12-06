using System.Windows.Media.Imaging;

namespace ClientMessengerHttpUpdated.LocalDatabaseClasses
{
    internal sealed class User
    {
        public required long Id { get; set; }
        public required BitmapImage ProfilPic { get; set; }
        public required string Username { get; set; } = string.Empty;
    }
}
