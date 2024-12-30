# events Module

The `events` module provides information about the various types of events
that scripts can react to.

(script-api-event-types)=
## Supported Event Types

The following event types are supported in server-side scripts.

|Event|Details Type|Purpose|
|----------------------------------------------|---------------------|----------------------------------------|
|`events.Core_Tick`                            |`nil`                |Sent at the beginning of each game tick.|
|`events.Core_Network_Logout`                  |`EntityEventDetails` |Sent when a player logs out.            |
|`events.Server_Persistence_PlayerEnteredWorld`|`EnitityEventDetails`|Sent when a player logs in.             |
