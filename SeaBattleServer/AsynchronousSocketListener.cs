using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SeaBattleClassLibrary.DataProvider;
using SeaBattleClassLibrary.Game;

namespace SeaBattleServer
{
    partial class AsynchronousSocketListener
    {
        private static readonly List<GameSession> sessions = new List<GameSession>();

        // State object for reading client data asynchronously  
        private class StateObject
        {
            // Client  socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 1024;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }

        // Thread signal.
        private static ManualResetEvent allDone = new ManualResetEvent(false);

        private static void StartListening()
        {
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).Last();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
            // Create a TCP/IP socket.
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                Console.WriteLine(localEndPoint.ToString());

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), state);
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            string content = string.Empty;

            // Retrieve the state object and the handler socket from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read more data.
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the client. Display it on the console.
                    Console.WriteLine("\nRead {0} bytes from socket. \n Data : {1}\n", content.Length, content);
              
                    content = content.Remove(content.LastIndexOf(JsonStructInfo.EndOfMessage));
                    Request.RequestTypes dataType = RequestHandler.GetRequestType(content);
                    string result = RequestHandler.GetJsonRequestResult(content);
                    string additionalContent = RequestHandler.GetAdditionalContent(content);

                    switch (dataType)
                    {
                        case Request.RequestTypes.Ping:
                            SendOk(handler, true);
                            break;
                        case Request.RequestTypes.AddGame:
                            AddGame(handler, RequestHandler.GetAddGameResult(result));
                            break;
                        case Request.RequestTypes.Shot:
                            Shot(handler, RequestHandler.GetGame(additionalContent), RequestHandler.GetLocation(result));
                            break;
                        case Request.RequestTypes.SetField:
                            SetField(handler, RequestHandler.GetGameFieldResult(result));
                            break;
                        case Request.RequestTypes.BadRequest:
                            SendError(handler, true);
                            break;
                        case Request.RequestTypes.GetGames:
                            GiveGames(handler);
                            break;
                        case Request.RequestTypes.JoinTheGame:
                            JoinTheGame(handler, RequestHandler.GetJoinTheGameResult(result));
                            break;
                    }
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
        }


        private static void Shot(Socket handler, BeginGame game, Location shotLocation)
        {
            GameSession session = null;
            lock (sessions)
            {
                session = sessions.Find(x => x.Player1?.IPEndPoint == handler.RemoteEndPoint || x.Player2?.IPEndPoint == handler.RemoteEndPoint);
            }

            if (session == null)
            {
                handler?.Shutdown(SocketShutdown.Both);
                handler?.Close();
            }

            if (!session.CanGo)
            {
                BeginReceive(handler);
                return;
            }


            Player player1 = session.Player1.IPEndPoint == handler.RemoteEndPoint ? session.Player1 : session.Player2;
            Player player2 = session.Player1.IPEndPoint != handler.RemoteEndPoint ? session.Player1 : session.Player2;

            if (session.WhoseTurn.IPEndPoint != handler.RemoteEndPoint)
                return;

            session.CanGo = false;

            Ship ship = player2.GameField.Shot(shotLocation);

            string message = AnswerHandler.GetShotResultMessage(ship, shotLocation);
            byte[] data = Encoding.UTF8.GetBytes(message);
            int bytesSent = player1.PlayerSocket.Send(data, 0, data.Length, SocketFlags.None);
            Console.WriteLine($"Sent {bytesSent} bytes to {player1.PlayerSocket.RemoteEndPoint.ToString()}.\n{message}\n\n");

            message = AnswerHandler.GetShotResultMessage(shotLocation);
            data = null;
            data = Encoding.UTF8.GetBytes(message);
            bytesSent = player2.PlayerSocket.Send(data, 0, data.Length, SocketFlags.None);
            Console.WriteLine($"Sent {bytesSent} bytes to {player2.PlayerSocket.RemoteEndPoint.ToString()}.\n{message}\n\n");

            if (player2.GameField.IsGameOver)
            {
                player1?.PlayerSocket?.Shutdown(SocketShutdown.Both);
                player1?.PlayerSocket?.Close();
                player2?.PlayerSocket?.Shutdown(SocketShutdown.Both);
                player2?.PlayerSocket?.Close();

                lock (sessions)
                {
                    sessions.Remove(session);
                }
            }

            if (ship == null)
            {
                session.WhoseTurn = player2;
                BeginReceive(player2.PlayerSocket);
            }
            else
            {
                session.WhoseTurn = player1;
                BeginReceive(player1.PlayerSocket);
            }

            session.CanGo = true;
        }

        /// <summary>
        /// Задать игровое поле.
        /// </summary>
        /// <param name="gameField">Игровое поле.</param>
        private static void SetField(Socket handler, GameField gameField)
        {
            GameSession game = null;
            Player player = null;
            Player secondPlayer = null;

            try
            {
                lock (sessions)
                {
                    game = sessions.Find(x => x.Player1?.IPEndPoint == handler.RemoteEndPoint || x.Player2?.IPEndPoint == handler.RemoteEndPoint);
                }

                player = game.Player1.IPEndPoint == handler.RemoteEndPoint ? game.Player1 : game.Player2;
                secondPlayer = game.Player1.IPEndPoint != handler.RemoteEndPoint ? game.Player1 : game.Player2;

                player.GameField = gameField;

                // если второй игрок не присоединился
                if (secondPlayer == null)
                {
                    string message = AnswerHandler.AwaitSecondPlayer();
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    int bytesSent = player.PlayerSocket.Send(data, 0, data.Length, SocketFlags.None);
                    Console.WriteLine($"Sent {bytesSent} bytes to {player.PlayerSocket.RemoteEndPoint.ToString()}.\n{message}\n");
                }
                // если оба игрока готовы.
                else if (secondPlayer.GameField != null && player.GameField != null)
                {
                    string message = AnswerHandler.GetGameReadyMessage(true);
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    int bytesSent = secondPlayer.PlayerSocket.Send(data, 0, data.Length, SocketFlags.None);
                    Console.WriteLine($"Sent {bytesSent} bytes to {secondPlayer.PlayerSocket.RemoteEndPoint.ToString()}.\n{message}\n");


                    message = AnswerHandler.GetGameReadyMessage(false);
                    data = Encoding.UTF8.GetBytes(message);
                    bytesSent = player.PlayerSocket.Send(data, 0, data.Length, SocketFlags.None);
                    Console.WriteLine($"Sent {bytesSent} bytes to {player.PlayerSocket.RemoteEndPoint.ToString()}.\n{message}\n");

                    game.WhoseTurn = secondPlayer;

                    // Ждать выстрела от player.
                    BeginReceive(secondPlayer.PlayerSocket);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                lock (sessions)
                {
                    sessions.Remove(game);
                }

                if (game != null)
                {
                    player?.PlayerSocket?.Close();
                    secondPlayer?.PlayerSocket?.Close();
                }

                handler?.Close();
            }
        }

        /// <summary>
        /// Добавить второго игрока в игру.
        /// </summary>
        /// <param name="beginGame">Информация о втором игроке и сессии к которой он хочет присоединиться.</param>
        private static void JoinTheGame(Socket handler, BeginGame beginGame)
        {
            try
            {
                GameSession game = null;
                lock (sessions)
                {
                    if (sessions.Any(x => x.SessionName.Equals(beginGame.GameName, StringComparison.OrdinalIgnoreCase)))
                    {
                        game = sessions.Find(x => x.SessionName == beginGame.GameName);
                        game.Player2 = new Player(handler, beginGame.PlayerName);
                    }
                }

                if (game?.GameStarted ?? false)
                {
                    SendOk(handler, false);
                    BeginReceive(handler);
                }
                else
                {
                    SendError(handler, true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                handler?.Close();
            }
        }

        /// <summary>
        /// Добавляет новую игру в sessions.
        /// </summary>
        /// <param name="beginGame">Информация о сессии, которую неоходимо добавить.</param>
        private static void AddGame(Socket handler, BeginGame beginGame)
        {
            bool exist = false;
            GameSession game = null;

            try
            {
                if (beginGame == null || beginGame?.GameName == null || beginGame?.PlayerName == null)
                {
                    SendError(handler, false);
                    return;
                }

                lock (sessions)
                {
                    exist = sessions.Count(x => x.SessionName.Equals(beginGame?.GameName, StringComparison.OrdinalIgnoreCase)) > 0 ? true : false;

                    if (!exist)
                    {
                        game = new GameSession(beginGame.GameName);
                        game.Player1 = new Player(handler, beginGame.PlayerName);
                        sessions.Add(game);
                    }
                }

                if (exist)
                {
                    SendError(handler, true);
                }
                else
                {
                    SendOk(handler, false);
                    BeginReceive(handler);
                }
            }
            catch (Exception e)
            {
                lock (sessions)
                {
                    sessions.Remove(game);
                }

                Console.WriteLine(e);
                handler?.Close();
            }
        }

        /// <summary>
        /// Получить доступные игры и отправить.
        /// </summary>
        private static void GiveGames(Socket handler)
        {
            try
            {
                List<BeginGame> bg = new List<BeginGame>();

                lock (sessions)
                {
                    foreach (var game in sessions)
                    {
                        if (!game.GameStarted)
                            bg.Add(new BeginGame() { GameName = game.SessionName, PlayerName = game.Player1?.Name ?? game.Player2.Name });
                    }
                }

                string data = AnswerHandler.GetGamesMessage(bg);
                byte[] byteData = Encoding.UTF8.GetBytes(data);
                Console.WriteLine($"Sending: {data}");
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                handler?.Close();
            }
        }

        /// <summary>
        /// Отправить сообщение о неудачном запросе.
        /// </summary>
        private static void SendError(Socket handler, bool closeSocket = false)
        {
            string data = AnswerHandler.GetErrorMessage();
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            Console.WriteLine($"Sending: {data}");

            if (closeSocket)
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
            else
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallbackSaveConnect), handler);
        }

        /// <summary>
        /// Отправить Ок.
        /// </summary>
        private static void SendOk(Socket handler, bool closeSocket = false)
        {
            string data = AnswerHandler.GetOkMessage();
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            Console.WriteLine($"Sending: {data}");

            if (closeSocket)
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
            else
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallbackSaveConnect), handler);
        }

        /// <summary>
        /// Вызывается после отправки сообщения. Закрывает соединение.
        /// </summary>
        private static void SendCallback(IAsyncResult ar)
        {
            Socket handler = null;
            try
            {
                handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.\n", bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                handler.Close();
            }
        }

        /// <summary>
        /// Вызывается после отправки сообщения. Не закрывает соединение.
        /// </summary>
        private static void SendCallbackSaveConnect(IAsyncResult ar)
        {
            Socket handler = null;
            try
            {
                handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                handler?.Close();
            }
        }

        /// <summary>
        /// Начать прослушку.
        /// </summary>
        /// <param name="socket"></param>
        private static void BeginReceive(Socket socket)
        {
            StateObject state = new StateObject() { workSocket = socket };
            socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), state);
        }

        /// <summary>
        /// Асинхронно отправляет данные на подключенный Socket. Оставляет соединение открытым.
        /// </summary>
        /// <param name="socket">Socket на который нужно отправить данные.</param>
        /// <param name="message">Сообщение.</param>
        private static void BeginSendSaveConnect(Socket socket, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallbackSaveConnect), socket);
        }

        /// <summary>
        /// Асинхронно отправляет данные на подключенный Socket. Закрывает соединение после отправки.
        /// </summary>
        /// <param name="socket">Socket на который нужно отправить данные.</param>
        /// <param name="message">Сообщение.</param>
        private static void BeginSend(Socket socket, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
        }

        static void Main(string[] args)
        {
            try
            {
                StartListening();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
