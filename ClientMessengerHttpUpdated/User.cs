using System.Windows.Media.Imaging;

namespace ClientMessengerHttpUpdated
{
    internal sealed class User
    {
        public required long Id { get; set; }
        public BitmapImage ProfilPic { get; set; }
        public required string Username { get; set; } = string.Empty;
    }
}
