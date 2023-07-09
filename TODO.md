# Sovereign Engine To-Do Items

## Networking

### Event Client/Server

* Implement `IConnectionMappingOutboundPipelineStage` for client to route all
  events to the main connection.
* Implement `IConnectionMappingOutboundPipelineStage` for server to route
  events to any interested connection.
* Wire up the outbound event queues in the network managers.
* Test network code.
