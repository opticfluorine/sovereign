# Glossary

This document summarizes the definitions of some common technical terms used throughout
Sovereign Engine and its documentation.

| Term             | Definition                                                                                                                                   |
|------------------|----------------------------------------------------------------------------------------------------------------------------------------------|
| account          | Registered user that can be logged into; owns zero or more player characters.                                                                |
| block            | Unit cube of a material; the world is built from a grid of blocks.                                                                           |
| component        | Piece of data that may be associated with an entity.                                                                                         |
| entity           | Single object that exists in the game world.                                                                                                 |
| entity, active   | Entity stored in memory and actively processed.                                                                                              |
| entity, inactive | Entity stored on disk but not in memory.                                                                                                     |
| entity, local    | Entity which only exists locally and is not synchronized over the network.                                                                   |
| event            | Message passed between systems.                                                                                                              |
| event server     | UDP server that sends and receives events via the network.                                                                                   |
| player           | Shortened term for *player character*.                                                                                                       |
| player character | Entity corresponding to a character controlled by a player.                                                                                  |
| tick             | Unit of time corresponding to one update to the game state.                                                                                  |
| system           | Small software service responsible for a specific part of game logic.                                                                        |
| update radius    | Radius from center of a world segment within which entities are synchronized between server and clients, specified in world segment lengths. |
| world segment    | Cubic region of the world, 32 blocks in length.                                                                                              |