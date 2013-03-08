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
using System.IO;
using System.IO.IsolatedStorage;
using WordGridGame.Managers;
using System.Xml.Serialization;
#if WINDOWS_PHONE
using Microsoft.Phone.Shell;
using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Info;
#endif
namespace WordGridGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        ParticleEngine loadingScreenParticleEngine; // just a small particle effect for loading 
        GameStateManager gameStateManager;
        Shared shared;
        Vector2 camera;

        // splash screen stuff
        Texture2D loadingParticle;
        private int particleCounter = 0; // want to emit 1 every 2 frames
        private bool splashScreen = true;

#if WINDOWS_PHONE
//String phoneID = "0";
#endif

        public Main()
        {
//#if WINDOWS_PHONE
 //           var value = (byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId");
 //           phoneID = Convert.ToBase64String(value);
//#endif
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            shared = Shared.Instance;

            graphics.PreferredBackBufferHeight = 800 - 32;
            graphics.PreferredBackBufferWidth = 480;
            graphics.SupportedOrientations = DisplayOrientation.Portrait;

            graphics.IsFullScreen = false;
            AssetHelper.SetGDM(graphics);
#if WINDOWS_PHONE
            PhoneApplicationService.Current.Activated += new EventHandler<ActivatedEventArgs>(GameActivated);
            PhoneApplicationService.Current.Deactivated += new EventHandler<DeactivatedEventArgs>(GameDeactivated);
            PhoneApplicationService.Current.Closing += new EventHandler<ClosingEventArgs>(GameClosing);
            PhoneApplicationService.Current.Launching += new EventHandler<LaunchingEventArgs>(GameLaunching);
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
#endif
#if WINDOWS
            this.Components.Add(new GamerServicesComponent(this));
#endif
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize(); // enumerates through components and initializes them
            TargetElapsedTime = TimeSpan.FromTicks(333333);  // fps 30
            this.IsMouseVisible = true;
#if DEBUG
         //   Guide.SimulateTrialMode = true;
#endif

        }

        public void LaunchTrialFeatureTick()
        {
        }

        public void LaunchPaidFeatureTick()
        {
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            loadingScreenParticleEngine = new ParticleEngine(new Vector2(0, 0));
            loadingParticle = Content.Load<Texture2D>("loadingparticle");

            camera.X = 0;
            camera.Y = 1152;
        }
#if WINDOWS_PHONE
        /// <summary>
        /// Saves the full state to the state object and the persistent state to 
        /// isolated storage.
        /// </summary>
        void GameDeactivated(object sender, DeactivatedEventArgs e)
        {
            if (!splashScreen)
            {
                if (shared.gamestate == Shared.State.GAME)
                    gameStateManager.SaveGameState();
                shared.saveData.WriteToStorage();
            }
        }

        /// <summary>
        /// Loads the full state from the state object.
        /// </summary>
        void GameActivated(object sender, ActivatedEventArgs e)
        {
            // shared.saveData = SaveData.LoadFromStorage();
        }

        /// <summary>
        /// Saves persistent state to isolated storage.
        /// </summary>
        void GameClosing(object sender, ClosingEventArgs e)
        {
            if (!splashScreen)
            {
                if (shared.gamestate == Shared.State.GAME)
                    gameStateManager.SaveGameState();
                shared.saveData.WriteToStorage();
            }

        }


        /// <summary>
        /// this launches when the program launches for the very first time
        /// </summary>
        void GameLaunching(object sender, LaunchingEventArgs e)
        {
            if (Guide.IsTrialMode)
            {
                LaunchTrialFeatureTick();
            }
            else
            {
                LaunchPaidFeatureTick();
            }
            //shared.saveData = SaveData.LoadFromStorage();
        }

