using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WordGridGame
{
    class Button
    {
        enum ButtonType { STRING, BUTTON }
        private Shared shared;
        private string text;
        private Texture2D image;
        private Vector2 position;
        private ButtonType type;
        private float width;
        private float height;
        public SpriteFont font { get; set; }
        private float bounce;
        private bool bounceDirection;
        public Rectangle rect;
        public Color color { get; set; }
        public float alpha { get; set; }
        public float scale;
        public Vector2 origin;
        public Button()
        {
            Initialize();
        }

        // constructor for string
        public Button(string t, int x, int y, SpriteFont font)
        {
            Initialize();
            this.font = font;
            InitString(t, x, y);
        }
        // constructor for string without specified font type
        public Button(string t, int x, int y)
        {
            Initialize();
            this.font = shared.fontManager.GetFont("testfont");
            InitString(t, x, y);
        }
        // constructor for image
        public Button(Texture2D image, int x, int y)
        {
            Initialize();
            position.X = x;
            position.Y = y;
            type = ButtonType.BUTTON;
            width = image.Width;
            height = image.Height;
            rect = new Rectangle((int)x, (int)y, (int)width, (int)height);
            this.image = image;
        }
        // string constructors share this function
        private void InitString(string t, int x, int y)
        {
            text = t;
            position.X = x;
            position.Y = y;
            type = ButtonType.STRING;
            width = font.MeasureString(text).X;
            height = font.MeasureString(text).Y;
            bounce = 4;
            bounceDirection = false;

            origin.X = width / 2.0f;
            origin.Y = height / 2.0f;
            rect = new Rectangle((int)x - (int)(width / 2.0f), (int)y - (int)(height / 2.0f), (int)width, (int)height);
        }
        public Vector2 GetPosition()
        {
            return position;
        }
        public string GetString()
        {
            return text;
        }
        public void SetScale(float s)
        {
            scale = s;
        }
        public void Initialize()
        {
            position = new Vector2();
            origin = new Vector2(0,0);
            shared = Shared.Instance;
            color = Color.White;
            alpha = 1.0f;
            scale = 1.0f;
        }
        public void ChangeFont(SpriteFont font)
        {
            this.font = font;
        }
        public void ChangeText(string t)
        {
            if (t != text)
                InitString(t, (int)position.X, (int)position.Y);
        }
        public void Update()
        {
            if (type == ButtonType.STRING)
            {
                rect.Y = (int)position.Y - (int)(height / 2.0f) + (int)bounce;
                if (bounce <= 0 || bounce >= 5)
                {
                    bounceDirection = !bounceDirection;
                }

                if (bounceDirection)
                {
                    bounce-=(float)((shared.gameTime.ElapsedGameTime.TotalMilliseconds/1000.0) * 10);
                }
                else
                {
                    bounce+=(float)((shared.gameTime.ElapsedGameTime.TotalMilliseconds/1000.0) * 10);
                }

                if (shared.input.IsMousePressed && rect.Contains(shared.input.MouseX, shared.input.MouseY))
                {
                 //   text = shared.input.MouseY.ToString() + " " + position.Y + " " + rect.Y;
                    scale = 1.5f;
                }
                else
                {
                    scale = 1.0f;
                }
            }
            else if (type == ButtonType.BUTTON)
            {
                if (shared.input.IsMousePressed && rect.Contains(shared.input.MouseX, shared.input.MouseY))
                {
                    scale = 1.5f;
                }
                else
                {
                    scale = 1.0f;
                }
            }
        }
        public bool Clicked()
        {
            
            if (shared.input.WasMouseJustReleased && rect.Contains(shared.input.MouseX, shared.input.MouseY))
            {
                shared.soundManager.Play("button");
                return true;
            }
            return false;
        }

        public void Draw()
        {
            if (type == ButtonType.STRING)
            {
                shared.spritebatch.DrawString(font, text, new Vector2(position.X, position.Y + bounce), color * alpha, 0, origin, scale, 0, 0);
            }
            else if (type == ButtonType.BUTTON)
            {
                shared.spritebatch.Draw(image, position, null,Color.White,0,new Vector2(0,0),scale,0,0);
            }
        }
    }
}
