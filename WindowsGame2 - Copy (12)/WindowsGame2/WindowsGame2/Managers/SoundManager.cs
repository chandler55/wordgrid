using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace WordGridGame.Managers
{
    class SoundManager
    {
        Dictionary<string, SoundEffect> sounds;
        Game game;
        Shared shared;
        public SoundManager()
        {
        }

        public void Initialize(Game g)
        {
            shared = Shared.Instance;
            game = g;
            sounds = new Dictionary<string, SoundEffect>();
        }

        private void AddSound(string filename)
        {
            sounds.Add(filename, game.Content.Load<SoundEffect>(filename));
        }

        public SoundEffect GetSoundEffect(string soundName)
        {
            try
            {
                return AssetHelper.Get<SoundEffect>(soundName);
            }
            catch (KeyNotFoundException e)
            {
                // too slow to write a line
                Console.WriteLine(e.ToString());

                return AssetHelper.Get<SoundEffect>("button");
            }
        }
        public void Play(string soundName)
        {
            if (shared.saveData.soundOn)
                GetSoundEffect(soundName).Play();
        }
        public void Play(string soundName, float volume, float pitch, float pan)
        {
            if (shared.saveData.soundOn)
                GetSoundEffect(soundName).Play(volume, pitch, pan);
        }
    }
}
