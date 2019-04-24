using SeaBattleClassLibrary.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleServer
{
    internal class GameSession
    {
        public string SessionName { get; set; } = null;

        public Player Player1 { get; set; } = null;

        public Player Player2 { get; set; } = null;
    }
}
