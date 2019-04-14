using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleClassLibrary.Game
{
    [DataContract(Name = "ship")]
    public class Ship : NotifyPropertyChanged
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
        public Orientation Orientation;

        /// <summary>
        /// Попадания по кораблю.
        /// </summary>
        [DataMember(Name = "hits")]
        public bool[] Hits;
        private Location _location = new Location();

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

        public Ship(int id, ShipClass shipClass = ShipClass.OneDeck, Orientation orientation = Orientation.Vertical)
        {
            Id = id;
            ShipClass = shipClass;
            Orientation = orientation;
            Hits = new bool[(int)ShipClass];
        }
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
        Horizontal,
        Vertical
    }

    [DataContract(Name = "location")]
    public class Location : NotifyPropertyChanged
    {
        private int x;
        private int y;

        /// <summary>
        /// Размер поля.
        /// </summary>
        [DataMember(Name = "size")]
        public readonly int Size;

        public Location(int x = 0, int y = 0, int size = 10)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));
            if (x > size || x < 0)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (y > size || y < 0)
                throw new ArgumentOutOfRangeException(nameof(y));

            Size = size;
            this.x = x;
            this.y = y;
        }

        [DataMember(Name = "x")]
        public int X
        {
            get => x;
            set
            {
                if (value > Size || value < 0)
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
                if (value > Size || value < 0)
                    throw new ArgumentOutOfRangeException(nameof(Y));

                y = value;
                OnPropertyChanged(nameof(Y));
            }
        }
    }
}
