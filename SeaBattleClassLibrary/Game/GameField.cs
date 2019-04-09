using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleClassLibrary.Game
{
    class GameField
    {
        /// <summary>
        /// Попадания на поле (места куда уже нельзя бить). 
        /// </summary>
        public bool[,] HitsField = new bool[10, 10];

        /// <summary>
        /// Корабли на поле.
        /// </summary>
        public List<Ship> Ships = new List<Ship>(10);
    }
}
