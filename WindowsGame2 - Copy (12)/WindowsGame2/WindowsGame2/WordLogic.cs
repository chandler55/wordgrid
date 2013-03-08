using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
namespace WordGridGame
{
    class WordLogic
    {
        private const int MAX_WORD_LEN = 17;
        private const bool SEEN = true;
        private const bool NOT_SEEN = false;

        public char[,] grid;
        string[] dictionary;

        Shared shared;
        Random random;

        //scrabble way of giving letters
        private string letterDistribution = "AAAAAAAAABBCCDDDDDEEEEEEEEEEEEEFFGGGHHHHIIIIIIIIJKLLLLMMNNNNNOOOOOOOOPPQRRRRRRSSSSSTTTTTTTUUUUVVWWXYYZ";
        private string letterDistribution2 = "AAAAAAAAABBCCDDDDDEEEEEEEEEEEEEGGGHHHHIIIIIIIIJKLLLLMMNNNNNOOOOOOOOSSSSSTTTTTTTUUUUVVWWXYYZ";
        private string remainingDistribution;

        //boggle way of giving letters
        private string[] letterDice;
        private List<string> letterDiceTemp;
        private List<string> gridAnswers;
        private string wordTemp;
        private List<string> prefixList;
        private Tries trieDictionary;

        //stuff for solving the word grid
        private bool[,] seen_grid;
        private int[] letterPoints;

        public WordLogic()
        {
            shared = Shared.Instance;
            random = new Random();
            // InitializeDice();
            letterDiceTemp = new List<string>();
            gridAnswers = new List<string>();
            prefixList = new List<string>();

            wordTemp = "test";
            InitializeGrid();
            InitializeLetterPoints();

        }

        public char GetLetter(int x, int y) { return grid[x, y]; }
        public void InitializeGrid()
        {
            /*
            letterDiceTemp.Clear();

            for (int i = 0; i < 25; i++)
            {
                letterDiceTemp.Add(letterDice[i]);
            }
             * */
            grid = new char[shared.gridSize, shared.gridSize];

            for (int i = 0; i < shared.gridSize; i++)
            {
                for (int j = 0; j < shared.gridSize; j++)
                {
                    grid[i, j] = RandomLetter();
                }
            }
            // DebugGrid();

        }
        public char RandomLetter()
        {
            // boggle method
            /*
            int chosenDie = random.Next(letterDiceTemp.Count);
            char letter = letterDiceTemp[chosenDie][random.Next(6)];
            letterDiceTemp.Remove(letterDiceTemp[chosenDie]);
            */
            char letter;
            if (shared.isTrialMode)
            {
                letter = letterDistribution2[random.Next(letterDistribution2.Length)];
            }
            else
            {
                letter = letterDistribution[random.Next(letterDistribution.Length)];
            }
            return letter;
        }
        public void InitializeDictionary()
        {
            string levelPath = string.Format("Content/TWL06.txt");
            Stream stream = TitleContainer.OpenStream(levelPath);
            TextReader tr = new StreamReader(stream);
            int counter = 0;
            dictionary = new string[178691];
            trieDictionary = new Tries();
            while (tr.Peek() > -1)
            {
                dictionary[counter] = tr.ReadLine();
                /* if (!prefixList.Contains(dictionary[counter])) {
                     if (dictionary[counter].Length > 4)
                         prefixList.Add(dictionary[counter].Remove(4));
                     else if (dictionary[counter].Length == 4)
                         prefixList.Add(dictionary[counter]);
                 }/**/

                // takes too much memory?
                //trieDictionary.Insert(tr.ReadLine());
                counter++;


            }

            tr.Close();

            // build prefix list for fast searching of prefixes

        }

