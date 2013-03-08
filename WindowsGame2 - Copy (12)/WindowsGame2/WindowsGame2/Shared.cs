using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WordGridGame.Managers;
using Microsoft.Xna.Framework.Input.Touch;
using WordGridGame.Controls;
using PerformanceUtility.GameDebugTools;
using Microsoft.Xna.Framework.Input;
namespace WordGridGame
{
    public enum TransitionType { SQUARES, CURTAINS, SHRINK, FALLINGLINES, NONE }

    class ZoomText
    {
        public bool zoom;
        public string text;
        public Vector2 pos;
    }

    class Shared
    {
        public enum State { GAME, MAINMENU, TITLESCREEN, SPLASH, HIGHSCORES, TUTORIAL, OPTIONS, FORCETRANSITION, RESUME, NONE }
        private static Shared instance;
        public float gameWidth = 480;
        public float gameHeight = 800 - 32;
        public ContentManager content;
        public SpriteBatch spritebatch;
        public GraphicsDevice graphicsDevice;
        public DebugText debug;
        public Input input;
        public InputState inputstate;
        public WordLogic wordlogic;
        public TextureManager textureManager;
        public SoundManager soundManager;
        public FontManager fontManager;
        public GraphicsDeviceManager graphicsDeviceManager;
        public ParticleEngine particleEngine;
        public GameTime gameTime;

        // handles the transitions
        public State gamestate = State.MAINMENU;
        public State nextState = Shared.State.NONE;
        public TransitionType forceTransitionType;

        public ZoomText zoomText;
        public DrawContext drawContext;
        public DebugSystem debugSystem;
        public Game game;
        public int gridSize = 6;
        public Random random;
        public SaveData saveData;
        public bool forcePause;
        public bool isTrialMode;
        public bool checkTrial;
        private Shared() { }

