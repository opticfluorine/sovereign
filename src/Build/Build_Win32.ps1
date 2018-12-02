# Automatic build script for Windows platforms
# Copyright (c) 2018 opticfluorine
# See top-level LICENSE file for license information.

# This script should be run from the src directory.
# Both nuget and msbuild must be on the PATH.

# Restore dependencies from NuGet.
# nuget restore

# Build all projects.
msbuild 'Sovereign Engine.sln' /restore