#endif


        protected override void OnExiting(object sender, System.EventArgs args)
        {
#if WINDOWS
            if (!splashScreen)
            {
                // Save the game state (in this case, the typed text).
                if (shared.gamestate == Shared.State.GAME)
                    gameStateManager.SaveGameState();
                shared.saveData.WriteToStorage();
            }
            base.OnExiting(sender, args);
#endif
        }


        protected override void OnActivated(object sender, EventArgs args)
        {
#if WINDOWS
            if (shared.gamestate == Shared.State.GAME)
            {
                shared.forcePause = false;
            }
#endif
            base.OnActivated(sender, args);
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {

#if WINDOWS
            if (shared.gamestate == Shared.State.GAME)
            {
                shared.forcePause = true;
            }
#endif
            base.OnDeactivated(sender, args);
        }
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            Content.Unload();
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (splashScreen)
            {
                AssetHelper.LoadOne(this);
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                    this.Exit();
                //particleCounter++;
                //if (particleCounter % 1 == 0)
                //    loadingScreenParticleEngine.EmitLoadingScreenParticle(loadingParticle);
                //loadingScreenParticleEngine.Update();
                if (AssetHelper.Loaded)
                {
                    splashScreen = false;
                    gameStateManager = new GameStateManager();
                    gameStateManager.Initialize();
                    shared = Shared.Instance;
                    shared.spritebatch = spriteBatch;
                    shared.debugSystem.TimeRuler.StartFrame();
                    shared.isTrialMode = Guide.IsTrialMode;
                }
            }
            else
            {
                // if user buys game mid-game, check trial flag again 
                if (shared.checkTrial)
                {
                    shared.isTrialMode = Guide.IsTrialMode;
                }
                shared.debugSystem.TimeRuler.StartFrame();
                // Start measuring time for "Update".
                shared.debugSystem.TimeRuler.BeginMark("Update", Color.Blue);
                shared.gameTime = gameTime;
                shared.Update();
#if WINDOWS
                // Input
                if (shared.input.WasMouseJustReleased)
                {
                    //       shared.soundManager.GetSoundEffect("Laser").Play();
                }

                if (shared.input.WasKeyReleased(Keys.Escape))
                {
                    this.Exit();
                }

                if (shared.input.WasKeyReleased(Keys.W))
                {
                    if (shared.gamestate == Shared.State.GAME)
                        gameStateManager.SaveGameState();
                }
                if (shared.input.WasKeyReleased(Keys.S))
                {
                    gameStateManager.LoadGameState();
                }
#endif
                // main game logic goes here
                gameStateManager.Update();
                bool test = IsFixedTimeStep;
                shared.debugSystem.TimeRuler.EndMark("Update");
            }
            base.Update(gameTime);
        }
        private void callBack(IAsyncResult ar)
        {
            int? result = Guide.EndShowMessageBox(ar);
            if (result == 0)
            {
            }
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (splashScreen)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                //Currently loading, display progress
                spriteBatch.Draw(AssetHelper.Get<Texture2D>("standardbackground"),
                    new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
                //   loadingScreenParticleEngine.Draw(spriteBatch);
                SpriteFont font = AssetHelper.Get<SpriteFont>("orangefont");
                string loadingStr = "Loading : " + AssetHelper.PercentLoaded + "%";
                Vector2 origin = new Vector2(font.MeasureString(loadingStr).X / 2.0f, font.MeasureString(loadingStr).Y / 2.0f);
                spriteBatch.DrawString(font,
                    loadingStr, new Vector2(240, 768 / 2.0f - 50), Color.White, 0, origin, 1.0f, 0, 0);
                Texture2D tex = AssetHelper.Get<Texture2D>("loadingbarbackground");
                origin = new Vector2(tex.Width, tex.Height) / 2.0f;
                spriteBatch.Draw(tex, new Rectangle(240, 410, tex.Width, tex.Height), null, Color.White, 0, origin, 0, 0);
                tex = AssetHelper.Get<Texture2D>("loadingbar");
                origin = new Vector2(tex.Width, tex.Height) / 2.0f;

                spriteBatch.Draw(tex, new Vector2(240, 410),
                    new Rectangle(0, 0, (int)(tex.Width * (AssetHelper.PercentLoaded / 100.0f)), (int)tex.Height), Color.White, 0, origin, 1.0f, 0, 0);
                spriteBatch.End();
            }
            else
            {
                shared.debugSystem.TimeRuler.BeginMark("Draw", Color.Yellow);
                shared.gameTime = gameTime;

                GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin();
                gameStateManager.Draw();
                shared.Draw();
                spriteBatch.End();

                // Stop measuring time for "Draw".
                shared.debugSystem.TimeRuler.EndMark("Draw");
            }
            base.Draw(gameTime);
        }
    }
}
