using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.GamerServices;
using System.Text.RegularExpressions;

namespace WordGridGame.Screens
{
    public class GameTransientState
    {
        public char[] grid;
        public float secondsLeftInGame;
        public int score;
        public string[] listOfWords;
        public HighWordScores highestScoringWords;
    }

    class MainGame : GameState
    {
        private Random random;
        private const int UNTOUCHED = -1;
        private const int FALLING = -2;
        private double moonX, moonY;
        private const float ORBITS_RADIUS = 480 * 0.43f;
        private double zenith = 0;
        private double timeOfDay = 0; // 0 to 24

        private float secondsLeftInGame = 120;
        private float timeLimit = 120;
        private int clockMinutes;
        private int clockSeconds;
        private Color clockColor;  //make red if few seconds left
        private float clockScale;
        private float clockX;

        // game information
        private int score;
        private string userWord;
        // used to detect word change, for particle effects on finding a legit word
        private string lastUserWord;

        // tap&drag interface
        private int[,] tapDragGrid;
        private Rectangle[,] tapDragGridRectangle;
        private Vector2[,] gridCenter;
        private int fingerTracker; // to make sure we select boxes that are adjacent
        private Vector2 boxTracker; // track what box the user is touching
        private bool wordMatches;

        // new blocks animation
        private LetterTile[] letterTiles;
        private LetterTile[,] latestTiles; // holds future grid info, where after animations are done
        private List<LetterTile> disappearingTiles;
        private List<LetterTile> listToDelete;
        private bool[,] gridAvailable;
        private Rectangle scissorRect = new Rectangle(0, 288, 480, 480); // so falling new tiles dont cover the sky

        // words in the background
        private List<string> listOfWords;
        private List<Vector2> listOfWordsCoordinates;
        private Vector2 backgroundCursor;

        // private float offsetY; // if user puts in a ton of words, got to move the list upwards to make room
        // animation 
        private string userLastWord = "";
        private float animateLastWord; // counter 
        private Vector2 animateLastWordLocation, animateLastWordTarget; // smoothly move it to background
        private Vector2 animateLastWordSpeed;
        private Vector2 animateSizeSpeed;
        private Vector2 scaleOfLastWord;

        // how many points the word is worth
        private string pointsString;

        // animate the points worth if user chooses word
        private Vector2 animateWorth;
        private int userWordWorth;
        private float animateWorthCounter;
        private Vector2 lastWordXY;
        private float animateWorthAlpha;

        // pause mechanic
        private bool paused;
        private Button pauseButton;
        private RenderTarget2D pauseBackground;
        private Color[] tempData, oldData;
        private bool justPaused;
        private bool tempBackgroundRendered;
        private float pausedImageFade;
        private Button resumeGameButton;
        private Button exitGameButton;
        private Button restartGameButton;

        // animation at the start of the game
        private float readySetGoAnim;
        private float startAnim;

        // rewarding animations for player, new word! & great/excellent/amazing/wow
        private Vector2 newWordPos;
        private bool newWordAnimating;
        private bool newWordDirection; // true = right
        private float newWordPause; // pause so user can actually read it 

        private Vector2 rewardWordPos;
        private bool rewardWordAnimating;
        private bool rewardWordDirection; // true = right
        private float rewardWordPause; // pause so user can actually read it 
        private int clockY;

        // end game stuff
        private bool endGame; // is the game finished
        private float endGameAnimation;
        private Button newGameButton;
        private bool checkedHighScore;

        // hold the 5 highest scoring words
        private HighWordScores highestScoringWords;

        // no time limit mode
        private bool timeLimitModeOn;

        // for keeping game state across activation/deactivation
        private GameTransientState transientState;

        // sound stuff
        private float pitchVolume;

        // debugging
        float debug1 = 0;
        float debug2 = 0;
        float debug3 = 0;
        float debug4 = 0;
        float debug5 = 0;
        float debug6 = 0;
        float debug7 = 0;
        float debug8 = 0;

        // pause logo animation
        float pauseAnimation;

        public MainGame()
            : base()
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            random = new Random();
            gridAvailable = new bool[6, 12];
            tapDragGrid = new int[shared.gridSize, shared.gridSize];
            tapDragGridRectangle = new Rectangle[shared.gridSize, shared.gridSize];

            // touch boxes
            for (int i = 0; i < shared.gridSize; i++)
            {
                for (int j = 0; j < shared.gridSize; j++)
                {
                    int tileSize = (480 / shared.gridSize);
                    int buffer = 10; // the amount of space around the box
                    tapDragGridRectangle[j, i] = new Rectangle((j * tileSize) + buffer, (320 - 32) + (i * tileSize) + buffer, tileSize - (buffer * 2), tileSize - (buffer * 2));
                }
            }

            // find out centers of grid for drawing purposes
            gridCenter = new Vector2[shared.gridSize, shared.gridSize];
            for (int i = 0; i < shared.gridSize; i++)
            {
                for (int j = 0; j < shared.gridSize; j++)
                {
                    int tileSize = (480 / shared.gridSize);
                    gridCenter[j, i] = new Vector2(j * tileSize + (tileSize / 2), (320 - 32) + i * tileSize + (tileSize / 2));
                }
            }

            backgroundCursor = new Vector2(0, 0);
            animateLastWordLocation = new Vector2();
            animateLastWordTarget = new Vector2();
            animateLastWordSpeed = new Vector2();
            animateSizeSpeed = new Vector2();
            scaleOfLastWord = new Vector2();
            animateWorth = new Vector2();
            lastWordXY = new Vector2();

            pauseButton = new Button(shared.textureManager.GetTexture("pauseButton"), 0, 0);
            pauseBackground = new RenderTarget2D(shared.graphicsDevice, 480, 800 - 32);
            pauseButton.rect = new Rectangle(0, 0, 100, 50);
            tempData = new Color[768 * 480];
            oldData = new Color[768 * 480];
            Color pauseMenuButtonColors = new Color(111, 255, 50);
            int align_x = 240;
            resumeGameButton = new Button("resume", align_x, 300, shared.fontManager.GetFont("pointsfont"));
            resumeGameButton.color = pauseMenuButtonColors;

            exitGameButton = new Button("exit to main menu", align_x, 600, shared.fontManager.GetFont("pointsfont"));
            exitGameButton.color = pauseMenuButtonColors;

