# Events Module

The `Events` module provides information about the various types of events
that scripts can react to.

(script-api-event-types)=
## Supported Event Types

The following event types are supported in server-side scripts.

| Event                                      | Details Type                                          | Purpose                                |
|--------------------------------------------|-------------------------------------------------------|----------------------------------------|
|Events.Core_Tick                            |nil                                                    |Sent at the beginning of each game tick.|
|Events.Core_Network_Logout                  |[EntityEventDetails](#script-types-entityeventdetails) |Sent when a player logs out.            |
|Events.Server_Persistence_PlayerEnteredWorld|[EntityEventDetails](#script-types-entityeventdetails) |Sent when a player logs in.             |
|Events.Server_Chat_MuteAdded                |[ModerationEventDetails](#script-types-moderationeventdetails)|Sent when a chat mute is added for a player.|
|Events.Server_Chat_MuteRemoved              |[ModerationEventDetails](#script-types-moderationeventdetails)|Sent when a chat mute is removed for a player.|
