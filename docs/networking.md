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
periodically as an idempotent primary defining event.

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

| Function            | Direction        | Ordering Required | Reliability Required |
|---------------------|------------------|-------------------|----------------------|
| Chat                | Bidirectional    | Yes               | Yes                  |
| Keep-Alive          | Bidirectional    | No                | No                   |
| Login               | Bidirectional    | Yes               | Yes                  |
| Player Input        | Client -> Server | Yes               | No                   |
| Send Entity Updates | Server -> Client | No                | No                   |
| Send World Data     | Server -> Client | No                | Yes                  |

For functions where ordering is required, the ordering of packets is only
important within the set of packets related to that function. For example,
if a chat packet and a world data packet were both being sent, the order in
which the two packets are delivered is not important. However, if two chat
packets were being sent, then the order is important. The engine therefore
groups packets by function into separate channels in order to avoid
unnecessary performance hits.

## Connections

### Authentication

The first stage of establishing a connection is to authenticate. This is done
out-of-band via a REST API exposed by the server. This REST API should be
placed behind a TLS termination proxy (e.g. nginx configured for this role)
to provide proper security.

Once authentication is complete, a shared secret is established between the
server and client. This secret is used as the HMAC key for message validation.

### Player Character Selection

Following successful authentication, the client must select a player character
to use. Each account is linked to zero or more player characters. The client
may also create a player character; creating a player character automatically
selects the new player character for the session. Similar to authentication,
this is all accomplished via a REST API exposed by the server.

### Event Server Connection

Finally, once the client has selected the player character to be used, the
client must connect to the event server. This is done by opening a connection
via the LiteNetLib library to the server, passing the account ID as the
connection key. Establishing the event server connection completes the
connection process.

## World State Synchronization

The server is responsible for providing updates to each connected client to
allow the clients to maintain a locally synchronized world state with the server
as described above. Several mechanisms exist to facilitate this synchronization
for various aspects of the world state. Taken together in sequence, they allow
each client to efficiently synchronize the local world state to the server with
a tolerable level of error.

Local state synchronization is achieved one world segment at a time (see the
[world structure](world_structure.md) and [persistence](persistence.md) documentation
for details). Synchronization for a world segment is an ongoing process that starts when
a player character moves into the *update radius* of that world segment. The update
radius is specified as an integer number of world segment lengths, and the distance from
a player to a world segment is rounded up to the nearest multiple of the world segment
length when determining synchronization requirements.
Synchronization may only end once the player character has exited the radius. When a
player character enters the radius, we say that player *subscribes* to that world segment.
Similarly, the player is said to *unsubscribe* from a world segment when synchronization
ends. The server is responsible for tracking player movement in and out of world segments.

### Block Synchronization

Block entities are by far the largest set of world state that must be synchronized for a
world segment. Each world segments consists of up to 32768 blocks. However, this data
is expected to be *mostly* static with few updates.

When a player subscribes to a world segment, it first requests the latest state of all
block data in that world segment via an asynchronous REST API. The server then provides
a binary blob encoding a twice-compressed form of the block data. Simultaneously, the
server also begins to send single-block updates via the event server using a set of
events which specify the coordinates of the affected block and the nature of the change.
The client unpacks the binary blob to obtain an initial set of blocks, then applies
the received events to maintain synchronization. Block synchronization depends only on
the coordinates of the blocks - the entity ID of a block entity is never synchronized
between the server and client.

### Entity Synchronization

Non-block entities are similarly synchronized between the server and a client whenever
the player associated with a client is within the update radius of the world segment
that contains the entity. Refer to the activation rules specified in the
[persistence documentation](persistence.md) for details on how entities are associated
to specific world segments.

When a player subscribes to a world segment, all non-block entities in that world segment
are initially advertised to the client by a sequence of events specifying the entity ID
and the *publicly visible* components of that entity. Note that not all components are
considered to be publicly visible. If the client already knows about an advertised entity,
it accepts the newly advertised data as authoritative and overwrites its existing data
wherever there may be a conflict. This process serves to ensure that there is a
synchronized table of entity IDs shared by server and client at all times.

Once the entity IDs are synchronized, updates to the entity proceed via the same events
that mutate entity state on the server. These events, where they might mutate publicly
visible components of an entity, are relayed via the event server to the client. See
the above discussion on primary (idempotent) defining events for additional details.
