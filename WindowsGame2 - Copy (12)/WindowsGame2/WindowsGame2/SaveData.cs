using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;
using System.IO;
using System.Collections;
using System.Collections.Specialized;

namespace WordGridGame
{


    public class SaveData
    {
        private bool[] wordFound; // has to be private because it uses a different save file
        private const int AMOUNT_WORDS = 178691;
        public bool timeLimitMode;
        public HighWordScore bestScoringWord;
        public int totalWordsFound;
        public int uniqueWordsFound;
        public int[] letterWordsFound;
        public HighScoresTop10 highScores;
        public bool musicOn;
        public bool soundOn;
        public bool isSavedGameAvailable;
        public string defaultHighScoreName;
        private BitArray wordsFoundArray;
        // game data

        public SaveData()
        {
            Initialize();
        }

        public void Initialize()
        {
            musicOn = false;
            soundOn = true;          
            timeLimitMode = true;
            isSavedGameAvailable = false;
            defaultHighScoreName = "";
            ClearHighScoresData();
            wordsFoundArray = new BitArray(AMOUNT_WORDS);
            /*
            // temp data
            highScores.highScore[0].bestWord = "asdfgh";
            highScores.highScore[0].score = 123;
            highScores.highScore[0].time = DateTime.Now;

            totalWordsFound = 55;
            letterWordsFound[5] = 5;
            letterWordsFound[8] = 10;
            letterWordsFound[3] = 1;
            uniqueWordsFound = 24;
            bestScoringWord.word = "crown";
            bestScoringWord.score = 120;*/
        }
        public void ClearHighScoresData()
        {
            wordFound = new bool[AMOUNT_WORDS];
            bestScoringWord = new HighWordScore();
            bestScoringWord.score = 0;
            bestScoringWord.word = "";
            totalWordsFound = 0;
            uniqueWordsFound = 0;
            letterWordsFound = new int[10];
            highScores = new HighScoresTop10();
            highScores.Clear();
        }
        public void WriteStateToTransient()
        {

        }
        public void LoadStateFromTransient()
        {

        }
        public bool IsWordFound(int x)
        {
            return wordFound[x];
        }
        public void SetWordFound(int x)
        {
            wordFound[x] = true;
        }
        public void WriteToStorage()
        {

#if WINDOWS
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain();
#else
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();
#endif
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
            

            using (IsolatedStorageFileStream stream = savegameStorage.CreateFile("savedata.dat"))
            {
                serializer.Serialize(stream, this);
                stream.Close();
                stream.Dispose();
            }

            for (int i = 0; i < AMOUNT_WORDS; i++)
            {
                wordsFoundArray[i] = wordFound[i];  
            }
            wordsFoundArray[0] = true;
            wordsFoundArray[1] = true;
 
            byte[] bytes = ToByteArray(wordsFoundArray);
            using (IsolatedStorageFileStream stream = savegameStorage.CreateFile("wordsfound.dat"))
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(bytes);
                writer.Close();
                stream.Close();
                stream.Dispose();
            }

        }

        public static SaveData LoadFromStorage()
        {

#if WINDOWS
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
#else
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
#endif

            SaveData saveData;
            if (storage.FileExists("savedata.dat"))
            {

                IsolatedStorageFileStream stream =

                storage.OpenFile("savedata.dat", FileMode.Open);

                XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
                saveData = serializer.Deserialize(stream) as SaveData;
                stream.Close();
                stream.Dispose();
            }
            else
            {
                saveData = new SaveData();
            }

            if (storage.FileExists("wordsfound.dat"))
            {
                IsolatedStorageFileStream stream = storage.OpenFile("wordsfound.dat", FileMode.Open);
                BinaryReader reader = new BinaryReader(stream);
                int numBytes = AMOUNT_WORDS / 8;
                if (AMOUNT_WORDS % 8 != 0) numBytes++;

                byte[] bytes = new byte[numBytes];

                reader.Read(bytes, 0, numBytes);
                saveData.wordsFoundArray = new BitArray(bytes);
                reader.Close();
                stream.Close();
                stream.Dispose();
            }
            else
            {
                for (int i = 0; i < AMOUNT_WORDS; i++)
                {
                    saveData.wordFound[i] = false;
                }
            }
            return saveData;
            /* */
        }
        public byte[] ToByteArray(BitArray bits)
        {
            int numBytes = bits.Count / 8;
            if (bits.Count % 8 != 0) numBytes++;

            byte[] bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                    bytes[byteIndex] |= (byte)(1 << (7 - bitIndex));

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }
    }

}
