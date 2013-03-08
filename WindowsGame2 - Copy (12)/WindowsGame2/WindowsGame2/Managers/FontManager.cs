using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WordGridGame.Managers
{
    class FontManager
    {
        Dictionary<string, SpriteFont> fonts;
        Game game;

        public FontManager()
        {
        }

        public void Initialize(Game g)
        {
            game = g;
            fonts = new Dictionary<string, SpriteFont>();
            AssetHelper.Get<SpriteFont>("wowpoints").Spacing = -14;
            AssetHelper.Get<SpriteFont>("awesomepoints").Spacing = -14;
            AssetHelper.Get<SpriteFont>("greatpoints").Spacing = -14;
            AssetHelper.Get<SpriteFont>("excellentpoints").Spacing = -14;
            AssetHelper.Get<SpriteFont>("normalpoints").Spacing = -14;
            AssetHelper.Get<SpriteFont>("menuheader").Spacing = -8;
            AssetHelper.Get<SpriteFont>("orangefont").Spacing = -3;
            AssetHelper.Get<SpriteFont>("whiteclearfont").Spacing = 2;
            AssetHelper.Get<SpriteFont>("endgamepoints").Spacing = -5;
            AssetHelper.Get<SpriteFont>("pointsfont").Spacing = 1;
            AssetHelper.Get<SpriteFont>("backgroundfont").Spacing = -2;
            AssetHelper.Get<SpriteFont>("bigfont").Spacing = 5;
            AssetHelper.Get<SpriteFont>("pointsfont2").Spacing = -8;
            AssetHelper.Get<SpriteFont>("testfont").Spacing = -20;
            AssetHelper.Get<SpriteFont>("scorefont").Spacing = -8;
        }

        private void AddFont(string filename)
        {
            fonts.Add(filename, game.Content.Load<SpriteFont>(filename));
        }

        public SpriteFont GetFont(string fontName)
        {
            try
            {
                return AssetHelper.Get<SpriteFont>(fontName);
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine(e.ToString());
                return AssetHelper.Get<SpriteFont>("Courier New");
            }
        }
    }
}
