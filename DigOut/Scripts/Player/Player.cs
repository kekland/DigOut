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
    public class Player
    {
        public string Name = "Kekland"; //Player name 

        public Vector2 PlayerVelocity; //Player velocity
        
        public Vector2 Position = new Vector2(0, 0); //Player's screen center position

        float PlayerSpeed = 2f; //Player's speed

        /// <summary>
        /// Function used to give player's world position (in pixels).
        /// </summary>
        /// <param name="Cam">Camera position (in pixels).</param>
        /// <returns>Player's world position (in pixels).</returns>
        Vector2 PlayerPositionWorld(Vector2 Cam)
        {
            //Calculate camera position
            return Cam + Position;
        }

        /// <summary>
        /// Function used to handle keyboard input.
        /// </summary>
        /// <param name="keyboardState">Current state of keyboard.</param>
        /// <param name="CameraPosition">Current position of camera.</param>
        /// <param name="p">Our parent : Game1.</param>
        public void HandleKeyboardInput(KeyboardState keyboardState, Vector2 CameraPosition, Game1 p)
        {
            //Position = center of the screen.
            Position = new Vector2(p.graphics.PreferredBackBufferWidth / 2, p.graphics.PreferredBackBufferHeight / 2);

            //If W is pressed : we should jump
            if (keyboardState.IsKeyDown(Keys.W))
            {
                //If there is block underneath us that means that we are grounded
                if (p.GetBlock((int)Math.Round(PlayerPositionWorld(CameraPosition).X / WorldMetrics.SpriteSize),
                             (int)Math.Round((PlayerPositionWorld(CameraPosition).Y + PlayerSpeed) / WorldMetrics.SpriteSize)) == 1)
                {
                    //Add velocity to jump
                    PlayerVelocity.Y = -10f;
                }
            }

            //If A is pressed : we should go left
            if (keyboardState.IsKeyDown(Keys.A))
            {
                //If there is no obstacle on the left
                if (p.GetBlock((int)Math.Round((PlayerPositionWorld(CameraPosition).X - PlayerSpeed - p.Player.Width / 2) / WorldMetrics.SpriteSize),
                             (int)Math.Floor(PlayerPositionWorld(CameraPosition).Y / WorldMetrics.SpriteSize)) != 1)
                {
                    //Move left
                    PlayerVelocity.X = -PlayerSpeed;
                }
                else
                {
                    //Otherwise : stop
                    PlayerVelocity.X = 0;
                }
            }
            //If D is pressed : we should go right
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                //If there is no obstacle on the right
                if (p.GetBlock((int)Math.Round((PlayerPositionWorld(CameraPosition).X + PlayerSpeed) / WorldMetrics.SpriteSize),
                             (int)Math.Floor(PlayerPositionWorld(CameraPosition).Y / WorldMetrics.SpriteSize)) != 1)
                {
                    //Move right
                    PlayerVelocity.X = PlayerSpeed;
                }
                else
                {
                    //Otherwise : stop
                    PlayerVelocity.X = 0;
                }
            }
            else
            {
                //If we are not pressing anything : stop
                PlayerVelocity.X = 0;
            }
            
            //If there is no blocks underneath us : fall down by gravity;
            if (p.GetBlock((int)Math.Round(PlayerPositionWorld(CameraPosition).X / WorldMetrics.SpriteSize),
                         (int)Math.Round((PlayerPositionWorld(CameraPosition).Y + PlayerVelocity.Y) / WorldMetrics.SpriteSize)) != 1)
            {
                PlayerVelocity.Y += WorldMetrics.GravityAcceleration;
            }
            else
            {
                //Otherwise : stop.
                PlayerVelocity.Y = 0;
            }
        }
    }
}
