# Adding Events

Sovereign Engine utilizes events for cross-network communication as well as
internal communication within each application. It is frequently necessary to add
new events to the client and/or server in the process of adding new systems, features,
or game mechanics. This guide outlines the process of adding a new event and
(optionally) configuring the event to be repeated over the event connection.

## Adding a New Event

Adding a new event without network replication is a straightforward process:

1. Add a new event to the `EventIds` enum. Note that this enum is organized by
   application (client/server/"core") and system. The general naming convention for
   events is to name them after the primary system responsible for processing the
   event and its location.
   :::{note}
   Specifying the application of an event indicates the application that is
   responsible for processing the event, not the sending application. For example, 
   `Client_EntitySynchronization_Sync` is processed by the client to synchronize an
   entity with the server, but it is exclusively sent by the client. Similarly,
   `Core_Movement_Move` is sent and processed by both the client and server.
   :::
2. If the event needs to carry any information, either select an existing
   `IEventDetails` class or create a new one to carry this information. Event details
   classes should be named by their contents rather than the function of their
   corresponding events so that they may be more easily reused across multiple
   events.
3. Subscribe to the event in any systems that need to process it by adding the new
   event ID to the `ISystem.EventIdsOfInterest` property.
4. Add or update a `Controller` and/or `InternalController` class as needed to expose
   a method-based API for sending the new event. `Controller` classes should be in
   the same namespace as the primary processing system, whereas `InternalController`
   classes should be in the same namespace as the primary system responsible for
   sending the event.
   :::{note}
   `Controller` classes are generally preferred for `Core` events and for events that
   are sent and processed within a single application; they provide an asynchronous
   API for interacting with the system. `InternalController` classes are necessary
   when the event is processed exclusively in a different application than where it
   is sent as the processing system will not be present in the sending application.
   :::

## Replicating an Event via the Network

Many events need to be replicated over the event connection in one or both directions.
By default, events are not replicated over the network, and events are not accepted
over the network unless the receiving application is explicity configured to do so.
This configuration requires a number of changes:

1. Add the event ID to the `IOutboundEventSet` class for any application that will
   be sending the event over the network.
2. Add the event ID to the `XAllowedEventsInboundPipelineStage` class (where `X`
   is `Client` or `Server`) for any application that will be receiving the event
   over the network.
3. Add the event ID to `DeliveryMethodOutboundPipelineStage` with an appropriate
   network delivery method. When in doubt, choose `DeliveryMethod.ReliableUnordered`.
4. Add the event ID to `ValidationInboundPipelineStage` with the appropriate
   `IEventDetailsValidator` class for the `IEventDetails` class associated with
   the event. You may need to create a new `IEventDetailsValidator` class if the
   event uses a new `IEventDetails` class or one that has not been used with a
   network replicated event yet.
   :::{note}
   If the event has no associated details, it must still be added to
   `ValidationInboundPipelineStage` with the `NullEventDetailsValidator` validator,
   otherwise the event will be rejected by the receiving application.
   :::
5. If the event will be sent from the server, add the event ID to
   `ServerConnectionMappingOutboundPipelineStage` with the appropriate connection
   mapper. The connection mapper is responsible for determining which connections
   will receive the event when it is sent by the server. Connection mappers typically
   use information in the event details to determine which players should receive
   the event.
   :::{note}
   Connection mappers can map events to any connection, not just to players in the
   game world. This distinction is important for template entity synchronization,
   where the template entities are loaded once at initial login and then kept in
   sync through a series of events that are broadcast to all connections by the
   server.
   :::
6. If the event will be sent from client to server, and if the event details must
   contain the entity ID of the player who sent the event, update
   `SourceEntityMappingInboundPipelineStage` with a new function that modifies
   the corresponding event details to contain the entity ID of the sender.
   :::{warning}
   Never trust the client to send the correct player entity ID for themselves. A
   malicious client could send an event with a spoofed entity ID. The
   `SourceEntityMappingInboundPipelineStage` mitigates this threat by overriding
   the entity ID in the details with the entity ID associated with the event
   server connection that received the event.
   :::
