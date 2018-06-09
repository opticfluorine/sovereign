using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SFML.Window.Keyboard;

namespace Engine8.ClientCore.Systems.Input
{

    /// <summary>
    /// Tracks the state of the keyboard.
    /// </summary>
    public class KeyboardState
    {

        public bool[] KeysDown { get; private set; } = new bool[128];

        /// <summary>
        /// Registers that a key has been pressed.
        /// </summary>
        /// <param name="key">Key that was pressed.</param>
        public void KeyDown(Key key)
        {
            if (key != Key.Unknown)
            {
                KeysDown[(int)key] = true;
            }
        }

        /// <summary>
        /// Registers that a key has been released.
        /// </summary>
        /// <param name="key">Key that was released.</param>
        public void KeyUp(Key key)
        {
            if (key != Key.Unknown)
            {
                KeysDown[(int)key] = false;
            }
        }

    }

}
