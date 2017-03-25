using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
namespace DigOut
{
    public struct BlockData
    {
        public string Name;

        public int R, G, B;

        public int RarityL, RarityR; //0 - 1000
        public int GrowChance; //0-10
        public BlockData(string name, int r, int g, int b, int ral, int rar, int gc)
        {
            Name = name;
            R = r;
            G = g;
            B = b;
            RarityL = ral;
            RarityR = rar;
            GrowChance = gc;
        }

        public Color GetColor()
        {
            return new Color(R / 255f, G / 255f, B / 255f);
        }
    }
}
