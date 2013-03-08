using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WordGridGame
{
    class Star
    {
        public float x;
        public float y;
        public bool alive;
        public float life;
        private Shared shared;
        public float size;
        public float speed;
        public float alpha;
        public Color color;
        public Star() 
        {

        }

        public void Initialize()
        {
            x = y = life = -1;
            alive = false;
            shared = Shared.Instance;
            color = new Color(255,255,255,255);
        }

        public void Spawn()
        {
            alive = true;
            x = shared.random.Next(480*4);
            y = shared.random.Next(800);
            size = (float)shared.random.NextDouble() * 5;
            speed = (float)shared.random.NextDouble() * 100;
            alpha = (float)shared.random.NextDouble() * 255;
            color = Color.White * (float)(alpha/255.0);
        }

        public void Update()
        {
            x -= (float)(shared.gameTime.ElapsedGameTime.Milliseconds/1000.0) * speed;
            if (x <= -100)
            {
                Spawn();
            }
        }
        public void Draw()
        {
      //      shared.spritebatch.DrawString(shared.fontManager.GetFont("Courier New"), alpha.ToString(), new Vector2((int)x, (int)y), Color.White);
            shared.spritebatch.Draw(shared.textureManager.GetTexture("smallStar"), new Rectangle((int)x, (int)y, (int)size, (int)size), color);
           
        }
    }
}
