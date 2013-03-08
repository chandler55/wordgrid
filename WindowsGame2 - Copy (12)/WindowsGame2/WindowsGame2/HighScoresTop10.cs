using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace WordGridGame
{

    public struct HighScoreData
    {
        public DateTime time;
        public string name;
        public int score;
        public string bestWord;
        public int bestWordScore;
    }

    public class HighScoresTop10
    {
        public HighScoreData[] highScore;
        public HighScoresTop10()
        {
            highScore = new HighScoreData[11];
        }

        public void AddScore(string name, int s, string w, int bestWordScore)
        {
            highScore[10].name = name;
            highScore[10].score = s;
            highScore[10].bestWord = w;
            highScore[10].bestWordScore = bestWordScore;
            highScore[10].time = DateTime.Now;
            int i = 9;
            while (i >= 0 && s > highScore[i].score)
            {
                highScore[i + 1] = highScore[i];
                i--;
            }

            highScore[i + 1].name = name;
            highScore[i + 1].score = s;
            highScore[i + 1].bestWord = w;
            highScore[i + 1].bestWordScore = bestWordScore;
            highScore[i + 1].time = DateTime.Now;
        }

        public int CheckScore(int score)
        {
            highScore[10].score = score;
            highScore[10].time = DateTime.Now;
            int i = 9;
            while (i >= 0 && score > highScore[i].score)
            {
                i--;
            }
            return i + 1;
        }

        public void Clear()
        {
            for (int i = 0; i < 11; i++)
            {
                highScore[i].name = "";
                highScore[i].score = 0;
                highScore[i].bestWord = "";
                highScore[i].bestWordScore = 0;
            }
        }
    }
}
