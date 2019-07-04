# Sovereign Engine Networking

The Sovereign Engine networking code is designed as an extension of the event
mechanism used by both the client and server. The engine communicates over the
network by serializing and sending the minimum set of events necessary to
reconstruct the current game state at the remote endpoint.

## Event-Based Networking Architecture

### Game State

The game state at any given time is defined by the current server tick and
the set of all entities and components at that tick. Both the client and the
server maintain a game state at all times; these game states are not identical,
nor is the client game state strictly a subset of the server game state.

The global game state is the subset of the server game state that potentially
overlaps with any client game state. Equivalently, the global game state is the
subset of the server game state that may be communicated to clients. For
example, this would include visible entities (such as players, NPCs, and items)
and any observable components (such as position, velocity, and HP), but would 
exclude any server or client secrets as well as any internal state (e.g. 
components related to rendering, GUI-related components, caching lifetimes, 
etc.). 

The full global game state is always known to the server, but it is not
necessarily known to any individual client. Clients only need to know about
the subset of the global game state that is visible to the player.

The global game state is not directly communicated over the network. Instead,
the events from which the global game state is constructed are sent over the
network. The set of relevant events are referred to as the _primary defining
events_. The set of events which may induce a primary defining event are
referred to as _secondary defining events_. All other events are collectively
referred to as _nondefining events_. The sequence of all produced primary
defining events is sufficient to reconstruct the global game state, and so the
secondary defining events and the nondefining events are not sent over the
network.

### Client-Server Relationship

Sovereign Engine uses a client-server model where many clients connect to a
single server over the network. The server is fully authoritative; that is,
it is always considered the authority on the current global game state.

The server specifies the global game state to the clients by sending 
authoritative events to the clients. The global game state may then be
reconstructed by applying the events received from the server. The client shall
adopt the state described by the server in all cases, including any cases where
the state conflicts with the local state of the client. This is similar to the
idea of "event sourcing" where the sequence of events is the primary
representation of the data. Sovereign Engine stops short of full event
sourcing; the entities and components are the primary representation of the
data, and the sequence of events are only a secondary representation used to
communicate the minimum subset of the global same state over the network.

The client informs the server of any attempted changes to the global game
state (e.g. a change in the position of the player in response to player
input) by forwarding the relevant events. The client may adopt the changed 
state by processing these _speculative events_, until such time as an updated 
state is determined from the _authoritative events_ sent by the server. The 
server will not send any acknowledgment or rejection packets in response to 
speculative events; the server event sequence is the authoritative definition 
of the global game state.

### Designing Primary Defining Events

Primary defining events must create the server-defined authoritative global
game state when processed by the client, regardless of whether the client has
previously processed speculative events that produced an invalid state. As
such, primary defining events that affect component values should specify
absolute values of the components. For example, an entity movement event
should specify the new position of the entity following the event, not an
offset relative to the current entity position.

Relative-valued events are still useful as they are composable; for example,
two relative HP changes will have a cumulative effect, whereas two absolute
HP changes in the same tick will only apply the last value to be processed.
Relative-valued events should be used in system logic and treated as
secondary defining events. Their cumulative effect should then be communicated
once per tick as a primary defining event.

A simple method for reducing multiple secondary defining events into a single
primary defining event is to extend `BaseComponentReducer<T>`. The main 
drawback of this method is that the primary event is not issued until the next
tick as the components are only updated at a tick boundary, leading to a worst
case latency of (tick length + 1/2 * round trip time). Consequently the server
will lag to player inputs by a minimum of the tick length, though this overhead
becomes negligible for high-ping clients. The lagging effects should average 
out over several ticks as the server sends authoritative updates, but if not, 
more advanced compensation methods will be required.


## Packets

### Packet Reliability

Sovereign Engine sends packets between clients and servers using UDP. UDP is
a stateless protocol that offers no reliability or ordering guarantees. Any
ordering or reliability needed by Sovereign must be supported at the
application layer.

Packets sent between the client and server may be categorized based on their
need for ordering and/or reliability. The table below categorizes some common
types of communication used by Sovereign Engine.

Function | Direction | Ordering Required | Reliability Required
--- | --- | --- | ---
Chat | Bidirectional | Yes | Yes
Keep-Alive | Bidirectional | No | No
Login | Bidirectional | Yes | Yes
Player Input | Client -> Server | Yes | No
Send Entity Updates | Server -> Client | No | No
Send World Data | Server -> Client | No | Yes

For functions where ordering is required, the ordering of packets is only
important within the set of packets related to that function. For example,
if a chat packet and a world data packet were both being sent, the order in
which the two packets are delivered is not important. However, if two chat
packets were being sent, then the order is important. The engine therefore
groups packets by function into separate channels in order to avoid
unnecessary performance hits.


## Connections

### Authentication

> :information_source: Authentication support is targeted for Milestone 2.

The first stage of establishing a connection is to authenticate. This is done
out-of-band via a REST API exposed by the server. This REST API should be 
placed behind a TLS termination proxy (e.g. nginx configured for this role) 
to provide proper security.

The authentication method is currently undefined, though OAuth 2.0 support is
planned.

Once authentication is complete, a shared secret is established between the
server and client. This secret is used as the HMAC key for message validation.

