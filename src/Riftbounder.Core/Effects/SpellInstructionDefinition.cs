namespace Riftbounder.Core.Effects;

public sealed record SpellInstructionDefinition
{
    public SpellInstructionDefinition(
        string id,
        IReadOnlyList<int> targetIndexes,
        int amount = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(targetIndexes);

        if (targetIndexes.Any(index => index < 0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetIndexes),
                "Target indexes cannot be negative.");
        }

        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Instruction amount cannot be negative.");
        }

        Id = id;
        TargetIndexes = targetIndexes.ToArray();
        Amount = amount;
    }

    public string Id { get; }

    public IReadOnlyList<int> TargetIndexes { get; }

    public int Amount { get; }
}
