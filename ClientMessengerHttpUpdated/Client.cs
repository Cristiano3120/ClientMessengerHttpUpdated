using LiteDB;
using System.IO;
using System.Net.WebSockets;

namespace ClientMessengerHttpUpdated
{
    internal static class Client
    {
        private static readonly ClientWebSocket _server;

        static Client()
        {
            _server = new ClientWebSocket();
        }

        internal static async void Start()
        {
            await Logger.LogAsync("Starting!");
        Connecting:
            try
            {
                await _server.ConnectAsync(GetServerAdress(true), CancellationToken.None);
                _ = Logger.LogAsync("Connected successfully to the server!");
                _ = Task.Run(() => ReceiveMessages(_server, CancellationToken.None));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                _ = Logger.LogAsync("Retrying in 3 seconds");
                await Task.Delay(3000);
                goto Connecting;
            }
            var pathToCacheDB = @"C:\Users\Crist\source\repos\ClientMessengerHttpUpdated\ClientMessengerHttpUpdated\NeededFiles\CachedData.db";
        }

        private static Uri GetServerAdress(bool testing)
        {
            if (testing)
            {
                return new Uri("ws://127.0.0.1:5000/");
            }
            else
            {
                var streamReader = new StreamReader(GetPathByFilename("ServerAdress.txt"));
                return new Uri(streamReader.ReadToEnd());
            }
        }

        internal static string GetPathByFilename(string filename)
        {
#if DEBUG
            return Path.Combine(@"C:\Users\Crist\source\repos\ClientMessengerHttp\ClientMessengerHttpUpdated\NeededFiles", filename);
#else
            var exePath = Assembly.GetExecutingAssembly().Location;
            var index = exePath.IndexOf("ClientMesseger");
            exePath = exePath.Remove(index);
            var NeddedFilesDirectory = Path.Combine(exePath, "NeededFiles");
            return Path.Combine(NeddedFilesDirectory, filename);
#endif
        }
    }
}
