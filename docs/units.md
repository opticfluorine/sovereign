# Unit Conventions

This document specifies the conventions for units used internally by
Sovereign Engine.

## Base Units

Type | Definition
--- | ---
Position | Grid units; 1.0 = one block width
Velocity | Grid units per second
System time | Microseconds (us)

## Derived Units

Type | Definition | Definition Location
--- | --- | ---
Game tick | 10 milliseconds (ms) | `IEngineConfiguration` implementations
Time since tick | Seconds since start of the current tick | N/A
Tile width | 32 pixels | `IClientConfiguration` implementations

