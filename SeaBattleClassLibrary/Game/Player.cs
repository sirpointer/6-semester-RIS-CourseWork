using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SeaBattleClassLibrary.Game
{
    public class Player
    {
        public string Name { get; set; } = null;

        public Socket PlayerSocket { get; set; } = null;

        public GameField Field { get; set; } = null;
    }
}
