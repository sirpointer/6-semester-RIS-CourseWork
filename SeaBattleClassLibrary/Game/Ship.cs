using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace SeaBattleClassLibrary.Game
{
    [DataContract(Name = "ship")]
    [DebuggerDisplay("X={Location.X} Y={Location.Y}, ShipClass={ShipClass}, Orientation={Orientation}, Hits={Hits}")]
    public class Ship : NotifyPropertyChanged, ICloneable
    {
        /// <summary>
        /// Уникальный идентификатор корабля на поле.
        /// </summary>
        [DataMember(Name = "id")]
        public readonly int Id;

        /// <summary>
        /// Позиция корабля (левая верхняя клетка).
        /// </summary>
        [DataMember(Name = "location")]
        public Location Location
        {
            get
            {
                return _location;
            }
            set
            {
                _location = value;
                OnPropertyChanged(nameof(Location));
            }
        }

        /// <summary>
        /// Класс (длина) корабля.
        /// </summary>
        [DataMember(Name = "shipClass")]
        public readonly ShipClass ShipClass;

        /// <summary>
        /// Ориентация корабля на поле.
        /// </summary>
        [DataMember(Name = "orientation")]
        public Orientation Orientation { get; set; }

        /// <summary>
        /// Попадания по кораблю.
        /// </summary>
        [DataMember(Name = "hits")]
        public bool[] Hits;

        private Location _location;

        /// <summary>
        /// Потоплен ли корабль.
        /// </summary>
        public bool IsDead
        {
            get
            {
                if (Hits.Count(x => x) == (int)ShipClass)
                    return true;
                else
                    return false;
            }
        }

        public Ship(int id, ShipClass shipClass = ShipClass.OneDeck, Orientation orientation = Orientation.Horizontal, Location location = null)
        {
            Id = id;
            ShipClass = shipClass;
            Orientation = orientation;
            Hits = new bool[(int)ShipClass];
            _location = location ?? new Location();
        }

        /// <summary>
        /// Находится ли корабль на поле.
        /// </summary>
        public bool IsSet
        {
            get
            {
                if (Location.IsUnset)
                    return false;

                if (Orientation == Orientation.Horizontal)
                {
                    if (Location.X + (int)ShipClass - 1 < Location.Size)
                        return true;
                }
                else
                {
                    if (Location.Y + (int)ShipClass - 1 < Location.Size)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Получить ширину, которую занимает корабль на поле.
        /// </summary>
        public int ShipWidth
        {
            get
            {
                return Orientation == Orientation.Horizontal ? (int)ShipClass : 1;
            }
        }

        /// <summary>
        /// Получить высоту, которую занимает корабль на поле.
        /// </summary>
        public int ShipHeight
        {
            get
            {
                return Orientation == Orientation.Vertical ? (int)ShipClass : 1;
            }
        }

        /// <summary>
        /// Получить крайнюю правую точку корабля на поле.
        /// </summary>
        public Location RightLocation
        {
            get
            {
                return Orientation == Orientation.Horizontal ? new Location(Location.X + ShipWidth - 1, Location.Y) : Location.Clone() as Location;
            }
        }

        /// <summary>
        /// Получить крайнюю нижнюю точку корабля на поле.
        /// </summary>
        public Location DownLocation
        {
            get
            {
                return Orientation == Orientation.Vertical ? new Location(Location.X, Location.Y + ShipHeight - 1) : Location.Clone() as Location;
            }
        }

        /// <summary>
        /// Лежит ли данная координата на корабле.
        /// </summary>
        public bool ContainLocation(Location location)
        {
            for (int x = Location.X; x <= RightLocation.X; x++)
            {
                for (int y = Location.Y; y <= DownLocation.Y; y++)
                {
                    if (location.Equals(new Location(x, y)))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Выстрел по кораблю.
        /// </summary>
        /// <returns>true если было попадание, false если нет.</returns>
        public bool Shot(Location location)
        {
            Location hitLocation = Location.Clone() as Location;

            for (int x = 0; x < ShipWidth && hitLocation.X + x < Location.Size; x++)
            {
                for (int y = 0; y < ShipHeight && hitLocation.Y + y < Location.Size; y++)
                {
                    hitLocation.X = Location.X + x;
                    hitLocation.Y = Location.Y + y;

                    if (hitLocation.Equals(location))
                    {
                        int pos = x != 0 ? x : y;
                        Hits[pos] = true;
                        return true;
                    }
                }
            }

            return false;
        }

        public object Clone() => new Ship(Id, ShipClass, Orientation, Location.Clone() as Location);
    }

    /// <summary>
    /// Класс (длина) корабля.
    /// </summary>
    public enum ShipClass
    {
        /// <summary>
        /// Однопалубный.
        /// </summary>
        OneDeck = 1,

        /// <summary>
        /// Двухпалубный.
        /// </summary>
        TwoDeck = 2,

        /// <summary>
        /// Трехпалубный.
        /// </summary>
        ThreeDeck = 3,

        /// <summary>
        /// Четырехпалубный.
        /// </summary>
        FourDeck = 4
    }

    /// <summary>
    /// Ориентация корабля на поле.
    /// </summary>
    public enum Orientation
    {
        Horizontal = 0,
        Vertical = 1
    }

    /// <summary>
    /// Предствляет позицию корабля на поле.
    /// </summary>
    [DataContract(Name = "location")]
    [DebuggerDisplay("X={X}, Y={Y}")]
    public class Location : NotifyPropertyChanged, IEquatable<Location>, ICloneable
    {
        private int x;
        private int y;

        /// <summary>
        /// Размер поля.
        /// </summary>
        [DataMember(Name = "size")]
        public const int Size = 10;

        /// <summary>
        /// Позиция корабля на поле.
        /// Если X или Y равно -1, то корабль не на поле.
        /// </summary>
        /// <param name="x">X.</param>
        /// <param name="y">Y.</param>
        /// <param name="check">Если координаты могут выходить за допустимые пределы, то они станут равны -1.</param>
        public Location(int x = -1, int y = -1, bool check = false)
        {
            if (x > Size || x < -1)
            {
                if (check)
                    x = -1;
                else
                    throw new ArgumentOutOfRangeException(nameof(x), $"Значение должно лежать в диапозоне от -1 до {Size}");
            }
                
            if (y > Size || y < -1)
            {
                if (check)
                    y = -1;
                else
                    throw new ArgumentOutOfRangeException(nameof(y), $"Значение должно лежать в диапозоне от -1 до {Size}");
            }
            
            this.x = x;
            this.y = y;
        }

        [DataMember(Name = "x")]
        public int X
        {
            get => x;
            set
            {
                if (value >= Size || value < -1)
                    throw new ArgumentOutOfRangeException(nameof(X), $"Значение должно лежать в диапозоне от -1 до {Size}");

                x = value;
                OnPropertyChanged(nameof(X));
            }
        }

        [DataMember(Name = "y")]
        public int Y
        {
            get => y;
            set
            {
                if (value >= Size || value < -1)
                    throw new ArgumentOutOfRangeException(nameof(Y), $"Значение должно лежать в диапозоне от -1 до {Size}");

                y = value;
                OnPropertyChanged(nameof(Y));
            }
        }

        /// <summary>
        /// Корабль вне поля.
        /// </summary>
        public bool IsUnset => (Y == -1 || X == -1) ? true : false;

        /// <summary>
        /// Корабль на поле.
        /// </summary>
        public bool IsSet => !IsUnset;

        /// <summary>
        /// Попытка задать координаты корабля.
        /// </summary>
        /// <param name="x">X.</param>
        /// <param name="y">Y.</param>
        /// <returns>Если координаты вне допустимого диапозона, то они не задаются и возвращается false.</returns>
        public bool TrySet(int x, int y)
        {
            if (x >= -1 && x < Size && y >= -1 && y < Size)
            {
                X = x;
                Y = y;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Equals(Location other) => (IsSet && other.IsSet && other.X == X && other.Y == Y);

        public object Clone() => new Location(X, Y);
    }
}
