# Sprint 1 Hotfix 3

## Fix

Renamed the namespace containing `Game` and `Player`:

- Before: `Riftbounder.Engine.Game`
- After: `Riftbounder.Engine.Games`

The singular namespace name collided with the `Game` type in `GameTests.cs`, causing CS0118.

## Files changed

- `src/Riftbounder.Engine/Game/Game.cs`
- `src/Riftbounder.Engine/Game/Player.cs`
- `tests/Riftbounder.Engine.Tests/GameTests.cs`

## Validation

Run:

```powershell
dotnet clean Riftbounder.slnx
dotnet build Riftbounder.slnx --configuration Release
dotnet test Riftbounder.slnx --configuration Release
```
