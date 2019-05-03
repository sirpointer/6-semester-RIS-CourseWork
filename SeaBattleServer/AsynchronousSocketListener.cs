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

                /*foreach (var ip in ipHostInfo.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        Console.WriteLine(ip.ToString());
                    }
                }*/

               

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
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
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
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
              
                    content = content.Remove(content.LastIndexOf(JsonStructInfo.EndOfMessage));
                    Request.RequestTypes dataType = RequestHandler.GetRequestType(content);
                    string result = RequestHandler.GetJsonRequestResult(content);

                    switch (dataType)
                    {
                        case Request.RequestTypes.Ping:
                            SendOk(handler, true);
                            break;
                        case Request.RequestTypes.AddGame:
                            AddGame(handler, RequestHandler.GetAddGameResult(result));
                            break;
                        case Request.RequestTypes.Shot:
                            break;
                        case Request.RequestTypes.SetField:
                            break;
                        case Request.RequestTypes.BadRequest:
                            SendError(handler, true);
                            break;
                        case Request.RequestTypes.GetGames:
                            GiveGames(handler);
                            break;
                        case Request.RequestTypes.JoinTheGame:
                            break;
                    }

                    // Echo the data back to the client.  
                    //Send(handler, content);
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private static void JoinTheGame(Socket handler, BeginGame beginGame)
        {
            lock (sessions)
            {
                if (sessions.Any(x => x.SessionName.Equals(beginGame.GameName, StringComparison.OrdinalIgnoreCase)))
                {
                    GameSession game = sessions.Find(x => x.SessionName == beginGame.GameName);
                    game.Player2 = new Player(handler, beginGame.PlayerName);

                }
            }
        }

        /// <summary>
        /// Добавляет новую игру в sessions.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="beginGame"></param>
        private static void AddGame(Socket handler, BeginGame beginGame)
        {
            if (beginGame == null)
                SendError(handler, false);

            bool exist = false;

            lock (sessions)
            {
                exist = sessions.Count(x => x.SessionName.Equals(beginGame.GameName, StringComparison.OrdinalIgnoreCase)) > 0 ? true : false;

                if (!exist)
                {
                    GameSession game = new GameSession(beginGame.GameName);
                    game.Player1 = new Player(handler, beginGame.PlayerName);
                    sessions.Add(game);
                }
            }

            if (exist)
                SendError(handler, true);
            else
                SendOk(handler);
        }

        private static void GiveGames(Socket handler)
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
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        /// <summary>
        /// Отправить сообщение о неудачном запросе.
        /// </summary>
        private static void SendError(Socket handler, bool closeSocket = false)
        {
            string data = AnswerHandler.GetErrorMessage();
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            Thread.Sleep(5000);

            if (closeSocket)
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
            else
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallbackSaveConnect), handler);
        }

        private static void SendOk(Socket handler, bool closeSocket = false)
        {
            string data = AnswerHandler.GetOkMessage();
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            if (closeSocket)
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
            else
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallbackSaveConnect), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void SendCallbackSaveConnect(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            StartListening();
        }
    }
}
