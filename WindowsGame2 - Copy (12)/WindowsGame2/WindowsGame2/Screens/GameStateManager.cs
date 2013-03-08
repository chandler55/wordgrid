using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WordGridGame.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization;

namespace WordGridGame.Managers
{
    class GameStateManager
    {
        const float TransitionSpeed = 1.0f;
        const float ZoomyTextLifespan = 0.75f;


        public TransitionType transitionMode { get; set; }
        private const int MAX_STATES = 7;
        private Shared shared;

        private GameState[] states;

        // stuff needed for transitioning
        private TransitionType transitionType;
        private float transitionTimer = float.MaxValue;
        RenderTarget2D transitionRenderTarget;
        public Matrix ScaleMatrix { get; private set; }
        public Texture2D BlankTexture { get; private set; }

        // Zoomy text provides visual feedback when selecting menu items.
        class ZoomyText
        {
            public string Text;
            public Vector2 Position;
            public float Age = 0;
        }
        static List<ZoomyText> zoomyTexts = new List<ZoomyText>();

        //private 
        public GameStateManager()
        {
        }

        public void Initialize()
        {
            shared = Shared.Instance;
            states = new GameState[MAX_STATES];
            states[(int)Shared.State.GAME] = new MainGame();
            states[(int)Shared.State.MAINMENU] = new MainMenu();
            states[(int)Shared.State.TITLESCREEN] = new Splash();
            states[(int)Shared.State.SPLASH] = new Splash();
            states[(int)Shared.State.HIGHSCORES] = new HighScores();
            states[(int)Shared.State.TUTORIAL] = new Tutorial();
            states[(int)Shared.State.OPTIONS] = new Options();

            /*    long test = GC.GetTotalMemory(true);
                GameState[] what = new GameState[100];
                for (int i = 0; i < 100; i++)
                {
                    what[i] = new MainGame();
                    what[i].Initialize();
                }
                Console.WriteLine(GC.GetTotalMemory(true) - test);/**/
            // trying to figure out how much each game takes , roughly 3mb

            for (int i = 0; i < MAX_STATES; i++)
            {
                states[i].Initialize();
            }
            BlankTexture = new Texture2D(shared.graphicsDevice, 1, 1);
            BlankTexture.SetData(new Color[] { Color.White });
            transitionRenderTarget = new RenderTarget2D(shared.graphicsDevice, (int)shared.gameWidth, (int)shared.gameHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, 0);
        }

        public void Update()
        {


            if (transitionTimer < float.MaxValue)
                transitionTimer += (float)shared.gameTime.ElapsedGameTime.TotalSeconds;

            if (shared.input.WasKeyReleased(Microsoft.Xna.Framework.Input.Keys.O))
            {
                shared.debugSystem.DebugCommandUI.Echo(transitionTimer.ToString());
            }

            bool savedMode = shared.saveData.timeLimitMode;
            states[(int)shared.gamestate].Update();
            if (savedMode != shared.saveData.timeLimitMode)
            {
                states[(int)Shared.State.GAME].DeleteSavedGame(); // should delete whenever settings change
            }

            UpdateZoomyText(shared.gameTime);

            if (shared.nextState != Shared.State.NONE)
            {

                if (shared.nextState == Shared.State.FORCETRANSITION)
                {
                    shared.nextState = shared.gamestate;
                }
                // if next state is resume, load game 

                BeginTransition(shared.gamestate);
                if (shared.nextState == Shared.State.RESUME)
                {
                    shared.gamestate = Shared.State.GAME;
                }
                else
                {
                    shared.gamestate = shared.nextState;
                }

                switch (shared.nextState)
                {
                    case Shared.State.GAME:
                        transitionType = TransitionType.SQUARES;
                        states[(int)Shared.State.GAME].InitGame();
                        states[(int)Shared.State.GAME].DeleteSavedGame(); //  delete any saved game
                        break;
                    case Shared.State.HIGHSCORES:
                        transitionType = TransitionType.CURTAINS;
                        break;
                    case Shared.State.TITLESCREEN:
                        transitionType = TransitionType.CURTAINS;
                        break;
                    case Shared.State.MAINMENU:
                        transitionType = TransitionType.FALLINGLINES;
                        break;
                    case Shared.State.TUTORIAL:
                        transitionType = TransitionType.CURTAINS;
                        break;
                    case Shared.State.OPTIONS:
                        transitionType = TransitionType.CURTAINS;
                        break;
                    case Shared.State.RESUME:
                        transitionType = TransitionType.SQUARES;
                        states[(int)Shared.State.GAME].InitGame();
                        states[(int)Shared.State.GAME].LoadFromStorage();
                        break;
                    default:
                        transitionType = TransitionType.SHRINK;
                        break;
                }
                if (shared.forceTransitionType != TransitionType.NONE)
                {
                    transitionType = shared.forceTransitionType;
                    shared.forceTransitionType = TransitionType.NONE;
                }
                shared.nextState = Shared.State.NONE;
                if (shared.zoomText.zoom)
                {
                    shared.zoomText.zoom = false;
                    SpawnZoomyText(shared.zoomText.text, shared.zoomText.pos);
                }
            }
        }

