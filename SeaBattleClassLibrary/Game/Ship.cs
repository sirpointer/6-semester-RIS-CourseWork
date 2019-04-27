using System;
using System.Linq;
using System.Runtime.Serialization;

namespace SeaBattleClassLibrary.Game
{
    [DataContract(Name = "ship")]
    public class Ship : NotifyPropertyChanged, ICloneable
    {
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
        /// Класс корабля.
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

        public Ship(int id, ShipClass shipClass = ShipClass.OneDeck, Orientation orientation = Orientation.Vertical, Location location = null)
        {
            Id = id;
            ShipClass = shipClass;
            Orientation = orientation;
            Hits = new bool[(int)ShipClass];
            _location = location ?? new Location();
        }

        public object Clone() => new Ship(Id, ShipClass, Orientation, Location.Clone() as Location);
    }

    public enum ShipClass
    {
        OneDeck = 1,
        TwoDeck = 2,
        ThreeDeck = 3,
        FourDeck = 4
    };

    public enum Orientation
    {
        Horizontal = 0,
        Vertical = 1
    }

    [DataContract(Name = "location")]
    public class Location : NotifyPropertyChanged, IEquatable<Location>, ICloneable
    {
        private int x;
        private int y;

        /// <summary>
        /// Размер поля.
        /// </summary>
        [DataMember(Name = "size")]
        public readonly int Size;

        /// <summary>
        /// Позиция корабля на поле.
        /// Если X или Y равно -1, то корабль не на поле.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        public Location(int x = -1, int y = -1)
        {
            Size = 10;

            if (x > Size || x < -1)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (y > Size || y < -1)
                throw new ArgumentOutOfRangeException(nameof(y));
            
            this.x = x;
            this.y = y;
        }

        [DataMember(Name = "x")]
        public int X
        {
            get => x;
            set
            {
                if (value > Size || value < -1)
                    throw new ArgumentOutOfRangeException(nameof(X));

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
                if (value > Size || value < -1)
                    throw new ArgumentOutOfRangeException(nameof(Y));

                y = value;
                OnPropertyChanged(nameof(Y));
            }
        }

        public bool IsUnset => (Y == -1 || X == -1) ? true : false;

        public bool IsSet => !IsUnset;


        public bool Equals(Location other) => (other.X == X && other.Y == Y) ? true : false;

        public object Clone() => new Location(X, Y);
    }
}
