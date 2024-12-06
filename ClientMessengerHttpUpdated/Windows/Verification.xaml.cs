using System.Diagnostics;
using System.Text.Json;
using System.Windows;

namespace ClientMessengerHttpUpdated
{
    public partial class Verification : Window
    {
        private static Stopwatch _stopwatch;
        internal readonly string _email;
        internal readonly string _password;
        public Verification(string email, string password)
        {
            InitializeComponent();
            ClientUI.BindWindowStateButtons(btnClose, btnMinimize, btnMaximize, dragPanel);
            SignUpButton.Click += SignUp_Click;
            _stopwatch = new Stopwatch();
            _email = email;
            _password = password;
        }

        internal async void SignUp_Click(object sender, RoutedEventArgs args)
        {
            if (VerificationBox.Text == string.Empty || !int.TryParse(VerificationBox.Text, out var result))
            {
                await ClientUI.ShowError(VerificationError);
                return;
            }

            if (!ClientUI.CanSendRequest(_stopwatch))
            {
                await ClientUI.ShowError(VerificationError, "Calm down! Wait a second");
                return;
            }

            var payload = new
            {
                code = OpCode.VerificationProcess,
                verificationCode = result,
                email = _email,
            };
            var jsonString = JsonSerializer.Serialize(payload);
            await Client.SendPayloadAsync(jsonString);
        }
    }
}
