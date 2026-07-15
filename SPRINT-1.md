# Sprint 1 delivery

## Suggested commit

`AG-001: bootstrap typed zones and card transfers`

## Added

- .NET 10 solution and shared build settings.
- `Riftbounder.Core` with typed identifiers, cards, zone kinds and zones.
- `Riftbounder.Engine` with players, game registration, transfers and drawing.
- 11 xUnit tests covering the first invariants.
- Architecture documentation and repository setup.

## Validation status

The files were statically reviewed in the generation environment. The .NET SDK is not installed in that environment, so `dotnet restore`, `dotnet build`, and `dotnet test` could not be executed here.

Run locally:

```bash
dotnet restore Riftbounder.slnx
dotnet build Riftbounder.slnx --configuration Release
dotnet test Riftbounder.slnx --configuration Release
```

## Acceptance criteria

- All projects restore and build with zero warnings.
- All 11 tests pass.
- A card cannot be registered in two zones.
- Drawing moves the top card from Main Deck to Hand.
- Drawing from an empty deck is explicit and non-mutating.
