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
            
        }

        public string Source
        {
            get
            {
                switch (ShipClass)
                {
                case ShipClass.OneDeck:
                    return Orientation == Orientation.Horizontal ? "ms-appx:///Assets/Ships/1.jpg" : "ms-appx:///Assets/Ships/5.jpg";
                case ShipClass.TwoDeck:
                    return Orientation == Orientation.Horizontal ? "ms-appx:///Assets/Ships/2.jpg" : "ms-appx:///Assets/Ships/6.jpg";
                case ShipClass.ThreeDeck:
                    return Orientation == Orientation.Horizontal ? "ms-appx:///Assets/Ships/3.jpg" : "ms-appx:///Assets/Ships/7.jpg";
                case ShipClass.FourDeck:
                    return Orientation == Orientation.Horizontal ? "ms-appx:///Assets/Ships/4.jpg" : "ms-appx:///Assets/Ships/8.jpg";
                }

                return string.Empty;
            }
        }

    }
}
