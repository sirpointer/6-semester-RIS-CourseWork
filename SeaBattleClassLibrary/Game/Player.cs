using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace SeaBattleClassLibrary.Game
{
    /// <summary>
    /// Представляет информацию об игроке.
    /// </summary>
    [DebuggerDisplay("{IPEndPoint}, CanShot={CanShot}")]
    public class Player
    {
        /// <summary>
        /// Имя игрока.
        /// </summary>
        public string Name { get; set; } = null;

        /// <summary>
        /// Подключенный сокет.
        /// </summary>
        public Socket PlayerSocket { get; set; } = null;
        
        /// <summary>
        /// Порт сервера.
        /// </summary>
        private const int port = 11000;

        /// <summary>
        /// Игровое поле игрока.
        /// </summary>
        public GameField GameField { get; set; } = null;

        /// <summary>
        /// Remote endpoint.
        /// </summary>
        public IPEndPoint IPEndPoint { get; set; } = null;

        /// <summary>
        /// Может ли стрелять.
        /// </summary>
        public bool? CanShot { get; set; } = null;

        public Player()
        {
            PlayerSocket = null;
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
    }
}
