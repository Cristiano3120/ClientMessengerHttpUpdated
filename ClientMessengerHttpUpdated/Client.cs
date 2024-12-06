using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClientMessengerHttpUpdated
{
    internal static class Client
    {
        private static readonly ClientWebSocket _server;
        internal static readonly List<Relationship> _relationships;
        internal static readonly Lock _lock;
        internal static User _user;

        static Client()
        {
            _server = new ClientWebSocket();
            _relationships = new List<Relationship>();
            _lock = new Lock();
        }

        internal static async Task Start()
        {
            await Logger.LogAsync("Starting!");
        Connecting:
            try
            {
                await _server.ConnectAsync(GetServerAdress(true), CancellationToken.None);
                await Logger.LogAsync("Connected successfully to the server!");
                _ = Task.Run(() => ReceiveMessages(_server, CancellationToken.None));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, false);
                _ = Logger.LogAsync("Retrying in 3 seconds");
                await Task.Delay(3000);
                goto Connecting;
            }
        }

        #region Receiving and handling data

        private static async Task ReceiveMessages(ClientWebSocket server, CancellationToken cancellationToken)
        {
            var buffer = new byte[65536];
            var completeMessage = new StringBuilder();
            while (server.State == WebSocketState.Open)
            {
                try
                {
                    WebSocketReceiveResult receivedData = await server.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    _ = Logger.LogAsync(ConsoleColor.Yellow, $"[RECEIVED]: The received payload is {receivedData.Count} bytes long");

                    if (receivedData.CloseStatus is WebSocketCloseStatus closeStatus)
                    {
                        await HandleClosingMessage(closeStatus, receivedData.CloseStatusDescription!);
                    }

                    var receivedDataAsString = Convert.ToBase64String(buffer, 0, receivedData.Count);
                    completeMessage.Append(receivedDataAsString);
                    JsonElement? message;

                    if (receivedData.EndOfMessage)
                    {
                        message = Security.DecyrptMessage(completeMessage.ToString());
                        if (message is JsonElement root)
                        {
                            completeMessage.Clear();
                            _ = Logger.LogAsync(ConsoleColor.Magenta, $"[RECEIVED]: {root}");
                            OpCode opCode = root.GetProperty("code").GetOpCode();

                            _ = HandleMessage(opCode, root);
                        }
                        else
                        {
                            await server.CloseAsync
                                (WebSocketCloseStatus.InvalidPayloadData,
                                "The message couldn´t be decrypted.",
                                CancellationToken.None);
                            break;
                        }
                    }
                    else
                    {
                        _ = Logger.LogAsync("The message is being sent in parts. Waiting for the next part");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
        }

        private static async Task HandleMessage(OpCode opCode, JsonElement root)
        {
            try
            {
                _ = Logger.LogAsync(ConsoleColor.Green, $"[RECEIVED]: Received opCode {opCode}");
                switch (opCode)
                {
                    case OpCode.ReceiveRSA:
                        await Security.SendAes(root);
                        break;
                    case OpCode.ServerReadyToReceive:
                        await ReadSettings();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var login = new Login();
                            login.Show();
                            ClientUI.CloseAllWindowsExceptOne(login);
                        });
                        break;
                    case OpCode.ResponseRequestToCreateAcc:
                        HandleServerMessages.HandleResponseRequestToCreateAcc(root);
                        break;
                    case OpCode.VerificationResult:
                        await HandleServerMessages.HandleResponseToVerification(root);
                        break;
                    case OpCode.UnexpectedError:
                        await HandleServerMessages.HandleUnexpectedError(root);
                        break;
                    case OpCode.ResponseToLogin:
                        await HandleServerMessages.HandleResponseToLogin(root);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        #endregion

        #region Sending Data

        internal static async Task SendPayloadAsync(string payload)
        {
            _ = Logger.LogAsync(ConsoleColor.Blue, $"[SENDING(Aes)]: {payload}");
            ArgumentNullException.ThrowIfNull(payload);
            if (_server.State != WebSocketState.Open)
            {
                return;
            }

            var compressedData = Security.CompressData(payload);
            var buffer = Security.EncryptAes(compressedData);
            var bufferLengthOfServer = 65536;
            var parts = (int)Math.Ceiling((double)buffer.Length / bufferLengthOfServer);

            if (parts > 1)
            {
                var partedBuffer = buffer.Chunk(bufferLengthOfServer).ToArray();
                for (var i = 0; i < partedBuffer.Length; i++)
                {
                    var item = partedBuffer[i];
                    var endOfMessage = i == partedBuffer.Length - 1;

                    await _server.SendAsync(new ArraySegment<byte>(item), WebSocketMessageType.Binary, endOfMessage, CancellationToken.None);
                }
            }
            else
            {
                await _server.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }


        /// <summary>
        /// Encrypts with RSA
        /// </summary>
        /// <param name="rsaKey">The public key needed to encrypt</param>
        internal static async Task SendPayloadAsync(string payload, RSAParameters rsaKey)
        {
            _ = Logger.LogAsync(ConsoleColor.Blue, $"[SENDING(RSA)]: {payload}");
            ArgumentNullException.ThrowIfNull(payload);
            if (_server.State != WebSocketState.Open)
            {
                return;
            }

            var compressedData = Security.CompressData(payload);
            var buffer = Security.EncryptRSA(rsaKey, compressedData);

            var bufferLengthOfServer = 65536;
            var parts = (int)Math.Ceiling((double)buffer.Length / bufferLengthOfServer);

            if (parts > 1)
            {
                var partedBuffer = buffer.Chunk(bufferLengthOfServer).ToArray();
                for (var i = 0; i < partedBuffer.Length; i++)
                {
                    var item = partedBuffer[i];
                    var endOfMessage = i == partedBuffer.Length - 1;

                    await _server.SendAsync(new ArraySegment<byte>(item), WebSocketMessageType.Binary, endOfMessage, CancellationToken.None);
                }
            }
            else
            {
                await _server.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }

        #endregion

        #region Starting helper methods

        private static async Task ReadSettings()
        {
            try
            {
                if (LoadSettings() is not JsonElement settings)
                {
                    return;
                }

                var shouldTryToAutoLogin = settings.GetProperty("AutoLogin").GetBoolean();

                if (shouldTryToAutoLogin && Security.TryGetLoginData(out var email, out var password))
                {
                    await TryToAutoLogin(email, password);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private static JsonElement? LoadSettings()
        {
            var filePath = GetPathByFilename("Settings", FileType.json);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Settings file not found at: {filePath}");

            try
            {
                return JsonDocument.Parse(File.ReadAllText(filePath)).RootElement;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return null;
            }
        }

        private static async Task TryToAutoLogin(string email, string password)
        {
            var payload = new
            {
                code = OpCode.RequestToLogin,
                email,
                password
            };

            var jsonString = JsonSerializer.Serialize(payload);
            await SendPayloadAsync(jsonString).ConfigureAwait(false);
        }

        internal static BitmapImage ByteArrayToBitmapImage(byte[] byteArray)
        {
            if (byteArray.Length == 0)
                throw new ArgumentException("Byte array is empty!");

            using var stream = new MemoryStream(byteArray);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        private static Uri GetServerAdress(bool testing)
        {
            if (testing)
            {
                return new Uri("ws://127.0.0.1:5000/");
            }
            else
            {
                var streamReader = new StreamReader(GetPathByFilename("ServerAdress", FileType.txt));
                return new Uri(streamReader.ReadToEnd());
            }
        }

        #endregion

        internal static async Task CloseConnectionToServer()
        {
            await _server.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Payload was not correct", CancellationToken.None);
        }


        private static async Task HandleClosingMessage(WebSocketCloseStatus socketCloseStatus, string reason)
        {
            await Logger.LogAsync([$"The server closed the connection.", $"Close status: {socketCloseStatus}", $"Reason: {reason}"]);
        }

        /// <summary>
        /// Read the comment for the filename param!
        /// </summary>
        /// <param name="filename">Just the name of the file without the type of file (.txt for exmample)</param>
        /// <returns></returns>
        internal static string GetPathByFilename(string filename, FileType fileType)
        {
            var file = filename += $".{fileType}";
#if DEBUG
            return Path.Combine(@"C:\Users\Crist\source\repos\ClientMessengerHttpUpdated\ClientMessengerHttpUpdated\NeededFiles", file);
#else
            var exePath = Assembly.GetExecutingAssembly().Location;
            var index = exePath.IndexOf("ClientMesseger");
            exePath = exePath.Remove(index);
            var NeddedFilesDirectory = Path.Combine(exePath, "NeededFiles");
            return Path.Combine(NeddedFilesDirectory, file);
#endif
        }
    }
}
