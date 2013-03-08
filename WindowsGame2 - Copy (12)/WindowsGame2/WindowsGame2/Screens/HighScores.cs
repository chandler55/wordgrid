using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WordGridGame.Controls;

namespace WordGridGame.Screens
{
    class HighScores : GameState
    {
        private Vector2 origin;
        private Button exitButton;
        private bool statsScreen;
        private Button statsButton;
        private Button highscoresButton;
        public HighScores()
            : base()
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            origin = new Vector2();
            exitButton = new Button("exit", 60, 675);
            statsButton = new Button("stats", 300, 675);
            highscoresButton = new Button("highscores", 300, 675);
            statsScreen = false;
        }

        public override void Update()
        {
            base.Update();
            exitButton.Update();

            if (exitButton.Clicked() || shared.input.backPressed)
            {
                shared.nextState = Shared.State.MAINMENU;
                shared.zoomText.zoom = true;
                shared.zoomText.pos = exitButton.GetPosition();
                shared.zoomText.text = exitButton.GetString();
                statsScreen = false;
            }
            if (statsScreen)
            {
                highscoresButton.Update();
                if (highscoresButton.Clicked())
                {
                    statsScreen = false;
                }
            }
            else
            {
                statsButton.Update();
                if (statsButton.Clicked())
                {
                    statsScreen = true;
                }
            }


        }

