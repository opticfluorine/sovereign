using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Movement.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Systems.Movement
{

    /// <summary>
    /// Provides a public API for controlling the movement of entities.
    /// </summary>
    public class MovementController
    {

        /// <summary>
        /// Event sender used for communication with the event loop.
        /// </summary>
        private readonly EventSender eventSender;

        public MovementController(EventSender eventSender)
        {
            this.eventSender = eventSender;
        }

        /// <summary>
        /// Sets the movement for the given entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="rateX"></param>
        /// <param name="rateY"></param>
        /// <param name="eventTime"></param>
        public void SetMovement(ulong entity, float rateX, float rateY,
            ulong eventTime=Event.TIME_IMMEDIATE)
        {
            var details = new SetVelocityEventDetails()
            {
                EntityId = entity,
                RateX = rateX,
                RateY = rateY,
            };
            var ev = new Event(EventId.Core_Set_Velocity, details, eventTime);
        }

    }

}
