# Riftbounder

A rules-accurate, open-source Riftbound engine and analysis platform.

## Sprint 1 scope

This bootstrap establishes the first domain primitives:

- strongly typed card and player identifiers;
- cards and owned zones;
- players with main deck and hand zones;
- card transfer and draw operations;
- invariant-focused unit tests.

No Riftbound-specific card mechanics are implemented yet.

## Requirements

- .NET SDK 10.0.302 or a compatible .NET 10 feature band.

## Build and test

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

## Repository layout

```text
src/
  Riftbounder.Core/      Domain primitives with no engine behavior
  Riftbounder.Engine/    Game operations and state transitions
tests/
  Riftbounder.Core.Tests/
  Riftbounder.Engine.Tests/
docs/
  architecture.md
```
