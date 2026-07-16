using Riftbounder.Engine.Cards;

namespace Riftbounder.Engine.Actions;

public sealed record PlayCardResult
{
    private PlayCardResult(
        PlayCardChainItem? chainItem,
        PlayCardFailure failure)
    {
        ChainItem = chainItem;
        Failure = failure;
    }

    public PlayCardChainItem? ChainItem { get; }

    public PlayCardFailure Failure { get; }

    public bool Succeeded => Failure is PlayCardFailure.None;

    public static PlayCardResult Success(
        PlayCardChainItem chainItem) =>
        new(chainItem, PlayCardFailure.None);

    public static PlayCardResult Failed(
        PlayCardFailure failure)
    {
        if (failure is PlayCardFailure.None)
        {
            throw new ArgumentException(
                "A failed result requires a failure reason.",
                nameof(failure));
        }

        return new PlayCardResult(null, failure);
    }
}
