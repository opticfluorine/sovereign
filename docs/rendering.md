# Rendering Process

This document describes the strategy used by the Sovereign Client for
rendering.

## Rendering Architecture

To avoid strong coupling to a specific graphics API, the Sovereign Client uses
a modular rendering architecture consisting of two components, the high-level
rendering coordinator and the low-level renderer.

The rendering coordinator handles high-level details common to all renderers
such as producing the required data buffers. The renderer itself handles
communication with the underlying graphics API. The renderer is intended to
be interchangeable to provide portability. 

Initially the renderer was implemented using Direct3D 11.
With the deprecation of the SharpDX library, the Direct3D 11 renderewr has been
replaced by a cross-platform renderer backed by the Veldrid library. Veldrid is
able to render using Direct3D, OpenGL, OpenGL ES, and Vulkan backends. This 
provides rendering support for Windows clients while also enabling
native Linux clients (and potentially Mac OS X clients via the Metal backend,
though official Mac OS X support is not planned). Currently the Veldrid-based
renderer only supports the Vulkan backend.

## Rendering Coordinator

The behavior of the rendering coordinator is dependent on the state of the client.
This allows transitions between different scenes, e.g. startup splash screens,
the main menu, and the in-game mode.

### Main Menu

TODO

### In-Game Mode

### Game World

The rendering coordinator for the game world reduces the game state to a series
of single-layer rendering passes ordered from low to high z. The ith pass renders
all sprites with z in the interval [i, i + 1) along with the front face of any
blocks at z = i + 1. Postprocessing (e.g. lighting shaders) is done after every
rendering pass.

### UI Overlay

TODO

## Renderer

TODO