        public static Shared Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Shared();
                }
                return instance;
            }
        }

        public void Initialize(Game game, GraphicsDevice gd, SpriteBatch s, GraphicsDeviceManager gmd)
        {
            this.game = game;
            graphicsDeviceManager = gmd;
            content = game.Content;
            graphicsDevice = gd;
            debug = new DebugText(game);
            debug.Initialize();
            debug.LoadContent();
            debugSystem = DebugSystem.Initialize(game, "Courier New");
            input = new Input();
            inputstate = new InputState();
            wordlogic = new WordLogic();
            wordlogic.InitializeDictionary();
            spritebatch = s;
            forceTransitionType = TransitionType.NONE;
            textureManager = new TextureManager();
            textureManager.Initialize(game);
            soundManager = new SoundManager();
            soundManager.Initialize(game);
            fontManager = new FontManager();
            fontManager.Initialize(game);

            drawContext = new DrawContext();
            drawContext.GameTime = gameTime;
            drawContext.Device = gd;
            drawContext.SpriteBatch = s;
            drawContext.DrawOffset = new Vector2(0, 0);
            TouchPanel.EnabledGestures = GestureType.Flick | GestureType.VerticalDrag | GestureType.DragComplete;
            random = new Random();
            particleEngine = new ParticleEngine(new Vector2(400, 300));
            zoomText = new ZoomText();
            zoomText.zoom = false;
            saveData = new SaveData();
            saveData.Initialize();
            forcePause = false;
            checkTrial = false;
            debugSystem.TimeRuler.Visible = false;
           debugSystem.TimeRuler.ShowLog = false;
         //   debugSystem.FpsCounter.Visible
        }
        public void UnloadContent()
        {
            debug.UnloadContent();
        }

        public void Draw()
        {
            debug.Draw(spritebatch);
            particleEngine.Draw(spritebatch);
        }

        public void Update()
        {

            drawContext.GameTime = gameTime;
            inputstate.Update();
            input.Update();

            // debug stuff
            // Show/Hide FPS counter by press A button.
#if DEBUG
            if (input.WasKeyReleased(Keys.A))
            {
                debugSystem.FpsCounter.Visible = !debugSystem.FpsCounter.Visible;
            }

            // Show/Hide TimeRuler by press B button.
            if (input.WasKeyReleased(Keys.B))
            {
                debugSystem.TimeRuler.Visible = !debugSystem.TimeRuler.Visible;
            }

            // Show/Hide TimeRuler log by press X button.
            if (input.WasKeyReleased(Keys.X))
            {
                debugSystem.TimeRuler.Visible = true;
                debugSystem.TimeRuler.ShowLog = !debugSystem.TimeRuler.ShowLog;
            }

            // fake a finger flick
#if WINDOWS
            if (input.WasKeyReleased(Microsoft.Xna.Framework.Input.Keys.R))
            {
                inputstate.Gestures.Add(new GestureSample(GestureType.Flick, new TimeSpan(0, 1, 55), new Vector2(), new Vector2(), new Vector2(0, 1500), new Vector2()));
                inputstate.Gestures.Add(new GestureSample(GestureType.DragComplete, new TimeSpan(0, 1, 55), new Vector2(), new Vector2(), new Vector2(0, 0), new Vector2()));
            }
            if (input.WasKeyReleased(Microsoft.Xna.Framework.Input.Keys.F))
            {
                inputstate.Gestures.Add(new GestureSample(GestureType.VerticalDrag, new TimeSpan(0, 1, 55), new Vector2(), new Vector2(), new Vector2(0, -200), new Vector2()));
                inputstate.Gestures.Add(new GestureSample(GestureType.Flick, new TimeSpan(0, 1, 55), new Vector2(), new Vector2(), new Vector2(0, -1500), new Vector2()));
                inputstate.Gestures.Add(new GestureSample(GestureType.DragComplete, new TimeSpan(0, 1, 55), new Vector2(), new Vector2(), new Vector2(0, 0), new Vector2()));
            }
#endif

            // initialize a new grid of letters with N
            if (input.WasKeyReleased(Keys.N))
            {
                debug.TimerStart();
                //wordlogic.SolveWordGrid();
                debug.TimerEnd();
            }
#endif
            particleEngine.Update();
        }

        // format text in a rectangle, returns location of last word
        public void SetTextWidth(ref string text, SpriteFont spriteFont, float width)
        {
            float before = DateTime.Now.Millisecond;

            // get a vector representing the size of our string
            Vector2 textSize = spriteFont.MeasureString(text);

            // if text already fits, don't do anything
            if (textSize.X < width)
                return;

            // remove existing newline chars in case it has already been formatted
            text = text.Replace("\n", "");

            // loop through all characters in string, if a character extends past our width
            float lineTotal = 0;
            float wordTotal = 0;
            int charCount = 0;
            int i = 0;
            do
            {
                // if we find whitespace, we're reached the end of a word
                if (char.IsWhiteSpace(text, i))
                {
                    // measure the length of the word
                    wordTotal = spriteFont.MeasureString(text.Substring(i - charCount, charCount)).X;
                    // if the word extends past our width, we insert
                    // a \n before the word to pop it to the next line
                    if ((lineTotal + wordTotal) > width && lineTotal != 0)
                    {
                        text = text.Insert(i - charCount, "\n");
                        lineTotal = wordTotal;
                        charCount = 0;
                    }
                    // otherwise the word fits on our current line
                    else
                    {
                        lineTotal += wordTotal + 1;
                        charCount = 0;
                        i++;
                    }
                }
                else
                {
                    charCount++;
                    i++;
                }
            } while (i < text.Length);
        }
        public Vector2 SetTextWidth3(ref string text, SpriteFont spriteFont, string lastWord)
        {
            float before = DateTime.Now.Millisecond;
            Vector2 lastWordCoordinate = new Vector2(0, 0);
            int i = 0;
            while (i < text.Length)
            {
                if (text[i] == '\n')
                {
                    lastWordCoordinate.X = 0;
                }
                else
                {
                    lastWordCoordinate.X += spriteFont.MeasureString(text[i].ToString()).X;
                    lastWordCoordinate.X -= 2;
                }
                i++;
            }
            lastWordCoordinate.X -= spriteFont.MeasureString(lastWord + " ").X - 2;
            lastWordCoordinate.Y = spriteFont.MeasureString(text).Y - spriteFont.MeasureString(lastWord + " ").Y;

            return lastWordCoordinate;
        }
        // modified of above function to try and get position of latest word
        public Vector2 SetTextWidth2(ref string text, SpriteFont spriteFont, float width)
        {
            Vector2 lastWordCoordinate = new Vector2(0, 0);
            // get a vector representing the size of our string
            Vector2 textSize = spriteFont.MeasureString(text);

            // if text already fits, don't do anything
            //  lastWordCoordinate.X = textSize.X;
            lastWordCoordinate.X = textSize.X - 70;
            if (textSize.X < width)
                return lastWordCoordinate;

            // remove existing newline chars in case it has already been formatted
            text = text.Replace("\n", "");
            lastWordCoordinate.X = 0;
            // loop through all characters in string, if a character extends past our width
            float lineTotal = 0;
            float wordTotal = 0;
            int charCount = 0;
            int i = 0;
            do
            {
                // if we find whitespace, we're reached the end of a word
                if (char.IsWhiteSpace(text, i))
                {
                    // measure the length of the word
                    wordTotal = spriteFont.MeasureString(text.Substring(i - charCount, charCount)).X;
                    // if the word extends past our width, we insert
                    // a \n before the word to pop it to the next line
                    if ((lineTotal + wordTotal) > width && lineTotal != 0)
                    {
                        text = text.Insert(i - charCount, "\n");
                        lineTotal = wordTotal;
                        charCount = 0;
                        lastWordCoordinate.Y += textSize.Y - 4;
                        lastWordCoordinate.X = 0;

                    }
                    // otherwise the word fits on our current line
                    else
                    {
                        lastWordCoordinate.X += wordTotal + spriteFont.MeasureString(" ").X - 2;
                        string test = text.Substring(i - charCount, charCount);
                        char test2 = text[i];
                        lineTotal += wordTotal + 1;
                        charCount = 0;
                        i++;
                    }
                }
                else
                {
                    string what = text.Substring(i, 1);
                    // lastWordCoordinate.X += spriteFont.MeasureString(what).X;
                    charCount++;
                    i++;
                }
            } while (i < text.Length);
            //lastWordCoordinate.X -= wordTotal + spriteFont.MeasureString(" ").X -4;
            return lastWordCoordinate;
        }

    }
}
