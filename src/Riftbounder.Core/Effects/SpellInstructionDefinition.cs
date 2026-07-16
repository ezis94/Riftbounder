namespace Riftbounder.Core.Effects;

public sealed record SpellInstructionDefinition
{
    public SpellInstructionDefinition(
        string id,
        IReadOnlyList<int> targetIndexes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(targetIndexes);

        if (targetIndexes.Any(index => index < 0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetIndexes),
                "Target indexes cannot be negative.");
        }

        Id = id;
        TargetIndexes = targetIndexes.ToArray();
    }

    public string Id { get; }

    public IReadOnlyList<int> TargetIndexes { get; }
}
