Sovereign Engine Projects
=========================

This section describes the various projects that comprise the engine.

Client Projects
---------------

### ClientCore

`ClientCore` is the main class library for the client-specific code.

### D3D11Renderer

`D3D11Renderer` is an implementation of the rendering interfaces defined in
ClientCore that uses Direct3D 11.

### Shaders

`Shaders` contains the HLSL shaders used by `D3D11Renderer`. Building this
project produces the compiled shader bytecode and copies it to the
`SovereignClient` project.

### SovereignClient

`SovereignClient` is an executable wrapper around ClientCore that configures the
engine to run as a client.

### SovereignClientSetup

`SovereignClientSetup` is a Wix project for creating a Windows installer for
the client.

Server Projects
---------------

### Persistence

`Persistence` contains the persistence system used for interacting with the
database.

### ServerCore

`ServerCore` is the main class library for the server-specific code.

### SovereignServer

`SovereignServer` is an executable wrapper around `ServerCore` that configures the
engine to run as a server.

### StandaloneWorldGen

`StandaloneWorldGen` is a command line wrapper around `WorldGen` to allow for
offline world generation for development purposes.

### WorldGen

`WorldGen` is a utility library for procedurally generating game worlds.

Common Projects
---------------

### EngineCore

`EngineCore` is the main common class library that contains the bulk of the
game logic.

### EngineUtil

`EngineUtil` provides reusable software components such as collections that
are used throughout the other projects.

### WorldLib

`WorldLib` defines a data model for describing a game world. Refer to
`world_structure.md` for further information.

