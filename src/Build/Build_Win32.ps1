# Automatic build script for Windows platforms
# Copyright (c) 2018 opticfluorine
# See top-level LICENSE file for license information.

# This script should be run from the src directory.
# msbuild must be on the PATH.

# Build all projects.
msbuild 'Sovereign Engine.sln' /restore