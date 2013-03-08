using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using System.Diagnostics;

namespace WordGridGame
{
    /// <summary>
    /// Asset Helper class which supports loading your game content in small
    /// chunks, then retrieving it from a cache later
    /// </summary>
    public static class AssetHelper
    {
        private static Shared shared;
        private static GraphicsDeviceManager gdm;
        private static Game g;
        /// <summary>
        /// Private list of assets, the first asset in the list is guarenteed to 
        /// load before the body of any Update or Draw calls is made
        /// </summary>
        private static List<Asset> assets = new List<Asset>{
            new Asset(FirstLoad),
            new Asset("button", typeof(SoundEffect)),
            new Asset("star", "star",typeof(Texture2D)),
            new Asset("diamond", typeof(Texture2D)),
            new Asset("circle", typeof(Texture2D)),
            new Asset("greentile", typeof(Texture2D)),
            new Asset("moon", typeof(Texture2D)),
            new Asset("sky", typeof(Texture2D)),      
            new Asset("smallStar", typeof(Texture2D)),
            new Asset("clickcircle", typeof(Texture2D)),
            new Asset("horizontal_linker", typeof(Texture2D)),
            new Asset("diagonal_linker", typeof(Texture2D)),
      //      new Asset("sky2", typeof(Texture2D)),
            new Asset("pointsBox", typeof(Texture2D)),
            new Asset("pauseButton", typeof(Texture2D)),
            new Asset("paused", typeof(Texture2D)),
            new Asset("3", typeof(Texture2D)),
            new Asset("2", typeof(Texture2D)),
            new Asset("1", typeof(Texture2D)),
            new Asset("tileSelect", typeof(SoundEffect)),
            new Asset("start", typeof(Texture2D)),
            new Asset("newword", typeof(Texture2D)),
            new Asset("wow", typeof(Texture2D)),
            new Asset("excellent", typeof(Texture2D)),
            new Asset("great", typeof(Texture2D)),
            new Asset("awesome", typeof(Texture2D)),
            new Asset("testfont", typeof(SpriteFont)),
            new Asset("scorefont", typeof(SpriteFont)),
            new Asset("pointsfont2", typeof(SpriteFont)),
            new Asset("bigfont", typeof(SpriteFont)),
            new Asset("backgroundfont", typeof(SpriteFont)),
            new Asset("pointsfont", typeof(SpriteFont)),
            new Asset("letterpointsfont", typeof(SpriteFont)),
            new Asset("endgamepoints", typeof(SpriteFont)),
            new Asset("whiteclearfont", typeof(SpriteFont)),
            new Asset("orangefont", typeof(SpriteFont)),
            new Asset("menuheader", typeof(SpriteFont)),
            //new Asset("endgamesquare", typeof(Texture2D)),
            new Asset("normalpoints", typeof(SpriteFont)),
            new Asset("excellentpoints", typeof(SpriteFont)),
            new Asset("greatpoints", typeof(SpriteFont)),
            new Asset("awesomepoints", typeof(SpriteFont)),
            new Asset("wowpoints", typeof(SpriteFont)),    
            new Asset("startanimation", typeof(SoundEffect)),
            new Asset("nogood", typeof(SoundEffect)),
            new Asset("good", typeof(SoundEffect)),
            new Asset("matchword", typeof(SoundEffect)),
            new Asset("endgame", typeof(SoundEffect)),
            new Asset("reward", typeof(SoundEffect)),
            new Asset("titlescreen", typeof(Texture2D)),
            new Asset("letters", typeof(Texture2D)),
            new Asset("trialword", typeof(Texture2D)),
            new Asset(LoadShared),
            new Asset(LoadSaveData),
            new Asset(FinishedLoading),
         };


        /// <summary>
        /// Index storing which asset we are currently loading
        /// </summary>
        private static int index = 0;

        /// <summary>
        /// Map between asset keys and their loaded data
        /// </summary>
        private static Dictionary<string, object> data = new Dictionary<string, object>();

        #region Properties
        /// <summary>
        /// Returns the percent of assets which have been loaded as an integer 0 to 100
        /// </summary>
        public static int PercentLoaded
        {
            get
            {
                return (100 * index) / assets.Count;
            }
        }

        /// <summary>
        /// Returns true if all loading is complete
        /// </summary>
        public static bool Loaded
        {
            get
            {
                return index >= assets.Count;
            }
        }
        #endregion

        #region AssetHelper Methods

