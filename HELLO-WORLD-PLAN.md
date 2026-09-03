# Plan: Simple Hello World Application

## Summary

Add a minimal .NET console application, `HelloWorld`, that prints `Hello, World!` to standard
output and exits. The project lives under `src/Util/HelloWorld` alongside the other utility
projects and is added to `Sovereign.sln`. It has no dependencies on other projects in the
solution and no external package references.

## Project File

**File:** `src/Util/HelloWorld/HelloWorld.csproj` (new)
- SDK-style project with `OutputType=Exe`
- `TargetFramework` = `net10.0` (matches the rest of the solution)
- `ImplicitUsings` = `enable`, `Nullable` = `enable`
- `AssemblyName` = `Sovereign.HelloWorld`, `RootNamespace` = `Sovereign.HelloWorld`
  (matches the `Sovereign.EcsBenchmark` naming convention)
- No `ProjectReference` or `PackageReference` entries

## Program

**File:** `src/Util/HelloWorld/Program.cs` (new)
- A single top-level statement: `Console.WriteLine("Hello, World!");`

## Solution

**File:** `src/Sovereign.sln`
- Register the new project with
  `dotnet sln src/Sovereign.sln add src/Util/HelloWorld/HelloWorld.csproj`

## Build and Test

1. Full rebuild of the entire solution with `dotnet build` in Debug configuration.
2. Run `dotnet run --project src/Util/HelloWorld` and verify the output is exactly
   `Hello, World!`.
