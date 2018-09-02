Sovereign Engine Projects
=========================

This section describes the various projects that comprise the engine.

Client Projects
---------------

### ClientCore

ClientCore is the main class library for the client-specific code.

### D3D11Renderer

D3D11Renderer is an implementation of the rendering interfaces defined in
ClientCore that uses Direct3D 11.

### SovereignClient

SovereignClient is an executable wrapper around ClientCore that configures the
engine to run as a client.

Server Projects
---------------

### SovereignServer

SovereignServer is an executable wrapper around EngineCore that configures the
engine to run as a server.

### WorldGen

WorldGen is a utility library for procedurally generating game worlds and
world domains.

### StandaloneWorldGen

StandaloneWorldGen is a command line wrapper around WorldGen to allow for
offline world generation for development purposes.

Common Projects
---------------

### EngineCore

EngineCore is the main common class library that contains the bulk of the
game logic.

### EngineUtil

EngineUtil provides reusable software components such as collections that
are used throughout the other projects.

### WorldLib

WorldLib defines a data model for describing a game world. Refer to
world_structure.md for further information.

