using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
namespace WordGridGame
{
 
    public struct HighWordScore
    {
        public int score;
        public string word;
    }
    /// <summary>
    /// a class to hold high scores data, 0 is the highest rank
    /// </summary>
    /// 

    public class HighWordScores
    {

        public HighWordScore[] highWordScores;

        public HighWordScores()
        {
            highWordScores = new HighWordScore[6];
        }

        public void AddScore(int s, string w)
        {
            highWordScores[5].score = s;
            highWordScores[5].word = w;

            int i = 4;
            while (i >= 0 && s > highWordScores[i].score)
            {
                highWordScores[i + 1] = highWordScores[i];
                i--;
            }

            highWordScores[i + 1].score = s;
            highWordScores[i + 1].word = w;
        }

        public void Clear()
        {
            for (int i = 0; i < 6; i++)
            {
                highWordScores[i].score = 0;
                highWordScores[i].word = "";
            }
        }
    }
}