        public void Draw()
        {
            ScaleMatrix = Matrix.CreateScale(shared.graphicsDeviceManager.PreferredBackBufferWidth / shared.gameWidth, shared.graphicsDeviceManager.PreferredBackBufferHeight / shared.gameHeight, 1);
            states[(int)shared.gamestate].Draw();
            DrawTransition();
            DrawZoomyText();

        }

        private void DrawTransition()
        {
            if (transitionTimer >= TransitionSpeed)
            {

                return;
            }

            float mu = (float)(transitionTimer / TransitionSpeed);
            float alpha = 1 - mu;
            switch (transitionType)
            {
                case TransitionType.SQUARES:
                    DrawSpinningSquaresTransition(mu, alpha);
                    break;
                case TransitionType.CURTAINS:
                    DrawOpenCurtainsTransition(alpha);
                    break;
                case TransitionType.SHRINK:
                    DrawShrinkAndSpinTransition(mu, alpha);
                    break;
                case TransitionType.FALLINGLINES:
                    DrawFallingLinesTransition(mu);
                    break;
            }

        }

        /// <summary>
        /// Begins a transition effect, capturing a copy of the current screen into the transitionRenderTarget.
        /// </summary>
        void BeginTransition(Shared.State oldState)
        {
            ScaleMatrix = Matrix.Identity;

            shared.graphicsDevice.SetRenderTarget(transitionRenderTarget);

            // Draw the old menu screen into the rendertarget.
            shared.spritebatch.Begin(0, null, null, null, null, null, ScaleMatrix);
            states[(int)oldState].Draw();
            shared.spritebatch.End();
            // Force the rendertarget alpha channel to fully opaque.
            shared.spritebatch.Begin(0, BlendState.Additive);
            shared.spritebatch.Draw(BlankTexture, new Rectangle(0, 0, (int)shared.gameWidth, (int)shared.gameHeight), new Color(0, 0, 0, 255));
            shared.spritebatch.End();

            shared.graphicsDevice.SetRenderTarget(null);

            // Initialize the transition state.
            transitionTimer = (float)shared.game.TargetElapsedTime.TotalSeconds;
            transitionMode = TransitionType.SQUARES;
        }