        // use binary search to find word
        public bool IsValidWord(string word)
        {
            int bottom, center, top, result;
            bottom = 0;
            top = dictionary.Length - 1;

            while (true)
            {
                center = (bottom + top) / 2;
                result = String.Compare(word, dictionary[center]);
                if (result == 0)
                {
                    return true;
                }
                else if (bottom >= top)
                {
                    break;
                }
                else if (result > 0)
                {
                    bottom = center + 1;
                }
                else
                {
                    top = center - 1;
                }

            }
            return false;
        }
        public int GetWordIndex(string word)
        {
            int bottom, center, top, result;
            bottom = 0;
            top = dictionary.Length - 1;

            while (true)
            {
                center = (bottom + top) / 2;
                result = String.Compare(word, dictionary[center]);
                if (result == 0)
                {
                    return center;
                }
                else if (bottom >= top)
                {
                    break;
                }
                else if (result > 0)
                {
                    bottom = center + 1;
                }
                else
                {
                    top = center - 1;
                }

            }
            return -1;
        }
        /// <summary>
        ///  solve the word grid
        /// </summary>
        public void SolveWordGrid()
        {
            seen_grid = new bool[shared.gridSize, shared.gridSize];
            gridAnswers.Clear();

            for (int y = 0; y < shared.gridSize; y++)
            {
                for (int x = 0; x < shared.gridSize; x++)
                {
                    // dfs(x, y, 0);
                    dfs2(x, y, 0, trieDictionary.root.GetChild(grid[x, y]));
                }
            }

            gridAnswers.Sort();
        }
        /// <summary>
        /// depth first search using trie
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="wordLength"></param>
        /// <param name="t"></param>
        public void dfs2(int x, int y, int wordLength, TrieNode t)
        {
            if (x < 0 || y < 0 || x >= shared.gridSize || y >= shared.gridSize)
                return;
            if (seen_grid[x, y] == SEEN)
            {
                return;
            }

            if (wordTemp.Length > wordLength)
                wordTemp = wordTemp.Remove(wordLength);

            seen_grid[x, y] = SEEN;
            wordTemp += grid[x, y];
            wordLength++;
            if (wordTemp[wordLength - 1] == 'Q')
            {
                wordTemp += 'U';
                wordLength++;
            }

            if (t.isWord && wordTemp.Length >= 3)
            {
                if (!gridAnswers.Contains(wordTemp))
                    gridAnswers.Add(wordTemp);
            }

            if (wordLength < MAX_WORD_LEN)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int x2 = x + j;
                    if (x2 < 0 || x2 >= shared.gridSize) continue;

                    for (int i = -1; i <= 1; i++)
                    {
                        int y2 = y + i;
                        if (y2 < 0 || y2 >= shared.gridSize) continue;
                        if (seen_grid[x2, y2]) continue;
                        if (t.Contains(grid[x2, y2]))
                        {
                            dfs2(x2, y2, wordLength, t.GetChild(grid[x2, y2]));
                        }

                    }
                }
            }

