using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SeaBattleClassLibrary.Game
{
    /// <summary>
    /// Представляет базовый класс для игрового поля. Является абстрактным.
    /// </summary>
    [DebuggerDisplay("{Ships[0]\nShips[1]\nShips[2]\nShips[3]\nShips[4]\n" +
        "Ships[5]\nShips[6]\nShips[7]\nShips[8]\nShips[9]\n}")]
    public abstract class Field : NotifyPropertyChanged
    {
        /// <summary>
        /// Попадания на поле (места куда уже нельзя бить).
        /// </summary>
        public bool[,] HitsField = new bool[10, 10];

        /// <summary>
        /// Корабли на поле.
        /// </summary>
        public readonly List<Ship> Ships;

        public Field(List<Ship> ships)
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

        /// <summary>
        /// Окончена ли игра.
        /// </summary>
        public bool IsGameOver
        {
            get
            {
                if (Ships.Count == 10)
                {
                    foreach (var ship in Ships)
                    {
                        if (!ship.IsDead)
                            return false;
                    }
                    return true;
                }
                return false;
            }
        }
        
        protected Field()
        {
            Ships = new List<Ship>(10);
        }
    }

    /// <summary>
    /// Представляет игровое поле, где известны все корабли.
    /// </summary>
    public class GameField : Field
    {
        public GameField() : base()
        {
            for (int i = 1; i <= 4; i++)
            {
                Ships.Add(new Ship(i, ShipClass.OneDeck));
            }
            for (int i = 5; i <= 7; i++)
            {
                Ships.Add(new Ship(i, ShipClass.TwoDeck));
            }
            for (int i = 8; i <= 9; i++)
            {
                Ships.Add(new Ship(i, ShipClass.ThreeDeck));
            }
            Ships.Add(new Ship(10, ShipClass.FourDeck));
        }

        public GameField(List<Ship> ships) : base(ships)
        {

        }

        /// <summary>
        /// Происходит, когда произведен выстрел по полю.
        /// </summary>
        public event EventHandler<ShotEventArgs> ShotMyField;

        private void OnShotMyField(object sender, ShotEventArgs e)
        {
            ShotMyField?.Invoke(sender, e);
        }

        /// <summary>
        /// Выстрел по полю.
        /// </summary>
        /// <param name="location"></param>
        /// <returns>Возвращается корабль, по которому попали. null если был промах.</returns>
        public Ship Shot(Location location)
        {
            if (location.IsUnset)
                throw new ArgumentException("По данной позиции нельзя стрелять", nameof(location));

            if (HitsField[location.X, location.Y])
                throw new ArgumentException("По данной позиции нельзя стрелять", nameof(location));

            Ship target = null;

            foreach (Ship ship in Ships)
            {
                if (ship.Shot(location))
                {
                    HitsField[location.X, location.Y] = true;

                    target = ship;
                    break;
                }
            }

            List<Location> hits = new List<Location>();
            ShotResult shotResult = ShotResult.Miss;


            if (target == null)
            {
                hits.Add(location.Clone() as Location);
            }
            else if (target.IsDead)
            {
                for (int x = target.Location.X - 1; x <= target.Location.X + target.ShipWidth; x++)
                {
                    for (int y = target.Location.Y - 1; y <= target.Location.Y + target.ShipHeight; y++)
                    {
                        if (x < 10 && y < 10 && x > -1 && y > -1)
                        {
                            hits.Add(new Location(x, y));
                        }
                    }
                }

                shotResult = ShotResult.Kill;
            } 
            else
            {
                hits.Add(location.Clone() as Location);
                shotResult = ShotResult.Damage;
            }

            OnShotMyField(this, new ShotEventArgs(hits, shotResult, target != null ? target.Clone() as Ship : null));

            return target;
        }

        #region Set Ship Location

        /// <summary>
        /// Задает новое расположение кораблю, проверяя не накладывается ли он на другие.
        /// </summary>
        /// <param name="ship">Корабль который необходимо установить.</param>
        /// <param name="location">Позиция куда должен встать корабль.</param>
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
        /// <returns>true - если не накладывется, иначе false.</returns>
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
        /// <param name="targetLocation">Позиция которую нужно проверить.</param>
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

    /// <summary>
    /// Представляет игровое поле, где корабли неизвестны.
    /// Вражеское поле.
    /// </summary>
    public class EnemyGameField : Field
    {
        /// <summary>
        /// Происходит, когда был произведен выстрел по полю.
        /// </summary>
        public event EventHandler<ShotEventArgs> EnemyShot;

        private void OnEnemyShot(object sender, ShotEventArgs e)
        {
            EnemyShot?.Invoke(sender, e);
        }

        public EnemyGameField() : base() { }

        /// <summary>
        /// Выстрел по полю.
        /// </summary>
        /// <param name="location">Позиция выстрела.</param>
        /// <param name="shotResult">Результат выстрела.</param>
        /// <param name="ship">Корабль по которому был произведен выстрел.</param>
        public void Shot(Location location, ShotResult shotResult, Ship ship)
        {
            List<Location> hits = new List<Location>();

            switch (shotResult)
            {
                case ShotResult.Miss:
                    hits.Add((Location)location.Clone());
                    break;
                case ShotResult.Damage:
                    hits.Add((Location)location.Clone());
                    break;
                case ShotResult.Kill:
                    for (int x = ship.Location.X - 1; x <= ship.Location.X + ship.ShipWidth; x++)
                    {
                        for (int y = ship.Location.Y - 1; y <= ship.Location.Y + ship.ShipHeight; y++)
                        {
                            if (x < 10 && y < 10 && x > -1 && y > -1)
                            {
                                hits.Add(new Location(x, y));
                            }
                        }
                    }

                    Ship s = (Ship)ship.Clone();
                    for (int i = 0; i < s.Hits.Length; i++)
                        s.Hits[i] = true;

                    Ships.Add(s);
                    break;
            }

            foreach (var hit in hits)
            {
                HitsField[hit.X, hit.Y] = true;
            }

            OnEnemyShot(this, new ShotEventArgs(hits, shotResult, ship));
        }
    }

    /// <summary>
    /// Представляет данные о выстреле.
    /// </summary>
    public class ShotEventArgs : EventArgs
    {
        public ShotEventArgs(List<Location> hits, ShotResult shotResult, Ship ship)
        {
            Ship = ship;
            Hits = hits;
            ShotResult = shotResult;
        }

        /// <summary>
        /// Позиции, которые затронул выстел.
        /// </summary>
        public List<Location> Hits { get; }
        
        /// <summary>
        /// Результат выстрела.
        /// </summary>
        public ShotResult ShotResult { get; }

        /// <summary>
        /// Корабль, по которому был произведен выстрел.
        /// </summary>
        public Ship Ship { get; }
    }

    /// <summary>
    /// Результат выстрела по полю.
    /// </summary>
    public enum ShotResult
    {
        /// <summary>
        /// Убил.
        /// </summary>
        Kill,

        /// <summary>
        /// Промах.
        /// </summary>
        Miss,

        /// <summary>
        /// Попал.
        /// </summary>
        Damage
    }

}
