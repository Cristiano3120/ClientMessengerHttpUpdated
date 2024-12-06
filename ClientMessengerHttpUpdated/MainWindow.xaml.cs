using System.Windows;

namespace ClientMessengerHttpUpdated
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ClientUI.BindWindowStateButtons(btnClose, btnMinimize, btnMaximize, dragPanel);
            _ = Client.Start();
        }
    }
}