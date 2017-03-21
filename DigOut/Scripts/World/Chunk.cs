using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DigOut
{
    public struct Chunk
    {
        public Block[,] Blocks;
        public int ChunkX;
        public int ChunkY;
        private Game1 parent;
        public Chunk(int Width, int Height, int x, int y, Game1 p)
        {
            Blocks = new Block[Width, Height];
            for(int i = 0; i < Width; i++)
            {
                for(int j = 0; j < Height; j++)
                {
                    Blocks[i, j] = new Block();
                }
            }
            ChunkX = x;
            ChunkY = y;
            parent = p;
        }

        public void SetBlock(int X, int Y, int To, bool recalc = false)
        {
            Blocks[X, Y].ID = To;
            Blocks[X, Y].Hardness = 10;
            if (recalc)
            {
                Recalculate();
                if (X == 0)
                {
                    parent.RecalculateChunk(ChunkX - 1, ChunkY);
                }
                if (Y == 0)
                {
                    parent.RecalculateChunk(ChunkX, ChunkY - 1);
                }
                if (X == (WorldMetrics.ChunkSizeX - 1))
                {
                    parent.RecalculateChunk(ChunkX + 1, ChunkY);
                }
                if (Y == (WorldMetrics.ChunkSizeY - 1))
                {
                    parent.RecalculateChunk(ChunkX, ChunkY + 1);
                }
            }
        }

        public int GetBlock(int X, int Y) {
            return Blocks[X, Y].ID;
        }

        public int GetBlockType(int X, int Y)
        {
            return Blocks[X, Y].State;
        }

        public void Smooth()
        {
            for (int i = 0; i < WorldMetrics.ChunkSizeX; i++)
            {
                for (int j = 0; j < WorldMetrics.ChunkSizeY; j++)
                {
                    int adjacentCount = parent.GetAdjacentCount((ChunkX * WorldMetrics.ChunkSizeX) + i, ((ChunkY * WorldMetrics.ChunkSizeY) + j));
                    if (adjacentCount > 4)
                    {
                        Blocks[i, j].ID = 1;
                    }
                    else if (adjacentCount < 4)
                    {
                        Blocks[i, j].ID = 0;
                    }
                }
            }
        }

        public void Recalculate()
        {
            for (int i = 0; i < WorldMetrics.ChunkSizeX; i++)
            {
                for (int j = 0; j < WorldMetrics.ChunkSizeY; j++)
                {
                    if (GetBlock(i, j) == 1)
                    {
                        int state = parent.GetState((ChunkX * WorldMetrics.ChunkSizeX) + i, (ChunkY * WorldMetrics.ChunkSizeY) + j);
                        Blocks[i, j].State = state;
                    }
                }
            }
        }
    }

}
