using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections;
namespace WordGridGame.Managers
{
    class TextureManager
    {
        Dictionary<string, Texture2D> textures;
        Game game;
        Shared shared;
        public TextureManager()
        {  
        }

        public void Initialize(Game g)
        {
            shared = Shared.Instance;
            game = g;
            textures = new Dictionary<string,Texture2D>();
            

        }
        private void AddTexture(string filename)
        {
            textures.Add(filename,game.Content.Load<Texture2D>(filename));
        }
        public Texture2D GetTexture(string textureName)
        {
            try
            {
                return AssetHelper.Get<Texture2D>(textureName);
            }
            catch (KeyNotFoundException e)
            {

                Console.WriteLine(e.ToString());
                return AssetHelper.Get<Texture2D>("moon");
            }
        }
        public Vector2 GetImageCenter(string textureName)
        {
            return new Vector2(GetTexture(textureName).Width / 2.0f, GetTexture(textureName).Height / 2.0f);
        }
    }
}
