using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

namespace WordGridGame.Screens
{
    class MainMenu : GameState
    {
        enum ButtonName { STARTGAME, OPTIONS, STATS, TUTORIAL, EXIT, RESUME, BUY }
        private SpriteFont font;
        private Star[] stars;
        private const int NUM_STARS = 150;
        private Button[] buttons;
        public MainMenu() : base()
        {
  
        }

        public override void Initialize()
        {
            base.Initialize();
            buttons = new Button[7];
            font = shared.fontManager.GetFont("testfont");
            float align_x;
            align_x = 240;
            
            buttons[(int)ButtonName.STARTGAME] = new Button("new game", (int)align_x, 375);
            buttons[(int)ButtonName.OPTIONS] = new Button("options", (int)align_x, 375 + 60 + 60);
            buttons[(int)ButtonName.STATS] = new Button("high scores", (int)align_x, 375 + 60);
            buttons[(int)ButtonName.TUTORIAL] = new Button("tutorial", (int)align_x, 375 + 60 + 60 + 60);
            buttons[(int)ButtonName.EXIT] = new Button("exit", (int)align_x, 375 + 60 + 60 + 60 + 60);
            buttons[(int)ButtonName.RESUME] = new Button("resume", (int)align_x, 375 - 60);
            buttons[(int)ButtonName.BUY] = new Button("buy", (int)align_x + 150, 375 + 60 + 60 + 60 + 60 + 90);
            stars = new Star[NUM_STARS];
            for (int i = 0; i < NUM_STARS; i++)
            {
                stars[i] = new Star();
                stars[i].Initialize();
                stars[i].Spawn();
            }
        }


        public override void Update()
        {
            if (shared.input.backPressed)
            {
                shared.game.Exit();
            }
            //stars in background
            foreach (Star i in stars)
            {
                i.Update();
            }
            foreach (Button b in buttons)
            {
                if (!shared.isTrialMode && b.GetString() == "buy")
                {
                    continue;
                }

                if (b.GetString() == "resume")
                    if (!shared.saveData.isSavedGameAvailable)
                        continue;

                b.Update();
                if (b.Clicked())
                {
                    switch (b.GetString())
                    {
                        case "new game":
                            shared.zoomText.zoom = true;
                            shared.zoomText.pos = buttons[(int)ButtonName.STARTGAME].GetPosition();
                            shared.zoomText.text = buttons[(int)ButtonName.STARTGAME].GetString();
                            shared.nextState = Shared.State.GAME;
                            break;
                        case "options":
                            shared.zoomText.zoom = true;
                            shared.zoomText.pos = buttons[(int)ButtonName.OPTIONS].GetPosition();
                            shared.zoomText.text = buttons[(int)ButtonName.OPTIONS].GetString();
                            shared.nextState = Shared.State.OPTIONS;
                            break;
                        case "high scores":
                            shared.zoomText.zoom = true;
                            shared.zoomText.pos = buttons[(int)ButtonName.STATS].GetPosition();
                            shared.zoomText.text = buttons[(int)ButtonName.STATS].GetString();
                            shared.nextState = Shared.State.HIGHSCORES;
                            break;
                        case "tutorial":
                            shared.zoomText.zoom = true;
                            shared.zoomText.pos = buttons[(int)ButtonName.TUTORIAL].GetPosition();
                            shared.zoomText.text = buttons[(int)ButtonName.TUTORIAL].GetString();
                            shared.nextState = Shared.State.TUTORIAL;
                            break;
                        case "exit":
                            shared.game.Exit();
                            break;
                        case "resume":
                            shared.zoomText.zoom = true;
                            shared.zoomText.pos = buttons[(int)ButtonName.RESUME].GetPosition();
                            shared.zoomText.text = buttons[(int)ButtonName.RESUME].GetString();
                            shared.nextState = Shared.State.RESUME;
                            break;
                        case "buy":
                            Guide.ShowMarketplace(0);
                            shared.checkTrial = true;
                            break;
                    }
                }
                
            }
            base.Update();

        }

        public override void Draw()
        {
            base.Draw();
            shared.spritebatch.Draw(shared.textureManager.GetTexture("standardbackground"), new Vector2(0, 0), Color.White);
            shared.spritebatch.Draw(shared.textureManager.GetTexture("titlescreen"), new Vector2(88, 200), Color.White);

            //stars in background
            foreach (Star i in stars)
            {
                i.Draw();
            }

            //menu items
            foreach (Button b in buttons)
            {
                if (b.GetString() == "resume")
                    if (!shared.saveData.isSavedGameAvailable)
                        continue;
                if (!shared.isTrialMode && b.GetString() == "buy")
                {
                    continue;
                }
                b.Draw();      
            }
            if (shared.isTrialMode)
            {
                Vector2 origin;
                string trialString = "Note: trial mode does not have the \nletters R, Q, P, and F";
                origin = shared.fontManager.GetFont("orangefont").MeasureString(trialString) / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), trialString, new Vector2(240, 100), Color.White, 0, origin, 1.0f, 0, 0);
                shared.spritebatch.Draw(shared.textureManager.GetTexture("trialword"), new Vector2(350, 245+25), Color.White);
            }
        }
    }
}
