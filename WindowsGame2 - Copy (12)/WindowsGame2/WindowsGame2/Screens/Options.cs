using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
namespace WordGridGame.Screens
{
    class Options : GameState
    {
        private Vector2 origin;
        private Button exitButton;
        private Button soundButton;
        private Button timeLimitButton;
        private Button clearHighScoresButton;
        public Options()
            : base()
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            origin = new Vector2();
            exitButton = new Button("exit", 60, 675);
            soundButton = new Button("on", 330, 250);
            timeLimitButton = new Button("on", 330, 350);
            clearHighScoresButton = new Button("clear high scores", 280, 520, shared.fontManager.GetFont("orangefont"));
        }
        private void callBack(IAsyncResult ar)
        {
            int? result = Guide.EndShowMessageBox(ar);
            if (result == 0)
            {
                shared.saveData.ClearHighScoresData();
            }
        }
        public override void Update()
        {
            base.Update();
            clearHighScoresButton.Update();
            if (clearHighScoresButton.Clicked())
            {
                Guide.BeginShowMessageBox(PlayerIndex.One, "High Scores",
                   "Are you sure you want to clear the\nhigh scores data?",
                   new string[] { "Yes", "No" },
                   0, MessageBoxIcon.Alert, callBack, null);
            }
            exitButton.Update();
            if (exitButton.Clicked() || shared.input.backPressed)
            {
                shared.nextState = Shared.State.MAINMENU;
                shared.zoomText.zoom = true;
                shared.zoomText.pos = exitButton.GetPosition();
                shared.zoomText.text = exitButton.GetString();
            }
            if (shared.saveData.soundOn)
            {
                soundButton.ChangeText("on");
            }
            else
            {
                soundButton.ChangeText("off");
            }
            if (shared.saveData.timeLimitMode)
            {
                timeLimitButton.ChangeText("on");
            }
            else
            {
                timeLimitButton.ChangeText("off");
            }

            soundButton.Update();
            if (soundButton.Clicked())
            {
                shared.saveData.soundOn = !shared.saveData.soundOn;
            }
            timeLimitButton.Update();
            if (timeLimitButton.Clicked())
            {
                shared.saveData.timeLimitMode = !shared.saveData.timeLimitMode;
            }
        }

        public override void Draw()
        {

            base.Draw();
            shared.spritebatch.Draw(shared.textureManager.GetTexture("standardbackground"), new Vector2(0, 0), Color.White);
            origin = shared.fontManager.GetFont("menuheader").MeasureString("options") / 2.0f;
            shared.spritebatch.DrawString(shared.fontManager.GetFont("menuheader"), "options", new Vector2(240, 100), Color.White, 0, origin, 1.0f, 0, 0);
            
            exitButton.Draw();
            clearHighScoresButton.Draw();
            soundButton.Draw();
            timeLimitButton.Draw();
            shared.spritebatch.DrawString(shared.fontManager.GetFont("testfont"), "sound", new Vector2(50, 220), Color.White);
            shared.spritebatch.DrawString(shared.fontManager.GetFont("testfont"), "time limit", new Vector2(50, 320), Color.White);
        }
    }
}
