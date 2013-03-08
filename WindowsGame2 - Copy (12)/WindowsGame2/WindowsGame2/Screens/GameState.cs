using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace WordGridGame.Screens
{
    /// <summary>
    /// A GameState is a single layer that has update and draw logic, and which
    /// can be combined with other layers to build up a complex menu system.
    /// For instance the main menu, the options menu, the "are you sure you
    /// want to quit" message box, and the main game itself are all implemented
    /// as screens.
    class GameState
    {
        protected Shared shared;

        public GameState()
        {
        }

        public virtual void Initialize()
        {
            shared = Shared.Instance;
        }
        public virtual void InitGame()
        {
        }
        public virtual void Draw()
        {
        }

        public virtual void Update()
        {
        }

        public virtual bool LoadFromStorage()
        {
            return false;
        }

        public virtual void SaveToStorage()
        {
        }

        public virtual void DeleteSavedGame()
        {
            
        }

    }
}
