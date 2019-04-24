using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleClassLibrary.Game
{
    [DataContract(Name = "gameField")]
    public class GameField
    {
        /// <summary>
        /// Попадания на поле (места куда уже нельзя бить). 
        /// </summary>
        [DataMember(Name = "hitsField")]
        public bool[,] HitsField = new bool[10, 10]; // Надо что-то по интереснее придумать, это как то тупо.

        /// <summary>
        /// Корабли на поле.
        /// </summary>
        [DataMember(Name = "ships")]
        public readonly ObservableCollection<Ship> Ships;


        public GameField()
        {
            List<Ship> ships = new List<Ship>(10);

            for (int i = 1; i <= 4; i++)
            {
                ships.Add(new Ship(i, ShipClass.OneDeck));
            }
            for (int i = 5; i <= 7; i++)
            {
                ships.Add(new Ship(i, ShipClass.TwoDeck));
            }
            for (int i = 8; i <= 9; i++)
            {
                ships.Add(new Ship(i, ShipClass.ThreeDeck));
            }
            ships.Add(new Ship(10, ShipClass.FourDeck));

            Ships = new ObservableCollection<Ship>(ships);
        }

        public GameField(List<Ship> ships)
        {
            if (ships == null || ships.Count != 10)
                throw new ArgumentException();
            if (ships.Count(x => x.ShipClass == ShipClass.OneDeck) != 4)
                throw new ArgumentException();
            if (ships.Count(x => x.ShipClass == ShipClass.TwoDeck) != 3)
                throw new ArgumentException();
            if (ships.Count(x => x.ShipClass == ShipClass.ThreeDeck) != 2)
                throw new ArgumentException();
            if (ships.Count(x => x.ShipClass == ShipClass.FourDeck) != 1)
                throw new ArgumentException();
            for (int i = 0; i < ships.Count; i++)
            {
                if (ships.Where(x => x.Id != ships[i].Id).Any(x => x.Id == ships[i].Id))
                    throw new ArgumentException();
            }

            Ships = new ObservableCollection<Ship>(ships);
        }


        public bool SetShipLocation(Ship ship, Location location)
        {
            bool locationSet = false;

            List<Ship> anotherShips = Ships.Where(x => x.Id != ship.Id) as List<Ship>;

            foreach (Ship another in anotherShips)
            {

            }

            throw new NotImplementedException();

            return locationSet;
        }
    }
}
