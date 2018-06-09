using Engine8.EngineCore.Events;
using Engine8.EngineCore.Events.Details;
using Engine8.EngineCore.Systems.Movement.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Systems.Input
{

    /// <summary>
    /// Maps player input to movement events.
    /// </summary>
    public class PlayerInputMovementMapper
    {

        /// <summary>
        /// Event communicator.
        /// </summary>
        private EventCommunicator eventCommunicator;

        public PlayerInputMovementMapper(EventCommunicator eventCommunicator)
        {
            this.eventCommunicator = eventCommunicator;
        }

        /// <summary>
        /// Updates player movement based on player input.
        /// </summary>
        /// <param name="up">Input state for upward movement.</param>
        /// <param name="down">Input state for downward movement.</param>
        /// <param name="left">Input state for leftward movement.</param>
        /// <param name="right">Input state for rightward movement.</param>
        public void UpdateMovement(bool up, bool down, bool left, bool right)
        {
            /* Check whether movement has started or stopped. */
            Event ev;
            if (up || down || left || right)
            {
                /* Compute direction of the movement. */
                float dx = (right ? 1.0f : 0.0f) - (left ? 1.0f : 0.0f);
                float dy = (down ? 1.0f : 0.0f) - (up ? 1.0f : 0.0f);

                /* Fire a movement event. */
                var details = new SetVelocityEventDetails()
                {
                    EntityId = 0,
                    RateX = dx,
                    RateY = dy,
                };
                ev = new Event(EventId.Core_Set_Velocity, details);
            }
            else
            {
                var details = new EntityEventDetails()
                {
                    EntityId = 0,
                };
                ev = new Event(EventId.Core_End_Movement, details);
            }

            /* Fire the event. */
            eventCommunicator.SendEvent(ev);
        }

    }

}
