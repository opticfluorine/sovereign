# Plan: Simple Hello World Application

## Summary

Add a minimal C# console application that prints `Hello, World!` to standard output. The application is a standalone utility project under `src/Util` with no dependencies on the engine, following the existing conventions of the utility projects in that directory (e.g., `CreateUpdaterResourceSet`).

## Project File

**File:** `src/Util/HelloWorld/HelloWorld.csproj` (new)
- SDK-style project, same pattern as `src/Util/CreateUpdaterResourceSet/CreateUpdaterResourceSet.csproj`
- `OutputType`: `Exe`
- `TargetFramework`: `net10.0`
- `ImplicitUsings`: `enable`
- `Nullable`: `enable`
- `TreatWarningsAsErrors`: `true`
- No `ProjectReference` or `PackageReference` entries (the application has no dependencies)

## Program Source

**File:** `src/Util/HelloWorld/Program.cs` (new)
- Standard GPLv3 license header comment, matching other source files in the repository (e.g., `src/Util/CreateUpdaterResourceSet/Program.cs`)
- Top-level statements: `Console.WriteLine("Hello, World!");`

## Solution Registration

**File:** `src/Sovereign.sln`
- Add the new project to the solution under the existing `Util` solution folder:
  ```
  dotnet sln src/Sovereign.sln add src/Util/HelloWorld/HelloWorld.csproj --solution-folder Util
  ```

## Build and Test

1. Full rebuild of the entire solution in the Debug configuration: `dotnet build -c Debug`
2. Run the application and verify the output: `dotnet run --project src/Util/HelloWorld` must print `Hello, World!`
