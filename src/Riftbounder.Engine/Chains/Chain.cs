namespace Riftbounder.Engine.Chains;

public sealed class Chain
{
    private readonly List<IChainItem> _items = [];

    public int Count => _items.Count;

    public bool IsEmpty => _items.Count == 0;

    public IReadOnlyList<IChainItem> Items => _items;

    public void Push(IChainItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (_items.Any(existing => existing.Id == item.Id))
        {
            throw new InvalidOperationException(
                $"Chain item {item.Id} is already on the Chain.");
        }

        _items.Add(item);
    }

    public IChainItem Peek()
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException("The Chain is empty.");
        }

        return _items[^1];
    }

    public IChainItem Pop()
    {
        IChainItem item = Peek();
        _items.RemoveAt(_items.Count - 1);
        return item;
    }

    public void Clear() => _items.Clear();
}
