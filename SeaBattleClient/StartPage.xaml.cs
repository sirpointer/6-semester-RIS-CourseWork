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

        Player player = null;

        /// <summary>
        /// Переход на данную страницу.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
                player = e.Parameter as Player;
            else
                player = new Player();
        }

        public StartPage()
        {
            this.InitializeComponent();
            progresRing.IsActive = false;
        }

        /// <summary>
        /// Порт сервера.
        /// </summary>
        private const int port = 11000;

        // The response from the remote device.  
        private static String response = String.Empty;

        /// <summary>
        /// Управление потоком.
        /// </summary>
        public static ManualResetEvent pingDone = new ManualResetEvent(false);

        /// <summary>
        /// Блокировать/активировать элементы управления на странице.
        /// </summary>
        private void ElementEnable(bool enabled)
        {
            progresRing.IsActive = !enabled;

            btnCreateGame.IsEnabled = enabled;
            btnJoinGame.IsEnabled = enabled;
            tbInputIP.IsEnabled = enabled;
        }

        /// <summary>
        /// Создать игру.
        /// </summary>
        private async void BtnCreateGame_Click(object sender, RoutedEventArgs e)
        {
            string ip = tbInputIP.Text.Trim();

            if (!string.IsNullOrEmpty(ip))
            {
                IPEndPoint remoteEP = null;
                ElementEnable(false);

                await Task.Run(() =>
                {
                    remoteEP = ConnectServer(ip);
                });

                if (remoteEP == null)
                {
                    MainPage.MainFrame?.Navigate(typeof(ErrorPage));
                    return;
                }

                player.IPEndPoint = remoteEP;
                ElementEnable(true);

                MainPage.MainFrame?.Navigate(typeof(CreateGamePage), player);
            }
        }

        /// <summary>
        /// Присоединиться к игре.
        /// </summary>
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
                    MainPage.MainFrame?.Navigate(typeof(ErrorPage));
                    return;
                }

                player.IPEndPoint = remoteEP;
                ElementEnable(true);

                MainPage.MainFrame?.Navigate(typeof(JoinGamePage), player);
            }
        }

        /// <summary>
        /// Попытка установить соединение с сервером.
        /// </summary>
        /// <param name="ip">ip адрес сервера.</param>
        private IPEndPoint ConnectServer(string ip)
        {
            try
            {
                IPEndPoint remoteEP;
                pingDone.Reset();
                System.Net.IPAddress addr = System.Net.IPAddress.Parse(ip);
                remoteEP = new IPEndPoint(addr, port);

                Socket client = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                StateObject state = new StateObject();
                state.workSocket = client;
                state.obj = this;
                
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), state);
                pingDone.WaitOne();
                return remoteEP;
            }
            catch (Exception e)
            {
                MainPage.MainFrame?.Navigate(typeof(ErrorPage));
                Debug.WriteLine(e);
                return null;
            }
        }

        #region ConnectServer

        /// <summary>
        /// Установка соединения.
        /// </summary>
        /// <param name="ar"></param>
        private static async void ConnectCallback(IAsyncResult ar)
        {
            Frame parent = null;
            StartPage page = null;
            try
            {  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                page = (StartPage)state.obj;

                await page.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    parent = page.Parent as Frame;
                });
 
                client.EndConnect(ar);
                Debug.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());
                
                JObject jObject = new JObject();
                jObject.Add(JsonStructInfo.Type, Request.EnumTypeToString(Request.RequestTypes.Ping));
                jObject.Add(JsonStructInfo.Result, "");

                string s = jObject.ToString() + JsonStructInfo.EndOfMessage;
  
                Send(state, s);
            }
            catch (Exception e)
            {
                GoToErrorPage(page);
                Debug.WriteLine(e.ToString());
                return;
            }
        }

        /// <summary>
        /// Отправка сообщения на сервер.
        /// </summary>
        private static void Send(StateObject state, String data)
        {
            StartPage page = null;
            try
            {
                Socket client = state.workSocket;
                page = (StartPage)state.obj;

                byte[] byteData = Encoding.UTF8.GetBytes(data);
                
                client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), state);
            }
            catch(Exception e)
            {
                GoToErrorPage(page);
                Debug.WriteLine(e.ToString());
                return;
            }

        }

        /// <summary>
        /// Сообщение доставлено.
        /// </summary>
        private static void SendCallback(IAsyncResult ar)
        {
            StartPage page = null;
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                page = (StartPage)state.obj;
                int bytesSent = client.EndSend(ar);
                Debug.WriteLine("Sent {0} bytes to server.", bytesSent);
                
                Receive(state);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                GoToErrorPage(page);
                return;
            }
        }

        /// <summary>
        /// Начать ожидание сообщения от сервера.
        /// </summary>
        private static void Receive(StateObject state)
        {
            StartPage page = null;
            try
            {
                Socket client = state.workSocket;
                page = (StartPage)state.obj;
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                GoToErrorPage(page);
                Debug.WriteLine(e.ToString());
                return;
            }
        }

        /// <summary>
        /// Получение сообщения.
        /// </summary>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            StartPage page = null;
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                page = (StartPage)state.obj;

                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                    
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
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
                GoToErrorPage(page);
                Debug.WriteLine(e.ToString());
                return;
            }
        }

        #endregion

        /// <summary>
        /// Переход на страницу ошибки.
        /// </summary>
        public static async void GoToErrorPage(Page page)
        {
            await page.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MainPage.MainFrame?.Navigate(typeof(ErrorPage));
            });
        }
    }
}
