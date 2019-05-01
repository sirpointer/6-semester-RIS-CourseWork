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

        #region Set Ship Location

        /// <summary>
        /// Задает новое расположение кораблю, проверяя не накладывается ли он на другие.
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="location"></param>
        /// <returns>True если расположение задано, false если нет.</returns>
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

        /// <summary>
        /// Проверяет не накладывается ли <paramref name="targetShip"/> на <paramref name="anotherShip"/>.
        /// </summary>
        /// <param name="targetShip">Корабль, который следует расположить на поле.</param>
        /// <param name="anotherShip">Корабль, который уже лежит на поле.</param>
        /// <returns></returns>
        private bool CheckOverlay(Ship targetShip, Ship anotherShip)
        {
            int left = (anotherShip.Location.X - 1) >= 0 ? (anotherShip.Location.X - 1) : anotherShip.Location.X;
            int up = (anotherShip.Location.Y - 1) >= 0 ? (anotherShip.Location.Y - 1) : anotherShip.Location.Y;
            Location leftUp = new Location(left, up);

            int right = (anotherShip.RightLocation.X + 1) > 9 ? anotherShip.RightLocation.X : anotherShip.RightLocation.X + 1;
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

                while (targetLocation.Y <= endY)
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

        /// <summary>
        /// Проверяет не попадает ли <paramref name="targetLocation"/> на позиции в диапозоне 
        /// от <paramref name="leftUp"/> до <paramref name="right"/> и <paramref name="down"/>
        /// </summary>
        /// <param name="targetLocation"></param>
        /// <param name="leftUp">Левый верхняя позиция.</param>
        /// <param name="right">На сколько позиция лежит вправо.</param>
        /// <param name="down">На сколько позиция лежит вниз.</param>
        /// <returns>True если не попадает, false если попадает.</returns>
        private bool CheckOverlay(Location targetLocation, Location leftUp, int right, int down)
        {
            for (int x = leftUp.X; x <= right; x++)
            {
                for (int y = leftUp.Y; y <= down; y++)
                {
                    Location location = new Location(x, y, true);

                    if (location.IsUnset)
                        continue;

                    if (targetLocation.Equals(location))
                        return false;
                }
            }

            return true;
        }

        #endregion
    }
}
