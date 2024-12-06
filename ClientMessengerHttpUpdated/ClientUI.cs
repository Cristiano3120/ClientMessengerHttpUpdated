using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ClientMessengerHttpUpdated
{
    internal static class ClientUI
    {
        internal static void BindWindowStateButtons(Button closeButton, Button minimizeButton, Button maximizeButton, StackPanel WindowStateButtonsPanel)
        {
            closeButton.Click += CloseWindow;
            minimizeButton.Click += MinimizeWindow;
            maximizeButton.Click += MaximizeWindow;
            WindowStateButtonsPanel.MouseLeftButtonDown += DragWindow;
        }

        internal static bool CanSendRequest(Stopwatch stopwatch)
        {
            if (stopwatch == null || stopwatch.IsRunning == false)
            {
                stopwatch?.Start();
                return true;
            }
                
            var cooldown = 2;
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(cooldown))
            {
                stopwatch.Reset();
                return true;
            }
            return false;
        }

        internal static async Task ShowError(TextBlock element)
        {
            if (element.Visibility == Visibility.Visible)
                return;

            element.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            element.Visibility = Visibility.Collapsed;
        }

        internal static async Task ShowError(TextBlock element, string error)
        {
            if (element.Visibility == Visibility.Visible)
                return;

            var oldReason = element.Text;
            element.Text = error;
            element.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            element.Visibility = Visibility.Collapsed;
            element.Text = oldReason;
        }

        internal static T? GetWindow<T>()
        {
            return Application.Current.Windows.OfType<T>().FirstOrDefault();
        }

        internal static void CloseAllWindowsExceptOne(Window window)
        {
            foreach (Window item in Application.Current.Windows)
            {
                if (item != window)
                { 
                    Application.Current.Dispatcher.Invoke(item.Close);
                }
            }
        }

        #region Control window state/ Position
        private static void MinimizeWindow(object sender, RoutedEventArgs args)
        {
            var window = Window.GetWindow((Button)sender);
            window.WindowState = WindowState.Minimized;
        }

        private static void CloseWindow(object sender, RoutedEventArgs args)
        {
            var window = Window.GetWindow((Button)sender);
            window?.Close();
        }

        private static void MaximizeWindow(object sender, RoutedEventArgs args)
        {
            var window = Window.GetWindow((Button)sender);
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        #endregion

        #region DragWindow

        private static void DragWindow(object sender, MouseButtonEventArgs args)
        {
            var window = Window.GetWindow((StackPanel)sender);
            if (window.WindowState == WindowState.Maximized)
            {
                Window dragWindow = new()
                {
                    ShowActivated = false,
                    Height = 1,
                    Width = 1,
                };
                dragWindow.Show();
                dragWindow.Height = 450;
                dragWindow.Width = 800;
                AdjustWindowPosition(window, dragWindow, args);
                _ = DelayHidingWindow(dragWindow);
                dragWindow.DragMove();
                PositionWindowToDraggedPos(window, dragWindow);
                dragWindow.Close();
            }
            else
            {
                window.DragMove();
            }
        }

        /// <summary>
        /// This delays the hiding of the drag window so it only hides after the dragging starts.
        /// Otherwise the window would stay open while dragging which looks weird.
        /// </summary>
        private static async Task DelayHidingWindow(Window window)
        {
            await Task.Delay(1);
            window.Hide();
        }

        /// <summary>
        /// Moves the original window to the pos the user wants it to be(the pos of the dragWindow). 
        /// </summary>
        /// <param name="window"></param>
        /// <param name="dragWindow"></param>
        private static void PositionWindowToDraggedPos(Window window, Window dragWindow)
        {
            var ySmoothnessOffset = 10;
            window.WindowState = WindowState.Normal;
            window.Height = dragWindow.ActualHeight;
            window.Width = dragWindow.ActualWidth;
            window.Top = dragWindow.Top - ySmoothnessOffset;
            window.Left = dragWindow.Left;
        }

        /// <summary>
        /// Calculates the pos of the smaller dragWindow in relation to the bigger original window.
        /// </summary>
        /// <param name="args">The EventArgs needed to get the mouse corsur pos</param>
        private static void AdjustWindowPosition(Window window, Window dragWindow, MouseEventArgs args)
        {
            Point mousePos = args.GetPosition(window);
            var originalMouseX = mousePos.X;

            var originalWidth = SystemParameters.WorkArea.Width;
            var newWidth = dragWindow.ActualWidth;

            var relativePosition = originalMouseX / originalWidth;
            var newMouseX = relativePosition * newWidth;
            var offset = originalMouseX - newMouseX;

            dragWindow.Left = offset;
            dragWindow.Top = mousePos.Y;
        }

        #endregion
    }
}
