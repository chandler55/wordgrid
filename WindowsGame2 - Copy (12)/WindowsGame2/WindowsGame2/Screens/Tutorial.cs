using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WordGridGame.Screens
{
    class Tutorial : GameState
    {
        private Vector2 origin;
        private Button exitButton;
        public Tutorial()
            : base()
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            origin = new Vector2();
            exitButton = new Button("exit", 60, 675);
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
            }
        }

        public override void Draw()
        {
            base.Draw();
            shared.spritebatch.Draw(shared.textureManager.GetTexture("standardbackground"), new Vector2(0, 0), Color.White);
            shared.spritebatch.Draw(shared.textureManager.GetTexture("tutorial"), new Vector2(0, 0), Color.White);
            origin = shared.fontManager.GetFont("menuheader").MeasureString("tutorial") / 2.0f;
            shared.spritebatch.DrawString(shared.fontManager.GetFont("menuheader"), "tutorial", new Vector2(240, 60), Color.White,0,origin,1.0f,0,0);
            exitButton.Draw();
        }
    }
}
