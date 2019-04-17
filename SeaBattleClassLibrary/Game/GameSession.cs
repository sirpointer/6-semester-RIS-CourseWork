using System;
using System.Net;

namespace SeaBattleClassLibrary.Game
{
    public class GameSession
    {
        private bool player1Turn = true;

        /// <summary>
        /// Имя игровой сесии.
        /// </summary>
        public string GameName { get; set; } = null;

        public GameField Field1 { get; set; } = new GameField();

        public GameField Field2 { get; set; } = new GameField();

        public PlayerInfo Player1 { get; set; } = null;

        public PlayerInfo Player2 { get; set; } = null;

        /// <summary>
        /// Чей ход.
        /// </summary>
        public PlayerInfo WhoseTurn
        {
            get
            {
                bool turn = player1Turn;
                player1Turn = !player1Turn;
                return turn ? Player1 : Player2;
            }
        }
    }

    public class PlayerInfo : IEquatable<PlayerInfo>
    {
        public IPEndPoint IP { get; set; } = null; // RemoteEndPoint

        public string Name { get; set; } = null;

        public bool Equals(PlayerInfo other)
        {
            if (IP == null || Name == null)
                return false;

            bool ip = IP.Address == other.IP.Address && IP.Port == other.IP.Port;
            bool name = Name.Equals(other.Name, StringComparison.Ordinal);

            return ip && name;
        }
    }
}
