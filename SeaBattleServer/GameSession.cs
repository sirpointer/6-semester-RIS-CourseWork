using SeaBattleClassLibrary.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleServer
{
    [DebuggerDisplay("{SessionName}\n{Player1}\n{Player2}")]
    internal class GameSession
    {
        /// <summary>
        /// Имя игровой сессии.
        /// </summary>
        public string SessionName { get; set; } = null;

        /// <summary>
        /// Игрок 1.
        /// </summary>
        public Player Player1 { get; set; } = null;

        /// <summary>
        /// Игрок 2.
        /// </summary>
        public Player Player2 { get; set; } = null;

        public GameSession(string sessionName)
        {
            SessionName = sessionName;
        }

        /// <summary>
        /// Чей ход.
        /// </summary>
        public Player WhoseTurn { get; set; } = null;

        /// <summary>
        /// Можно ли ходить или ход еще обрабатывается?
        /// </summary>
        public bool CanGo { get; set; } = true;

        /// <summary>
        /// Оба игрока в игре.
        /// </summary>
        public bool GameStarted => Player1 != null && Player2 != null ? true : false;
    }
}
