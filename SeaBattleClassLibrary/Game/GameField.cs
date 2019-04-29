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
                if (ships.Count(x => x.Id == ships[i].Id) != 1)
                    throw new ArgumentException("У кораблей есть одинаковые id");
            }

            Ships = ships;
            Ships.Capacity = 10;
        }

        public bool SetShipLocation(Ship ship, Location location)
        {
            if (location.IsUnset)
                return false;

            Ship targetShip = ship.Clone() as Ship;
            targetShip.Location = location.Clone() as Location;

            if (!targetShip.IsSet)
                return false;

            List<Ship> anotherShips = Ships.Where(x => x.Id != ship.Id).ToList();
            
            foreach (Ship another in anotherShips)
            {
                if (!another.IsSet)
                    continue;

                bool overlay = !CheckOverlay(targetShip, another);

                if (overlay)
                    return false;
            }

            ship.Location = location;
            return true;
        }

        private bool CheckOverlay(Ship targetShip, Ship anotherShip)
        {
            Location leftUp = new Location(anotherShip.Location.X - 1, anotherShip.Location.Y - 1);
            int right = anotherShip.RightLocation.X + 1;
            int down = (anotherShip.DownLocation.Y + 1) > 9 ? anotherShip.DownLocation.Y : anotherShip.DownLocation.Y + 1;
            Location targetLocation = targetShip.Location.Clone() as Location;

            if (targetShip.Orientation == Orientation.Horizontal)
            {
                int endX = targetShip.RightLocation.X;

                while (targetLocation.X <= endX)
                {
                    bool overlay = !CheckOverlay(targetLocation, leftUp, right, down);

                    if (overlay)
                        return false;

                    if (!targetLocation.TrySet(targetLocation.X + 1, targetLocation.Y))
                        break;
                }
            }
            else
            {
                int endY = targetShip.DownLocation.Y;

                while (targetLocation.X <= endY)
                {
                    bool overlay = !CheckOverlay(targetLocation, leftUp, right, down);

                    if (overlay)
                        return false;

                    if (!targetLocation.TrySet(targetLocation.X, targetLocation.Y + 1))
                        break;
                }
            }

            return true;
        }

        private bool CheckOverlay(Location targetLocation, Location leftUp, int right, int down)
        {
            for (int x = leftUp.X; x <= leftUp.X + right; x++)
            {
                for (int y = leftUp.Y; y <= leftUp.Y + down; y++)
                {
                    if (targetLocation.Equals(new Location(x, y)))
                        return false;
                }
            }

            return true;
        }
    }
}
