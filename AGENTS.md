# AGENTS.md

Guidance for AI coding agents working on Sovereign Engine.

## Project Overview

Sovereign Engine is an open source 2.5D voxel-based online RPG engine written in C#
(.NET 9 or later), running on Windows and Linux. It combines a 2D graphical style with
a 3D voxel-based world and includes a client, a server, an integrated editor, and a
Lua 5.4 server-side scripting engine. Licensed under GPLv3.

Key resources:

- `README.md` — features, installation, controls, chat commands.
- `docs/manual/` — Sphinx documentation (creators' and developers' manuals). The
  developers' manual covers the ECS, systems, networking, physics, and debugging.
- `docs/dev/dev_troubleshooting.md` — development troubleshooting notes.
- `CHANGELOG.md` — release history.

## Repository Layout

- `src/Sovereign.sln` — the main solution.
- `src/Common/` — code shared by client and server: `EngineCore` (ECS core,
  distributed event loop), `EngineUtil`, `NetworkCore`, `WorldGen`,
  `WorldManagement`, `Scripting`, `Performance`, `UpdaterCore`.
- `src/Server/` — server-side systems: `ServerCore`, `Accounts`, `Persistence`,
  `ServerNetwork`, `SovereignServer` (the executable; `Data/` contains packages,
  scripts, and worldgen data).
- `src/Client/` — client-side systems: `ClientCore`, `VeldridRenderer`,
  `SovereignClient` (the executable).
- `src/Test/` — unit tests (`TestEngineCore`, `TestServerCore`, etc.) and
  `IntegrationTests`.
- `src/Util/` — helper tools and benchmarks.

## Architecture

- Entity-Component-System (ECS) with asynchronous interactions between systems
  mediated by a distributed event loop. Systems subscribe to event IDs and can run
  in parallel.
- Entities are identified by 64-bit entity IDs. Persisted entities start at
  0x7FFF000000000000; template entities (flywheel pattern) at 0x7FFE000000000000;
  block entities at 0x6FFF000000000000; other volatile entities are smaller.
- Components are stored in columnar collections deriving from
  `BaseComponentCollection<T>`.
- Systems communicate only through a Controller class (read-write access,
  typically asynchronous via `IEventSender`) or a Services class (read-only,
  synchronous). Everything else in a system is private implementation detail.
  Controllers using `IEventSender` take it as the first parameter of public methods.
- When creating a new component type, use the `add-component` skill.

## Building and Testing

- Build with the **Debug** configuration. Always perform a full rebuild of the
  entire solution (`dotnet build` on `src/Sovereign.sln`) after making changes and
  verify that it compiles.
- If any file under `src/Common/` or `src/Server/` changes, run the server with the
  `run-server` skill after a successful full rebuild to verify it still runs
  correctly (Debug configuration).
- If files in `docs/manual/` change, test the documentation build with the Python
  venv: `source ~/venv/sphinx/bin/activate`.

## Conventions

- Add docxml comments to all public members and private methods; one-sentence
  summaries. No docxml comments on private fields/constants; no remarks sections
  except where truly necessary for public API understanding.
- All non-binary files must end with a newline.
- When testing bitwise flags in C#, prefer `&` over `HasFlag` for performance.
