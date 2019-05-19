using Newtonsoft.Json.Linq;
using SeaBattleClassLibrary.DataProvider;
using SeaBattleClassLibrary.Game;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace SeaBattleClient
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class StartPage : Page
    {

        public class StateObject
        {
            // Client socket.  
            public Socket workSocket = null;
            // Size of receive buffer.  
            public const int BufferSize = 256;
            // Receive buffer.  
            public byte[] buffer = new byte[BufferSize];
            // Received data string.  
            public StringBuilder sb = new StringBuilder();

            public object obj = null;
        }

        public Player Model
        {
            get
            {
                return this.DataContext as Player;
            }
        }

        Player player = null;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
                player=e.Parameter as Player;
        }

        public StartPage()
        {
            this.InitializeComponent();
            progresRing.IsActive = false;
        }

        private const int port = 11000;
        // The response from the remote device.  
        private static String response = String.Empty;

        public static ManualResetEvent pingDone = new ManualResetEvent(false);

        private async void BtnCreateGame_Click(object sender, RoutedEventArgs e)
        {
            string ip = tbInputIP.Text.Trim();

            if(!string.IsNullOrEmpty(ip))
            {
                IPEndPoint remoteEP = null;
                ElementEnable(false);

                await Task.Run(() =>
                {
                    remoteEP = ConnectServer(ip);
                });

                if (remoteEP == null)
                {
                    (Parent as Frame).Navigate(typeof(ErrorPage));
                    return;
                }

                Model.IPEndPoint = remoteEP;
                ElementEnable(true);

                (Parent as Frame).Navigate(typeof(CreateGamePage), Model);
            }
        }

        private async void BtnJoinGame_Click(object sender, RoutedEventArgs e)
        {
            string ip = tbInputIP.Text.Trim();

            if (ip != null && ip != string.Empty)
            {
                IPEndPoint remoteEP = null;
                ElementEnable(false);

                await Task.Run(() =>
                {
                    remoteEP = ConnectServer(ip);
                });

                if (remoteEP == null)
                {
                    (Parent as Frame).Navigate(typeof(ErrorPage));
                    return;
                }



                Model.IPEndPoint = remoteEP;
                ElementEnable(true);

                (Parent as Frame).Navigate(typeof(JoinGamePage), Model);
            }
        }

        private void ElementEnable(bool enabled)
        {
            progresRing.IsActive = !enabled;

            btnCreateGame.IsEnabled = enabled;
            btnJoinGame.IsEnabled = enabled;
            tbInputIP.IsEnabled = enabled;
        }

        private IPEndPoint ConnectServer(string ip)
        {
            try
            {
                IPEndPoint remoteEP;
                pingDone.Reset();
                IPHostEntry ipHostInfo = Dns.GetHostEntry(ip);
                IPAddress ipAddress = ipHostInfo.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).Last();
                remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.  
                Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                StateObject state = new StateObject();
                state.workSocket = client;
                state.obj = this;

                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), state);
                pingDone.WaitOne();
                return remoteEP;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        private static void ConnectCallback(IAsyncResult ar)  ///////////////////////////////////////////
        {
            try
            {
                // Retrieve the socket from the state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Complete the connection.  
                client.EndConnect(ar);

                Debug.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());


                JObject jObject = new JObject();
                jObject.Add(JsonStructInfo.Type, Request.EnumTypeToString(Request.RequestTypes.Ping));
                jObject.Add(JsonStructInfo.Result, "");

                string s = jObject.ToString() + JsonStructInfo.EndOfMessage;

                // Send test data to the remote device.  
                Send(state, s);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                //(Parent as Frame).Navigate(typeof(ErrorPage));
            }
        }

        private static void Send(StateObject state, String data) ////////////////////////////////////
        {
            try
            {

                Socket client = state.workSocket;
                // Convert the string data to byte data using ASCII encoding.  
                byte[] byteData = Encoding.UTF8.GetBytes(data);

                // Begin sending the data to the remote device.  
                client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), state);
            } catch
            {
                //(Parent as Frame).Navigate(typeof(ErrorPage));
            }

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
                Debug.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                Receive(state);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private static void Receive(StateObject state)
        {
            try
            {
                Socket client = state.workSocket;
                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                StartPage startPage = state.obj as StartPage;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }

                    client.Shutdown(SocketShutdown.Both);
                    client.Close();

                    pingDone.Set();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }
}
