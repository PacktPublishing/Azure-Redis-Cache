using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RedisDemo.Chat.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Variables
        private ObservableCollection<Message> collection;
        private ConnectionMultiplexer redis;
        private ISubscriber subscriber;

        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;

            // Set the variables etc.
            setThingsUp();
        }

        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Terminate the sessions and efficiently close the streams.
            if(redis != null)
            {
                var sub = redis.GetSubscriber();
                await sub.UnsubscribeAsync(username.Text.Trim().ToLower());
                await redis.CloseAsync();
            }
        }

        // Set the variables and the focus of user.
        private void setThingsUp()
        {
            collection = new ObservableCollection<Message>();
            username.IsEnabled = true;
            setUsernameBtn.IsEnabled = false;

            username.Focus();

            // Set the list data source;
            messagesList.ItemsSource = collection;
        }

        private async void setUsernameBtn_Click(object sender, RoutedEventArgs e)
        {
            // Prevent resubmission of the request.
            setUsernameBtn.IsEnabled = false;

            // Establish conncetion, asynchronously.
            redis = await ConnectionMultiplexer.ConnectAsync("<connStr>");

            if(redis != null)
            {
                if(redis.IsConnected)
                {
                    // Subscribe to our username
                    subscriber = redis.GetSubscriber();
                    await subscriber.SubscribeAsync(username.Text.Trim().ToLower(), (channel, value) =>
                    {
                        string buffer = value;
                        var message = JsonConvert.DeserializeObject<Message>(buffer);
                        message.ReceivedAt = DateTime.Now;

                        // This function runs on a background thread, thus dispatcher is needed.
                        Dispatcher.Invoke(() =>
                        {
                            collection.Add(message);
                        });
                    });

                    // Enable the messaging buttons and box.
                    message.IsEnabled = true;
                    message.Focus();
                    
                    Title += " : " + username.Text.Trim();

                    username.IsEnabled = false;
                } else
                {
                    MessageBox.Show("We could not connect to Azure Redis Cache service. Try again later.");
                    setUsernameBtn.IsEnabled = true;
                }
            } else
            {
                setUsernameBtn.IsEnabled = true;
            }
        }

        // Send the message
        private async void sendMessageBtn_Click(object sender, RoutedEventArgs e)
        {
            var content = message.Text.Trim();

            // Get the recipient name, e.g. @someone hi there!
            var recipient = content.Split(' ')[0].Replace("@", "").Trim().ToLower();

            // Create the message payload.
            var blob = new Message();
            blob.Sender = username.Text.Trim();
            blob.Content = content;

            // Send the message.
            var received = await subscriber.PublishAsync(recipient, JsonConvert.SerializeObject(blob));

            // If no recipient got the message, show the error.
            if (received == 0)
            {
                MessageBox.Show($"Sorry, '{recipient}' is not active at the moment.");
            }
            message.Text = "";
        }

        // Before the app launch, we need to set the username of current active user.
        private void username_TextChanged(object sender, TextChangedEventArgs e)
        {
            setUsernameBtn.IsEnabled = !string.IsNullOrEmpty(username.Text.Trim());
        }

        // To enable the send button.
        private void message_TextChanged(object sender, TextChangedEventArgs e)
        {
            // We are only supporting messages directly to user, so a username is expected.
            sendMessageBtn.IsEnabled = message.Text.Trim().StartsWith("@") && 
                                        !string.IsNullOrEmpty(message.Text);
        }
    }
}
