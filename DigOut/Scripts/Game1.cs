using Math = System.Math;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Text.RegularExpressions;

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

		Texture2D GroundTexture; //Ground texture sheet
		Texture2D[] BreakTextures; //Damage textures (5 states)
		Texture2D Cursor; //Cursor texture
		Texture2D BackgroundTexture; //Background texture
		public Texture2D Player; //Player texture

		FrameCounter FPSCounter = new FrameCounter(); //FPS counter class
		SpriteFont defaultFont; //Font reference

		Vector2 mousePosition; //Mouse position
		Vector2 CameraPosition; //Camera position
		Vector2 mouseCameraPosition; //Mouse position (World)

		int previousScrollValue; //Prev. scroll value
		public Player player; //Player reference

		string seed = "why4ch"; //Current world seed
		public Chunk[,] Chunks; //Chunks array

		string currentPlayerState; //DEBUG
		public BlockData[] Data;
		public Game1()
		{
			//Initialize game
			graphics = new GraphicsDeviceManager(this);
			//graphics.IsFullScreen = true;
			graphics.PreferredBackBufferWidth = 1366;
			graphics.PreferredBackBufferHeight = 768;
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
			GroundTexture = Content.Load<Texture2D>("Environment/Blocks/Terrain/Terrain_SpriteSheet");
			BreakTextures = new Texture2D[5];

			for (int i = 1; i <= 5; i++)
			{
				BreakTextures[i - 1] = Content.Load<Texture2D>("Environment/Blocks/Damage/" + i.ToString());
			}

			//Load other textures
			Player = Content.Load<Texture2D>("UI/Cursor");
			defaultFont = Content.Load<SpriteFont>("Fonts/Arial");
			Cursor = Content.Load<Texture2D>("UI/Cursor");
			BackgroundTexture = Content.Load<Texture2D>("Environment/Blocks/Background/Background");
			player = new Player();


			//Block data
			Data = new BlockData[]
			{
				new BlockData("Stone", 61, 61, 61, 0, 900, 10),
				new BlockData("Copper", 197, 105, 70, 900, 910, 4),
				new BlockData("Iron", 151, 176, 212, 910, 920, 4),
				new BlockData("Aluminum", 173, 178, 183, 920, 930, 3),
				new BlockData("Coal", 33, 33, 33, 930, 940, 5),
				new BlockData("Lithium", 158, 183, 188, 940, 945, 2),
				new BlockData("Sulfur", 224, 250, 103, 945, 950, 1),
				new BlockData("Titanium", 242, 242, 242, 950, 955, 2),
				new BlockData("Chrome", 204, 202, 228, 955, 958, 1),
				new BlockData("Nickel", 229, 214, 132, 958, 968, 3),
				new BlockData("Silver", 215, 205, 243, 968, 971, 1),
				new BlockData("Tin", 144, 153, 184, 971, 981, 4),
				new BlockData("Tungsten", 44, 45, 51, 981, 986, 2),
				new BlockData("Lead", 51, 29, 60, 986, 996, 3),
				new BlockData("Iridium", 244, 243, 245, 996, 997, 0),
				new BlockData("Uranium", 146, 217, 136, 997, 998, 1),
				new BlockData("Gold", 239, 169, 46, 998, 1000, 1)
			};

			//Generate world :
			//Create random instance
			IOModule.Init();

			//string worldData = IOModule.ReadFull();
			string worldData = "";
			if (worldData == "")
			{
				NoiseGenerator.generateNoise(6, 45, 0.16, 0.45, seed);
				Random r = new Random(seed.GetHashCode());
				for (int ChunkX = 0; ChunkX < WorldMetrics.ChunkCountX; ChunkX++)
				{
					for (int ChunkY = 0; ChunkY < WorldMetrics.ChunkCountY; ChunkY++)
					{
						//Loop through all chunks
						Chunks[ChunkX, ChunkY] = new Chunk(WorldMetrics.ChunkSizeX, WorldMetrics.ChunkSizeY, ChunkX, ChunkY, this);
						for (int x = 0; x < WorldMetrics.ChunkSizeX; x++)
						{
							for (int y = 0; y < WorldMetrics.ChunkSizeY; y++)
							{
								//Loop through all blocks and randomize thems

								//Generate noise
								float noise = (float)NoiseGenerator.Noise(ChunkX * WorldMetrics.ChunkSizeX + x, ChunkY * WorldMetrics.ChunkSizeY + y);

								Chunks[ChunkX, ChunkY].Blocks[x, y].ID = (noise >= 0.4f) ? 1 : 0;
								Chunks[ChunkX, ChunkY].Blocks[x, y].Hardness = 6;
								if (Chunks[ChunkX, ChunkY].GetBlock(x, y) == 0)
								{
									CameraPosition = new Vector2(ChunkX * WorldMetrics.ChunkSizeX + x, ChunkY * WorldMetrics.ChunkSizeY + y);
									CameraPosition -= new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
								}
								else
								{
									int i = 1;
									foreach (BlockData bd in Data)
									{
										float rL = bd.RarityL / 1000f;
										float rR = bd.RarityR / 1000f;
										if (rL <= noise && rR > noise)
										{
											Chunks[ChunkX, ChunkY].Blocks[x, y].ID = i;
											if (i != 1)
											{
												PopulateOre(ChunkX * WorldMetrics.ChunkSizeX + x, ChunkY * WorldMetrics.ChunkSizeY + y, i, r);
											}
											break;
										}
										i++;
									}
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
							//Chunks[x, y].Smooth(r);
						}
					}
				}
			}
			else
			{
				worldData = worldData.Replace("\r", "");

				string[] DataSplit = worldData.Split('\n');

				int index = 0;
				for (int ChunkX = 0; ChunkX < WorldMetrics.ChunkCountX; ChunkX++)
				{
					for (int ChunkY = 0; ChunkY < WorldMetrics.ChunkCountY; ChunkY++)
					{
						string a = DataSplit[index];
						index++;
						//Loop through all chunks
						Chunks[ChunkX, ChunkY] = new Chunk(WorldMetrics.ChunkSizeX, WorldMetrics.ChunkSizeY, ChunkX, ChunkY, this);
						for (int x = 0; x < WorldMetrics.ChunkSizeX; x++)
						{
							string Row = DataSplit[index];
							string[] RowCols = Row.Split(' ');
							for (int y = 0; y < WorldMetrics.ChunkSizeY; y++)
							{
								Chunks[ChunkX, ChunkY].Blocks[x, y].ID = System.Convert.ToInt32(RowCols[y]);
								Chunks[ChunkX, ChunkY].Blocks[x, y].Hardness = 6;
							}
							index++;
						}
					}
				}
				string CamPos = DataSplit[index];
				float xc = (float)System.Convert.ToDouble(CamPos.Split(',')[0]);
				float yc = (float)System.Convert.ToDouble(CamPos.Split(',')[1]);
				CameraPosition = new Vector2(xc, yc);
			}


			//Recalculate block states
			for (int x = 0; x < WorldMetrics.ChunkCountX; x++)
			{
				for (int y = 0; y < WorldMetrics.ChunkCountY; y++)
				{
					Chunks[x, y].Recalculate();
				}
			}
			ScreenWidthSquared = (int)Math.Pow(graphics.PreferredBackBufferWidth / 2f, 2) * 2f;
		}

		public void PopulateOre(int X, int Y, int Type, Random r)
		{
			int growState = r.Next(0, 15);
			string bin = System.Convert.ToString(growState, 2).PadLeft(4, '0');
			for(int i = 0; i < 4; i++)
			{
				char c = bin[i];
				if(c == '1')
				{
					switch(i)
					{
						case 0: SetBlockDirectly(X - 1, Y, Type); break;
						case 1: SetBlockDirectly(X, Y + 1, Type); break;
						case 2: SetBlockDirectly(X + 1, Y, Type); break;
						case 3: SetBlockDirectly(X, Y - 1, Type); break;
						default: break;
					}
				}
			}
		}

		void SetBlockDirectly(int X, int Y, int To)
		{
			VectorInt Chunk = new VectorInt(X / WorldMetrics.ChunkSizeX, Y / WorldMetrics.ChunkSizeY);
			VectorInt Block = new VectorInt(X % WorldMetrics.ChunkSizeX, Y % WorldMetrics.ChunkSizeY);
			if(Chunk.X < 0 || Chunk.Y < 0 || Chunk.X >= WorldMetrics.ChunkCountX || Chunk.Y >= WorldMetrics.ChunkCountY)
			{
				return;
			}
			if(Chunks[Chunk.X, Chunk.Y].Blocks == null)
			{
				return;
			}
			if(Block.X < 0 || Block.Y < 0 || Block.X >= WorldMetrics.ChunkSizeX || Block.Y >= WorldMetrics.ChunkSizeY)
			{
				return;
			}
			Chunks[Chunk.X, Chunk.Y].Blocks[Block.X, Block.Y].ID = To;
			Chunks[Chunk.X, Chunk.Y].Blocks[Block.X, Block.Y].Hardness = 6;
		}

		protected override void UnloadContent()
		{
			IOModule.ClearText();
			for (int x = 0; x < WorldMetrics.ChunkCountX; x++)
			{
				for (int y = 0; y < WorldMetrics.ChunkCountY; y++)
				{
					Chunks[x, y].SaveData();
				}
			}
			IOModule.ManualOpen();
			IOModule.WriteString(CameraPosition.X + "," + CameraPosition.Y);
			IOModule.ManualClose();
		}

		//Tool options
		float ToolSpeed = 0.02f;
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
				//if (Math.Round(Vector2.DistanceSquared(mouseCameraPosition, PlayerPositionWorld()) / WorldMetrics.SpriteSize) < 400f)
				//{
				//Calculate block position
				int X = (int)mouseCameraPosition.X / WorldMetrics.SpriteSize;
				int Y = (int)mouseCameraPosition.Y / WorldMetrics.SpriteSize;

				//Check for availability
				if (X >= 0 && Y >= 0 && X < WorldMetrics.ChunkCountX * WorldMetrics.ChunkSizeX && Y < WorldMetrics.ChunkCountY * WorldMetrics.ChunkSizeY && GetBlock(X, Y) == 0)
				{
					//Check for time
					if (Time - PreviousPlaceTime >= ToolSpeed)
					{
						//Set our block
						SetBlock(X, Y, 1);
						PreviousPlaceTime = Time;
						player.isMining = true;
					}
				}
				else
				{
					player.isMining = false;
				}
				//}
			}
			else
			{
				player.isMining = false;
			}

			//If RMB is pressed : destroy block
			if (mouseState.RightButton == ButtonState.Pressed)
			{
				//If distance is OK
				//if (Math.Round(Vector2.DistanceSquared(mouseCameraPosition, PlayerPositionWorld()) / WorldMetrics.SpriteSize) < 400f)
				//{
				//Calculate block position
				int X = (int)mouseCameraPosition.X / WorldMetrics.SpriteSize;
				int Y = (int)mouseCameraPosition.Y / WorldMetrics.SpriteSize;

				//Check for availability
				if (X >= 0 && Y >= 0 && X < WorldMetrics.ChunkCountX * WorldMetrics.ChunkSizeX && Y < WorldMetrics.ChunkCountY * WorldMetrics.ChunkSizeY && GetBlock(X, Y) != 0)
				{
					//Check for time 
					if (Time - PreviousBreakTime >= ToolSpeed)
					{
						//Set our block
						SetBlock(X, Y, 0);
						PreviousBreakTime = Time;
					}
					player.isMining = true;
				}
				else
				{
					player.isMining = false;
				}
				//}
			}
			else
			{
				player.isMining = false;
			}

			//Get keyboard state and calculate player's velocity based on keyboard input
			var keyboardState = Keyboard.GetState();
			currentPlayerState = player.HandleKeyboardInput(keyboardState, CameraPosition, this);

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

		float ScreenWidthSquared;
		protected override void Draw(GameTime gameTime)
		{
			//Clear our background
			GraphicsDevice.Clear(new Color(75, 75, 75));

			//Begin sprite batch : We need to clamp as point and blend with alpha
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

			//Draw chunks based on current camera's position clamped to chunk grid
			int xGrid = (int)Math.Ceiling((PlayerPositionWorld().X / (float)WorldMetrics.ChunkSizeX) / (float)WorldMetrics.SpriteSize);
			int yGrid = (int)Math.Ceiling((PlayerPositionWorld().Y / (float)WorldMetrics.ChunkSizeY) / (float)WorldMetrics.SpriteSize);


			int drawingCount = 0;
			int chunkCount = 0;
			int fullBlockCount = 0;
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
					chunkCount++;
					//Loop through chunk's blocks
					for (int x = 0; x < WorldMetrics.ChunkSizeX; x++)
					{
						for (int y = 0; y < WorldMetrics.ChunkSizeY; y++)
						{
							//Calculate block's position
							int blockX = xChunk * WorldMetrics.ChunkSizeX + x;
							int blockY = yChunk * WorldMetrics.ChunkSizeY + y;
							Vector2 screenPos = new Vector2(blockX, blockY) * WorldMetrics.SpriteSize;
							fullBlockCount++;
							if (Vector2.DistanceSquared(screenPos, PlayerPositionWorld()) <= ScreenWidthSquared)
							{
								////Calculate block's color
								//float PlayerToBlockDistance = Vector2.DistanceSquared(PlayerPositionGrid(), new Vector2(blockX, blockY));
								//float ColorValue = (300f - PlayerToBlockDistance) / 300f;
								Color color = Color.White;
								//if (ColorValue > 0f)
								//{
								//    color = new Color(ColorValue, ColorValue, ColorValue);
								//}
								//else
								//{
								//    color = Color.Black;
								//}

								//Draw background
								//spriteBatch.Draw(BackgroundTexture, new Vector2(blockX * WorldMetrics.SpriteSize, blockY * WorldMetrics.SpriteSize) - CameraPosition, null, color, 0f, Vector2.Zero, WorldMetrics.SpriteSize / 16f, SpriteEffects.None, 1f);

								//If block is solid (Not air)
								if (GetBlock(blockX, blockY) != 0)
								{
									//Draw it
									int State = Chunks[xChunk, yChunk].GetBlockType(x, y);
									Rectangle SourceRectangle = new Rectangle((State * 16) % 64, ((State * 16) / 64) * 16, 16, 16);

									Color colorToAttach = Data[Chunks[xChunk, yChunk].GetBlock(x, y) - 1].GetColor();
									//spriteBatch.Draw(GroundTexture, new Vector2(blockX * WorldMetrics.SpriteSize, blockY * WorldMetrics.SpriteSize) - CameraPosition, null, new Color(0.5f, 0.5f, 0.5f), 0f, Vector2.Zero, WorldMetrics.SpriteSize / 16f, SpriteEffects.None, 0.5f);
									spriteBatch.Draw(GroundTexture, new Vector2(blockX * WorldMetrics.SpriteSize, blockY * WorldMetrics.SpriteSize) - CameraPosition, SourceRectangle, colorToAttach, 0f, Vector2.Zero, WorldMetrics.SpriteSize / 16f, SpriteEffects.None, 0.5f);
									//Get health
									int health = Chunks[xChunk, yChunk].GetBlockHealth(x, y);

									//If block is damaged : display damage
									if (health < 5)
									{
										spriteBatch.Draw(BreakTextures[health], new Vector2(blockX * WorldMetrics.SpriteSize, blockY * WorldMetrics.SpriteSize) - CameraPosition, null, color, 0f, Vector2.Zero, WorldMetrics.SpriteSize / 16f, SpriteEffects.None, 0f);
									}
									drawingCount++;
								}
							}
						}
					}

				}
			}

			int MouseGridX = (int)Math.Floor(mouseCameraPosition.X / WorldMetrics.SpriteSize);
			int MouseGridY = (int)Math.Floor(mouseCameraPosition.Y / WorldMetrics.SpriteSize);

			int BlockID = 0;
			if (MouseGridX > 0 && MouseGridY > 0 && MouseGridX <= WorldMetrics.ChunkSizeX * WorldMetrics.ChunkCountX &&
				MouseGridY <= WorldMetrics.ChunkSizeY * WorldMetrics.ChunkCountY)
			{
				BlockID = Chunks[MouseGridX / WorldMetrics.ChunkSizeX, MouseGridY / WorldMetrics.ChunkSizeY].
					GetBlock(MouseGridX % WorldMetrics.ChunkSizeX, MouseGridY % WorldMetrics.ChunkSizeY);
			}

			if (BlockID != 0)
			{
				var name = Data[BlockID - 1].Name;
				spriteBatch.DrawString(defaultFont, name, mousePosition, Color.Black);
			}
			//Draw cursor
			spriteBatch.Draw(Cursor, new Vector2(mousePosition.X - 16, mousePosition.Y - 16), Color.White);

			//Draw player
			spriteBatch.Draw(Player, player.Position - new Vector2(16, 16), Color.Pink);

			//Format our fps and draw it
			var fps = string.Format("FPS : {0}, Full tiles drawn : {3},Tiles drawn : {1}, Chunk drawn : {2}", FPSCounter.AverageFramesPerSecond, drawingCount, chunkCount, fullBlockCount);
			spriteBatch.DrawString(defaultFont, fps, new Vector2(0, 0), Color.White);

			var pos = string.Format("Player : {0}", currentPlayerState);
			spriteBatch.DrawString(defaultFont, pos, new Vector2(0, 40), Color.White);
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
		public int GetState(int x, int y, bool type = true)
		{
			//Initialize state
			int state = 0;

			int BlockIDThis = GetBlock(x, y);
			if (type)
			{
				//If there is block on the left : add 1
				if (GetBlock(x - 1, y) == BlockIDThis) { state += 1; }

				//If there is block on the right : add 4
				if (GetBlock(x + 1, y) == BlockIDThis) { state += 4; }

				//If there is block on the top : add 2
				if (GetBlock(x, y - 1) == BlockIDThis) { state += 2; }

				//If there is block on the bottom : add 8
				if (GetBlock(x, y + 1) == BlockIDThis) { state += 8; }

				return state;
			}
			//If there is block on the left : add 1
			if (GetBlock(x - 1, y) != 0) { state += 1; }

			//If there is block on the right : add 4
			if (GetBlock(x + 1, y) != 0) { state += 4; }

			//If there is block on the top : add 2
			if (GetBlock(x, y - 1) != 0) { state += 2; }

			//If there is block on the bottom : add 8
			if (GetBlock(x, y + 1) != 0) { state += 8; }

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
					count += (GetBlock(x, y) != 0) ? 1 : 0;
				}
			}

			//Return counter
			return count;
		}

		public int GetAdjacentOre(int X, int Y)
		{
			//Initialize counter

			//Loop through adjacent blocks
			int id1 = GetBlock(X - 1, Y);
			int id2 = GetBlock(X + 1, Y);
			int id3 = GetBlock(X, Y - 1);
			int id4 = GetBlock(X, Y + 1);

			if (id1 > 1)
			{
				return id1;
			}
			else if (id2 > 1)
			{
				return id2;
			}
			else if (id3 > 1)
			{
				return id3;
			}
			else if (id4 > 1)
			{
				return id4;
			}
			return 1;
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
			if (x == -1 || y == -1 || x == WorldMetrics.ChunkCountX || y == WorldMetrics.ChunkCountY)
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

		public Vector2 PlayerPositionGrid()
		{
			return (CameraPosition + player.Position) / WorldMetrics.SpriteSize;
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
		public static int ChunkCountX = 8;
		public static int ChunkCountY = 8;

		//Sprite size (In pixels)
		public static int SpriteSize = 16;

		//PLANET SETTINGS
		public static float GravityAcceleration = 0.3f; //const g - acceleration
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

	public struct Pair<T>
	{
		T First;
		T Second;

		public Pair(T first, T second)
		{
			First = first;
			Second = second;
		}
	}
}
