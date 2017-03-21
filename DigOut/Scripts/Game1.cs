using Math = System.Math;
using System.Collections;
using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
using Random = System.Random;
namespace DigOut
{
    public class Game1 : Game
    {
        //Types
        public GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int toDrawChunksThreshold = 4; //How much chunks to display near camera
        //const int chunkDisplayThreshold = 30;

        Texture2D[] GroundTextures; //Ground textures (15 states)
        Texture2D[] BreakTextures; //Damage textures (5 states)
        Texture2D Cursor; //Cursor texture
        public Texture2D Player; //Player texture

        FrameCounter FPSCounter = new FrameCounter(); //FPS counter class
        SpriteFont defaultFont; //Font reference

        Vector2 mousePosition; //Mouse position
        Vector2 CameraPosition; //Camera position
        Vector2 mouseCameraPosition; //Mouse position (World)

        int previousScrollValue; //Prev. scroll value
        public Player player; //Player reference

        string seed = "Abra"; //Current world seed
        public Chunk[,] Chunks; //Chunks array




        public Game1()
        {
            //Initialize game
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 768;

            graphics.PreferredBackBufferWidth = 1366;
            graphics.PreferMultiSampling = false;
            //graphics.SynchronizeWithVerticalRetrace = false;
            //IsFixedTimeStep = false;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            //Create sprite batch
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            //Create chunks
            Chunks = new Chunk[WorldMetrics.ChunkCountX, WorldMetrics.ChunkCountY];

            //Load ground and damage textures
            GroundTextures = new Texture2D[16];
            BreakTextures = new Texture2D[5];
            for (int i = 0; i < 16; i++)
            {
                GroundTextures[i] = Content.Load<Texture2D>("Environment/Blocks/Terrain/" + i.ToString());
            }
            for(int i = 1; i <= 5; i++)
            {
                BreakTextures[i - 1] = Content.Load<Texture2D>("Environment/Blocks/Damage/" + i.ToString());
            }

            //Load other textures
            Player = Content.Load<Texture2D>("Environment/Blocks/Terrain/15");
            defaultFont = Content.Load<SpriteFont>("Fonts/Arial");
            player = new Player();
            Cursor = Content.Load<Texture2D>("UI/Cursor");

            //Generate world :
            //Create random instance
            Random r = new Random(seed.GetHashCode());
            for (int ChunkX = 0; ChunkX < WorldMetrics.ChunkCountX; ChunkX++)
            {
                for (int ChunkY = 0; ChunkY < WorldMetrics.ChunkCountY; ChunkY++)
                {
                    //Loop through all chunks
                    Chunks[ChunkX, ChunkY] = new Chunk(WorldMetrics.ChunkSizeX, WorldMetrics.ChunkSizeY, ChunkX, ChunkY, this);
                    for(int x = 0; x < WorldMetrics.ChunkSizeX; x++)
                    {
                        for(int y = 0; y < WorldMetrics.ChunkSizeY; y++)
                        {
                            //Loop through all blocks and randomize thems
                            Chunks[ChunkX, ChunkY].SetBlock(x, y, (r.Next(0, 100) > 45) ? 1 : 0);
                            if(Chunks[ChunkX, ChunkY].GetBlock(x, y) == 0)
                            {
                                CameraPosition = new Vector2(ChunkX * WorldMetrics.ChunkSizeX + x, ChunkY * WorldMetrics.ChunkSizeY + y);
                            }
                        }
                    }
                }
            }

            //Loop through all chunks and smooth them using Cellular Automata
            for (int x = 0; x < WorldMetrics.ChunkCountX; x++)
            {
                for (int y = 0; y < WorldMetrics.ChunkCountY; y++)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Chunks[x, y].Smooth();
                    }
                }
            }

