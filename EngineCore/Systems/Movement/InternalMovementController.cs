using Engine8.EngineCore.Events;
using Engine8.EngineCore.Systems.Movement.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.EngineCore.Systems.Movement
{

    /// <summary>
    /// Defines a private API for use within the MovementSystem to
    /// control the movement of entities.
    /// </summary>
    public class InternalMovementController
    {

        /// <summary>
        /// Event sender used for communication with the event loop.
        /// </summary>
        private readonly EventSender eventSender;

        public InternalMovementController(EventSender eventSender)
        {
            this.eventSender = eventSender;
        }

        /// <summary>
        /// Schedules a movement of an entity to occur at the given time.
        /// 
        /// This method is intended to be used within the MovementSystem only.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="movementPhase">Unique movement phase identifier.</param>
        /// <param name="movementTime">System time of the movement, in us.</param>
        internal void ScheduleMovement(ulong entityId, uint movementPhase,
            ulong movementTime)
        {
            var details = new MoveOnceEventDetails()
            {
                EntityId = entityId,
                MovementPhase = movementPhase,
            };
            var ev = new Event(EventId.Core_Move_Once, details, movementTime);

            eventSender.SendEvent(ev);
        }

    }
}