        public override void Draw()
        {

            base.Draw();
            if (statsScreen)
            {
                int align_column1 = 150;
                int align_column2 = 360;
                int linespacing = 50;
                int rowspacing = 170;
                shared.spritebatch.Draw(shared.textureManager.GetTexture("standardbackground"), new Vector2(0, 0), Color.White);
                origin = shared.fontManager.GetFont("menuheader").MeasureString("statistics") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("menuheader"), "statistics", new Vector2(240, 100), Color.White, 0, origin, 1.0f, 0, 0);
                origin = shared.fontManager.GetFont("orangefont").MeasureString("best scoring word:") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), "best scoring word:", new Vector2(align_column1, rowspacing), Color.White, 0, origin, 1.0f, 0, 0);

                origin = shared.fontManager.GetFont("orangefont").MeasureString("total words found:") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), "total words found:", new Vector2(align_column1, rowspacing + linespacing), Color.White, 0, origin, 1.0f, 0, 0);

                origin = shared.fontManager.GetFont("orangefont").MeasureString("unique words found:") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), "unique words found:", new Vector2(align_column1, rowspacing + linespacing * 2), Color.White, 0, origin, 1.0f, 0, 0);
                linespacing = 25;
                rowspacing = 275;
                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString("unique") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), "unique", new Vector2(align_column1, rowspacing + linespacing * 2), Color.White, 0, origin, 1.0f, 0, 0);

                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString("3-letter words:") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), "3-letter words:", new Vector2(align_column1, rowspacing + linespacing * 3), Color.White, 0, origin, 1.0f, 0, 0);
                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString("4-letter words:") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), "4-letter words:", new Vector2(align_column1, rowspacing + linespacing * 4), Color.White, 0, origin, 1.0f, 0, 0);
                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString("5-letter words:") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), "5-letter words:", new Vector2(align_column1, rowspacing + linespacing * 5), Color.White, 0, origin, 1.0f, 0, 0);
                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString("6-letter words:") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), "6-letter words:", new Vector2(align_column1, rowspacing + linespacing * 6), Color.White, 0, origin, 1.0f, 0, 0);
                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString("7-letter words:") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), "7-letter words:", new Vector2(align_column1, rowspacing + linespacing * 7), Color.White, 0, origin, 1.0f, 0, 0);
                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString("8+ letter words:") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), "8+ letter words:", new Vector2(align_column1, rowspacing + linespacing * 8), Color.White, 0, origin, 1.0f, 0, 0);

                // 2nd column
                linespacing = 50;
                rowspacing = 170;
                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString(shared.saveData.bestScoringWord.word + " (" + shared.saveData.bestScoringWord.score + ")") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), shared.saveData.bestScoringWord.word + " (" + shared.saveData.bestScoringWord.score + ")", new Vector2(align_column2, rowspacing), Color.White, 0, origin, 1.0f, 0, 0);

                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString(shared.saveData.totalWordsFound.ToString()) / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), shared.saveData.totalWordsFound.ToString(), new Vector2(align_column2, rowspacing + linespacing), Color.White, 0, origin, 1.0f, 0, 0);

                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString(shared.saveData.uniqueWordsFound.ToString()) / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), shared.saveData.uniqueWordsFound.ToString(), new Vector2(align_column2, rowspacing + linespacing * 2), Color.White, 0, origin, 1.0f, 0, 0);
                linespacing = 25;
                rowspacing = 275;
                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString(shared.saveData.letterWordsFound[3].ToString()) / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), shared.saveData.letterWordsFound[3].ToString(), new Vector2(align_column2, rowspacing + linespacing * 3), Color.White, 0, origin, 1.0f, 0, 0);
                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString(shared.saveData.letterWordsFound[4].ToString()) / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), shared.saveData.letterWordsFound[4].ToString(), new Vector2(align_column2, rowspacing + linespacing * 4), Color.White, 0, origin, 1.0f, 0, 0);
                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString(shared.saveData.letterWordsFound[5].ToString()) / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), shared.saveData.letterWordsFound[5].ToString(), new Vector2(align_column2, rowspacing + linespacing * 5), Color.White, 0, origin, 1.0f, 0, 0);
                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString(shared.saveData.letterWordsFound[6].ToString()) / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), shared.saveData.letterWordsFound[6].ToString(), new Vector2(align_column2, rowspacing + linespacing * 6), Color.White, 0, origin, 1.0f, 0, 0);
                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString(shared.saveData.letterWordsFound[7].ToString()) / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), shared.saveData.letterWordsFound[7].ToString(), new Vector2(align_column2, rowspacing + linespacing * 7), Color.White, 0, origin, 1.0f, 0, 0);
                origin = shared.fontManager.GetFont("whiteclearfont").MeasureString(shared.saveData.letterWordsFound[8].ToString()) / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), shared.saveData.letterWordsFound[8].ToString(), new Vector2(align_column2, rowspacing + linespacing * 8), Color.White, 0, origin, 1.0f, 0, 0);


                highscoresButton.Draw();
            }
            else
            {
                shared.spritebatch.Draw(shared.textureManager.GetTexture("standardbackground"), new Vector2(0, 0), Color.White);
                origin = shared.fontManager.GetFont("menuheader").MeasureString("high") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("menuheader"), "high", new Vector2(120, 100), Color.White, 0, origin, 1.0f, 0, 0);
                origin = shared.fontManager.GetFont("menuheader").MeasureString("scores") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("menuheader"), "scores", new Vector2(320, 100), Color.White, 0, origin, 1.0f, 0, 0);

                int align_column1 = 90;
                int align_column2 = 260;
                int align_column3 = 420;
                int linespacing = 40;
                int rowspacing = 170;
                origin = shared.fontManager.GetFont("orangefont").MeasureString("name") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), "name", new Vector2(align_column1, rowspacing), Color.White, 0, origin, 1.0f, 0, 0);
                origin = shared.fontManager.GetFont("orangefont").MeasureString("best word") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), "best word", new Vector2(align_column2, rowspacing), Color.White, 0, origin, 1.0f, 0, 0);
                origin = shared.fontManager.GetFont("orangefont").MeasureString("points") / 2.0f;
                shared.spritebatch.DrawString(shared.fontManager.GetFont("orangefont"), "points", new Vector2(align_column3, rowspacing), Color.White, 0, origin, 1.0f, 0, 0);
                rowspacing += 50;
                for (int i = 0; i < 10; i++)
                {
                    if (shared.saveData.highScores.highScore[i].score == 0)
                    {
                        continue;
                    }

                    //  origin = shared.fontManager.GetFont("whiteclearfont").MeasureString(shared.saveData.highScores.highScore[i].time.ToString("MMM d yyyy")) / 2.0f;
                    //shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), shared.saveData.highScores.highScore[i].time.ToString("MMM d yyyy"), new Vector2(align_column1, rowspacing + (linespacing) * i), Color.White, 0, origin, 1.0f, 0, 0);
                    // lets show the name instead
                    origin = shared.fontManager.GetFont("whiteclearfont").MeasureString(shared.saveData.highScores.highScore[i].name) / 2.0f;
                    shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), shared.saveData.highScores.highScore[i].name, new Vector2(align_column1, rowspacing + (linespacing) * i), Color.White, 0, origin, 1.0f, 0, 0);

                    origin = shared.fontManager.GetFont("whiteclearfont").MeasureString(shared.saveData.highScores.highScore[i].bestWord.ToString().ToLower() + " (" + shared.saveData.highScores.highScore[i].bestWordScore + ")") / 2.0f;
                    shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), shared.saveData.highScores.highScore[i].bestWord.ToString().ToLower() + " (" + shared.saveData.highScores.highScore[i].bestWordScore + ")",
                        new Vector2(align_column2, rowspacing + (linespacing) * i), Color.White, 0, origin, 1.0f, 0, 0);
                    origin = shared.fontManager.GetFont("whiteclearfont").MeasureString(shared.saveData.highScores.highScore[i].score.ToString()) / 2.0f;
                    shared.spritebatch.DrawString(shared.fontManager.GetFont("whiteclearfont"), shared.saveData.highScores.highScore[i].score.ToString(), new Vector2(align_column3, rowspacing + (linespacing) * i), Color.White, 0, origin, 1.0f, 0, 0);
                }
                statsButton.Draw();
            }

            exitButton.Draw();
        }
    }
}