            restartGameButton = new Button("restart", align_x, 450, shared.fontManager.GetFont("pointsfont"));
            restartGameButton.color = pauseMenuButtonColors;

            newGameButton = new Button("new game", align_x, 550, shared.fontManager.GetFont("pointsfont"));
            newGameButton.color = pauseMenuButtonColors;
            newGameButton.rect.Width += 40;
            newGameButton.rect.Height += 40;

            highestScoringWords = new HighWordScores();
            listOfWordsCoordinates = new List<Vector2>();
            InitGame();
        }

        public override void InitGame()
        {
            letterTiles = new LetterTile[shared.gridSize * shared.gridSize * 2];
            latestTiles = new LetterTile[shared.gridSize, shared.gridSize * 2];
            disappearingTiles = new List<LetterTile>();
            listToDelete = new List<LetterTile>();

            timeOfDay = 18; // 0 to 24
            secondsLeftInGame = 120;
            timeLimit = 120;

            shared.wordlogic.InitializeGrid();

            // copy letters to this class's tiles
            CopyGridToTiles();
            listOfWords = new List<string>();
            listOfWords.Clear();
            backgroundCursor.X = 5;
            backgroundCursor.Y = 40;
            animateLastWord = 0;
            pointsString = "";
            animateWorthCounter = 0;
            paused = false;
            justPaused = false;
            tempBackgroundRendered = false;
            pausedImageFade = 0;

            score = 0;
            clockScale = 0;
            userWord = "";
            readySetGoAnim = 2.5f;
            startAnim = readySetGoAnim + 1;
            fingerTracker = 0;
            EmptyGrid();
            boxTracker.X = -1;
            boxTracker.Y = -1;

            zenith = -(timeOfDay / 12.0f * Math.PI - (Math.PI / 2.0));

            // calculate clock
            clockSeconds = (int)secondsLeftInGame % 60;
            clockMinutes = (int)Math.Floor(secondsLeftInGame / 60.0);
            

            newWordPos = new Vector2(-300, 252);
            newWordAnimating = false;
            rewardWordPos = new Vector2(-300, 222);
            rewardWordAnimating = false;
            clockY = 27;
            endGame = false;
            endGameAnimation = 1;
            highestScoringWords.Clear();
            timeLimitModeOn = shared.saveData.timeLimitMode;
            checkedHighScore = false;
            pitchVolume = 0;
            listOfWordsCoordinates.Clear();
            SetupBackgroundWordsCoordinates();
            pauseAnimation = 1.0f;
            rewardWordPause = 2;
        }

        public override void Update()
        {
            base.Update();

            // TODO take out this debugging 
            if (shared.input.WasKeyReleased(Keys.F))
            {
                secondsLeftInGame = 2;
            }

            if (paused)
            {
                if (tempBackgroundRendered)
                {
                    justPaused = false;
                }
                pauseButton.Update();
                if (pauseButton.Clicked() || shared.input.backPressed)
                {
                    paused = false;
                }
                resumeGameButton.Update();
                if (resumeGameButton.Clicked())
                {
                    paused = false;
                }
                exitGameButton.Update();
                if (exitGameButton.Clicked())
                {
                    if (!endGame)
                    {
                        SaveToStorage();
                    }
                    shared.nextState = Shared.State.MAINMENU;
                }
                restartGameButton.Update();
                if (restartGameButton.Clicked())
                {
                    shared.nextState = Shared.State.FORCETRANSITION;
                    InitGame();
                }
                return;
            }

            pauseButton.Update();
            if (shared.input.backPressed && endGame)
            {
                shared.nextState = Shared.State.MAINMENU;
            }
            else if (pauseButton.Clicked() || shared.forcePause || shared.input.backPressed)
            {
                shared.forcePause = false;
                justPaused = true;
                tempBackgroundRendered = false;
                paused = true;
            }
            if (startAnim > 0)
            {
                startAnim -= shared.gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            }
            if (readySetGoAnim > 0)
            {
                readySetGoAnim -= shared.gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                if (Math.Floor(readySetGoAnim) != Math.Floor(readySetGoAnim + shared.gameTime.ElapsedGameTime.Milliseconds / 1000.0f) && readySetGoAnim > 0)
                {
                    shared.soundManager.Play("startanimation");
                }
                if (readySetGoAnim <= 0)
                {
                    //shared.particleEngine.EmitBigExplosion();
                    shared.soundManager.Play("startanimation", 1.0f, 0.5f, 0f);
                }
                return;
            }

            /// game logic ///
            /// 
            if (secondsLeftInGame <= 0 && timeLimitModeOn)
            {
                secondsLeftInGame = 0;
                endGame = true;

                // add high score if we havent yet
                if (!checkedHighScore)
                {
                    shared.soundManager.Play("endgame");
                    checkedHighScore = true;
                    int highScorePosition = shared.saveData.highScores.CheckScore(score);
                    if (highScorePosition < 10)
                    {
                        string position;
                        string numberEnding;
                        switch (highScorePosition)
                        {
                            case 0:
                                numberEnding = "st";
                                break;
                            case 1:
                                numberEnding = "nd";
                                break;
                            case 2:
                                numberEnding = "rd";
                                break;
                            default:
                                numberEnding = "th";
                                break;
                        }
                        position = (highScorePosition + 1).ToString() + numberEnding;

                        Guide.BeginShowKeyboardInput(0, "New High Score", "You placed " + position + " with " + score.ToString() + " points! \n\n Please enter your name: ", shared.saveData.defaultHighScoreName, HighScoresAdd, null);
                    }

                }
            }
            if (endGame)
            {
                if (endGameAnimation > 0)
                {
                    endGameAnimation -= shared.gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                }

                newGameButton.Update();
                if (newGameButton.Clicked())
                {
                    shared.nextState = Shared.State.FORCETRANSITION;
                    // shared.forceTransitionType = TransitionType.SQUARES;
                    InitGame();
                }
                return;
            }
            RewardAnimations(); // rewarding animations (excellent, wow, awesome etc)
            UserChooseWord(); // user chooses a word logic 
            TileFallingLogic(); // tiles falling logic
            TapDragLogic(); // user selection logic
            ClockAndMoon(); // update moon calculations

            // make word green if word matches
            MatchWordEffects();

            //update letter tiles
            for (int i = 0; i < letterTiles.Length; i++)
            {
                letterTiles[i].Update();
            }

        }
        private void HighScoresAdd(IAsyncResult ar)
        {
            string name = Guide.EndShowKeyboardInput(ar);
            if (name != null)
            {
                name = Regex.Replace(name, @"[^a-zA-Z0-9\s]", string.Empty);
                shared.saveData.defaultHighScoreName = name;
                shared.saveData.highScores.AddScore(shared.saveData.defaultHighScoreName, score, highestScoringWords.highWordScores[0].word, highestScoringWords.highWordScores[0].score);
            }
        }
        private void RewardAnimations()
        {
            if (newWordAnimating)
            {
                if (newWordDirection)
                {
                    if (newWordPos.X < 20)
                        newWordPos.X += 40;
                    else
                    {
                        newWordDirection = false;
                        newWordPause = 1;
                    }
                }
                else
                {
                    if (newWordPause > 0)
                    {
                        newWordPause -= shared.gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                    }
                    else
                    {
                        if (newWordPos.X > -300)
                            newWordPos.X -= 40;
                        else
                            newWordAnimating = false;
                    }
                }
            }
            if (rewardWordAnimating)
            {
                if (rewardWordDirection)
                {
                    if (rewardWordPos.X < 20)
                        rewardWordPos.X += 40;
                    else
                    {
                        rewardWordDirection = false;
                        rewardWordPause = 2;
                    }
                }
                else
                {
                    if (rewardWordPause > 0)
                    {
                        rewardWordPause -= shared.gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                    }
                    else
                    {
                        rewardWordAnimating = false;
                        rewardWordPause = 2;
                    }
                }
            }
        }

