using Riftbounder.Core.Cards;

namespace Riftbounder.Engine.Results;

public readonly record struct DrawResult(bool Succeeded, Card? Card)
{
    public static DrawResult EmptyDeck => new(false, null);

    public static DrawResult Success(Card card) => new(true, card);
}
