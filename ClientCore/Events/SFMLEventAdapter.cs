/*
 * Engine8 Dynamic World MMORPG Engine
 * Copyright (c) 2018 opticfluorine
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using Engine8.ClientCore.Rendering.Display;
using Engine8.EngineCore.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Events
{

    /// <summary>
    /// Converts SFML events into engine events.
    /// </summary>
    public class SFMLEventAdapter : IEventAdapter
    {

        /// <summary>
        /// Main display.
        /// </summary>
        private readonly MainDisplay mainDisplay;

        /// <summary>
        /// Queue of converted events ready for dispatch.
        /// </summary>
        private readonly Queue<Event> eventQueue = new Queue<Event>();

        public SFMLEventAdapter(MainDisplay mainDisplay)
        {
            /* Register SFML event handlers. */
            this.mainDisplay = mainDisplay;
            var window = mainDisplay.RenderWindow;
            window.Closed += Window_Closed;
            window.GainedFocus += Window_GainedFocus;
            window.JoystickButtonPressed += Window_JoystickButtonPressed;
            window.JoystickButtonReleased += Window_JoystickButtonReleased;
            window.JoystickConnected += Window_JoystickConnected;
            window.JoystickDisconnected += Window_JoystickDisconnected;
            window.JoystickMoved += Window_JoystickMoved;
            window.KeyPressed += Window_KeyPressed;
            window.KeyReleased += Window_KeyReleased;
            window.LostFocus += Window_LostFocus;
            window.MouseButtonPressed += Window_MouseButtonPressed;
            window.MouseButtonReleased += Window_MouseButtonReleased;
            window.MouseEntered += Window_MouseEntered;
            window.MouseLeft += Window_MouseLeft;
            window.MouseMoved += Window_MouseMoved;
            window.MouseWheelScrolled += Window_MouseWheelScrolled;
            window.Resized += Window_Resized;
            window.SensorChanged += Window_SensorChanged;
            window.TextEntered += Window_TextEntered;
            window.TouchBegan += Window_TouchBegan;
            window.TouchEnded += Window_TouchEnded;
            window.TouchMoved += Window_TouchMoved;
        }

        public void PrepareEvents()
        {
            mainDisplay.RenderWindow.DispatchEvents();
        }

        public Event PollEvent()
        {
            /* Check if an event is available from the main window. */
            return eventQueue.Count > 0 ? eventQueue.Dequeue() : null;
        }

        private void Window_TouchMoved(object sender, SFML.Window.TouchEventArgs e)
        {
            
        }

        private void Window_TouchEnded(object sender, SFML.Window.TouchEventArgs e)
        {
            
        }

        private void Window_TouchBegan(object sender, SFML.Window.TouchEventArgs e)
        {
            
        }

        private void Window_TextEntered(object sender, SFML.Window.TextEventArgs e)
        {
            
        }

        private void Window_SensorChanged(object sender, SFML.Window.SensorEventArgs e)
        {
            
        }

        private void Window_Resized(object sender, SFML.Window.SizeEventArgs e)
        {
            
        }

        private void Window_MouseWheelScrolled(object sender, SFML.Window.MouseWheelScrollEventArgs e)
        {
            
        }

        private void Window_MouseMoved(object sender, SFML.Window.MouseMoveEventArgs e)
        {
            
        }

        private void Window_MouseLeft(object sender, EventArgs e)
        {
            
        }

        private void Window_MouseEntered(object sender, EventArgs e)
        {
            
        }

        private void Window_MouseButtonReleased(object sender, SFML.Window.MouseButtonEventArgs e)
        {
            
        }

        private void Window_MouseButtonPressed(object sender, SFML.Window.MouseButtonEventArgs e)
        {
            
        }

        private void Window_LostFocus(object sender, EventArgs e)
        {
            
        }

        private void Window_KeyReleased(object sender, SFML.Window.KeyEventArgs e)
        {
            
        }

        private void Window_KeyPressed(object sender, SFML.Window.KeyEventArgs e)
        {
            
        }

        private void Window_JoystickMoved(object sender, SFML.Window.JoystickMoveEventArgs e)
        {
            
        }

        private void Window_JoystickDisconnected(object sender, SFML.Window.JoystickConnectEventArgs e)
        {
            
        }

        private void Window_JoystickConnected(object sender, SFML.Window.JoystickConnectEventArgs e)
        {
            
        }

        private void Window_JoystickButtonReleased(object sender, SFML.Window.JoystickButtonEventArgs e)
        {
            
        }

        private void Window_JoystickButtonPressed(object sender, SFML.Window.JoystickButtonEventArgs e)
        {
            
        }

        private void Window_GainedFocus(object sender, EventArgs e)
        {
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            /* Map to a Core_Quit event. */
            var ev = new Event(EventId.Core_Quit);
            eventQueue.Enqueue(ev);
        }

    }

}
