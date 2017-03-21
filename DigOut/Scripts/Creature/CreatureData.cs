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
    public class CreatureData
    {
        //    Types

        //  Health
        public int MaximumHealth; // 100;
        public int CurrentHealth; // 90;

        //  Attack
        public float AttackSpeed; //10 per second
        public int Damage; // 10

        //  Movement
        public float Speed;
        public float JumpForce;

        //  AI
        public AIType AI;

        //  Animation
        public Texture2D CurrentTexture;

        public AnimationSheet AnimationWalk;
        public AnimationSheet AnimationWalkBackwards;

        public AnimationSheet AnimationJump;
        public AnimationSheet AnimationFall;

        public AnimationSheet AnimationAttack;
        public AnimationSheet AnimationDeath;

        public CreatureData(int maximumHealth, int currentHealth, float attackSpeed, int damage, float speed, float jumpForce, AIType aI, Texture2D currentTexture, AnimationSheet animationWalk, AnimationSheet animationWalkBackwards, AnimationSheet animationJump, AnimationSheet animationFall, AnimationSheet animationAttack, AnimationSheet animationDeath)
        {
            MaximumHealth = maximumHealth;
            CurrentHealth = currentHealth;
            AttackSpeed = attackSpeed;
            Damage = damage;
            Speed = speed;
            JumpForce = jumpForce;
            AI = aI;
            CurrentTexture = currentTexture;
            AnimationWalk = animationWalk;
            AnimationWalkBackwards = animationWalkBackwards;
            AnimationJump = animationJump;
            AnimationFall = animationFall;
            AnimationAttack = animationAttack;
            AnimationDeath = animationDeath;
        }
    }
}
