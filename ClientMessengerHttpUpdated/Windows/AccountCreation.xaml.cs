using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ClientMessengerHttpUpdated
{
    /// <summary>
    /// Interaktionslogik für AccountCreation.xaml
    /// </summary>
    public sealed partial class AccountCreation : Window
    {
        private readonly Stopwatch _stopwatch;
        public AccountCreation()
        {
            InitializeComponent();
            InitializeComboBoxes();
            _stopwatch = new Stopwatch();
            ClientUI.BindWindowStateButtons(btnClose, btnMinimize, btnMaximize, dragPanel);
            SignUpButton.Click += SignUp_Click;
            KeyDown += ((sender, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    SignUp_Click(sender, args);
                }
            });
            GoBackBtn.MouseLeftButtonDown += ((sender, args) =>
            {
                var login = new Login();
                login.Show();
                ClientUI.CloseAllWindowsExceptOne(login);
            });
        }

        internal async void SignUp_Click(object sender, RoutedEventArgs args)
        {
            if (!ClientUI.CanSendRequest(_stopwatch))
            {
                await ClientUI.ShowError(SpamError);
                return;
            }

            if (!CheckIfUserDataIsValid(out TextBlock? unvalidData, out DateOnly? formatedBirthdate))
            {
                await ClientUI.ShowError(unvalidData);
                return;
            }

            var email = Email.Text;
            var password = Password.Text;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var verificationWindow = new Verification(email, password);
                verificationWindow.Show();
                ClientUI.CloseAllWindowsExceptOne(verificationWindow);
            });

            var payload = new
            {
                code = OpCode.TryToCreateAnAcc,
                email = Email.Text,
                password = Password.Text,
                username = Username.Text,
                dateOfBirth = formatedBirthdate,
            };
            var jsonString = JsonSerializer.Serialize(payload);

            _ =  Task.Run(async () =>
            {
                await Client.SendPayloadAsync(jsonString);
            });
        }

        /// <summary>
        /// Checks if the enterd user data is valid and outs a formated version of the <c>birthdate</c> if <c>true</c>
        /// </summary>
        /// <param name="formatedBirthdate">If <c>true</c> the birthdate of the user as a DateTime obj</param>
        /// <returns></returns>
        private bool CheckIfUserDataIsValid([NotNullWhen(false)] out TextBlock? unvalidData, [NotNullWhen(true)] out DateOnly? formatedBirthdate)
        {
            unvalidData = null;
            formatedBirthdate = null;

            var day = Day.Text;
            var month = Month.Text;
            var year = Year.Text;

            if (!DateOnly.TryParse($"{day}/{month}/{year}", out DateOnly dateOfBirth))
            {
                unvalidData = DateOfBirthError;
                return false;
            }

            if (!IsValidEmail())
            {
                unvalidData = EmailError;
                return false;
            }

            var password = Password.Text;
            if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password))
            {
                unvalidData = PasswordError;
                return false;
            }

            var username = Username.Text;
            if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username))
            {
                unvalidData = UsernameError;
                return false;
            }

            formatedBirthdate = dateOfBirth;
            return true;
        }

        private bool IsValidEmail()
        {
            var emailPattern = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";
            var email = Email.Text;
            return !string.IsNullOrEmpty(email) && Regex.IsMatch(email, emailPattern);
        }

        private void InitializeComboBoxes()
        {
            for (var i = 1; i <= 31; i++)
            {
                var isSelected = i == 1;
                ComboBoxItem item = new()
                {
                    IsSelected = isSelected,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#343234")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#343234")),
                    FontWeight = FontWeights.Bold,
                    Content = $"{i}"
                };
                Day.Items.Add(item);
            }

            for (var i = 1; i <= 12; i++)
            {
                var isSelected = i == 1;
                ComboBoxItem item = new()
                {
                    IsSelected = isSelected,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#343234")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#343234")),
                    FontWeight = FontWeights.Bold,
                    Content = $"{i}"
                };
                Month.Items.Add(item);
            }

            for (var i = 2020; i >= 1950; i--)
            {
                var isSelected = i == 2020;
                ComboBoxItem item = new()
                {
                    IsSelected = isSelected,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#343234")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#343234")),
                    FontWeight = FontWeights.Bold,
                    Content = $"{i}"
                };
                Year.Items.Add(item);
            }
        }
    }
}
