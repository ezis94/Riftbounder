
using Riftbounder.Core.Resources;
using Riftbounder.Core.Runes;
namespace Riftbounder.Engine.Results;

public sealed record RuneAbilityResult(Rune Rune, int EnergyAdded, PowerType? PowerAdded);
