/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

namespace Sovereign.EngineCore.Events;

/// <summary>
///     Enumerates the event ID numbers.
/// </summary>
public enum EventId
{
    #region Core

    /// <summary>
    ///     Unknown event type.
    /// </summary>
    /// Associated details: None
    Unknown = 0,

    /// <summary>
    ///     Quit event ID.
    /// </summary>
    /// Associated details: None
    Core_Quit = 1,

    /// <summary>
    ///     Event sent at the beginning of a new tick.
    /// </summary>
    /// Associated details: None
    Core_Tick = 2,

    #region Movement

    /// <summary>
    ///     Request from client to server to move the player character.
    /// </summary>
    /// Associated details: RequestMoveEventDetails
    Core_Movement_RequestMove = 100,

    /// <summary>
    ///     Movement notification.
    /// </summary>
    /// Associated details: MoveEventDetails
    Core_Movement_Move = 101,

    #endregion Movement

    #region Blocks

    /// <summary>
    ///     Adds a single block.
    /// </summary>
    /// Associated details: BlockAddEventDetails
    Core_Block_Add = 200,

    /// <summary>
    ///     Adds a batch of blocks at once.
    /// </summary>
    /// Associated details: BlockAddBatchEventDetails
    Core_Block_AddBatch = 201,

    /// <summary>
    ///     Removes a single block.
    /// </summary>
    /// Associated details: EntityEventDetails
    Core_Block_Remove = 202,

    /// <summary>
    ///     Removes a batch of blocks at once.
    /// </summary>
    /// Associated details: BlockRemoveBatchEventDetails
    Core_Block_RemoveBatch = 203,

    #endregion Blocks

    #region Performance

    /// <summary>
    ///     Sent to perform a local event loop latency test.
    /// </summary>
    /// Associated details: TimeEventDetails
    Core_Performance_EventLatencyTest = 400,

    #endregion Performance

    #region Ping

    /// <summary>
    ///     Sent to initiate a ping.
    /// </summary>
    /// Associated details: None
    Core_Ping_Ping = 500,

    /// <summary>
    ///     Sent as reply to a ping.
    /// </summary>
    /// Associated details: None
    Core_Ping_Pong = 501,

    /// <summary>
    ///     Sent to command the ping system to initiate a ping.
    /// </summary>
    /// Associated details: None
    Core_Ping_Start = 502,

    /// <summary>
    ///     Sent to command the ping system to enable or disable automatic pinging.
    /// </summary>
    /// Associated details: AutoPingEventDetails
    Core_Ping_SetAuto = 503,

    #endregion Ping

    #region WorldManagement

    /// <summary>
    ///     Sent from server to client to announce subscription to a world segment.
    /// </summary>
    /// Associated details: WorldSegmentSubscriptionEventDetails
    Core_WorldManagement_Subscribe = 600,

    /// <summary>
    ///     Sent from server to client to announce unsubscription from a world segment.
    /// </summary>
    /// Associated details: WorldSegmentSubscriptionEventDetails
    Core_WorldManagement_Unsubscribe = 601,

    /// <summary>
    ///     Sent from server to client to announce that an entity has left a world segment.
    ///     Used to assist with unloading entities from the client if they exit the subscribed
    ///     area. Sent to clients subscribed to the previous world segment.
    /// </summary>
    /// Associated details: EntityChangeWorldSegmentEventDetails
    Core_WorldManagement_EntityLeaveWorldSegment = 602,

    #endregion WorldManagement

    #region Network

    /// <summary>
    ///     Sent from client to server to log out of the current player and return to player selection.
    /// </summary>
    /// Associated details: EntityEventDetails
    Core_Network_Logout = 700,

    #endregion Network

    #region Chat

    /// <summary>
    ///     Sent from client to server to send a chat message.
    /// </summary>
    /// Associated details: ChatEventDetails
    Core_Chat_Send = 800,

    /// <summary>
    ///     Sent from server to client to deliver a local chat message.
    /// </summary>
    /// Associated details: LocalChatEventDetails
    Core_Chat_Local = 801,

    /// <summary>
    ///     Sent from server to client to deliver a global chat message.
    /// </summary>
    /// Associated details: ChatEventDetails
    Core_Chat_Global = 802,

    /// <summary>
    ///     Sent from server to client to deliver a system message.
    /// </summary>
    /// Associated details: SystemChatEventDetails
    Core_Chat_System = 803,

    #endregion Chat

    #endregion Core

    #region Client

    #region Client_Input

    /// <summary>
    ///     Event sent when a key is pressed.
    /// </summary>
    /// Associated details: KeyEventDetails
    Client_Input_KeyDown = 100000,

    /// <summary>
    ///     Event sent when a key is released.
    /// </summary>
    /// Associated details: KeyEventDetails
    Client_Input_KeyUp = 100001,

    /// <summary>
    ///     Event sent to repeat an active movement keypress.
    /// </summary>
    /// Associated details: SequenceEventDetails
    Client_Input_RepeatMove = 100002,

    #endregion Client_Input

    #region Client_Camera

    /// <summary>
    ///     Event sent when the camera is attached to another entity.
    /// </summary>
    /// Associated details: EntityEventDetails
    Client_Camera_Attach = 100100,