            //Recalculate block states
            for (int x = 0; x < WorldMetrics.ChunkCountX; x++)
            {
                for (int y = 0; y < WorldMetrics.ChunkCountY; y++)
                {
                    Chunks[x, y].Recalculate();
                }
            }

        }

        protected override void UnloadContent()
        {

        }
        
        //Tool options
        float ToolSpeed = 0.05f;
        float PreviousBreakTime = 0f;
        float PreviousPlaceTime = 0f;


        protected override void Update(GameTime gameTime)
        {
            //If we need to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //Get mouse state and position
            var mouseState = Mouse.GetState();
            mousePosition = new Vector2(mouseState.X, mouseState.Y);

            //Calculate world mouse position
            mouseCameraPosition = mousePosition + CameraPosition;
            float Time = (float)gameTime.TotalGameTime.TotalSeconds;

            //If LMB is pressed : place block
            if (mouseState.LeftButton == ButtonState.Pressed)
            { 
                //If distance is OK
                if (Math.Round(Vector2.DistanceSquared(mouseCameraPosition, PlayerPositionWorld()) / WorldMetrics.SpriteSize) < 400f)
                {
                    //Calculate block position
                    int X = (int)mouseCameraPosition.X / WorldMetrics.SpriteSize;
                    int Y = (int)mouseCameraPosition.Y / WorldMetrics.SpriteSize;
                    
                    //Check for availability
                    if (X >= 0 && Y >= 0 && X < WorldMetrics.ChunkCountX * WorldMetrics.ChunkSizeX && Y < WorldMetrics.ChunkCountY * WorldMetrics.ChunkSizeY)
                    {
                        //Check for time
                        if (Time - PreviousPlaceTime >= ToolSpeed)
                        {
                            //Set our block
                            SetBlock(X, Y, 1);
                            PreviousPlaceTime = Time;
                        }
                    }
                }
            }

            //If RMB is pressed : destroy block
            if (mouseState.RightButton == ButtonState.Pressed)
            {
                //If distance is OK
                if (Math.Round(Vector2.DistanceSquared(mouseCameraPosition, PlayerPositionWorld()) / WorldMetrics.SpriteSize) < 400f)
                {
                    //Calculate block position
                    int X = (int)mouseCameraPosition.X / WorldMetrics.SpriteSize;
                    int Y = (int)mouseCameraPosition.Y / WorldMetrics.SpriteSize;

                    //Check for availability
                    if (X >= 0 && Y >= 0 && X < WorldMetrics.ChunkCountX * WorldMetrics.ChunkSizeX && Y < WorldMetrics.ChunkCountY * WorldMetrics.ChunkSizeY)
                    {
                        //Check for time 
                        if (Time - PreviousBreakTime >= ToolSpeed)
                        {
                            //Set our block
                            SetBlock(X, Y, 0);
                            PreviousBreakTime = Time;
                        }
                    }
                 }
            }

            //Get keyboard state and calculate player's velocity based on keyboard input
            var keyboardState = Keyboard.GetState();
            player.HandleKeyboardInput(keyboardState, CameraPosition, this);

            //Calculate camera position based on velocity
            CameraPosition += player.PlayerVelocity;

            //if(previousScrollValue > mouseState.ScrollWheelValue)
            //{
            //    WorldMetrics.SpriteSize--;
            //}
            //else if (previousScrollValue < mouseState.ScrollWheelValue)
            //{
            //    WorldMetrics.SpriteSize++;
            //}

            //Set scroll value
            previousScrollValue = mouseState.ScrollWheelValue;

            //Count FPS
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            FPSCounter.Update(deltaTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //Clear our background
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Begin sprite batch : We need to clamp as point and blend with alpha
            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);

            //Draw chunks based on current camera's position clamped to chunk grid
            int xGrid = (int)Math.Ceiling((CameraPosition.X / (float)WorldMetrics.ChunkSizeX) / (float)WorldMetrics.SpriteSize);
            int yGrid = (int)Math.Ceiling((CameraPosition.Y / (float)WorldMetrics.ChunkSizeY) / (float)WorldMetrics.SpriteSize);
            

            //Loop through all nearest chunks
            for (int xChunk = xGrid - toDrawChunksThreshold; xChunk <= xGrid + toDrawChunksThreshold; xChunk++)
            {
                for (int yChunk = yGrid - toDrawChunksThreshold; yChunk <= yGrid + toDrawChunksThreshold; yChunk++)
                {
                    //Check for chunk availability
                    if (xChunk < 0 || yChunk < 0 || xChunk >= WorldMetrics.ChunkCountX || yChunk >= WorldMetrics.ChunkCountY)
                    {
                        continue;
                    }
                    
                    //Loop through chunk's blocks
                    for (int x = 0; x < WorldMetrics.ChunkSizeX; x++)
                    {
                        for (int y = 0; y < WorldMetrics.ChunkSizeY; y++)
                        {
                            //Calculate block's position
                            int blockX = xChunk * WorldMetrics.ChunkSizeX + x;
                            int blockY = yChunk * WorldMetrics.ChunkSizeY + y;

                            //If block is solid (Not air)
                            if (GetBlock(blockX, blockY) == 1)
                            {
                                //Draw it
                                spriteBatch.Draw(GroundTextures[Chunks[xChunk, yChunk].GetBlockType(x, y)], new Vector2(blockX * WorldMetrics.SpriteSize, blockY * WorldMetrics.SpriteSize) - CameraPosition, null, Color.White, 0f, Vector2.Zero, WorldMetrics.SpriteSize / 16f, SpriteEffects.None, 0f);

                                //Get health
                                int health = Chunks[xChunk, yChunk].GetBlockHealth(x, y);
                                
                                //If block is damaged : display damage
                                if (health < 5)
                                {
                                    spriteBatch.Draw(BreakTextures[health], new Vector2(blockX * WorldMetrics.SpriteSize, blockY * WorldMetrics.SpriteSize) - CameraPosition, null, Color.White, 0f, Vector2.Zero, WorldMetrics.SpriteSize / 16f, SpriteEffects.None, 0f);
                                }
                            }
                        }
                    }
                        
                }
            }

            //Draw cursor
            spriteBatch.Draw(Cursor, new Vector2(mousePosition.X - 16, mousePosition.Y - 16), Color.White);

            //Draw player
            spriteBatch.Draw(Player, player.Position, Color.Pink);
            
            //Format our fps and draw it
            var fps = string.Format("FPS : {0}", FPSCounter.AverageFramesPerSecond);
            spriteBatch.DrawString(defaultFont, fps, new Vector2(0, 0), Color.Black);

            //Close sprite batch
            spriteBatch.End();
            base.Draw(gameTime);
        }

        //CHUNK AND BLOCK OPERATIONS

        /// <summary>
        /// Get block's texture state (0 - 15).
        /// </summary>
        /// <param name="x">World X position of the block.</param>
        /// <param name="y">World Y position of the block.</param>
        /// <returns>State of the block (0 - 15).</returns>
        public int GetState(int x, int y)
        {
            //Initialize state
            int state = 0;

            //If there is block on the left : add 1
            if (GetBlock(x - 1, y) == 1) { state += 1; }

            //If there is block on the right : add 4
            if (GetBlock(x + 1, y) == 1) { state += 4; }

            //If there is block on the top : add 2
            if (GetBlock(x, y - 1) == 1) { state += 2; }

            //If there is block on the bottom : add 8
            if (GetBlock(x, y + 1) == 1) { state += 8; }

            //Return state
            return state;
        }

        /// <summary>
        /// Get near blocks that are not air (0 - 8).
        /// </summary>
        /// <param name="X">World X position of the block.</param>
        /// <param name="Y">World Y position of the block.</param>
        /// <returns>Count of adjacent blocks that are not air (0 - 8).</returns>
        public int GetAdjacentCount(int X, int Y)
        {
            //Initialize counter
            int count = 0;

            //Loop through adjacent blocks
            for (int x = X - 1; x <= X + 1; x++)
            {
                for (int y = Y - 1; y <= Y + 1; y++)
                {
                    //If it's this block itself : go to next block
                    if (x == X && y == Y) { continue; }

                    //If block is not air : add our counter
                    count += GetBlock(x, y);
                }
            }
            
            //Return counter
            return count;
        }

        /// <summary>
        /// Get block's ID based on world X and Y coords.
        /// </summary>
        /// <param name="X">World X position of the block.</param>
        /// <param name="Y">World Y position of the block.</param>
        /// <returns>Block's ID</returns>
        public int GetBlock(int x, int y)
        {
            //Get chunk coords
            int chunkX = x / WorldMetrics.ChunkSizeX;
            int chunkY = y / WorldMetrics.ChunkSizeY;

            //Get block coords
            int blockX = x % WorldMetrics.ChunkSizeX;
            int blockY = y % WorldMetrics.ChunkSizeY;

            //If it's out of bounds return 1
            if (chunkX >= WorldMetrics.ChunkCountX || chunkX < 0)
            {
                return 1;
            }
            if (chunkY >= WorldMetrics.ChunkCountY || chunkY < 0)
            {
                return 1;
            }
            if (blockX >= WorldMetrics.ChunkSizeX || blockX < 0)
            {
                return 1;
            }
            if (blockY >= WorldMetrics.ChunkSizeY || blockY < 0)
            {
                return 1;
            }

            //Otherwise return this block
            return Chunks[chunkX, chunkY].GetBlock(blockX, blockY);
        }

        /// <summary>
        /// Set block based on world X and Y coordinates
        /// </summary>
        /// <param name="X">World X position of the block.</param>
        /// <param name="Y">World Y position of the block.</param>
        /// <param name="to">Block ID to set to</param>
        public void SetBlock(int x, int y, int to)
        {
            //Get chunk coords
            int chunkX = x / WorldMetrics.ChunkSizeX;
            int chunkY = y / WorldMetrics.ChunkSizeY;

            //Get block coords
            int blockX = x % WorldMetrics.ChunkSizeX;
            int blockY = y % WorldMetrics.ChunkSizeY;


            //If it's out of bounds : do nothing
            if (chunkX >= WorldMetrics.ChunkCountX || chunkX < 0)
            {
                return;
            }
            if (chunkY >= WorldMetrics.ChunkCountY || chunkY < 0)
            {
                return;
            }

            //Otherwise set the block, and update states
            Chunks[chunkX, chunkY].SetBlock(blockX, blockY, to, true);

        }

        /// <summary>
        /// Function to recalculate chunk.
        /// </summary>
        /// <param name="X">X position of the chunk.</param>
        /// <param name="Y">Y position of the chunk.</param>
        public void RecalculateChunk(int x, int y)
        {
            //If chunk is out of bounds : return
            if(x == -1 || y == -1 || x == WorldMetrics.ChunkCountX || y == WorldMetrics.ChunkCountY)
            {
                return;
            }

            //Otherwise recalculate chunk
            Chunks[x, y].Recalculate();
        }

        //PLAYER OPERATIONS
        public Vector2 PlayerPositionWorld()
        {
            //Return world player position (Screen)
            return CameraPosition + player.Position;
        }
        
    }

    /// <summary>
    /// Structure to hold 2 integers at once (Can be used as integer variant of Vector).
    /// </summary>
    public struct VectorInt
    {
        //X and Y integers, mainly used to determine coordinates
        public int X;
        public int Y;

        //Constructor
        public VectorInt(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// Class that determines world settings
    /// </summary>
    public static class WorldMetrics
    {
        //Chunk sizes (In blocks)
        public static int ChunkSizeX = 16;
        public static int ChunkSizeY = 16;

        //Chunk count
        public static int ChunkCountX = 32;
        public static int ChunkCountY = 32;

        //Sprite size (In pixels)
        public static int SpriteSize = 32;

        //PLANET SETTINGS
        public static float GravityAcceleration = 0.1f; //const g - acceleration
    }

    //EXTENSIONS
    public class FrameCounter
    {
        public FrameCounter()
        {
        }

        public long TotalFrames { get; private set; }
        public float TotalSeconds { get; private set; }
        public float AverageFramesPerSecond { get; private set; }
        public float CurrentFramesPerSecond { get; private set; }

        public const int MAXIMUM_SAMPLES = 100;

        private Queue<float> _sampleBuffer = new Queue<float>();

        public bool Update(float deltaTime)
        {
            CurrentFramesPerSecond = 1.0f / deltaTime;

            _sampleBuffer.Enqueue(CurrentFramesPerSecond);

            if (_sampleBuffer.Count > MAXIMUM_SAMPLES)
            {
                _sampleBuffer.Dequeue();
                AverageFramesPerSecond = _sampleBuffer.Average(i => i);
            }
            else
            {
                AverageFramesPerSecond = CurrentFramesPerSecond;
            }

            TotalFrames++;
            TotalSeconds += deltaTime;
            return true;
        }
    }
}
