using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleClassLibrary.Game
{
    public class Ship
    {
        /// <summary>
        /// Позиция корабля (левая верхняя клетка).
        /// </summary>
        public Location Location = new Location();

        /// <summary>
        /// Класс корабля.
        /// </summary>
        public ShipClass ShipClass;

        /// <summary>
        /// Ориентация корабля на поле.
        /// </summary>
        public Orientation Orientation;
    }

    public enum ShipClass
    {
        OneDeck = 1,
        TwoDeck = 2,
        ThreeDeck = 3,
        FourDeck = 4
    };

    public enum Orientation { Horizontal, Vertical }

    public class Location
    {
        private int x;
        private int y;

        /// <summary>
        /// Размер поля.
        /// </summary>
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

        public int X
        {
            get => x;
            set
            {
                if (value > Size || value < 0)
                    throw new ArgumentOutOfRangeException(nameof(X));

                x = value;
            }
        }

        public int Y
        {
            get => x;
            set
            {
                if (value > Size || value < 0)
                    throw new ArgumentOutOfRangeException(nameof(Y));

                y = value;
            }
        }
    }

}