        /// <summary>
        /// Transition effect where the image spins off toward the bottom left of the screen.
        /// </summary>
        void DrawShrinkAndSpinTransition(float mu, float alpha)
        {
            Vector2 origin = new Vector2(240, 400);
            Vector2 translate = (new Vector2(32, (int)shared.gameHeight - 32) - origin) * mu * mu;

            float rotation = mu * mu * -4;
            float scale = alpha * alpha;

            Color tint = Color.White * (float)Math.Sqrt(alpha);

            shared.spritebatch.Draw(transitionRenderTarget, origin + translate, null, tint, rotation, origin, scale, 0, 0);
        }
        /// <summary>
        /// Transition effect where the screen splits in half, opening down the middle.
        /// </summary>
        void DrawOpenCurtainsTransition(float alpha)
        {
            int w = (int)(240 * alpha * alpha);

            shared.spritebatch.Draw(transitionRenderTarget, new Rectangle(0, 0, w, (int)shared.gameHeight), new Rectangle(0, 0, 240, (int)shared.gameHeight), Color.White * alpha);
            shared.spritebatch.Draw(transitionRenderTarget, new Rectangle((int)shared.gameWidth - w, 0, w, (int)shared.gameHeight), new Rectangle(240, 0, 240, (int)shared.gameHeight), Color.White * alpha);
        }
        /// <summary>
        /// Transition effect where the image dissolves into a sequence of vertically falling lines.
        /// </summary>
        void DrawFallingLinesTransition(float mu)
        {
            Random random = new Random(23);

            const int segments = 45;

            for (int x = 0; x < segments; x++)
            {
                Rectangle rect = new Rectangle((int)shared.gameWidth * x / segments, 0, (int)shared.gameWidth / segments, (int)shared.gameHeight);

                Vector2 pos = new Vector2(rect.X, 0);

                pos.Y += (int)shared.gameHeight * (float)Math.Pow(mu, random.NextDouble() * 10);

                shared.spritebatch.Draw(transitionRenderTarget, pos, rect, Color.White);
            }
        }
        /// <summary>
        /// Transition effect where the screen splits into pieces, each spinning off in a different direction.
        /// </summary>
        void DrawSpinningSquaresTransition(float mu, float alpha)
        {
            Random random = new Random(23);
            int rows = 8;
            int columns = 5;
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Rectangle rect = new Rectangle((int)shared.gameWidth * x / columns,
                        (int)shared.gameHeight * y / rows, (int)shared.gameWidth / columns, (int)shared.gameHeight / rows);

                    Vector2 origin = new Vector2(rect.Width, rect.Height) / 2;

                    float rotation = (float)(random.NextDouble() - 0.5) * mu * mu * 2;
                    float scale = 1 + (float)(random.NextDouble() - 0.5f) * mu * mu;

                    Vector2 pos = new Vector2(rect.Center.X, rect.Center.Y);

                    pos.X += (float)(random.NextDouble() - 0.5) * mu * mu * 400;
                    pos.Y += (float)(random.NextDouble() - 0.5) * mu * mu * 400;

                    shared.spritebatch.Draw(transitionRenderTarget, pos, rect, Color.White * alpha, rotation, origin, scale, 0, 0);
                }
            }
        }

        /// <summary>
        /// Creates a new zoomy text menu item selection effect.
        /// </summary>
        public static void SpawnZoomyText(string text, Vector2 position)
        {
            zoomyTexts.Add(new ZoomyText { Text = text, Position = position });
        }
        /// <summary>
        /// Updates the zoomy text animations.
        /// </summary>
        static void UpdateZoomyText(GameTime gameTime)
        {
            int i = 0;

            while (i < zoomyTexts.Count)
            {
                zoomyTexts[i].Age += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (zoomyTexts[i].Age >= ZoomyTextLifespan)
                    zoomyTexts.RemoveAt(i);
                else
                    i++;
            }
        }

        /// <summary>
        /// Draws the zoomy text animations.
        /// </summary>
        void DrawZoomyText()
        {
            if (zoomyTexts.Count <= 0)
                return;

            // shared.spritebatch.Begin(0, null, null, null, null, null, ScaleMatrix);

            foreach (ZoomyText zoomyText in zoomyTexts)
            {
                Vector2 pos = zoomyText.Position;

                float age = zoomyText.Age / ZoomyTextLifespan;
                float sqrtAge = (float)Math.Sqrt(age);

                float scale = 1.333f + sqrtAge * 2f;

                float alpha = 1 - age;

                SpriteFont font = shared.fontManager.GetFont("testfont");


                Vector2 origin = font.MeasureString(zoomyText.Text) / 2;

                shared.spritebatch.DrawString(font, zoomyText.Text, pos, Color.Lerp(new Color(64, 64, 255), Color.White, sqrtAge) * alpha, 0, origin, scale, 0, 0);
            }

            //  shared.spritebatch.End();
        }

        /// <summary>
        /// save game state
        /// </summary>
        public void SaveGameState()
        {
            states[(int)Shared.State.GAME].SaveToStorage();
        }

        /// <summary>
        /// load the game state
        /// </summary>
        public void LoadGameState()
        {
            states[(int)Shared.State.GAME].LoadFromStorage();
        }
    }
}