        /// <summary>
        /// Loads the next asset in the list then returns, advancing the index by one step
        /// 
        /// Currently supports:
        /// Texture2D loading from raw PNG's for greater speed
        /// Model
        /// SpriteFont
        /// SoundEffect
        /// Delegate (Calls a custom worker method)
        /// 
        /// Note: This is where you would implement any additional asset types
        /// </summary>
        /// <param name="game">The game underwhich you are running</param>
        /// <returns>True if all assets have been loaded</returns>
        public static bool LoadOne(Game game)
        {
            g = game;
            DateTime now = DateTime.Now;
            if (index >= assets.Count) return true;

            Asset nextAsset = assets[index];

            if (nextAsset.Type == typeof(Texture2D))
            {
                data[nextAsset.Key] = LoadTextureStream(game.GraphicsDevice, nextAsset.Loc);
            }
            else if (nextAsset.Type == typeof(Model))
            {
                data[nextAsset.Key] = game.Content.Load<Model>(nextAsset.Loc);
            }
            else if (nextAsset.Type == typeof(SpriteFont))
            {
                data[nextAsset.Key] = game.Content.Load<SpriteFont>(nextAsset.Loc);
            }
            else if (nextAsset.Type == typeof(SoundEffect))
            {
                data[nextAsset.Key] = game.Content.Load<SoundEffect>(nextAsset.Loc);
            }
            else if (nextAsset.Type == typeof(Delegate))
            {
                nextAsset.Call(game);
            }

            index++;

            if (nextAsset.Key != null)
                Debug.WriteLine(nextAsset.Key.ToString() + " : " + (DateTime.Now.Millisecond - now.Millisecond).ToString());
            else
                Debug.WriteLine((DateTime.Now.Millisecond - now.Millisecond).ToString());

            return false;
        }

        /// <summary>
        /// Returns an asset by its key
        /// </summary>
        /// <typeparam name="T">The type of this asset</typeparam>
        /// <param name="key">The string based key of the asset</param>
        /// <returns>The asset object representing this key</returns>
        public static T Get<T>(string key)
        {
            if (!data.ContainsKey(key))
            {
                shared.spritebatch.End();
                data[key] = LoadTextureStream(g.GraphicsDevice, key);
                shared.spritebatch.Begin();
            }
            return (T)data[key];
        }

        /// <summary>
        /// LoadTextureStream method to speed up loading Texture2Ds from pngs,
        /// as described in 
        /// http://jakepoz.com/jake_poznanski__speeding_up_xna.html
        /// </summary>
        /// <param name="graphics">Graphics device to use</param>
        /// <param name="loc">Location of the image, root of the path is in the Content folder</param>
        /// <returns>A Texture2D with premultiplied alpha</returns>
        private static Texture2D LoadTextureStream(GraphicsDevice graphics, string loc)
        {
            Texture2D file = null;
            RenderTarget2D result = null;

            using (Stream titleStream = TitleContainer.OpenStream("Content\\" + loc + ".png"))
            {
                file = Texture2D.FromStream(graphics, titleStream);
            }

            //Setup a render target to hold our final texture which will have premulitplied alpha values
            result = new RenderTarget2D(graphics, file.Width, file.Height);

            graphics.SetRenderTarget(result);
            graphics.Clear(Color.Black);

            //Multiply each color by the source alpha, and write in just the color values into the final texture
            BlendState blendColor = new BlendState();
            blendColor.ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue;

            blendColor.AlphaDestinationBlend = Blend.Zero;
            blendColor.ColorDestinationBlend = Blend.Zero;

            blendColor.AlphaSourceBlend = Blend.SourceAlpha;
            blendColor.ColorSourceBlend = Blend.SourceAlpha;

            SpriteBatch spriteBatch = new SpriteBatch(graphics);
            spriteBatch.Begin(SpriteSortMode.Immediate, blendColor);
            spriteBatch.Draw(file, file.Bounds, Color.White);
            spriteBatch.End();

            //Now copy over the alpha values from the PNG source texture to the final one, without multiplying them
            BlendState blendAlpha = new BlendState();
            blendAlpha.ColorWriteChannels = ColorWriteChannels.Alpha;

            blendAlpha.AlphaDestinationBlend = Blend.Zero;
            blendAlpha.ColorDestinationBlend = Blend.Zero;

            blendAlpha.AlphaSourceBlend = Blend.One;
            blendAlpha.ColorSourceBlend = Blend.One;

            spriteBatch.Begin(SpriteSortMode.Immediate, blendAlpha);
            spriteBatch.Draw(file, file.Bounds, Color.White);
            spriteBatch.End();

            //Release the GPU back to drawing to the screen
            graphics.SetRenderTarget(null);

            return result as Texture2D;
        }
        #endregion

