using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleClassLibrary.Game
{
    [DataContract(Name = "gameField")]
    class GameField
    {
        /// <summary>
        /// Попадания на поле (места куда уже нельзя бить). 
        /// </summary>
        [DataMember(Name = "hitsField")]
        public bool[,] HitsField = new bool[10, 10];

        /// <summary>
        /// Корабли на поле.
        /// </summary>
        [DataMember(Name = "ships")]
        public List<Ship> Ships = new List<Ship>(10);
    }
}
