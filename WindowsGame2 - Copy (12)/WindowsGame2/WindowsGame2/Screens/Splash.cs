using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WordGridGame.Screens
{
    class Splash : GameState
    {
        public Splash()
            : base()
        {
        }

        public override void Initialize()
        {
            base.Initialize();

        }

        public override void Update()
        {
            base.Update();
        }

        public override void Draw()
        {
            base.Draw();
            shared.spritebatch.Draw(shared.textureManager.GetTexture("background"), new Vector2(0, 0), Color.White);
        }
    }
}
