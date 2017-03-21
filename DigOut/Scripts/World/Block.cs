using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigOut
{
    public struct Block
    {
        public int ID;
        public int State;

        public int Hardness;

        public Block(int iD, int state, int hardness)
        {
            ID = iD;
            State = state;
            Hardness = hardness;
        }
    }
}
