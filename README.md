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

## Sprint 2

Sprint 2 adds the first rules-mapped turn controller and typed event journal.

```powershell
dotnet build Riftbounder.slnx --configuration Release
dotnet test Riftbounder.slnx --configuration Release
```

See [`SPRINT-2.md`](SPRINT-2.md) and
[`docs/rules-coverage.md`](docs/rules-coverage.md).

## Sprint 3

Sprint 3 adds Rune Decks, runes on the board, ready/exhausted state,
Awaken resolution, and actual Channel resolution.

See [`SPRINT-3.md`](SPRINT-3.md).

## Sprint 4

Rune Pools, resource payment, and Basic Rune abilities. See `SPRINT-4.md`.

## AG-005

AG-005 adds the generic transactional Chain and the first GitHub Actions
workflow. See [`AG-005.md`](AG-005.md).
