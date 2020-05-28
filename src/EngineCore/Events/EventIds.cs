/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

namespace Sovereign.EngineCore.Events
{

    /// <summary>
    /// Enumerates the event ID numbers.
    /// </summary>
    public enum EventId
    {

        #region Core

        /// <summary>
        /// Unknown event type.
        /// </summary>
        /// Associated details: None
        Unknown = 0,

        /// <summary>
        /// Quit event ID.
        /// </summary>
        /// Associated details: None
        Core_Quit = 1,

        /// <summary>
        /// Event sent at the beginning of a new tick.
        /// </summary>
        /// Associated details: None
        Core_Tick = 2,

        #region Movement

        /// <summary>
        /// Attempted one-time relative movement of an entity.
        /// </summary>
        /// Associated details: MoveOnceEventDetails
        Core_Move_Once = 100,

        /// <summary>
        /// Sets the velocity of an entity.
        /// </summary>
        /// Associated details: SetVelocityEventDetails
        Core_Set_Velocity = 101,

        /// <summary>
        /// Ends the continous movement, if any, of an entity.
        /// </summary>
        /// Associated details: EntityEventDetails
        Core_End_Movement = 102,

        #endregion Movement

        #region Blocks

        /// <summary>
        /// Adds a single block.
        /// </summary>
        /// Associated details: BlockAddEventDetails
        Core_Block_Add = 200,

        /// <summary>
        /// Adds a batch of blocks at once.
        /// </summary>
        /// Associated details: BlockAddBatchEventDetails
        Core_Block_AddBatch = 201,

        /// <summary>
        /// Removes a single block.
        /// </summary>
        /// Associated details: EntityEventDetails
        Core_Block_Remove = 202,

        /// <summary>
        /// Removes a batch of blocks at once.
        /// </summary>
        /// Associated details: BlockRemoveBatchEventDetails
        Core_Block_RemoveBatch = 203,

        #endregion Blocks

        #region WorldManagement

        /// <summary>
        /// Event sent to load a world segment.
        /// </summary>
        /// Associated details: WorldSegmentEventDetails
        Core_WorldManagement_LoadSegment = 300,

        /// <summary>
        /// Unloads a world segment from memory.
        /// </summary>
        /// Associated details: WorldSegmentEventDetails
        Core_WorldManagement_UnloadSegment = 301,

        #endregion WorldManagement

        #region Performance

        /// <summary>
        /// Sent to perform a local event loop latency test.
        /// </summary>
        /// Associated details: TimeEventDetails
        Core_Performance_EventLatencyTest = 400,

        #endregion Performance

        #endregion Core

        #region Client

        #region Client_Input

        /// <summary>
        /// Event sent when a key is pressed.
        /// </summary>
        /// Associated details: KeyEventDetails
        Client_Input_KeyDown = 100000,

        /// <summary>
        /// Event sent when a key is released.
        /// </summary>
        /// Associated details: KeyEventDetails
        Client_Input_KeyUp = 100001,

        #endregion Client_Input

        #region Client_Camera

        /// <summary>
        /// Event sent when the camera is attached to another entity.
        /// </summary>
        /// Associated details: EntityEventDetails
        Client_Camera_Attach = 100100,

        /// <summary>
        /// Event sent when the camera is detached from another entity.
        /// </summary>
        /// Associated details: None
        Client_Camera_Detach = 100101,

        #endregion Client_Camera

        #endregion Client

        #region Server

        #region Server_Persistence

        /// <summary>
        /// Event sent to retrieve a specific entity from the database.
        /// </summary>
        /// Associated details: EntityEventDetails
        Server_Persistence_RetrieveEntity = 200000,

        /// <summary>
        /// Event sent to retrieve all entities positioned within a range
        /// from the database.
        /// </summary>
        /// Associated details: VectorPairEventDetails
        Server_Persistence_RetrieveEntitiesInRange = 200001,

        /// <summary>
        /// Event sent to synchronize the server with the database.
        /// </summary>
        /// Associated details: None
        Server_Persistence_Synchronize = 200099,

        #endregion Server_Persistence

        #endregion Server

    }

}