            seen_grid[x, y] = NOT_SEEN;
        }
        /// <summary>
        ///  Depth first search function to find words in the grid
        /// </summary>
        /// <param name="x">x coordinate of letter to start at</param>
        /// <param name="y">y coordinate of letter to start at</param>
        /// <param name="wordLength">keeps track of the temp word's length</param>
        public void dfs(int x, int y, int wordLength)
        {
            if (wordTemp.Length > wordLength)
                wordTemp = wordTemp.Remove(wordLength);

            if (x < 0 || y < 0 || x >= shared.gridSize || y >= shared.gridSize)
                return;
            if (seen_grid[x, y] == SEEN)
            {
                return;
            }

            //we already walked this path
            seen_grid[x, y] = SEEN;

            wordTemp += grid[x, y];
            wordLength++;

            // should only allow QU words
            if (wordTemp[wordLength - 1] == 'Q')
            {
                wordTemp += 'U';
                wordLength++;
            }

            // check word

            if (wordTemp.Length >= 3 && IsValidWord(wordTemp))
            {
                // add to the answers
                if (!gridAnswers.Contains(wordTemp))
                    gridAnswers.Add(wordTemp);
            }


            if (wordLength < MAX_WORD_LEN)
            {
                for (int j = -1; j <= 1; j++)
                    for (int i = -1; i <= 1; i++)
                        if ((i != 0 || j != 0))
                            dfs(x + i, y + j, wordLength);
            }

            seen_grid[x, y] = NOT_SEEN;
        }
        /// <summary>
        ///  creates letter dice , dice similar to boggle
        /// </summary>
        private void InitializeDice()
        {
            shared.debugSystem.DebugCommandUI.Echo("Initializing Dice");
            letterDice = new string[25];
            letterDice[0] = "AAAFRS";
            letterDice[1] = "AAEEEE";
            letterDice[2] = "AAFIRS";
            letterDice[3] = "ADENNN";
            letterDice[4] = "AEEEEM";
            letterDice[5] = "AEEGMU";
            letterDice[6] = "AEGMNN";
            letterDice[7] = "AFIRSY";
            letterDice[8] = "BJKQXZ";
            letterDice[9] = "CCNSTW";
            letterDice[10] = "CEIILT";
            letterDice[11] = "CEILPT";
            letterDice[12] = "CEIPST";
            letterDice[13] = "DHHNOT";
            letterDice[14] = "DHHLOR";
            letterDice[15] = "DHLNOR";
            letterDice[16] = "DDLNOR";
            letterDice[17] = "EIIITT";
            letterDice[18] = "EMOTTT";
            letterDice[19] = "ENSSSU";
            letterDice[20] = "FIPRSY";
            letterDice[21] = "GORRVW";
            letterDice[22] = "HIPRRY";
            letterDice[23] = "NOOTUW";
            letterDice[24] = "OOOTTU";
            int length = letterDice.Length;

        }
        public int GetLetterWorth(char c)
        {
            return letterPoints[(int)c - 65];
        }
        private void InitializeLetterPoints()
        {
            letterPoints = new int[26];
            letterPoints[0] = 1; // A
            letterPoints[1] = 3; // B
            letterPoints[2] = 3; // C
            letterPoints[3] = 2; // D
            letterPoints[4] = 1; // E
            letterPoints[5] = 4; // F
            letterPoints[6] = 2; // G
            letterPoints[7] = 3; // H
            letterPoints[8] = 1; // I
            letterPoints[9] = 8; // J
            letterPoints[10] = 5; // K
            letterPoints[11] = 2; // L
            letterPoints[12] = 3; // M
            letterPoints[13] = 2; // N
            letterPoints[14] = 1; // O
            letterPoints[15] = 3; // P
            letterPoints[16] = 8; // Q
            letterPoints[17] = 1; // R
            letterPoints[18] = 1; // S
            letterPoints[19] = 1; // T
            letterPoints[20] = 1; // U
            letterPoints[21] = 4; // V
            letterPoints[22] = 4; // W
            letterPoints[23] = 8; // X
            letterPoints[24] = 4; // Y
            letterPoints[25] = 8; // Z
        }
        private void DebugGrid()
        {
            grid[0, 0] = 'R';
            grid[1, 0] = 'S';
            grid[2, 0] = 'L';
            grid[3, 0] = 'C';
            grid[4, 0] = 'S';

            grid[0, 1] = 'D';
            grid[1, 1] = 'E';
            grid[2, 1] = 'I';
            grid[3, 1] = 'A';
            grid[4, 1] = 'E';

            grid[0, 2] = 'G';
            grid[1, 2] = 'N';
            grid[2, 2] = 'T';
            grid[3, 2] = 'R';
            grid[4, 2] = 'P';

            grid[0, 3] = 'Q';
            grid[1, 3] = 'Q';
            grid[2, 3] = 'I';
            grid[3, 3] = 'T';
            grid[4, 3] = 'E';

            grid[0, 4] = 'S';
            grid[1, 4] = 'M';
            grid[2, 4] = 'I';
            grid[3, 4] = 'D';
            grid[4, 4] = 'R';
        }
    }
}
