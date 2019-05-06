using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeaBattleClassLibrary.Game;

namespace SeaBattleClient
{
    class ClientShip : SeaBattleClassLibrary.Game.Ship
    {
        public ClientShip(int id, ShipClass shipClass = ShipClass.OneDeck, Orientation orientation = Orientation.Horizontal, Location location = null) : base(id, shipClass, orientation, location)
        {
        }
    }
}
