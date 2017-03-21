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
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int toDrawChunksThreshold = 4;
        const int chunkDisplayThreshold = 30;

        Texture2D[] GroundTextures;
        Texture2D[] BreakTextures;
        Texture2D Cursor;
        Texture2D Player;

        FrameCounter FPSCounter = new FrameCounter();
        SpriteFont defaultFont;

        Vector2 mousePosition;
        Vector2 CameraPosition;
        Vector2 mouseCameraPosition;

        int previousScrollValue;

        string seed = "Abra";
        public Chunk[,] Chunks;


        Vector2 PlayerPosition = new Vector2(0, 0);
        float PlayerSpeed = 2f;


        public Game1()
        {
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
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Chunks = new Chunk[WorldMetrics.ChunkCountX, WorldMetrics.ChunkCountY];

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

            Player = Content.Load<Texture2D>("Environment/Blocks/Terrain/15");
            defaultFont = Content.Load<SpriteFont>("Fonts/Arial");

            Random r = new Random(seed.GetHashCode());
            for (int ChunkX = 0; ChunkX < WorldMetrics.ChunkCountX; ChunkX++)
            {
                for (int ChunkY = 0; ChunkY < WorldMetrics.ChunkCountY; ChunkY++)
                {
                    Chunks[ChunkX, ChunkY] = new Chunk(WorldMetrics.ChunkSizeX, WorldMetrics.ChunkSizeY, ChunkX, ChunkY, this);
                    for(int x = 0; x < WorldMetrics.ChunkSizeX; x++)
                    {
                        for(int y = 0; y < WorldMetrics.ChunkSizeY; y++)
                        {
                            Chunks[ChunkX, ChunkY].SetBlock(x, y, (r.Next(0, 100) > 45) ? 1 : 0);
                            if(Chunks[ChunkX, ChunkY].GetBlock(x, y) == 0)
                            {
                                PlayerPosition = new Vector2(ChunkX * WorldMetrics.ChunkSizeX + x, ChunkY * WorldMetrics.ChunkSizeY + y);
                            }
                        }
                    }
                }
            }

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

            for (int x = 0; x < WorldMetrics.ChunkCountX; x++)
            {
                for (int y = 0; y < WorldMetrics.ChunkCountY; y++)
                {
                    Chunks[x, y].Recalculate();
                }
            }

            Cursor = Content.Load<Texture2D>("UI/Cursor");
        }

        protected override void UnloadContent()
        {

        }

        float ToolSpeed = 0.05f;
        float PreviousBreakTime = 0f;
        float PreviousPlaceTime = 0f;

        Vector2 PlayerVelocity;
        bool Landed = true;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            var mouseState = Mouse.GetState();
            mousePosition = new Vector2(mouseState.X, mouseState.Y);

            mouseCameraPosition = mousePosition + CameraPosition;
            float Time = (float)gameTime.TotalGameTime.TotalSeconds;
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (Math.Round(Vector2.DistanceSquared(mouseCameraPosition, PlayerPositionWorld()) / WorldMetrics.SpriteSize) < 400f)
                {
                    int X = (int)mouseCameraPosition.X / WorldMetrics.SpriteSize;
                    int Y = (int)mouseCameraPosition.Y / WorldMetrics.SpriteSize;
                    if (X >= 0 && Y >= 0 && X < WorldMetrics.ChunkCountX * WorldMetrics.ChunkSizeX && Y < WorldMetrics.ChunkCountY * WorldMetrics.ChunkSizeY)
                    {
                        if (Time - PreviousPlaceTime >= ToolSpeed)
                        {
                            SetBlock(X, Y, 1);
                            PreviousPlaceTime = Time;
                        }
                    }
                }
            }
            if (mouseState.RightButton == ButtonState.Pressed)
            {
                if (Math.Round(Vector2.DistanceSquared(mouseCameraPosition, PlayerPositionWorld()) / WorldMetrics.SpriteSize) < 400f)
                {
                    int X = (int)mouseCameraPosition.X / WorldMetrics.SpriteSize;
                    int Y = (int)mouseCameraPosition.Y / WorldMetrics.SpriteSize;
                    if (X >= 0 && Y >= 0 && X < WorldMetrics.ChunkCountX * WorldMetrics.ChunkSizeX && Y < WorldMetrics.ChunkCountY * WorldMetrics.ChunkSizeY)
                    {
                        if (Time - PreviousBreakTime >= ToolSpeed)
                        {
                            SetBlock(X, Y, 0);
                            PreviousBreakTime = Time;
                        }
                    }
                }
            }

            var keyboardState = Keyboard.GetState();

            PlayerPosition = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
            if (keyboardState.IsKeyDown(Keys.W))
            {
                if (GetBlock((int)Math.Round(PlayerPositionWorld().X / WorldMetrics.SpriteSize),
                             (int)Math.Round((PlayerPositionWorld().Y + PlayerSpeed) / WorldMetrics.SpriteSize)) == 1)
                {
                    PlayerVelocity.Y = -10f;
                }
            }


            if (keyboardState.IsKeyDown(Keys.A))
            {
                if (GetBlock((int)Math.Round((PlayerPositionWorld().X - PlayerSpeed - Player.Width / 2) / WorldMetrics.SpriteSize),
                             (int)Math.Floor(PlayerPositionWorld().Y / WorldMetrics.SpriteSize)) != 1)
                {
                    PlayerVelocity.X = -PlayerSpeed;
                }
                else
                {
                    PlayerVelocity.X = 0;
                }
            }
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                if (GetBlock((int)Math.Round((PlayerPositionWorld().X + PlayerSpeed) / WorldMetrics.SpriteSize),
                             (int)Math.Floor(PlayerPositionWorld().Y / WorldMetrics.SpriteSize)) != 1)
                {
                    PlayerVelocity.X = PlayerSpeed;
                }
                else
                {
                    PlayerVelocity.X = 0;
                }
            }
            else
            {
                PlayerVelocity.X = 0;
            }

            //if (GetBlock((int)Math.Round(PlayerPositionWorld().X / WorldMetrics.SpriteSize),
            //             (int)Math.Round((PlayerPositionWorld().Y - PlayerVelocity.Y) / WorldMetrics.SpriteSize)) == 1)
            //{
            //        PlayerVelocity.Y = 0;
            //}
            if (GetBlock((int)Math.Round(PlayerPositionWorld().X / WorldMetrics.SpriteSize),
                         (int)Math.Round((PlayerPositionWorld().Y + PlayerVelocity.Y) / WorldMetrics.SpriteSize)) != 1)
            {
                 PlayerVelocity.Y += 0.5f;
            }
            else
            {
                PlayerVelocity.Y = 0;
            }
            CameraPosition += PlayerVelocity;

            //if(previousScrollValue > mouseState.ScrollWheelValue)
            //{
            //    WorldMetrics.SpriteSize--;
            //}
            //else if (previousScrollValue < mouseState.ScrollWheelValue)
            //{
            //    WorldMetrics.SpriteSize++;
            //}

            previousScrollValue = mouseState.ScrollWheelValue;

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            FPSCounter.Update(deltaTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);

            //Draw chunks
            int xGrid = (int)Math.Ceiling((CameraPosition.X / (float)WorldMetrics.ChunkSizeX) / (float)WorldMetrics.SpriteSize);
            int yGrid = (int)Math.Ceiling((CameraPosition.Y / (float)WorldMetrics.ChunkSizeY) / (float)WorldMetrics.SpriteSize);
            

            for (int xChunk = xGrid - toDrawChunksThreshold; xChunk <= xGrid + toDrawChunksThreshold; xChunk++)
            {
                for (int yChunk = yGrid - toDrawChunksThreshold; yChunk <= yGrid + toDrawChunksThreshold; yChunk++)
                {
                    //Draw
                    if (xChunk < 0 || yChunk < 0 || xChunk >= WorldMetrics.ChunkCountX || yChunk >= WorldMetrics.ChunkCountY)
                    {
                        continue;
                    }
                    
                     for (int x = 0; x < WorldMetrics.ChunkSizeX; x++)
                     {
                         for (int y = 0; y < WorldMetrics.ChunkSizeY; y++)
                         {
                             int blockX = xChunk * WorldMetrics.ChunkSizeX + x;
                             int blockY = yChunk * WorldMetrics.ChunkSizeY + y;
                             if (GetBlock(blockX, blockY) == 1)
                             {
                                spriteBatch.Draw(GroundTextures[Chunks[xChunk, yChunk].GetBlockType(x, y)], new Vector2(blockX * WorldMetrics.SpriteSize, blockY * WorldMetrics.SpriteSize) - CameraPosition, null, Color.White, 0f, Vector2.Zero, WorldMetrics.SpriteSize / 16f, SpriteEffects.None, 0f);
                                int health = Chunks[xChunk, yChunk].GetBlockHealth(x, y);
                                if (health < 5)
                                {
                                    spriteBatch.Draw(BreakTextures[health], new Vector2(blockX * WorldMetrics.SpriteSize, blockY * WorldMetrics.SpriteSize) - CameraPosition, null, Color.White, 0f, Vector2.Zero, WorldMetrics.SpriteSize / 16f, SpriteEffects.None, 0f);
                                }
                             }
                         }
                     }
                        
                }
            }
            spriteBatch.Draw(Cursor, new Vector2(mousePosition.X - 16, mousePosition.Y - 16), Color.White);

            spriteBatch.Draw(Player, PlayerPosition, Color.Pink);

            var fps = string.Format("FPS : {0}", FPSCounter.AverageFramesPerSecond);

            spriteBatch.DrawString(defaultFont, fps, new Vector2(0, 0), Color.Black);


            spriteBatch.End();
            base.Draw(gameTime);
        }

        public int GetState(int x, int y)
        {
            int state = 0;

            if (GetBlock(x - 1, y) == 1) { state += 1; }
            
            if (GetBlock(x + 1, y) == 1) { state += 4; }
            
            if (GetBlock(x, y - 1) == 1) { state += 2; }
            
            if (GetBlock(x, y + 1) == 1) { state += 8; }

            return state;
        }

        public int GetAdjacentCount(int X, int Y)
        {
            int count = 0;
            for (int x = X - 1; x <= X + 1; x++)
            {
                for (int y = Y - 1; y <= Y + 1; y++)
                {
                    if (x == X && y == Y) { continue; }
                    count += GetBlock(x, y);
                }
            }
            return count;
        }

        int GetBlock(int x, int y)
        {
            int chunkX = x / WorldMetrics.ChunkSizeX;
            int chunkY = y / WorldMetrics.ChunkSizeY;

            int blockX = x % WorldMetrics.ChunkSizeX;
            int blockY = y % WorldMetrics.ChunkSizeY;

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
            return Chunks[chunkX, chunkY].GetBlock(blockX, blockY);
        }

        void SetBlock(int x, int y, int to)
        {
            int chunkX = x / WorldMetrics.ChunkSizeX;
            int chunkY = y / WorldMetrics.ChunkSizeY;

            int blockX = x % WorldMetrics.ChunkSizeX;
            int blockY = y % WorldMetrics.ChunkSizeY;

            if (chunkX >= WorldMetrics.ChunkCountX || chunkX < 0)
            {
                return;
            }
            if (chunkY >= WorldMetrics.ChunkCountY || chunkY < 0)
            {
                return;
            }
            Chunks[chunkX, chunkY].SetBlock(blockX, blockY, to, true);

        }
        
        public void RecalculateChunk(int x, int y)
        {
            if(x == -1 || y == -1 || x == WorldMetrics.ChunkCountX || y == WorldMetrics.ChunkCountY)
            {
                return;
            }
            Chunks[x, y].Recalculate();
        }

        public Vector2 PlayerPositionWorld()
        {
            return CameraPosition + PlayerPosition;
        }
        
    }

    public struct VectorInt
    {
        public int X;
        public int Y;

        public VectorInt(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public static class WorldMetrics
    {
        public static int ChunkSizeX = 16;
        public static int ChunkSizeY = 16;

        public static int ChunkCountX = 32;
        public static int ChunkCountY = 32;

        public static int SpriteSize = 32;
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
