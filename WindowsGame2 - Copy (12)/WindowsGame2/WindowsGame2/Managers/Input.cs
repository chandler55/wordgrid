using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;
namespace WordGridGame
{
    class Input
    {
        MouseState mousestate;
        private bool mouseJustReleased;
        private bool mousePressed;
        private bool[] keyJustReleased;
        private bool[] keyPressed;
        private int mouseX;
        private int mouseY;
        public bool backPressed { get; set;  }

        private bool mouseIsPressed;

        public int MouseX
        {
            get { return mouseX; }
            set { mouseX = value; }
        }
        public int MouseY
        {
            get { return mouseY; }
            set { mouseY = value; }
        }
        public Input()
        {
            mousePressed = false;
            mouseJustReleased = false;
            keyJustReleased = new bool[256];
            keyPressed = new bool[256];
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.FreeDrag;
            backPressed = false;
        }

        public bool IsMousePressed
        {
            get { return mousePressed; }
            set { mousePressed = value; }
        }

        public bool WasMouseJustReleased
        {
            get { return mouseJustReleased; }
            set { mouseJustReleased = value; }
        }

        public bool IsKeyPressed(Keys key)
        {
            return keyPressed[(int)key];
        }

        public bool WasKeyReleased(Keys key)
        {
            return keyJustReleased[(int)key];
        }

        public void Update()
        {
            // back button 
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                backPressed = true;
            else
                backPressed = false;
/*
            TouchCollection touchCollection = TouchPanel.GetState();
            foreach (TouchLocation tl in touchCollection)
            {      
                    mouseX = (int)tl.Position.X;
                    mouseY = (int)tl.Position.Y;

                   if (mouseJustReleased)
                       mouseJustReleased = false;
                    if (tl.State == TouchLocationState.Pressed)
                    {
                        mousePressed = true;
                    }
                    else
                    {
                        if (mousePressed)
                        {
                            mouseJustReleased = true;
                        }
                        mousePressed = false;
                    }
            }*/
            // phones don't have mouse/keyboard libraries
            
            // mouse stuff
            mousestate = Mouse.GetState();
            mouseX = mousestate.X;
            mouseY = mousestate.Y;
            if (mouseJustReleased)
                 mouseJustReleased = false;
            if (mousestate.LeftButton == ButtonState.Pressed)
            {
                mousePressed = true;
            }
            else
            {
                if (mousePressed)
                {
                    mouseJustReleased = true;
                }
                mousePressed = false;
            }
#if WINDOWS
            // keyboard stuff
            KeyboardState keyState = Keyboard.GetState();

            Array values = Enum.GetValues(typeof(Keys));

            foreach (Keys val in values)
            {
                //Console.WriteLine(String.Format("{0}: {1}", Enum.GetName(typeof(Keys), val), val));

                keyJustReleased[(int)val] = false;  
                if (keyState.IsKeyUp(val) && keyPressed[(int)val])
                {
                    keyJustReleased[(int)val] = true;          
                }
                if (keyState.IsKeyDown(val))
                {
                    keyPressed[(int)val] = true;
                }
                else
                {
                    keyPressed[(int)val] = false;
                }
            }
#endif

        }
    }
}
