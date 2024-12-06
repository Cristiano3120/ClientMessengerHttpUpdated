using ClientMessengerHttpUpdated.LocalDatabaseClasses;
using System.Text.Json;
using System.Windows;

namespace ClientMessengerHttpUpdated
{
    internal static class HandleServerMessages
    {
        internal static void HandleResponseRequestToCreateAcc(JsonElement root)
        {
            if (!root.GetProperty("successful").GetBoolean())
            {
                var emailInUse = root.GetProperty("email").GetBoolean();
                var usernameInUse = root.GetProperty("username").GetBoolean();
  
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var accountCreation = new AccountCreation();
                    accountCreation.Show();
                    ClientUI.CloseAllWindowsExceptOne(accountCreation);

                    if (emailInUse)
                    {
                        _ = ClientUI.ShowError(accountCreation.EmailError, "Email taken");
                    }

                    if (usernameInUse)
                    {
                        _ = ClientUI.ShowError(accountCreation.UsernameError, "Username taken");
                    }
                });
            }
        }

        internal static async Task HandleResponseToVerification(JsonElement root)
        {
            var status = root.GetProperty("successful").GetBoolean();
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                Verification verificationWindow = ClientUI.GetWindow<Verification>()!;
                if (status)
                {
                    Security.SaveLoginData(verificationWindow._email, verificationWindow._password);
                    var home = new Home();
                    home.Show();
                    ClientUI.CloseAllWindowsExceptOne(home);
                    return;
                }

                var error = root.GetProperty("error").GetString();
                await ClientUI.ShowError(verificationWindow.VerificationError, error!);
            });
        }

        internal static async Task HandleResponseToLogin(JsonElement root)
        {
            var successful = root.GetProperty("successful").GetBoolean();
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                if (successful)
                {
                    if (root.GetUser() is not User user)
                    {
                        await Client.CloseConnectionToServer();
                        return;
                    }

                    Client._user = user;
                    var home = new Home();
                    home.Show();
                    ClientUI.CloseAllWindowsExceptOne(home);
                    return;
                }

                Login? login = ClientUI.GetWindow<Login>();
                await ClientUI.ShowError(login!.EmailError, "Email or password is wrong");
            });
        }

        #region HandleUnexpectedErrors

        internal static async Task HandleUnexpectedError(JsonElement root)
        {
            UnexpectedError error = root.GetProperty("error").GetUnexpectedError();
            switch (error)
            {
                case UnexpectedError.FailedToPutUserIntoDb:
                    await HandleFailedToPutUserIntoDb();
                    break;
            }
        }

        private static async Task HandleFailedToPutUserIntoDb()
        {
            var createAccScreen = new AccountCreation();
            createAccScreen.Show();
            ClientUI.CloseAllWindowsExceptOne(createAccScreen);
            await ClientUI.ShowError(createAccScreen.EmailError, "Something went wrong! Pls try again");
        }

        #endregion
    }
}
