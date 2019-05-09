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
        public ClientShip(int id, ShipClass shipClass = ShipClass.OneDeck, Orientation orientation = Orientation.Horizontal, Location location = null) 
            : base(id, shipClass, orientation, location)
        {
            switch (shipClass)
            {
            case ShipClass.OneDeck:
                Source = orientation == Orientation.Horizontal ? "ms-appx:///Assets/Ships/1.jpg" : "ms-appx:///Assets/Ships/5.jpg";
                break;
            case ShipClass.TwoDeck:
                Source = orientation == Orientation.Horizontal ? "ms-appx:///Assets/Ships/2.jpg" : "ms-appx:///Assets/Ships/6.jpg";
                break;
            case ShipClass.ThreeDeck:
                Source = orientation == Orientation.Horizontal ? "ms-appx:///Assets/Ships/3.jpg" : "ms-appx:///Assets/Ships/7.jpg";
                break;
            case ShipClass.FourDeck:
                Source = orientation == Orientation.Horizontal ? "ms-appx:///Assets/Ships/4.jpg" : "ms-appx:///Assets/Ships/8.jpg";
                break;
            }
        }

        public string Source { get; }
    }
}
