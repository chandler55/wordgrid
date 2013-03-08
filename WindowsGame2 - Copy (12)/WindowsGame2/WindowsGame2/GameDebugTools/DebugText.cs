using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace WordGridGame
{

    public class DebugText 
    {
        SpriteFont font;

        public Game game;
        private string output = "hello world";
        private float output_x, output_y;

        private DateTime start = DateTime.Now;
        private double end = 0;
        private Shared shared;
        public DebugText(Game fgame) 
        {
            game = fgame;
            
        }

        public void Initialize()
        {
            shared = Shared.Instance;
        }
        public void LoadContent()
        {
            font = game.Content.Load<SpriteFont>("Courier New");
        }

        public void UnloadContent()
        {


        }
        public void TimerStart()
        {
            start = DateTime.Now;
        }
        public void TimerEnd()
        {
            end = DateTime.Now.Subtract(start).TotalMilliseconds/1000.0;
        }
        public void Update()
        {
            // TODO: Add your update code here
        }

        public void Draw(SpriteBatch spriteBatch)
        {
         
            Vector2 FontOrigin = font.MeasureString(output) / 2;

            // draw elapsedgametime
            Color color = Color.Black;
         //   spriteBatch.DrawString(font, "HELLO WORLD", new Vector2(output_x, output_y), color, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.5f);
           // spriteBatch.DrawString(font, end.ToString(), new Vector2(0, 750), color, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.5f);
           // spriteBatch.DrawString(font, (GC.GetTotalMemory(false) / 1000000.0).ToString(), new Vector2(0, 720), color, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.5f);
        }
        public void SetText(string text, float x, float y)
        {
            output = text;
            output_x = x;
            output_y = y;
        }
    }
}
