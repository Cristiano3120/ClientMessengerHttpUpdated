using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;

namespace ClientMessengerHttpUpdated
{
    /// <summary>
    /// Interaktionslogik für Login.xaml
    /// </summary>
    public sealed partial class Login : Window
    {
        private readonly Stopwatch _stopwatch;
        public Login()
        {
            InitializeComponent();
            ClientUI.BindWindowStateButtons(btnClose, btnMinimize, btnMaximize, dragPanel);
            _stopwatch = new Stopwatch();
            HyperlinkAccCreation.Click += HypelinkAccCreation_Click;
            SignInButton.Click += SignIn_Click;
            KeyDown += ((sender, args) =>
            {
                if (args.Key == System.Windows.Input.Key.Enter)
                {
                    SignIn_Click(sender, args);
                }
            });
        }

        #region Click

        internal async void SignIn_Click(object sender, RoutedEventArgs args)
        {
            if (!ClientUI.CanSendRequest(_stopwatch))
            {
                await ClientUI.ShowError(SpamError);
                return;
            }
            
            if (!IsValidPassword())
            {
                await ClientUI.ShowError(EmailError);
                return;
            }

            if (!IsValidEmail())
            {
                await ClientUI.ShowError(PasswordError);
                return;
            }

            var payload = new
            {
                code = OpCode.RequestToLogin,
                email = Email.Text,
                password = Password.Text,
            };
            var jsonString = JsonSerializer.Serialize(payload);

            _ = Task.Run(async () =>
            {
                await Client.SendPayloadAsync(jsonString);
            });
        }

        internal void HypelinkAccCreation_Click(object sender, RoutedEventArgs args)
        {
            var accCreationWindow = new AccountCreation();
            accCreationWindow.Show();
            Close();
        }

        #endregion

        private bool IsValidEmail()
        {
            var emailPattern = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";
            var email = Email.Text;
            return !string.IsNullOrEmpty(email) && Regex.IsMatch(email, emailPattern);
        }

        private bool IsValidPassword()
        {
            return !string.IsNullOrEmpty(Password.Text);
        }
    }
}
