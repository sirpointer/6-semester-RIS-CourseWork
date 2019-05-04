using Newtonsoft.Json.Linq;
using SeaBattleClassLibrary.DataProvider;
using SeaBattleClassLibrary.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
            progresRing.IsActive = true;
            playerName = tbPlayerName.Text;
            gameName = tbGameName.Text;
            IPEndPoint remoteEP = Model.IPEndPoint;
            //ring = new ProgressRing() { IsActive = true };

            await Task.Run(() => {
                pingDone.Reset();
                //IPHostEntry ipHostInfo = Dns.GetHostEntry(ip);
                //IPAddress ipAddress = ipHostInfo.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).First();

                // Create a TCP/IP socket.  
                Socket client = Model.PlayerSocket;//new Socket(remoteEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                StateObject state = new StateObject();
                state.workSocket = client;
                state.obj = this;

                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), state);
                pingDone.WaitOne();
            });

            //Model.IPEndPoint = remoteEP;
            progresRing.IsActive = false;
            (Parent as Frame).Navigate(typeof(BeginPage), Model);


            //(Parent as Frame).Navigate(typeof(BeginPage), Model);
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
                string message = Serializer<BeginGame>.SetSerializedObject(new BeginGame() { PlayerName = playerName, GameName = gameName });
                jObject.Add(JsonStructInfo.Result, message);
                //jObject.Add(JsonStructInfo.Result, message);

                string s = jObject.ToString() + JsonStructInfo.EndOfMessage;

                // Send test data to the remote device.  
                Send(state, s);
                // send ping

                // Signal that the connection has been made.  
                //connectDone.Set();
            } catch (Exception e)
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
                //sendDone.Set();
                Receive(state);

            } catch (Exception e)
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

                //if (bytesRead > 0)
                //{
                //    // There might be more data, so store the data received so far.  
                state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                // All the data has arrived; put it in response.  
                if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.  
                    //receiveDone.Set();

                    //client.Shutdown(SocketShutdown.Both);
                    //client.Close();

                    pingDone.Set();

                    //startPage.DeactivateRing();
                    //startPage.ring = new ProgressRing() { IsActive = false };
                    //startPage.progresRing.IsActive = false;
                    //(startPage.Parent as Frame).Navigate(typeof(CreateGamePage), startPage.Model);
                //}
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
