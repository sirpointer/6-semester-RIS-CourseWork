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
    public class GameField : NotifyPropertyChanged
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
        public readonly List<Ship> Ships;


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

            Ships = ships;
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
                    throw new ArgumentException("У кораблей есть одинаковые id");
            }

            Ships = ships;
            Ships.Capacity = 10;
        }

        public bool SetShipLocation(Ship ship, Location location)
        {
            if (location.IsUnset)
                return false;

            if (ship.Orientation == Orientation.Horizontal)
            {
                if (location.X + (int)ship.ShipClass >= location.Size)
                    return false;
            }
            else
            {
                if (location.Y + (int)ship.ShipClass >= location.Size)
                    return false;
            }

            return false;

            List<Ship> anotherShips = Ships.Where(x => x.Id != ship.Id) as List<Ship>;

            // Проверить. Лежит ли корабль на другом корабле или радом с ним.

            foreach (Ship another in anotherShips)
            {
                if (another.Location.IsUnset)
                    continue;

                Location pos = new Location(location.X, location.Y);

                if (another.Orientation == Orientation.Horizontal)
                {
                    bool overlay = !CheckHorizontalOverlay(ship.Clone() as Ship, another.Clone() as Ship);
                    if (overlay)
                        return false;
                }
            }

            ship.Location = location;

            throw new NotImplementedException();

            return true;
        }

        /// <summary>
        /// Проверяет наложение кораблей друг на друга, если <paramref name="anotherShip"/> расположен горизонтально.
        /// Метод НЕ менят позицию кораблей.
        /// </summary>
        /// <param name="targetShip">Корабль, который необходимо разместить.</param>
        /// <param name="anotherShip">Корабль который уже лежит на поле.</param>
        /// <returns>True если не накладывается, false если накладываются.</returns>
        private bool CheckHorizontalOverlay(Ship targetShip, Ship anotherShip)
        {
            bool targetShipIsValid = (targetShip.Location.X >= 0) && (targetShip.Location.Y >= 0)
                && (targetShip.Location.X + (int)targetShip.ShipClass <= 10);

            bool anotherShipIsValid = (anotherShip.Orientation == Orientation.Horizontal);

            if (!targetShipIsValid)
                return false;

            bool overlay = false;

            Location targetLocation = targetShip.Location.Clone() as Location;
            int endX = targetShip.Location.X + (int)targetShip.ShipClass;
            
            while (targetLocation.X <= endX)
            {
                for (int y = anotherShip.Location.Y - 1; y <= anotherShip.Location.Y + 1; y++)
                {
                    for (int x = anotherShip.Location.X - 1; x <= anotherShip.Location.X + (int)anotherShip.ShipClass + 1; x++)
                    {
                        if (targetLocation.Equals(new Location(x, y)))
                            return false;
                    }
                }

                targetLocation.X += 1;
            }

            return true;
        }
    }
}
