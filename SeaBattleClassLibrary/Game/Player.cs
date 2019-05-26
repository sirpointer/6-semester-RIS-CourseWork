using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace SeaBattleClassLibrary.Game
{
    [DebuggerDisplay("{IPEndPoint}, CanShot={CanShot}")]
    public class Player
    {
        public string Name { get; set; } = null;

        public Socket PlayerSocket { get; set; } = null;
        
        // The port number for the remote device.  
        private int port = 11000;

        public GameField GameField { get; set; } = null;

        public IPEndPoint IPEndPoint { get; set; } = null;

        public bool? CanShot { get; set; } = null;

        public Player()
        {
            PlayerSocket = GetSocket();
            GameField = new GameField();
        }

        public Player(Socket socket, string name)
        {
            PlayerSocket = socket;
            Name = name;
            IPEndPoint = (IPEndPoint)PlayerSocket.RemoteEndPoint;
        }

        public Player(GameField gameField)
        {
            GameField = gameField;
        }

        private Socket GetSocket()
        {
            return null;
            // Establish the remote endpoint for the socket.  
            // The name of the   
            // remote device is "host.contoso.com".  
            IPHostEntry ipHostInfo = Dns.GetHostEntry("192.168.43.221");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            return new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