        #region Game Loading Methods

        /// <summary>
        /// This method does any additional loading required before the first screen is
        /// ever presented to the user. Currently, it loads the assets needed to display a splash screen
        /// </summary>
        /// <param name="game"></param>
        private static void FirstLoad(Game game)
        {
            data["standardbackground"] = LoadTextureStream(game.GraphicsDevice, "standardbackground");
            data["orangefont"] = game.Content.Load<SpriteFont>("orangefont");
            Get<SpriteFont>("orangefont").Spacing = -3;
            data["loadingbar"] = LoadTextureStream(game.GraphicsDevice, "loadingbar");
            data["loadingbarbackground"] = LoadTextureStream(game.GraphicsDevice, "loadingbarbackground");
        }

        /// <summary>
        /// This is an example use of a Delegate asset type, where we want not only
        /// load the background music, but start playing it
        /// </summary>
        /// <param name="game"></param>
        private static void BackgroundMusic(Game game)
        {
            //Uncomment this code to play some background music in your game
            //Sorry, I can't include any because of copyrights ;)
            /*
            SoundEffect soundEffect = game.Content.Load<SoundEffect>("BackgroundSong");
            SoundEffectInstance instance = soundEffect.CreateInstance();
            instance.IsLooped = true;
            instance.Play();*/
        }

        private static void LoadShared(Game game)
        {
            shared = Shared.Instance;
            SpriteBatch spriteBatch = new SpriteBatch(game.GraphicsDevice);
            shared.Initialize(game, game.GraphicsDevice, spriteBatch, gdm);
            shared.debugSystem.DebugCommandUI.Echo("Content Loaded");
        }
        private static void LoadSaveData(Game game)
        {
            shared.saveData = SaveData.LoadFromStorage();
        }
        /// <summary>
        /// Example method to simulate some loading delay
        /// </summary>
        /// <param name="game"></param>
        private static void SimulateDelay(Game game)
        {
            Thread.Sleep(200);
        }
        public static void SetGDM(GraphicsDeviceManager g)
        {
            gdm = g;
        }

        /// <summary>
        /// Another example of a Delegate asset type, where we play a little sound to the user
        /// at then end of asset loading
        /// </summary>
        /// <param name="game"></param>
        private static void FinishedLoading(Game game)
        {
            //Uncomment this line to play an evil laugh at the end of loading
            //Get<SoundEffect>("EvilLaugh").CreateInstance().Play();
        }
        #endregion
    }

    /// <summary>
    /// Delegate describing a function that will perform a small bit of loading for the game
    /// </summary>
    /// <param name="game">Main Game Instance</param>
    delegate void AssetLoad(Game game);

    /// <summary>
    /// Describes a single asset during the load process,
    /// Either contains a key, a content location and type to be loaded
    /// or contains a delegate method to call
    /// </summary>
    class Asset
    {
        public string Key, Loc;
        public Type Type;
        public AssetLoad Call;

        #region Constructors
        /// <summary>
        /// Creates an asset which is described by a content location, a key, and the type of content stored
        /// </summary>
        /// <param name="key">string based key for later retrieval</param>
        /// <param name="loc">Content location string</param>
        /// <param name="type">Type of the content, ex. Texture2D, SoundEffect, Model</param>
        public Asset(string key, string loc, Type type)
        {
            this.Key = key;
            this.Loc = loc;
            this.Type = type;
        }
        /// <summary>
        /// Creates an asset which is described by a content location, and the key is the location
        /// </summary>
        /// <param name="key">string based key for later retrieval</param>
        /// <param name="type">Type of the content, ex. Texture2D, SoundEffect, Model</param>
        public Asset(string key, Type type)
        {
            this.Key = key;
            this.Loc = key;
            this.Type = type;
        }
        /// <summary>
        /// Creates an asset which is described by one method to call during the loading process
        /// </summary>
        /// <param name="loader">AssetLoad delegate method</param>
        public Asset(AssetLoad loader)
        {
            this.Call = loader;
            this.Type = typeof(Delegate);
        }
        #endregion
    }
}
