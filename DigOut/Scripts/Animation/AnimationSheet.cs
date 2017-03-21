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
    public class AnimationSheet
    {
        public string Name;
        public Texture2D[] TextureList;

        public AnimationSheet(string name, Texture2D[] textureList)
        {
            Name = name;
            TextureList = textureList;
        }

        int CurrentIndex = 0;

        public Texture2D NextFrame()
        {
            CurrentIndex += 1;

            if(CurrentIndex == TextureList.Length)
            {
                CurrentIndex = 0;
            }

            return TextureList[CurrentIndex];
        }

        public Texture2D GetFrame(int Index)
        {
            return TextureList[Index];
        }
    }
}
