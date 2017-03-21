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
    public class Creature
    {
        public CreatureData Data;

        public Vector2 Velocity;
        public bool LookingRight;

        public Vector2 Position;

        public void Move(Vector2 Direction)
        {
            if(Direction.X > 0 && !LookingRight || Direction.Y < 0 && LookingRight)
            {
                Reverse();
            }

            Velocity.X = Direction.X * Data.Speed;
        }

        public void Jump()
        {
            Velocity.Y += Data.JumpForce;
        }

        public void Attack(Creature Unit)
        {
            Unit.GetDamage(Data.Damage, this);
        }

        public void GetDamage(int Amount, Creature Source)
        {
            Data.CurrentHealth -= Amount;
            if(Data.CurrentHealth <= 0)
            {
                Death();
                return;
            }
        }

        public void Death()
        {
            //Die event
        }

        public void Reverse()
        {
            LookingRight = !LookingRight;
            //Reverse spritesheet
        }
    }
}
