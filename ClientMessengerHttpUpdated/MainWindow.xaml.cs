using System.Windows;

namespace ClientMessengerHttpUpdated
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Client.Start();
        }
    }
}