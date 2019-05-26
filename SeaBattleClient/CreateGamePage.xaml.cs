using Newtonsoft.Json.Linq;
using SeaBattleClassLibrary.DataProvider;
using SeaBattleClassLibrary.Game;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static SeaBattleClient.StartPage;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace SeaBattleClient
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class CreateGamePage : Page
    {
        public Player Model
        {
            get
            {
                return this.DataContext as Player;
            }
        }

        Player player = null;

        // The response from the remote device.  
        private static String response = String.Empty;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
                player = e.Parameter as Player;
        }

        public CreateGamePage()
        {
            this.InitializeComponent();
        }

        public static ManualResetEvent pingDone = new ManualResetEvent(false);
        public static string playerName = String.Empty;
        public static string gameName = string.Empty;

        private async void BtnStartGame_Click(object sender, RoutedEventArgs e)
        {
            playerName = tbPlayerName.Text.Trim();
            gameName = tbGameName.Text.Trim();

            if(!(string.IsNullOrEmpty(playerName) && string.IsNullOrEmpty(gameName)))
            {
                ElementEnable(false);
                IPEndPoint remoteEP = Model.IPEndPoint;
                Socket socket = null;

                await Task.Run(() =>
                {
                    pingDone.Reset();
                    // Create a TCP/IP socket.  
                    Socket client = new Socket(remoteEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    StateObject state = new StateObject();
                    state.workSocket = client;
                    state.obj = this;
                    socket = client;

                    // Connect to the remote endpoint.  
                    client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), state);
                    pingDone.WaitOne();
                });

                Model.PlayerSocket = socket;
                
                ElementEnable(true);
                (Parent as Frame).Navigate(typeof(BeginPage), Model);
            }
        }

        private void ElementEnable(bool enabled)
        {
            progresRing.IsActive = !enabled;

            btnCancle.IsEnabled = enabled;
            btnStartGame.IsEnabled = enabled;
            tbGameName.IsEnabled = enabled;
            tbPlayerName.IsEnabled = enabled;
        }

        private void BtnCancle_Click(object sender, RoutedEventArgs e)
        {
            Frame frame = (Parent as Frame);
            if (frame.CanGoBack)
                frame.GoBack();
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());


                JObject jObject = new JObject();
                jObject.Add(JsonStructInfo.Type, Request.EnumTypeToString(Request.RequestTypes.AddGame));
                string message = Serializer<BeginGame>.Serialize(new BeginGame() { PlayerName = playerName, GameName = gameName });
                jObject.Add(JsonStructInfo.Result, message);

                string s = jObject.ToString() + JsonStructInfo.EndOfMessage;

                // Send test data to the remote device.  
                Send(state, s);
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Send(StateObject state, String data)
        {
            Socket client = state.workSocket;
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), state);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                Receive(state);

            } 
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Receive(StateObject state)
        {
            try
            {
                Socket client = state.workSocket;
                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);
                
                state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                // All the data has arrived; put it in response.  
                if (state.sb.Length > 1)
                {
                    response = state.sb.ToString();
                }
                    
                pingDone.Set();
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
