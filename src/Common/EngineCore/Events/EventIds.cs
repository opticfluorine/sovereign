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

using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineUtil.Attributes;

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
    [ScriptableEvent] Core_Tick = 2,

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

    /// <summary>
    ///     Initiates a jump.
    /// </summary>
    /// Associated details: EntityEventDetails
    Core_Movement_Jump = 102,

    /// <summary>
    ///     Teleports an entity to a new position.
    /// </summary>
    /// <remarks>
    ///     Vector in the details is the new position.
    /// </remarks>
    /// Associated details: EntityVectorEventDetails
    Core_Movement_Teleport = 103,

    /// <summary>
    ///     Notifies clients that a nearby entity is about to teleport.
    /// </summary>
    /// Associated details:
    Core_Movement_TeleportNotice = 104,

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

    /// <summary>
    ///     Removes a block at a given grid position.
    /// </summary>
    /// Associated details: GridPositionEventDetails
    Core_Block_RemoveAt = 204,

    /// <summary>
    ///     Notification that a single block has been created or modified. Used for server-to-client block
    ///     synchronization.
    /// </summary>
    /// Associated details: BlockAddEventDetails
    Core_Block_ModifyNotice = 205,

    /// <summary>
    ///     Notification that a single block has been removed. Used for server-to-client block synchronization.
    /// </summary>
    /// Associated details: GridPositionEventDetails
    Core_Block_RemoveNotice = 206,

    /// <summary>
    ///     Announces that the block presence grid for a z plane in a world segment has been modified.
    /// </summary>
    /// Associated details: BlockPresenceGridUpdatedEventDetails
    Core_Block_GridUpdated = 207,

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

    /// <summary>
    ///     Signals that the entities of a world segment have been loaded.
    /// </summary>
    /// Associated details: WorldSegmentEventDetails
    Core_WorldManagement_WorldSegmentLoaded = 603,

    /// <summary>
    ///     Signals that the entities of a world segment have been unloaded.
    /// </summary>
    /// Associated details: WorldSegmentEventDetails
    Core_WorldManagement_WorldSegmentUnloaded = 604,

    #endregion WorldManagement

    #region Network

    /// <summary>
    ///     Sent from client to server to log out of the current player and return to player selection.
    /// </summary>
    /// Associated details: EntityEventDetails
    [ScriptableEvent(nameof(EntityEventDetails))]
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

    /// <summary>
    ///     Sent from server to client to deliver a generic message.
    /// </summary>
    /// Associated details: GenericChatEventDetails
    Core_Chat_Generic = 804,

    #endregion Chat

    #region Data

    /// <summary>
    ///     Announces that a global key-value pair has been updated.
    /// </summary>
    /// Associated details: KeyValueEventDetails
    Core_Data_GlobalSet = 900,

    /// <summary>
    ///     Announces that a global key-value pair has been removed.
    /// </summary>
    /// Associated details: StringEventDetails (specifying the key)
    Core_Data_GlobalRemoved = 901,

    /// <summary>
    ///     Announces that an entity key-value pair has been updated.
    /// </summary>
    /// Associated details: EntityKeyValueEventDetails
    Core_Data_EntityKeyValueSet = 902,

    /// <summary>
    ///     Announces that an entity key-value pair has been removed.
    /// </summary>
    /// Associated details: EntityStringEventDetails
    Core_Data_EntityKeyValueRemoved = 903,

    #endregion Data

    #region EntitySynchronization

    /// <summary>
    ///     Event sent from server to client to synchronize non-block entities.
    /// </summary>
    /// Associated details: EntityDefinitionEventDetails
    Core_EntitySync_Sync = 1000,

    /// <summary>
    ///     Event sent from server to client to desynchronize a non-block entity.
    /// </summary>
    /// <remarks>
    ///     The entity ID in the details corresponds to the root of the entity tree to be desynchronized.
    ///     The grid position is the world segment index for which the entity is being desynchronized.
    /// </remarks>
    /// Associated details: EntityDesyncEventDetails
    Core_EntitySync_Desync = 1001,

    /// <summary>
    ///     Event sent from server to client to synchronize a template entity.
    /// </summary>
    /// Associated details: TemplateEntityDefinitionEventDetails
    Core_EntitySync_SyncTemplate = 1002,

    #endregion EntitySynchronization

    #region Time

    /// <summary>
    ///     Event sent to announce the current in-game time.
    /// </summary>
    /// Associated details: IntEventDetails (value is in-game time in seconds)
    Core_Time_Clock = 1100,

    #endregion Time

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
    ///     Event sent when the mouse is moved.
    /// </summary>
    /// Associated details: MouseMotionEventDetails
    Client_Input_MouseMotion = 100002,

    /// <summary>
    ///     Event sent when a mouse button is pressed.
    /// </summary>
    /// Associated details: MouseButtonEventDetails
    Client_Input_MouseDown = 100003,

    /// <summary>
    ///     Event sent when a mouse button is released.
    /// </summary>
    /// Associated details: MouseButtonEventDetails
    Client_Input_MouseUp = 100004,

    /// <summary>
    ///     Event sent when the mouse wheel is scrolled.
    /// </summary>
    /// Associated details: MouseWheelEventDetails
    Client_Input_MouseWheel = 100005,

    /// <summary>
    ///     Event sent by the Input system when the mouse scrolls a full "tick".
    ///     A value of true indicates a tick up, a value of false indicates a tick down.
    /// </summary>
    /// Associated details: BooleanEventDetails
    Client_Input_MouseWheelTick = 100006,

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

    /// <summary>
    ///     Event sent immediately before the client requests to create or select a player.
    /// </summary>
    /// Associated details: None
    Client_Network_AboutToSelectPlayer = 100207,

    #endregion Client_Network


    #region Client_State

    /// <summary>
    ///     Event sent to update the state of an individual client state flag.
    /// </summary>
    /// Associated details: ClientStateFlagEventDetails
    Client_State_SetFlag = 100400,

    /// <summary>
    ///     Event sent to update the main menu state.
    /// </summary>
    /// Associated details: MainMenuStateEventDetails
    Client_State_SetMainMenuState = 100401,

    #endregion Client_State

    #region Client_WorldEdit

    /// <summary>
    ///     Event sent to update the Z-offset for world editing.
    /// </summary>
    /// Associated details: GenericEventDetails(int)
    Client_WorldEdit_SetZOffset = 10500,

    /// <summary>
    ///     Event sent to update the pen width for world editing.
    /// </summary>
    /// Associated details: GenericEventDetails(int)
    Client_WorldEdit_SetPenWidth = 10501,

    #endregion

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
    ///     Event sent when a player has entered the world and their data is fully loaded.
    ///     Follows a Server_Accounts_SelectPlayer and the associated data load.
    /// </summary>
    /// <remarks>
    ///     Details entity is the player entity
    /// </remarks>
    /// Associated details: EntityEventDetails
    [ScriptableEvent(nameof(EntityEventDetails))]
    Server_Persistence_PlayerEnteredWorld = 200002,

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
    ///     Requests the WorldManagement system to resynchronize a positioned entity with subscribers.
    /// </summary>
    /// Associated details: EntityEventDetails
    Server_WorldManagement_ResyncPositionedEntity = 200401,

    #endregion Server_WorldManagement

    #region Server_TemplateEntity

    /// <summary>
    ///     Requests the TemplateEntity system to create or update a template entity to match a supplied entity
    ///     definition.
    /// </summary>
    /// Associated details: EntityDefinitionEventDetails
    Server_TemplateEntity_Update = 200500,

    #endregion Server_TemplateEntity

    #region Server_WorldEdit

    /// <summary>
    ///     Requests that a block be set in a position using admin privileges. Sent by the client-side
    ///     admin world editor.
    /// </summary>
    /// Associated details: BlockAddEventDetails
    Server_WorldEdit_SetBlock = 200600,

    /// <summary>
    ///     Requests that a block be removed from a position using admin privileges. Sent by the client-side
    ///     admin world editor.
    /// </summary>
    /// Associated details: GridPositionEventDetails
    Server_WorldEdit_RemoveBlock = 200601,

    #endregion

    #region Server_Scripting

    /// <summary>
    ///     Requests that all scripts be reloaded.
    /// </summary>
    /// Associated details: None
    Server_Scripting_ReloadAll = 200700,

    /// <summary>
    ///     Requests that a specific script be reloaded.
    /// </summary>
    /// Associated details: StringEventDetails
    Server_Scripting_Reload = 200701,

    /// <summary>
    ///     Requests that any new scripts be loaded without reloading existing scripts.
    /// </summary>
    /// Associated details: None
    Server_Scripting_LoadNew = 200702,

    /// <summary>
    ///     Event used to signal a timed callback. Details contain a unique identifier for the callback request.
    /// </summary>
    /// Associated details: IntEventDetails
    Server_Scripting_TimedCallback = 200703,

    #endregion Server_Scripting

    #endregion Server
}