        public override void Draw()
        {

            base.Draw();
            shared.graphicsDevice.Clear(new Color(48, 61, 87));

            if (!tempBackgroundRendered && justPaused)
            {
                //shared.spritebatch.Draw(pauseBackground, new Vector2(0, 0), Color.White);
                shared.spritebatch.End();
                shared.graphicsDevice.SetRenderTarget(pauseBackground);
                shared.spritebatch.Begin();
                pausedImageFade = 0;
                resumeGameButton.alpha = exitGameButton.alpha = restartGameButton.alpha = 0;
            }
            else if (paused)
            {
                shared.spritebatch.Draw(pauseBackground, new Vector2(0, 0), Color.White);
                pauseButton.Draw();
                if (pausedImageFade < 1.0f)
                {
                    pausedImageFade += 0.05f;
                    resumeGameButton.alpha += 0.05f;
                    exitGameButton.alpha += 0.05f;
                    restartGameButton.alpha += 0.05f;
                }
                if (pauseAnimation < 1.0f)
                {
                    pauseAnimation += (float)(1 / 30.0) * 5;
                }
                if (pauseAnimation > 1.0f)
                {
                    pauseAnimation = 1.0f;
                }

                shared.spritebatch.Draw(shared.textureManager.GetTexture("paused"), new Vector2(240, 140), null, Color.White * pausedImageFade, 0, new Vector2(252 / 2.0f, 36 / 2.0f), pauseAnimation, 0, 0);
                if (pauseAnimation < 1.0f)
                {
                    resumeGameButton.SetScale(pauseAnimation);
                    exitGameButton.SetScale(pauseAnimation);
                    restartGameButton.SetScale(pauseAnimation);
                }
                
                resumeGameButton.Draw();
                exitGameButton.Draw();
                restartGameButton.Draw();
                shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), "(progress will be saved)", new Vector2(105, 650), Color.White * pausedImageFade, 0, new Vector2(0, 0), pauseAnimation,0,0);
                return;
            }

            // the tiles
            DrawTiles();

            shared.spritebatch.Draw(shared.textureManager.GetTexture("sky"), new Vector2(0, 0), Color.White);
            // the background has words that user entered as answers
            DrawBackgroundWords();
            // draw moon
            DrawMoon();

            float before = DateTime.Now.Millisecond;
            // clock  
            if (timeLimitModeOn)
                DrawClockAndScore();

            // draw user's word
            DrawUserWord();
            AnimateWordToBackground();

            // draw points worth
            DrawFloatingPointsAnimation();

            //draw tapdrag interface
            DrawTapDrag();
            ReadySetGoAnimations();

            // draw rewarding animations
            // don't like this one
            //shared.spritebatch.Draw(shared.textureManager.GetTexture("newword"), newWordPos, Color.White);
            if (rewardWordAnimating)
            {
                float alpha = rewardWordPause / 2.0f;
                if (userWordWorth > 100)
                {
                    shared.spritebatch.Draw(shared.textureManager.GetTexture("wow"), rewardWordPos, Color.White * alpha);
                }
                else if (userWordWorth > 50)
                {
                    shared.spritebatch.Draw(shared.textureManager.GetTexture("awesome"), rewardWordPos, Color.White * alpha);
                }
                else if (userWordWorth > 30)
                {
                    shared.spritebatch.Draw(shared.textureManager.GetTexture("excellent"), rewardWordPos, Color.White * alpha);
                }
                else if (userWordWorth > 15)
                {
                    shared.spritebatch.Draw(shared.textureManager.GetTexture("great"), rewardWordPos, Color.White * alpha);
                }
            }

            if (endGame)
            {
                shared.spritebatch.Draw(shared.textureManager.GetTexture("endgamesquare"), new Vector2(240, 400), null, Color.White * (1 - endGameAnimation), 0, shared.textureManager.GetImageCenter("endgamesquare"), 1.0f, 0, 0);
                Vector2 origin = shared.fontManager.GetFont("endgamepoints").MeasureString(score.ToString()) / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("endgamepoints"), score.ToString(), new Vector2(240, 230), Color.White * (1 - endGameAnimation), 0, origin, 1.0f, 0, 0);
                newGameButton.Draw();
                origin = shared.fontManager.GetFont("orangefont").MeasureString("highest scoring words") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), "highest scoring words", new Vector2(240, 300), Color.White * (1 - endGameAnimation), 0, origin, 1.0f, 0, 0);

                for (int i = 0; i < 5; i++)
                {
                    if (highestScoringWords.highWordScores[i].word != "")
                    {
                        string str = highestScoringWords.highWordScores[i].word + " (" + highestScoringWords.highWordScores[i].score.ToString() + " points)";
                        origin = shared.fontManager.GetFont("whiteclearfont").MeasureString(str) / 2.0f;
                        shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), str, new Vector2(240, 330 + i * 40), Color.White * (1 - endGameAnimation), 0, origin, 1.0f, 0, 0);
                    }
                }
            }

            if (shared.input.IsMousePressed)
            {
                //shared.spritebatch.Draw(shared.textureManager.GetTexture("wow"), new Vector2(50, 50), Color.White);
            }
            /*
            shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), debug1.ToString(), new Vector2(50, 100), Color.White);
            shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), debug2.ToString(), new Vector2(50, 150), Color.White);
            shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), debug3.ToString(), new Vector2(50, 200), Color.White);
            shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), debug4.ToString(), new Vector2(50, 250), Color.White);
            shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), debug5.ToString(), new Vector2(50, 300), Color.White);
            shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), debug6.ToString(), new Vector2(50, 350), Color.White);
            shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), debug7.ToString(), new Vector2(50, 400), Color.White);
            shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), debug8.ToString(), new Vector2(50, 450), Color.White);
             */
            // pause buton
            if (paused)
            {
                PausedScreen();
            }
            else
            {
                pauseButton.Draw();
            }


        }

        private void AnimateWordToBackground()
        {
            if (animateLastWord >= 0)
            {
                if (animateLastWord >= 2)
                    shared.spritebatch.DrawString(shared.fontManager.GetFont("bigfont"), userLastWord, animateLastWordLocation, Color.White, 0, new Vector2(0, 0), scaleOfLastWord, 0, 0);
                scaleOfLastWord -= animateSizeSpeed;
                animateLastWord -= 2.0f;
            }
            else
            {
                userLastWord = "";
            }
            shared.spritebatch.DrawString(shared.fontManager.GetFont("pointsfont"), pointsString, new Vector2(295, 240), Color.White);
        }

        private void DrawFloatingPointsAnimation()
        {
            if (animateWorthCounter > 0)
            {
                SpriteFont font = shared.fontManager.GetFont("normalpoints");
                if (userWordWorth > 100)
                {
                    font = shared.fontManager.GetFont("wowpoints");
                }
                else if (userWordWorth > 50)
                {
                    font = shared.fontManager.GetFont("awesomepoints");
                }
                else if (userWordWorth > 30)
                {
                    font = shared.fontManager.GetFont("excellentpoints");
                }
                else if (userWordWorth > 15)
                {
                    font = shared.fontManager.GetFont("greatpoints");
                }
                string pointsWorth = userWordWorth.ToString();
                int width = (int)font.MeasureString(pointsWorth).X + 5;
                Vector2 origin = new Vector2(font.MeasureString(pointsWorth).X / 2.0f, font.MeasureString(pointsWorth).Y / 2.0f);
                //    shared.spritebatch.Draw(shared.textureManager.GetTexture("pointsBox"), new Rectangle((int)animateWorth.X, (int)animateWorth.Y, width, 30), null, Color.White * animateWorthAlpha, 0, new Vector2(27, 15), 0, 0);
                if (animateWorth.X + (width / 2.0) > 480)
                {
                    animateWorth.X -= (float)(animateWorth.X + (width / 2.0) - 480);
                    animateWorth.X -= 10;
                }
                if (animateWorth.X - (width/2.0) < 0)
                {
                    animateWorth.X += (float)(width / 2.0);
                    animateWorth.X += 10;
                }
                shared.spritebatch.DrawString(font, pointsWorth, animateWorth, Color.White * animateWorthAlpha, 0,
                    origin, 1.25f, 0, 0);
                animateWorthCounter -= 1.0f;
                animateWorth.Y -= 2.0f;
                animateWorthAlpha -= 0.01f;
            }
            else
            {
                animateWorthCounter = 0;
            }
        }

        private void PausedScreen()
        {
            if (!tempBackgroundRendered && justPaused)
            {
                shared.spritebatch.End();
                shared.graphicsDevice.SetRenderTarget(null);
                shared.spritebatch.Begin();
                pauseBackground.GetData(tempData);

                // grayscale the image
                for (int i = 0; i < tempData.Length; i++)
                {
                    oldData[i] = tempData[i];
                    int x = i % 768;
                    int y = (i - x) / 480;
                    x += 1;
                    y += 1;

                    //float r = 0, g = 0, b = 0;
                    Color old = tempData[i];

                    byte color = (byte)(0.3f * old.R + 0.59f * old.G + 0.11f * old.B);
                    float intensity = 1.0f;
                    float brightness = 0.45f;
                    tempData[i].R = (byte)((color * intensity + old.R * (1 - intensity)) * brightness); // 0.75 to lower the brightness a bit
                    tempData[i].G = (byte)((color * intensity + old.G * (1 - intensity)) * brightness); // to see the buttons
                    tempData[i].B = (byte)((color * intensity + old.B * (1 - intensity)) * brightness);

                }
                pauseBackground.SetData(tempData);
                tempBackgroundRendered = true;
                pauseAnimation = 0;
            }
            shared.spritebatch.Draw(pauseBackground, new Vector2(0, 0), Color.White);
        }

        private void ReadySetGoAnimations()
        {
            Vector2 pos = new Vector2(0, 0);
            if (readySetGoAnim > 0)
            {
                int num = (int)readySetGoAnim + 1;
                if (num > 2)
                    return;

                Vector2 origin = new Vector2();
                Texture2D temp = shared.textureManager.GetTexture("3");
                float decimal_ = readySetGoAnim - (float)Math.Floor(readySetGoAnim);
                switch (num)
                {
                    case 3:
                        temp = shared.textureManager.GetTexture("3");
                        break;
                    case 2:
                        temp = shared.textureManager.GetTexture("2");
                        break;
                    case 1:
                        temp = shared.textureManager.GetTexture("1");
                        break;
                }
                pos.X = 240;
                pos.Y = 325;
                origin.X = temp.Width / 2.0f;
                origin.Y = temp.Height / 2.0f;
                shared.spritebatch.Draw(temp, pos, null, Color.White, 0, origin, 1.0f, 0, 0);
                if ((1 - decimal_) < 0.1)
                    shared.spritebatch.Draw(temp, pos, null, Color.White * (decimal_ / 2.0f), 0, origin, 1.0f + 8 * (1 - decimal_), 0, 0);

            }
            if (startAnim > 0 && startAnim < 1)
            {
                Texture2D temp = shared.textureManager.GetTexture("start");
                pos = new Vector2(240, 250 - (1 - (startAnim * 50)));
                Vector2 origin = new Vector2(temp.Width / 2, temp.Height / 2);
                shared.spritebatch.Draw(temp, pos, null, Color.White * startAnim, 0, origin, 1.0f, 0, 0);
            }
        }

        private void DrawTiles()
        {
            // need seperate state so new unseen tiles don't show
            //  shared.spritebatch.End();
            // shared.spritebatch.GraphicsDevice.ScissorRectangle = scissorRect;
            //RasterizerState state = new RasterizerState();
            //state.ScissorTestEnable = true;
            //shared.spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.Default, state);

            //draw letter tiles
            for (int i = 0; i < letterTiles.Length; i++)
            {
                letterTiles[i].paused = paused;
                letterTiles[i].Draw();
            }
            //            shared.spritebatch.End();
            //           shared.spritebatch.Begin();
        }

        private void SetupBackgroundWordsCoordinates()
        {
            listOfWordsCoordinates.Clear();
            int X = 10;
            int Y = 40;
            foreach (string s in listOfWords)
            {
                if (X + (int)shared.fontManager.GetFont("backgroundfont").MeasureString(s).X > 470)
                {
                    Y += 30;
                    X = 10;
                }
                listOfWordsCoordinates.Add(new Vector2(X, Y));
                X += (int)shared.fontManager.GetFont("backgroundfont").MeasureString(s).X;
                X += 5;
            }
            // list of words gets too big, clear everything
            if (Y > 240)
            {
                listOfWords.Clear();
                listOfWordsCoordinates.Clear();
            }
        }
        private void DrawBackgroundWords()
        {
            if (listOfWords.Count <= 0)
                return;
            int lastWordIndex = listOfWords.Count - 1;
            for (int i = 0; i < listOfWords.Count - 1; i++)
            {
                shared.spritebatch.DrawString(shared.fontManager.GetFont("backgroundfont"), listOfWords[i], listOfWordsCoordinates[i], Color.DimGray);
            }

            if (animateLastWord < 2)
            {
                if (animateLastWord <= 0)
                {
                    shared.spritebatch.DrawString(shared.fontManager.GetFont("backgroundfont"),
                   listOfWords[lastWordIndex], listOfWordsCoordinates[lastWordIndex], Color.DimGray);
                }
                else
                {
                    shared.spritebatch.DrawString(shared.fontManager.GetFont("backgroundfont"),
                   listOfWords[lastWordIndex], listOfWordsCoordinates[lastWordIndex], Color.White);
                }

            }
            else
            {
                if (animateLastWord == 30) // we are just starting to animate 
                {
                    animateLastWordTarget.X = listOfWordsCoordinates[lastWordIndex].X;
                    animateLastWordTarget.Y = listOfWordsCoordinates[lastWordIndex].Y;

                    animateLastWordSpeed = new Vector2((animateLastWordTarget.X - animateLastWordLocation.X) / 15.0f,
                        (animateLastWordTarget.Y - animateLastWordLocation.Y) / 15.0f);
                }
                animateLastWordLocation += animateLastWordSpeed;
            }

        }

        private void TapDragLogic()
        {
            if (shared.input.IsMousePressed)
            {
                for (int i = 0; i < shared.gridSize; i++)
                {
                    for (int j = 0; j < shared.gridSize; j++)
                    {
                        if (tapDragGridRectangle[j, i].Contains(shared.input.MouseX, shared.input.MouseY))
                        {
                            LetterTile tile;// = GetTileXY(j, i + 6);
                            tile = latestTiles[j, i + shared.gridSize];
                            if (tile != null && tile.state != LetterTile.AnimatingState.ACTIVE)
                                continue;

                            boxTracker.X = j;
                            boxTracker.Y = i;
                            // first tap
                            if (tapDragGrid[j, i] == UNTOUCHED && fingerTracker == 0)
                            {
                                AddLetter(j, i);
                                shared.soundManager.Play("tileSelect", 1.0f, pitchVolume - 0.1f, 0f);
                            }
                            // a new letter
                            if (tapDragGrid[j, i] == UNTOUCHED && IsLastAdjacent(j, i))
                            {
                                AddLetter(j, i);
                                shared.soundManager.Play("tileSelect", 1.0f, pitchVolume - 0.1f, 0f);
                            }
                            // going back a letter
                            if (tapDragGrid[j, i] == fingerTracker - 1)
                            {
                                RemoveLastSelection(j, i, fingerTracker - 1);
                                fingerTracker--;
                                userWord = userWord.Remove(userWord.Length - 1, 1);
                            }
                            lastWordXY.X = j * 80 + 40;
                            lastWordXY.Y = (320 - 32) + ((i) * 80) + 40;
                        }
                    }
                }
            }
            else
            {
                pitchVolume = 0;
                userWord = "";
                fingerTracker = 0;
                EmptyGrid();
                boxTracker.X = -1;
                boxTracker.Y = -1;
            }
        }
        private void DrawClockAndScore()
        {
            string clock = clockMinutes.ToString() + ":" + clockSeconds.ToString().PadLeft(2, '0');
            if (secondsLeftInGame <= 10)
            {
                clockColor = Color.Red;
                clockScale = (float)2.0;
                clockX = 230;
                clockY = 27 + 15;
            }
            else
            {
                clockScale = 1;
                clockColor = Color.White;
                clockX = 230;
            }


            //score
            Vector2 origin = shared.fontManager.GetFont("scorefont").MeasureString(clock) / 2;
            shared.spritebatch.DrawString(shared.fontManager.GetFont("scorefont"), clock, new Vector2(clockX, clockY), clockColor, 0, origin, clockScale, 0, 0);
            shared.spritebatch.DrawString(shared.fontManager.GetFont("scorefont"), score.ToString().PadLeft(8), new Vector2(350, 5), Color.White);
        }
        private void DrawTapDrag()
        {
            for (int i = 0; i < shared.gridSize; i++)
            {
                for (int j = 0; j < shared.gridSize; j++)
                {
                    if (tapDragGrid[j, i] != UNTOUCHED)
                    {
                        shared.spritebatch.Draw(shared.textureManager.GetTexture("clickcircle"), gridCenter[j, i], null, Color.White, 0, new Vector2(20, 20), 1, 0, 0);
                        DrawLinesAndDots(j, i);
                    }
                    LetterTile tile = GetTileXY(j, i + shared.gridSize);
                    //if (tile != null)
                    //  shared.spritebatch.DrawString(shared.fontManager.GetFont("Courier New"), tile.state.ToString(), gridCenter[j, i] + new Vector2(-15, 15), Color.Tomato);
                }
            }
        }
        private void DrawUserWord()
        {
            Vector2 origin = shared.fontManager.GetFont("bigfont").MeasureString(userWord) / 2;
            if (wordMatches)
            {
                shared.spritebatch.DrawString(shared.fontManager.GetFont("bigfont"), userWord, new Vector2(240, 160), Color.LightGreen, 0, origin, 1, 0, 0);
            }
            else
            {
                shared.spritebatch.DrawString(shared.fontManager.GetFont("bigfont"), userWord, new Vector2(240, 160), Color.White, 0, origin, 1, 0, 0);
            }
            if (userWord.Length >= 3 && shared.wordlogic.IsValidWord(userWord.ToUpper()))
            {
                pointsString = CalcPoints().ToString() + " points";
            }
            else
            {
                pointsString = "";
            }
        }
        private int CalcPoints()
        {
            int points = 0;
            for (int i = 0; i < userWord.Length; i++)
            {
                points += shared.wordlogic.GetLetterWorth(userWord[i].ToString().ToUpper()[0]);
            }
            if (userWord.Length <= 3)
            {
                return points * 1;
            }
            else if (userWord.Length <= 4)
            {
                return points * 2;
            }
            else if (userWord.Length <= 5)
            {
                return points * 4;
            }
            else if (userWord.Length <= 6)
            {
                return points * 7;
            }
            else
            {
                return points * (7 + (4 * (userWord.Length - 6)));
            }
        }
        // draw the moon, position based on time left
        private void DrawMoon()
        {
            zenith = -(timeOfDay / 12.0f * Math.PI - (Math.PI / 2.0));
            moonX = 480 * 0.5f + Math.Cos(zenith - Math.PI) * ORBITS_RADIUS - (37 / 2.0);
            moonY = 200 * 1.2f + Math.Sin(zenith - Math.PI) * ORBITS_RADIUS - (37 / 2.0);
            shared.spritebatch.Draw(shared.textureManager.GetTexture("moon"), new Vector2((float)moonX, (float)moonY), Color.White);
        }
        // let user know what they selected
        private void DrawLinesAndDots(int x, int y)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    int dx = x + j;
                    int dy = y + i;
                    if (dx < 0 || dx >= shared.gridSize || dy < 0 || dy >= shared.gridSize)
                        continue;

                    if (tapDragGrid[dx, dy] == (tapDragGrid[x, y] - 1))
                    {
                        if (dy == y)
                        {
                            if (dx == x + 1)
                                shared.spritebatch.Draw(shared.textureManager.GetTexture("horizontal_linker"), gridCenter[dx, dy] + new Vector2(-40, 0), null, Color.White, 0, new Vector2(44, 7), 1, 0, 0);
                            if (dx == x - 1)
                                shared.spritebatch.Draw(shared.textureManager.GetTexture("horizontal_linker"), gridCenter[dx, dy] + new Vector2(40, 0), null, Color.White, 0, new Vector2(44, 7), 1, 0, 0);
                        }

                        if (dx == x)
                        {
                            if (dy == y + 1)
                                shared.spritebatch.Draw(shared.textureManager.GetTexture("horizontal_linker"), gridCenter[dx, dy] + new Vector2(0, -40), null, Color.White, (float)(Math.PI / 2.0), new Vector2(44, 7), 1, 0, 0);
                            if (dy == y - 1)
                                shared.spritebatch.Draw(shared.textureManager.GetTexture("horizontal_linker"), gridCenter[dx, dy] + new Vector2(0, 40), null, Color.White, (float)(Math.PI / 2.0), new Vector2(44, 7), 1, 0, 0);
                        }

                        if (dx != x && dy != y)
                        {
                            if (dx == x - 1 && dy == y - 1)
                                shared.spritebatch.Draw(shared.textureManager.GetTexture("diagonal_linker"), gridCenter[x, y] + new Vector2(-40, -40), null, Color.White, (float)(Math.PI / 4.0), new Vector2(62, 7), 1, 0, 0);
                            else if (dx == x - 1 && dy == y + 1)
                                shared.spritebatch.Draw(shared.textureManager.GetTexture("diagonal_linker"), gridCenter[x, y] + new Vector2(-40, 40), null, Color.White, -1 * (float)(Math.PI / 4.0), new Vector2(62, 7), 1, 0, 0);
                            else if (dx == x + 1 && dy == y - 1)
                                shared.spritebatch.Draw(shared.textureManager.GetTexture("diagonal_linker"), gridCenter[x, y] + new Vector2(40, -40), null, Color.White, -1 * (float)(Math.PI / 4.0), new Vector2(62, 7), 1, 0, 0);
                            else if (dx == x + 1 && dy == y + 1)
                                shared.spritebatch.Draw(shared.textureManager.GetTexture("diagonal_linker"), gridCenter[x, y] + new Vector2(40, 40), null, Color.White, (float)(Math.PI / 4.0), new Vector2(62, 7), 1, 0, 0);
                        }/**/

                    }

                }
            }

        }
        // used for drawing letters
        private Rectangle GetLetterFromTexture(char letter)
        {
            int l = ((short)letter) - 65;
            if (l >= 0 && l <= 6)
            {
                return new Rectangle(l * 130, 0, 130, 130);
            }
            else if (l >= 7 && l <= 13)
            {
                return new Rectangle((l - 7) * 130, 130, 130, 130);
            }
            else if (l >= 14 && l <= 20)
            {
                return new Rectangle((l - 14) * 130, 260, 130, 130);
            }
            else if (l >= 21 && l <= 25)
            {
                return new Rectangle((l - 21) * 130, 390, 130, 130);
            }
            return new Rectangle();
        }
        // add a letter to the user's current word
        private void AddLetter(int x, int y)
        {
            tapDragGrid[x, y] = fingerTracker + 1;
            fingerTracker++;
            userWord += shared.wordlogic.GetLetter(x, y).ToString().ToLower();
            if (shared.wordlogic.GetLetter(x, y).ToString() == "Q")
            {
                userWord += "u";
            }
            pitchVolume += 0.2f;
            if (pitchVolume >= 1.1f)
                pitchVolume = 1.1f;
        }
        // user is going back a tile
        private void RemoveLastSelection(int x, int y, int f)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    int dx = x + j;
                    int dy = y + i;
                    if (dx < 0 || dx >= shared.gridSize || dy < 0 || dy >= shared.gridSize)
                        continue;

                    if (tapDragGrid[dx, dy] == fingerTracker)
                    {
                        tapDragGrid[dx, dy] = UNTOUCHED;
                        return;
                    }

                }
            }
        }
        // can only drag to boxes that are adjacent
        private bool IsLastAdjacent(int x, int y)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    int dx = x + j;
                    int dy = y + i;
                    if (dx < 0 || dx >= shared.gridSize || dy < 0 || dy >= shared.gridSize)
                        continue;

                    if (tapDragGrid[dx, dy] == fingerTracker)
                        return true;
                }
            }

            return false;
        }
        private void EmptyGrid()
        {
            for (int i = 0; i < shared.gridSize; i++)
            {
                for (int j = 0; j < shared.gridSize; j++)
                {
                    tapDragGrid[i, j] = UNTOUCHED;
                }
            }
        }
        private void SetTilesAboveAsFalling(int x, int y)
        {
            int tile_Y = y - 1;
            while (tile_Y >= 0)
            {
                LetterTile tile = GetTileXY(x, tile_Y);
                if (tile != null && tile.state == LetterTile.AnimatingState.ACTIVE)
                    tile.SetFalling();

                tile_Y--;
            }
        }

        private void AddTileToColumn(int x)
        {
            LetterTile freeTile = null;
            for (int i = 0; i < letterTiles.Length; i++)
            {
                if (letterTiles[i].state == LetterTile.AnimatingState.INACTIVE)
                {
                    freeTile = letterTiles[i];

                    break;
                }
            }
            // add a tile to one of the 6 slots
            int y = 5;
            while (y >= 0)
            {
                LetterTile tile = latestTiles[x, y];

                if (tile != null)
                {
                    if (tile.state == LetterTile.AnimatingState.INACTIVE)
                    {
                        freeTile.Initialize(x, y, shared.wordlogic.RandomLetter());
                        latestTiles[x, y] = freeTile;
                        latestTiles[x, y].SetTileFromAbove();
                        break;
                    }
                    y--;
                }
                else
                {
                    freeTile.Initialize(x, y, shared.wordlogic.RandomLetter());
                    latestTiles[x, y] = freeTile;
                    latestTiles[x, y].SetTileFromAbove();
                    break;
                }

            }

        }

        private LetterTile GetTileXY(int x, int y)
        {
            return latestTiles[x, y];
        }
        private bool TileIsDisappearing(int x, int y)
        {
            foreach (LetterTile l in disappearingTiles)
            {
                if (l.MatchXY(x, y))
                    return true;
            }
            return false;
        }
        private void ClearGridAvailable()
        {
            for (int i = 0; i < shared.gridSize * 2; i++)
            {
                for (int j = 0; j < shared.gridSize; j++)
                {
                    gridAvailable[j, i] = false;
                }
            }
        }
        private void TileFallingLogic()
        {
            foreach (LetterTile t in disappearingTiles)
            {
                if (t.state != LetterTile.AnimatingState.DISAPPEARING)
                {
                    listToDelete.Add(t);
                }
            }
            foreach (LetterTile t in listToDelete)
            {
                disappearingTiles.Remove(t);
            }
            listToDelete.Clear();

            // falling logic, place tiles far as low as possible without overlapping other tiles
            for (int i = (shared.gridSize * 2) - 1; i >= 0; i--)
            {
                for (int j = shared.gridSize - 1; j >= 0; j--)
                {
                    if (latestTiles[j, i] != null)
                    {
                        int y = 11;
                        while (y > i)
                        {
                            if (latestTiles[j, y] == null && !TileIsDisappearing(j, y))
                            {
                                latestTiles[j, i].SetDestination(j, y);
                                latestTiles[j, y] = latestTiles[j, i];
                                latestTiles[j, i] = null;
                                latestTiles[j, y].SetFalling();
                                if (y >= shared.gridSize)
                                    shared.wordlogic.grid[j, y - shared.gridSize] = latestTiles[j, y].letter;
                                if (i >= shared.gridSize)
                                {
                                    shared.wordlogic.grid[j, i - shared.gridSize] = ' ';
                                }

                                break;
                            }
                            y--;
                        }

                    }
                }
            }
        }

        private void UserChooseWord()
        {
            if (shared.input.WasMouseJustReleased)
            {
                if (userWord.Length >= 3 && shared.wordlogic.IsValidWord(userWord.ToUpper()))
                {
                    shared.soundManager.Play("good");
                    score += CalcPoints(); // add word score to total points
                    userWordWorth = CalcPoints();
                    highestScoringWords.AddScore(userWordWorth, userWord.ToUpper());
                    animateWorthCounter = 30;
                    animateWorthAlpha = 1.0f;

                    // clear selection interface 
                    ClearGridAvailable();
                    listOfWords.Add(userWord);

                    // set off animations
                    SetOffFallingAnimations();
                    userLastWord = string.Copy(userWord);

                    // animate it to the background
                    animateLastWordLocation.X = 240 - (shared.fontManager.GetFont("bigfont").MeasureString(userWord).X / 2.0f);
                    animateLastWordLocation.Y = 160 - (shared.fontManager.GetFont("bigfont").MeasureString(userWord).Y / 2.0f);
                    animateSizeSpeed.X = (1 - (shared.fontManager.GetFont("backgroundfont").MeasureString(userWord).X /
                        shared.fontManager.GetFont("bigfont").MeasureString(userWord).X)) / 14.0f;
                    animateSizeSpeed.Y = (1 - (shared.fontManager.GetFont("backgroundfont").MeasureString(userWord).Y /
                        shared.fontManager.GetFont("bigfont").MeasureString(userWord).Y)) / 14.0f;
                    animateLastWord = 30;
                    scaleOfLastWord = new Vector2(1, 1);
                    // put word in background
                    SetupBackgroundWordsCoordinates();

                    rewardWordAnimating = true;
                    rewardWordPos.X = -300;
                    rewardWordDirection = true; // going right first
                    
                      shared.saveData.totalWordsFound++;
                      if (userWordWorth > shared.saveData.bestScoringWord.score)
                      {
                          shared.saveData.bestScoringWord.score = userWordWorth;
                          shared.saveData.bestScoringWord.word = userWord;
                      }
                      if (!shared.saveData.IsWordFound(shared.wordlogic.GetWordIndex(userWord.ToUpper())))
                      {
                          shared.saveData.SetWordFound(shared.wordlogic.GetWordIndex(userWord.ToUpper()));
                          shared.saveData.uniqueWordsFound++;
                          if (userWord.Length < 8)
                              shared.saveData.letterWordsFound[userWord.Length]++;
                          else if (userWord.Length >= 8)
                              shared.saveData.letterWordsFound[8]++;
                      }
                      
                    if (userWordWorth > 100)
                    {
                        shared.soundManager.Play("reward", 1.0f, 0.8f, 0.0f);
                    }
                    else if (userWordWorth > 50)
                    {
                        shared.soundManager.Play("reward", 1.0f, 0.6f, 0.0f);
                    }
                    else if (userWordWorth > 30)
                    {
                        shared.soundManager.Play("reward", 1.0f, 0.4f, 0.0f);
                    }
                    else if (userWordWorth > 15)
                    {
                        shared.soundManager.Play("reward", 1.0f, 0.2f, 0.0f);
                    }
                }
                else if (userWord.Length >= 3)
                    shared.soundManager.Play("nogood");
            }

        }

        private void SetOffFallingAnimations()
        {
            for (int i = shared.gridSize - 1; i >= 0; i--)
            {
                for (int j = shared.gridSize - 1; j >= 0; j--)
                {
                    if (tapDragGrid[j, i] != UNTOUCHED)
                    {
                        LetterTile tile = GetTileXY(j, i + shared.gridSize);
                        if (tile == null)
                            continue;
                        animateWorth = lastWordXY;
                        tile.Disappear();
                        disappearingTiles.Add(tile);
                        latestTiles[j, i + shared.gridSize] = null;
                        AddTileToColumn(j);

                        //SetTilesAboveAsFalling(j, i + 6);
                        shared.wordlogic.grid[j, i] = ' ';
                    }
                }
            }
        }

        private void ClockAndMoon()
        {
            if (timeOfDay >= 6)
                timeOfDay = -6;
            timeOfDay = ((timeLimit - secondsLeftInGame) / timeLimit) * 12 + -6;

            // count down time
            secondsLeftInGame -= (float)shared.gameTime.ElapsedGameTime.TotalSeconds;

            if (secondsLeftInGame <= 0)
            {
                secondsLeftInGame = 0;
            }

            // calculate clock
            clockSeconds = (int)secondsLeftInGame % 60;
            clockMinutes = (int)Math.Floor(secondsLeftInGame / 60.0);
        }

        private void MatchWordEffects()
        {
            wordMatches = false;
            if (userWord.Length >= 3 && shared.wordlogic.IsValidWord(userWord.ToUpper()))
            {
                wordMatches = true;
                if (lastUserWord != userWord)
                {
                    //  shared.soundManager.Play("matchword",0.8f,0f,0f);
                    shared.particleEngine.EmitStars((int)(boxTracker.X * 80) + 40, 320 + (int)(boxTracker.Y * 80));
                }
            }
            lastUserWord = userWord;
        }


        public override void SaveToStorage()
        {
            if (endGame) // if game finished, forget it
                return;
            transientState = new GameTransientState();
            transientState.grid = new char[shared.gridSize * shared.gridSize];
            for (int i = 0; i < shared.gridSize; i++)
            {
                for (int j = 0; j < shared.gridSize; j++)
                {
                    transientState.grid[i * shared.gridSize + j] = shared.wordlogic.grid[i, j];
                }
            }

            transientState.secondsLeftInGame = secondsLeftInGame;
            transientState.listOfWords = new string[listOfWords.Count];
            listOfWords.CopyTo(transientState.listOfWords);
            transientState.score = score;
            transientState.highestScoringWords = highestScoringWords;

#if WINDOWS
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain();
#else
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();
#endif


            XmlSerializer serializer = new XmlSerializer(typeof(GameTransientState));
            using (IsolatedStorageFileStream stream = savegameStorage.CreateFile("gamestate.dat"))
            {

                serializer.Serialize(stream, transientState);
                stream.Close();
                stream.Dispose();
                shared.saveData.isSavedGameAvailable = true;
            }

        }

        public override bool LoadFromStorage()
        {

#if WINDOWS
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
#else
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
#endif
            XmlSerializer serializer = new XmlSerializer(typeof(GameTransientState));
            GameTransientState transientState;
            if (storage.FileExists("gamestate.dat"))
            {

                IsolatedStorageFileStream stream =

                storage.OpenFile("gamestate.dat", FileMode.Open);


                transientState = serializer.Deserialize(stream) as GameTransientState;
                stream.Close();
                stream.Dispose();

                // we don't need the state anymore
                storage.DeleteFile("gamestate.dat");


                for (int i = 0; i < shared.gridSize; i++)
                {
                    for (int j = 0; j < shared.gridSize; j++)
                    {
                        shared.wordlogic.grid[i, j] = transientState.grid[i * shared.gridSize + j];
                    }
                }
                secondsLeftInGame = transientState.secondsLeftInGame;
                for (int i = 0; i < transientState.listOfWords.Length; i++)
                {
                    listOfWords.Add(transientState.listOfWords[i]);
                }
                score = transientState.score;
                readySetGoAnim = startAnim = 0;
                CopyGridToTiles();
                highestScoringWords = transientState.highestScoringWords;
                shared.saveData.isSavedGameAvailable = false;
                SetupBackgroundWordsCoordinates();
                return true;
            }
            return false;
        }

        public override void DeleteSavedGame()
        {
#if WINDOWS
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
#else
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
#endif
            if (storage.FileExists("gamestate.dat"))
            {
                storage.DeleteFile("gamestate.dat");
            }

            shared.saveData.isSavedGameAvailable = false;
        }

        /// <summary>
        /// copy letters to this class's tiles
        /// </summary>
        private void CopyGridToTiles()
        {
            int counter = 0;
            for (int i = 0; i < shared.gridSize * 2; i++)
            {
                for (int j = 0; j < shared.gridSize; j++)
                {
                    if (counter >= shared.gridSize * shared.gridSize)
                    {
                        letterTiles[counter] = new LetterTile(j, i, shared.wordlogic.GetLetter(j, i - shared.gridSize));
                        latestTiles[j, i] = letterTiles[counter];
                    }
                    else
                    {
                        letterTiles[counter] = new LetterTile(j, i, '0');
                        latestTiles[j, i] = null;
                    }
                    gridAvailable[j, i] = true;
                    counter++;
                }
            }
        }
    }
}