    /// <summary>
    ///     Event sent when the camera is detached from another entity.
    /// </summary>
    /// Associated details: None
    Client_Camera_Detach = 100101,

    #endregion Client_Camera

    #region Client_Network

    /// <summary>
    ///     Event sent when the client believes the connection to the server has been lost.
    /// </summary>
    /// Associated details: None
    Client_Network_ConnectionLost = 100200,

    /// <summary>
    ///     Event sent to command ClientNetworkSystem to begin connecting to the server.
    ///     This causes an asynchronous authentication attempt to be made with the REST server.
    /// </summary>
    /// Associated details: BeginConnectionEventDetails
    Client_Network_BeginConnection = 100201,

    /// <summary>
    ///     Event sent when the client has failed to authenticate with the REST server.
    /// </summary>
    /// Associated details: ErrorEventDetails
    Client_Network_LoginFailed = 100202,

    /// <summary>
    ///     Event sent when the client has failed to establish an event server connection.
    /// </summary>
    /// Associated details: ErrorEventDetails
    Client_Network_ConnectionAttemptFailed = 100203,

    /// <summary>
    ///     Event sent when the client has successfully connected to the event server.
    /// </summary>
    /// Associated details: None
    Client_Network_Connected = 100204,

    /// <summary>
    ///     Event sent to set the entity ID corresponding to the logged in player.
    /// </summary>
    /// Associated details: EntityEventDetails
    Client_Network_PlayerEntitySelected = 100205,

    /// <summary>
    ///     Event sent to disconnect from the server if connected.
    /// </summary>
    /// Associated details: None
    Client_Network_EndConnection = 100206,

    #endregion Client_Network

    #region Client_EntitySynchronization

    /// <summary>
    ///     Event sent from server to client to synchronize non-block entities.
    /// </summary>
    /// Associated details: EntityDefinitionEventDetails
    Client_EntitySynchronization_Sync = 100300,

    /// <summary>
    ///     Event sent from server to client to desynchronize a non-block entity.
    /// </summary>
    /// <remarks>
    ///     The entity ID in the details corresponds to the root of the entity tree to be desynchronized.
    ///     The grid position is the world segment index for which the entity is being desynchronized.
    /// </remarks>
    /// Associated details: EntityDesyncEventDetails
    Client_EntitySynchronization_Desync = 100301,

    #endregion Client_EntitySynchronization

    #region Client_State

    /// <summary>
    ///     Event sent when the client has loaded a world segment from the server.
    /// </summary>
    /// Associated details: WorldSegmentEventDetails
    Client_State_WorldSegmentLoaded = 100400,

    /// <summary>
    ///     Event sent to update the state of an individual client state flag.
    /// </summary>
    /// Associated details: ClientStateFlagEventDetails
    Client_State_SetFlag = 100401,

    /// <summary>
    ///     Event sent to update the main menu state.
    /// </summary>
    /// Associated details: MainMenuStateEventDetails
    Client_State_SetMainMenuState = 100402,

    #endregion Client_State

    #endregion Client

    #region Server

    #region Server_Persistence

    /// <summary>
    ///     Event sent to retrieve a specific entity from the database.
    /// </summary>
    /// Associated details: EntityEventDetails
    Server_Persistence_RetrieveEntity = 200000,

    /// <summary>
    ///     Event sent to load a world segment from the database.
    /// </summary>
    /// Associated details: WorldSegmentEventDetails
    Server_Persistence_RetrieveWorldSegment = 200001,

    /// <summary>
    ///     Event sent to synchronize the server with the database.
    /// </summary>
    /// Associated details: None
    Server_Persistence_Synchronize = 200098,

    /// <summary>
    ///     Event sent to announce that entity synchronization is complete.
    /// </summary>
    /// Associated details: None
    Server_Persistence_SynchronizeComplete = 200099,

    #endregion Server_Persistence

    #region Server_Debug

    /// <summary>
    ///     Event sent to issue a debug command.
    /// </summary>
    /// Associated details: DebugCommandEventDetails
    Server_Debug_Command = 200100,

    #endregion Server_Debug

    #region Server_Network

    /// <summary>
    ///     Event sent to disconnect a client.
    /// </summary>
    /// Associated details: ConnectionIdEventDetails
    Server_Network_DisconnectClient = 200200,

    /// <summary>
    ///     Event sent when a client has disconnected.
    /// </summary>
    /// Associated details: ConnectionIdEventDetails
    Server_Network_ClientDisconnected = 200201,

    #endregion Server_Network

    #region Server_Accounts

    /// <summary>
    ///     Event sent to select a player character during account login.
    /// </summary>
    /// Associated details: SelectPlayerEventDetails
    Server_Accounts_SelectPlayer = 200300,

    #endregion Server_Accounts

    #region Server_WorldManagement

    /// <summary>
    ///     Signals that the entities of a world segment have been loaded.
    /// </summary>
    /// Asssociated details: WorldSegmentEventDetails
    Server_WorldManagement_WorldSegmentLoaded = 200400,

    /// <summary>
    ///     Requests the WorldManagement system to resynchronize a positioned entity with subscribers.
    /// </summary>
    /// Associated details: EntityEventDetails
    Server_WorldManagement_ResyncPositionedEntity = 200401,

    #endregion Server_WorldManagement

    #endregion Server
}