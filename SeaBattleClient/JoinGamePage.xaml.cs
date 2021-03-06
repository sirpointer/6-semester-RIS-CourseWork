﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeaBattleClassLibrary.DataProvider;
using SeaBattleClassLibrary.Game;
using System;
using System.Collections.Generic;
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
    public sealed partial class JoinGamePage : Page
    {
        public Player Model
        {
            get
            {
                return this.DataContext as Player;
            }
        }

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
        
        private static String response = String.Empty;

        Player player = null;

        public static ManualResetEvent pingDone = new ManualResetEvent(false);

        /// <summary>
        /// Переход на данную страницу.
        /// </summary>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
                player = e.Parameter as Player;

            progressRing.IsActive = true;

            IPEndPoint remoteEP = Model.IPEndPoint;

            List<BeginGame> beginGames = new List<BeginGame>();

            await Task.Run(() =>
            {
                pingDone.Reset();
  
                Socket client = new Socket(remoteEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                StateObject state = new StateObject();
                state.workSocket = client;
                state.obj = this;
                
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), state);
                pingDone.WaitOne();

                string result = GetJsonRequestResult(response);
                beginGames = Serializer<List<BeginGame>>.Deserialize(result);
            });

            listView.Items.Clear();

            foreach(var game in beginGames)
            {
                StackPanel stackPanel = new StackPanel();
                stackPanel.Children.Add(new TextBlock() { Text = $"Игра: {game.GameName}\n Игрок: {game.PlayerName}" });

                listView.Items.Add(stackPanel);
            }

            progressRing.IsActive = false;
        }

        /// <summary>
        /// Возвращает тело запроса.
        /// </summary>
        public static string GetJsonRequestResult(string jsonRequest)
        {
            JObject jObject;

            try
            {
                jObject = JObject.Parse(jsonRequest);
            } 
            catch (JsonReaderException e)
            {
                Console.WriteLine(e);
                return null;
            }

            if (jObject.ContainsKey(JsonStructInfo.Result))
            {
                string result = (string)jObject[JsonStructInfo.Result];
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Возвращает тип ответа
        /// </summary>
        public static Answer.AnswerTypes GetAnswerType(string jsonRequest)
        {
            JObject jObject;

            try
            {
                jObject = JObject.Parse(jsonRequest);
            } catch (JsonReaderException e)
            {
                Console.WriteLine();
                Console.WriteLine(e);
                Console.WriteLine(jsonRequest);
                Console.WriteLine();
                return Answer.AnswerTypes.Error;
            }

            if (jObject.ContainsKey(JsonStructInfo.Type))
            {
                string requestType = (string)jObject[JsonStructInfo.Type];
                return Answer.JsonTypeToEnum(requestType);
            } else
            {
                return Answer.AnswerTypes.Error;
            }
        }

        public JoinGamePage()
        {
            this.InitializeComponent();
        }


        private void BtnCancle_Click(object sender, RoutedEventArgs e)
        {
            Frame frame = (Parent as Frame);
            if (frame.CanGoBack)
                frame.GoBack();
        }

        /// <summary>
        /// Выбор игры
        /// </summary>
        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ElementEnabled(true);

            IPEndPoint remoteEP = Model.IPEndPoint;
            var a = e.AddedItems.ToList();

            string s = ((e.AddedItems.ToList()[0] as StackPanel).Children[0] as TextBlock).Text;
            string gameName = s.Substring(6, s.IndexOf('\n') - 6);
            Socket socket = null;

            await Task.Run(() =>
            {
                pingDone.Reset();

                // Create a TCP/IP socket.  
                Socket client = new Socket(remoteEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                StateObject state = new StateObject();
                state.workSocket = client;
                state.obj = gameName;
                socket = client;

                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback1), state);
                pingDone.WaitOne();
            });

            Model.PlayerSocket = socket;
            Answer.AnswerTypes dataType = GetAnswerType(response);

            if (dataType == Answer.AnswerTypes.Ok)
            {
                MainPage.MainFrame?.Navigate(typeof(BeginPage), Model);
            }

            ElementEnabled(false);
        }

        /// <summary>
        /// блокировка элементов
        /// </summary>
        private void ElementEnabled(bool enable)
        {
            progressRing.IsActive = enable;
            btnCancle.IsEnabled = !enable;
        }

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

        #region GetListGame

        /// <summary>
        /// попытка соединения
        /// </summary>
        private static void ConnectCallback(IAsyncResult ar)
        {
            JoinGamePage page = null;
            try
            { 
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;


                page = (JoinGamePage)state.obj;

                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());
                
                JObject jObject = new JObject();
                jObject.Add(JsonStructInfo.Type, Request.EnumTypeToString(Request.RequestTypes.GetGames));
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
            JoinGamePage page = null;
            try
            {
                Socket client = state.workSocket;

                page = (JoinGamePage)state.obj;

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
        private static async void SendCallback(IAsyncResult ar)
        {
            Frame parent = null;
            Page page = null;
            try
            {
                // Retrieve the socket from the state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                
                page = (JoinGamePage)state.obj;

                await page.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    parent = page.Parent as Frame;
                });
                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                Receive(state);

            } 
            catch (Exception e)
            {
                GoToErrorPage(page);
                Debug.WriteLine(e.ToString());
                return;
            }
        }

        /// <summary>
        /// Начать ожидание сообщения от сервера.
        /// </summary>
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

        /// <summary>
        /// Получение ответа от сервера
        /// </summary>
        private static async void ReceiveCallback(IAsyncResult ar)
        {
            Frame parent = null;
            Page page = null;
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                JoinGamePage startPage = state.obj as JoinGamePage;

                page = (JoinGamePage)state.obj;

                await page.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    parent = page.Parent as Frame;
                });
                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                } else
                {
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length > 1)
                    {
                        string resp = state.sb.ToString();
                        response = resp.Remove(resp.LastIndexOf(JsonStructInfo.EndOfMessage));
                    }

                    client.Shutdown(SocketShutdown.Both);
                    client.Close();

                    pingDone.Set();
                }
            } catch (Exception e)
            {
                GoToErrorPage(page);
                Debug.WriteLine(e.ToString());
                return;
            }
        }

        #endregion


        #region StartGame

        /// <summary>
        /// попытка соединения
        /// </summary>
        private static void ConnectCallback1(IAsyncResult ar)
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
                jObject.Add(JsonStructInfo.Type, Request.EnumTypeToString(Request.RequestTypes.JoinTheGame));
                string message = Serializer<BeginGame>.Serialize(new BeginGame() { GameName = state.obj.ToString() });
                jObject.Add(JsonStructInfo.Result, message);

                string s = jObject.ToString() + JsonStructInfo.EndOfMessage;

                // Send test data to the remote device.  
                Send1(state, s);
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Send1(StateObject state, String data)
        {
            Socket client = state.workSocket;
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback1), state);
        }

        /// <summary>
        /// Сообщение доставлено.
        /// </summary>
        private static void SendCallback1(IAsyncResult ar)
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
                Receive1(state);

            } 
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Начать ожидание сообщения от сервера.
        /// </summary>
        private static void Receive1(StateObject state)
        {
            try
            {
                Socket client = state.workSocket;
                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback1), state);
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Получение ответа от сервера
        /// </summary>
        private static void ReceiveCallback1(IAsyncResult ar)
        {
            try
            {
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
                    string resp = state.sb.ToString();
                    response = resp.Remove(resp.LastIndexOf(JsonStructInfo.EndOfMessage));
                }

                pingDone.Set();
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion
    }
}
