using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ClientMessengerHttpUpdated
{
    public partial class Home : Window
    {
        public Home()
        {
            InitializeComponent();
            ClientUI.BindWindowStateButtons(btnClose, btnMinimize, btnMaximize, dragPanel);
            BtnsSubcribeInit();

            (List<Relationship> relationships, List<Chat> chats) = CacheDatabase.GetData();
            lock (Client._lock)
            {
                foreach (Relationship relationship in relationships)
                {
                    Client._relationships.Add(relationship);
                }
            }
            
            PopulateFriendsList(relationships.Where(x => x.RelationshipState == RelationshipState.Friends).ToList());   
            PopulatePendinglist(relationships.Where(x => x.RelationshipState == RelationshipState.Pending).ToList());

            var friends = relationships.Where(x => x.RelationshipState == RelationshipState.Blocked).ToList();
            PopulateBlockedlist(friends);
            PopulateDmList(friends);
        }

        private void BtnsSubcribeInit()
        {
            btnPending.Click += (sender, args) =>
            {
                ChangePanelVisibility(PendingFriendRequestsPanel, PendingFriendRequestsTranslateTransform);
            };

            btnFriends.Click += (sender, args) =>
            {
                ChangePanelVisibility(FriendsPanel, FriendsPanelTranslateTransform);
            };

            btnAddFriend.Click += (sender, args) =>
            {
                ChangePanelVisibility(AddFriendsPanel, AddFriendsPanelTranslateTransform);
            };

            btnBlocked.Click += (sender, args) =>
            {
                ChangePanelVisibility(BlockedPanel, BlockedPanelTranslateTransform);
            };
        }

        private void ChangePanelVisibility(Grid panel, TranslateTransform translateTransform)
        {
            if (panel.Visibility == Visibility.Visible)
            {
                SlideOutAnimation(translateTransform, panel);
            }
            else
            {
                SlideInAnimation(translateTransform, panel);
            }
        }

        #region PopulateLists

        #region Chat

        #endregion

        #region Dms

        private void PopulateDmList(List<Relationship> friends)
        {
            foreach (Relationship friend in friends)
            {
                StackPanel stackPanel = BasicUserUI(friend);
                ChatsList.Items.Add(stackPanel);
            }
        }

        private void AddOneToDms(Relationship friend)
        {
            StackPanel stackPanel = BasicUserUI(friend);
            ChatsList.Items.Add(stackPanel);
        }

        private void RemoveFriendFromDms(Relationship friend)
        {
            StackPanel? item = ChatsList.Items.OfType<StackPanel>()
                .FirstOrDefault(x => (string)x.Tag == friend.User.Username);

            if (item == null)
                return;

            ChatsList.Items.Remove(item);
        }

        #endregion

        #region Pending

        private void PopulatePendinglist(List<Relationship> pendingRequest)
        {
            foreach (Relationship pending in pendingRequest)
            {
                StackPanel stackPanel = BasicUserUI(pending);
                CreateBtnsForPendinglistUI(stackPanel, pending);
                PendingFriendsList.Items.Add(stackPanel);
            }
        }

        private void CreateBtnsForPendinglistUI(StackPanel stackPanel, Relationship pendingRequest)
        {
            var acceptButton = new Button
            {
                Content = "Accept",
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#288444")),
                Foreground = Brushes.White,
                Width = 80,
                Height = 30,
                Margin = new Thickness(5),
                Tag = (pendingRequest.User.Username, RelationshipActions.Accept)
            };

            var declineButton = new Button
            {
                Content = "Decline",
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f44038")),
                Foreground = Brushes.White,
                Width = 80,
                Height = 30,
                Margin = new Thickness(5),
                Tag = (pendingRequest.User.Username, RelationshipActions.Decline)
            };

            var blockButton = new Button
            {
                Content = "Block",
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#302c34")),
                Foreground = Brushes.White,
                Width = 80,
                Height = 30,
                Margin = new Thickness(5),
                Tag = (pendingRequest.User.Username, RelationshipActions.Block),
            };
            acceptButton.Click += RelationshipStateChange_Click;
            declineButton.Click += RelationshipStateChange_Click;
            blockButton.Click += RelationshipStateChange_Click;

            stackPanel.Children.Add(acceptButton);
            stackPanel.Children.Add(declineButton);
            stackPanel.Children.Add(blockButton);
        }

        private void AddOneToPendinglist(Relationship pending)
        {
            StackPanel stackPanel = BasicUserUI(pending);
            CreateBtnsForPendinglistUI(stackPanel, pending);
            PendingFriendsList.Items.Add(stackPanel);
        }

        private void RemoveUserFromPendinglist(Relationship pending)
        {
            StackPanel? stackPanel = PendingFriendsList.Items.OfType<StackPanel>()
                .FirstOrDefault(x => (string)x.Tag == pending.User.Username);

            if (stackPanel == null)
                return;

            PendingFriendsList.Items.Remove(stackPanel);
        }

        #endregion

        #region Friends

        private void PopulateFriendsList(List<Relationship> friends)
        {
            foreach (Relationship friend in friends)
            {
                StackPanel stackPanel = BasicUserUI(friend);
                CreateBtnsForFriendlistUI(stackPanel, friend);
                FriendsList.Items.Add(stackPanel);
            }
        }

        private void AddOneToFriendlist(Relationship friend)
        {
            StackPanel stackPanel = BasicUserUI(friend);
            CreateBtnForBlockedlistUI(stackPanel, friend);
            FriendsList.Items.Add(stackPanel);
        }

        private void RemoveUserFromFriendlist(Relationship friend)
        {
            StackPanel? stackPanel = FriendsList.Items.OfType<StackPanel>()
                .FirstOrDefault(x => (string)x.Tag == friend.User.Username);

            if (stackPanel == null)
                return;

            FriendsList.Items.Remove(stackPanel);
        }

        private void CreateBtnsForFriendlistUI(StackPanel stackPanel, Relationship friend)
        {
            var blockButton = new Button
            {
                Content = "Block",
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#302c34")),
                Foreground = Brushes.White,
                Width = 80,
                Height = 30,
                Margin = new Thickness(5),
                Tag = (friend.User.Username, RelationshipActions.Block),
            };

            var deleteButton = new Button
            {
                Content = "Delete",
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e0343c")),
                Foreground = Brushes.White,
                Width = 80,
                Height = 30,
                Margin = new Thickness(5),
                Tag = (friend.User.Username, RelationshipActions.DeleteAsFriend),
            };
            blockButton.Click += RelationshipStateChange_Click;
            deleteButton.Click += RelationshipStateChange_Click;

            stackPanel.Children.Add(blockButton);
            stackPanel.Children.Add(deleteButton);
        }

        #endregion

        #region Blocked/Unblocked 

        private void PopulateBlockedlist(List<Relationship> blockedUsers)
        {
            foreach (Relationship user in blockedUsers)
            {
                StackPanel stackPanel = BasicUserUI(user);
                CreateBtnForBlockedlistUI(stackPanel, user);
                BlockedList.Items.Add(stackPanel);
            }
        }

        private void AddOneToBlockedlist(Relationship blockedUser)
        {
            StackPanel stackPanel = BasicUserUI(blockedUser);
            CreateBtnForBlockedlistUI(stackPanel, blockedUser);
            BlockedList.Items.Add(stackPanel);
        }

        private void CreateBtnForBlockedlistUI(StackPanel stackPanel, Relationship user)
        {
            var unblockButton = new Button
            {
                Content = "Unblock",
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#302c34")),
                Foreground = Brushes.White,
                Width = 80,
                Height = 30,
                Margin = new Thickness(5),
                Tag = (user.User.Username, RelationshipActions.Unblock),
            };
            unblockButton.Click += RelationshipStateChange_Click;

            stackPanel.Children.Add(unblockButton);
        }

        private void RemoveUserFromBlockedlist(Relationship user)
        {
            StackPanel? stackPanel = BlockedList.Items.OfType<StackPanel>()
                .FirstOrDefault(x => (string)x.Tag == user.User.Username);

            if (stackPanel == null)
                return;

            BlockedList.Items.Remove(stackPanel);
        }

        #endregion

        private async void RelationshipStateChange_Click(object sender, RoutedEventArgs args)
        {
            if (sender is Button button && button.Tag is (User relation, RelationshipActions task))
            {
                var payload = new
                {
                    code = 2,
                    task,
                    callerId = Client._user!.Id,
                    affectedUser = relation.Id,
                };
                var jsonString = JsonSerializer.Serialize(payload);
                await Client.SendPayloadAsync(jsonString);

                if (task is RelationshipActions.Unblock or RelationshipActions.DeleteAsFriend or RelationshipActions.Decline)
                {
                    lock (Client._lock)
                    {
                        Client._relationships.RemoveAll(x => x.User.Id == relation.Id);
                    }
                    return;
                }

                if (task is RelationshipActions.Accept or RelationshipActions.Block)
                {
                    lock (Client._lock)
                    {
                        Relationship? user = Client._relationships.Find(x => x.User.Id == relation.Id);
                        user!.RelationshipState = task == RelationshipActions.Accept
                            ? RelationshipState.Friends
                            :RelationshipState.Blocked;
                    }
                }
            };
        }
        

        /// <summary>
        /// Creates the basic UI like the profil picture and the username textblock
        /// </summary>
        private static StackPanel BasicUserUI(Relationship user)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5),
                Tag = user.User.Username,
            };

            var ellipse = new Ellipse
            {
                Width = 45,
                Height = 45,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var imageBrush = new ImageBrush()
            {
                ImageSource = user.User.ProfilPic,
                Stretch = Stretch.UniformToFill,
            };

            ellipse.Fill = imageBrush;

            var textBlockUsername = new TextBlock
            {
                Text = user.User.Username,
                Foreground = Brushes.White,
                FontSize = 18,
                Margin = new Thickness(10)
            };

            stackPanel.Children.Add(ellipse);
            stackPanel.Children.Add(textBlockUsername);
            return stackPanel;
        }

        #endregion

        #region Animations

        private void SlideInAnimation(TranslateTransform translateTransform, Grid grid)
        {
            BlockedPanel.Visibility = Visibility.Collapsed;
            AddFriendsPanel.Visibility = Visibility.Collapsed;
            FriendsPanel.Visibility = Visibility.Collapsed;
            PendingFriendRequestsPanel.Visibility = Visibility.Collapsed;

            grid.Visibility = Visibility.Visible;
            var slideInAnimation = new DoubleAnimation
            {
                From = grid.Width,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            translateTransform.BeginAnimation(TranslateTransform.XProperty, slideInAnimation);
        }

        private static void SlideOutAnimation(TranslateTransform translateTransform, Grid grid)
        {
            if (grid.Visibility == Visibility.Visible)
            {
                var slideOutAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = grid.Width,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                slideOutAnimation.Completed += (s, a) =>
                {
                    grid.Visibility = Visibility.Collapsed;
                };

                translateTransform.BeginAnimation(TranslateTransform.XProperty, slideOutAnimation);
            }
        }

        #endregion
    }
}
