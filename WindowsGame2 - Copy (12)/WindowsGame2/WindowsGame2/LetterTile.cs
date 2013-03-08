using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WordGridGame
{
    public class LetterTile
    {
        private const float FALLINGSPEED = 5.0f;
        public char letter = '0';
        public enum AnimatingState { INACTIVE, ACTIVE, DISAPPEARING, FALLING }
        public AnimatingState state;
        private int x;
        private int y;
        private int destination_x;
        private int destination_y;
        private int renderX, renderY;
        private Shared shared;
        private float fadeIn;
        private bool isNewTileFromAbove;
        public bool paused { get; set; } // if game paused don't draw letters

        // for falling animation
        private float fallingY;
        private float fallingSpeed = FALLINGSPEED;
        private float fallingGravity = 1.15f;

        // for disappearing animation
        private float disappearingAnimation;
        private float disappearingSpeed = 10.0f;

        public LetterTile(int x, int y, char l)
        {
            shared = Shared.Instance;
            Initialize(x, y, l);
        }
        public void Initialize(int x, int y, char l)
        {
            paused = false;
            letter = l;
            this.x = x;
            this.y = y;
            state = AnimatingState.INACTIVE;
            if (l != '0')
            {
                state = AnimatingState.ACTIVE;
            }
            disappearingAnimation = 0;
            fallingY = 0;
            destination_x = destination_y = -1;
            fadeIn = 1.0f;
            isNewTileFromAbove = false;
        }
        public void Update()
        {
            int tileSize = (480 / shared.gridSize);
            switch (state)
            {
                case AnimatingState.DISAPPEARING:
                    if (disappearingAnimation <= 0)
                    {
                        letter = '0';
                        state = AnimatingState.INACTIVE;
                        disappearingAnimation = 0;
                    }
                    else
                    {
                        disappearingAnimation -= disappearingSpeed;
                    }
                    break;
                case AnimatingState.FALLING:

                    // fallingSpeed *= fallingGravity;
                    fallingSpeed *= fallingGravity;
                    fallingY += fallingSpeed;
                    if (fadeIn <= 1.0f)
                    {
                        fadeIn += 0.05f;
                    }
                    if (destination_y != -1)
                    {
                        if ((int)((320 - 32) + (y - 6) * tileSize + fallingY) >= (int)((320 - 32) + (destination_y - 6) * tileSize))
                        {
                            fallingY = 0;
                            fallingSpeed = FALLINGSPEED;
                            state = AnimatingState.ACTIVE;
                            x = destination_x;
                            y = destination_y;
                            destination_y = destination_x = -1;
                            fadeIn = 1.0f;
                            isNewTileFromAbove = false;
                            //shared.wordlogic.grid[x, y-6] = letter;
                        }
                    }
                    break;
            }
        }

        public void Draw()
        {
            int tileSize = (480 / shared.gridSize);

            switch (state)
            {
                case AnimatingState.INACTIVE:
                    break;
                case AnimatingState.ACTIVE:
                    renderX = x * tileSize;
                    renderY = (320 - 32) + (y - 6) * tileSize;
                    shared.spritebatch.Draw(shared.textureManager.GetTexture("greentile"),
                        new Rectangle(renderX, renderY, tileSize, tileSize), Color.White);
                    if (!paused)
                    {
                        shared.spritebatch.Draw(shared.textureManager.GetTexture("letters"),
                           new Rectangle(renderX, renderY, tileSize, tileSize), GetLetterFromTexture(letter), Color.White);
                        shared.spritebatch.DrawString(shared.fontManager.GetFont("letterpointsfont"), shared.wordlogic.GetLetterWorth(letter).ToString(), new Vector2(renderX + 62, renderY + 56), Color.White);
                    }
                    break;
                case AnimatingState.DISAPPEARING:
                    int x_size = (int)((disappearingAnimation / 60.0) * tileSize);
                    renderX = x * tileSize + 40;
                    renderY = (320 - 32) + (y - 6) * tileSize + 40;
                    shared.spritebatch.Draw(shared.textureManager.GetTexture("greentile"),
                        new Rectangle(renderX, renderY, x_size, tileSize), null, Color.White, 0, new Vector2(48, 48), 0, 0);
                    shared.spritebatch.Draw(shared.textureManager.GetTexture("letters"),
                            new Rectangle(renderX, renderY, x_size, tileSize), GetLetterFromTexture(letter), Color.White, 0, new Vector2(65, 65), 0, 0);
                    break;
                case AnimatingState.FALLING:
                    renderX = x * tileSize;
                    renderY = (int)((320 - 32) + (y - 6) * tileSize + fallingY);
                    if (isNewTileFromAbove)
                    {
                        shared.spritebatch.Draw(shared.textureManager.GetTexture("greentile"),
                        new Rectangle(renderX, renderY, tileSize, tileSize), Color.White * fadeIn);
                    }
                    else
                    {
                        shared.spritebatch.Draw(shared.textureManager.GetTexture("greentile"),
                        new Rectangle(renderX, renderY, tileSize, tileSize), Color.White);
                    }
                    
                    if (!paused)
                    {
                        shared.spritebatch.Draw(shared.textureManager.GetTexture("letters"),
                            new Rectangle(renderX, renderY, tileSize, tileSize), GetLetterFromTexture(letter), Color.White);
                        shared.spritebatch.DrawString(shared.fontManager.GetFont("letterpointsfont"), shared.wordlogic.GetLetterWorth(letter).ToString(), new Vector2(renderX + 62, renderY + 56), Color.White);
                    }
                    break;
            }
        }
        public void SetDestination(int x, int y)
        {
            this.destination_x = x;
            this.destination_y = y;
        }
        public void SetTileFromAbove()
        {
            isNewTileFromAbove = true;
        }
        public void SetFalling()
        {
            if (letter == '0')
                return;
            state = AnimatingState.FALLING;
            fadeIn = 0.0f;
            //fallingY = 0;

        }
        public void Disappear()
        {
            state = AnimatingState.DISAPPEARING;
            disappearingAnimation = 60;
        }
        public bool MatchXY(int x, int y)
        {
            return (this.x == x && this.y == y);
        }
        private Rectangle GetLetterFromTexture(char letter)
        {
            int l = ((short)letter) - 65;
            if (l >= 0 && l <= 6)
            {
                return new Rectangle(l * 130, 0, 130, 130);
            }
            else if (l >= 7 && l <= 13)
            {
                return new Rectangle((l - 7) * 130, 130, 130, 130);
            }
            else if (l >= 14 && l <= 20)
            {
                return new Rectangle((l - 14) * 130, 260, 130, 130);
            }
            else if (l >= 21 && l <= 25)
            {
                return new Rectangle((l - 21) * 130, 390, 130, 130);
            }
            return new Rectangle();
        }
    }
}
