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
    /// <summary>
    /// Structure used to hold data about blocks.
    /// </summary>
    public struct Chunk
    {
        //Types
        public Block[,] Blocks; //Array of blocks

        public int ChunkX; //Chunk's X coordinate
        public int ChunkY; //Chunk's Y coordinate

        private Game1 parent; //Chunk's parent (Game1)
        
        //Constructor
        public Chunk(int Width, int Height, int x, int y, Game1 p)
        {
            //Set everything as new
            Blocks = new Block[Width, Height];
            for(int i = 0; i < Width; i++)
            {
                for(int j = 0; j < Height; j++)
                {
                    Blocks[i, j] = new Block(0, 0, 5);
                }
            }
            ChunkX = x;
            ChunkY = y;
            parent = p;
        }

        /// <summary>
        /// Function required to set block based on local X and Y coordinates.
        /// </summary>
        /// <param name="X">Local X coordinate of block.</param>
        /// <param name="Y">Local Y coordinate of block.</param>
        /// <param name="To">ID of block to set to.</param>
        /// <param name="recalc">Should we recalculate entire chunk.</param>
        public void SetBlock(int X, int Y, int To, bool recalc = false)
        {
            //Did the chunk change
            bool Changed = false;

            //If we want to destroy block
            if(To == 0 && Blocks[X, Y].ID == 1)
            {
                //Lower its hardness level
                Blocks[X, Y].Hardness--;

                //If the block got destroyed
                if(Blocks[X, Y].Hardness == -1)
                {
                    //Destory it entirely and set that chunk has changed
                    Blocks[X, Y].ID = 0;
                    Changed = true;
                }
            }
            //Otherwise, if we want to set block
            else if(To == 1 && Blocks[X, Y].ID == 0)
            {
                //Set chunk and set that chunk has changed
                Blocks[X, Y].ID = To;
                Blocks[X, Y].Hardness = 5;
                Changed = true;
            }

            //If we should recalculate and chunk was changed
            if (recalc && Changed)
            {
                //Recaulculate our chunk
                Recalculate();

                //Check for nearest chunks : should they change
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

        /// <summary>
        /// Function to get block's ID based on local X and Y coordinates of block.
        /// </summary>
        /// <param name="X">Local X coordinates of block.</param>
        /// <param name="Y">Local Y coordinates of block.</param>
        /// <returns>Block's ID.</returns>
        public int GetBlock(int X, int Y)
        {
            //Return our block's id
            return Blocks[X, Y].ID;
        }

        /// <summary>
        /// Function to get block's state based on local X and Y coordinates of block.
        /// </summary>
        /// <param name="X">Local X coordinates of block.</param>
        /// <param name="Y">Local Y coordinates of block.</param>
        /// <returns>Block's state.</returns>
        public int GetBlockType(int X, int Y)
        {
            //Return our block's state
            return Blocks[X, Y].State;
        }

        /// <summary>
        /// Function to get block's health based on local X and Y coordinates of block.
        /// </summary>
        /// <param name="X">Local X coordinates of block.</param>
        /// <param name="Y">Local Y coordinates of block.</param>
        /// <returns>Block's health.</returns>
        public int GetBlockHealth(int X, int Y)
        {
            //Return our block's health
            return Blocks[X, Y].Hardness;
        }

        /// <summary>
        /// Function used to smooth out chunk using Cellular Automata algorithm.
        /// </summary>
        public void Smooth()
        {
            //Loop through all blocks
            for (int i = 0; i < WorldMetrics.ChunkSizeX; i++)
            {
                for (int j = 0; j < WorldMetrics.ChunkSizeY; j++)
                {
                    //Get count of solid adjacent blocks
                    int adjacentCount = parent.GetAdjacentCount((ChunkX * WorldMetrics.ChunkSizeX) + i, ((ChunkY * WorldMetrics.ChunkSizeY) + j));

                    //If they are more than 4 : our block should be solid too, otherwise - air.
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

        /// <summary>
        /// Function used to recalculate chunk's block states.
        /// </summary>
        public void Recalculate()
        {
            //Loop through all blocks
            for (int i = 0; i < WorldMetrics.ChunkSizeX; i++)
            {
                for (int j = 0; j < WorldMetrics.ChunkSizeY; j++)
                {
                    //If block is solid :
                    if (GetBlock(i, j) == 1)
                    {
                        //Calculate its state and set it
                        int state = parent.GetState((ChunkX * WorldMetrics.ChunkSizeX) + i, (ChunkY * WorldMetrics.ChunkSizeY) + j);
                        Blocks[i, j].State = state;
                    }
                }
            }
        }
    }

}
