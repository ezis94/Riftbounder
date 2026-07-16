namespace Riftbounder.Engine.Actions;

public enum PlayCardFailure
{
    None,
    PlayerNotPermitted,
    CardNotInHand,
    CardNotOwnedByPlayer,
    UnsupportedCardType,
    InvalidPayment
}
