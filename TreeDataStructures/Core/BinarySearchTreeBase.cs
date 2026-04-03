using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null) 
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default; // use it to compare Keys

    public int Count { get; protected set; }
    
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys => InOrder().Select(entry => entry.Key).ToList();
    public ICollection<TValue> Values => InOrder().Select(entry => entry.Value).ToList();
    
    
    public virtual void Add(TKey key, TValue value)
    {
        if (Root == null)
        {
            Root = CreateNode(key, value);
            Count = 1;
            OnNodeAdded(Root);
            return;
        }

        TNode? current = Root;
        TNode? parent = null;
        while (current != null)
        {
            parent = current;
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0)
            {
                current.Value = value;
                return;
            }

            current = cmp < 0 ? current.Left : current.Right;
        }

        TNode newNode = CreateNode(key, value);
        newNode.Parent = parent;
        if (Comparer.Compare(key, parent!.Key) < 0)
        {
            parent.Left = newNode;
        }
        else
        {
            parent.Right = newNode;
        }

        Count++;
        OnNodeAdded(newNode);
    }

    
    public virtual bool Remove(TKey key)
    {
        TNode? node = FindNode(key);
        if (node == null) { return false; }

        RemoveNode(node);
        this.Count--;
        return true;
    }
    
    
    protected virtual void RemoveNode(TNode node)
    {
        if (node.Left == null)
        {
            TNode? replacement = node.Right;
            TNode? parent = node.Parent;
            Transplant(node, replacement);
            OnNodeRemoved(parent, replacement);
            return;
        }

        if (node.Right == null)
        {
            TNode? replacement = node.Left;
            TNode? parent = node.Parent;
            Transplant(node, replacement);
            OnNodeRemoved(parent, replacement);
            return;
        }

        TNode successor = Minimum(node.Right);
        if (!ReferenceEquals(successor.Parent, node))
        {
            TNode? fixupParent = successor.Parent;
            TNode? fixupChild = successor.Right;

            Transplant(successor, successor.Right);
            successor.Right = node.Right;
            if (successor.Right != null)
            {
                successor.Right.Parent = successor;
            }

            Transplant(node, successor);
            successor.Left = node.Left;
            if (successor.Left != null)
            {
                successor.Left.Parent = successor;
            }

            OnNodeRemoved(fixupParent, fixupChild);
            return;
        }

        TNode? child = successor.Right;
        Transplant(node, successor);
        successor.Left = node.Left;
        if (successor.Left != null)
        {
            successor.Left.Parent = successor;
        }

        OnNodeRemoved(successor, child);
    }

    public virtual bool ContainsKey(TKey key) => FindNode(key) != null;
    
    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        TNode? node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out TValue? val) ? val : throw new KeyNotFoundException();
        set => Add(key, value);
    }

    
    #region Hooks
    
    /// <summary>
    /// Вызывается после успешной вставки
    /// </summary>
    /// <param name="newNode">Узел, который встал на место</param>
    protected virtual void OnNodeAdded(TNode newNode) { }
    
    /// <summary>
    /// Вызывается после удаления. 
    /// </summary>
    /// <param name="parent">Узел, чей ребенок изменился</param>
    /// <param name="child">Узел, который встал на место удаленного</param>
    protected virtual void OnNodeRemoved(TNode? parent, TNode? child) { }
    
    #endregion
    
    
    #region Helpers
    protected abstract TNode CreateNode(TKey key, TValue value);
    
    
    protected TNode? FindNode(TKey key)
    {
        TNode? current = Root;
        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) { return current; }
            current = cmp < 0 ? current.Left : current.Right;
        }
        return null;
    }

    protected TNode Minimum(TNode node)
    {
        TNode current = node;
        while (current.Left != null)
        {
            current = current.Left;
        }

        return current;
    }

    protected void RotateLeft(TNode x)
    {
        TNode? y = x.Right;
        if (y == null) { return; }

        x.Right = y.Left;
        if (y.Left != null)
        {
            y.Left.Parent = x;
        }

        y.Parent = x.Parent;
        if (x.Parent == null)
        {
            Root = y;
        }
        else if (x.IsLeftChild)
        {
            x.Parent.Left = y;
        }
        else
        {
            x.Parent.Right = y;
        }

        y.Left = x;
        x.Parent = y;
    }

    protected void RotateRight(TNode y)
    {
        TNode? x = y.Left;
        if (x == null) { return; }

        y.Left = x.Right;
        if (x.Right != null)
        {
            x.Right.Parent = y;
        }

        x.Parent = y.Parent;
        if (y.Parent == null)
        {
            Root = x;
        }
        else if (y.IsLeftChild)
        {
            y.Parent.Left = x;
        }
        else
        {
            y.Parent.Right = x;
        }

        x.Right = y;
        y.Parent = x;
    }
    
    protected void RotateBigLeft(TNode x)
    {
        RotateLeft(x);
        if (x.Parent != null)
        {
            RotateLeft(x.Parent);
        }
    }
    
    protected void RotateBigRight(TNode y)
    {
        RotateRight(y);
        if (y.Parent != null)
        {
            RotateRight(y.Parent);
        }
    }
    
    protected void RotateDoubleLeft(TNode x)
    {
        if (x.Right != null)
        {
            RotateRight(x.Right);
        }
        RotateLeft(x);
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        if (y.Left != null)
        {
            RotateLeft(y.Left);
        }
        RotateRight(y);
    }
    
    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
        {
            Root = v;
        }
        else if (u.IsLeftChild)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }
        v?.Parent = u.Parent;
    }
    #endregion
    
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrder() => new TreeIterator(Root, TraversalStrategy.InOrder);
    
    private IEnumerable<TreeEntry<TKey, TValue>>  InOrderTraversal(TNode? node)
    {
        return new TreeIterator(node, TraversalStrategy.InOrder);
    }
    
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrder() => new TreeIterator(Root, TraversalStrategy.PreOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrder() => new TreeIterator(Root, TraversalStrategy.PostOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrderReverse() => new TreeIterator(Root, TraversalStrategy.InOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrderReverse() => new TreeIterator(Root, TraversalStrategy.PreOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrderReverse() => new TreeIterator(Root, TraversalStrategy.PostOrderReverse);
    
    /// <summary>
    /// Внутренний класс-итератор. 
    /// Реализует паттерн Iterator вручную, без yield return (ban).
    /// </summary>
    private struct TreeIterator : 
        IEnumerable<TreeEntry<TKey, TValue>>,
        IEnumerator<TreeEntry<TKey, TValue>>
    {
        private readonly TNode? _root;
        private readonly TraversalStrategy _strategy; // or make it template parameter?
        private readonly List<TreeEntry<TKey, TValue>> _entries;
        private int _index;
        
        public TreeIterator(TNode? root, TraversalStrategy strategy)
        {
            _root = root;
            _strategy = strategy;
            _entries = BuildEntries(root, strategy);
            _index = -1;
        }
        
        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => new TreeIterator(_root, _strategy);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        public TreeEntry<TKey, TValue> Current
        {
            get
            {
                if (_index < 0 || _index >= _entries.Count)
                {
                    throw new InvalidOperationException("Iterator is not positioned on an element.");
                }

                return _entries[_index];
            }
        }
        object IEnumerator.Current => Current;
        
        
        public bool MoveNext()
        {
            if (_index + 1 >= _entries.Count)
            {
                return false;
            }

            _index++;
            return true;
        }
        
        public void Reset()
        {
            _index = -1;
        }

        
        public void Dispose()
        {
            // TODO release managed resources here
        }

        private static List<TreeEntry<TKey, TValue>> BuildEntries(TNode? root, TraversalStrategy strategy)
        {
            TraversalStrategy baseStrategy = strategy switch
            {
                TraversalStrategy.InOrderReverse => TraversalStrategy.InOrder,
                TraversalStrategy.PreOrderReverse => TraversalStrategy.PreOrder,
                TraversalStrategy.PostOrderReverse => TraversalStrategy.PostOrder,
                _ => strategy
            };

            TraversalResult result = Build(root, baseStrategy);
            if (strategy is TraversalStrategy.InOrderReverse or TraversalStrategy.PreOrderReverse or TraversalStrategy.PostOrderReverse)
            {
                result.Entries.Reverse();
            }

            return result.Entries;
        }

        private static TraversalResult Build(TNode? node, TraversalStrategy strategy)
        {
            if (node == null)
            {
                return new TraversalResult([], 0);
            }

            TraversalResult left = Build(node.Left, strategy);
            TraversalResult right = Build(node.Right, strategy);
            int height = Math.Max(left.Height, right.Height) + 1;
            TreeEntry<TKey, TValue> entry = new(node.Key, node.Value, height);
            List<TreeEntry<TKey, TValue>> entries = [];

            if (strategy == TraversalStrategy.PreOrder)
            {
                entries.Add(entry);
                entries.AddRange(left.Entries);
                entries.AddRange(right.Entries);
            }
            else if (strategy == TraversalStrategy.PostOrder)
            {
                entries.AddRange(left.Entries);
                entries.AddRange(right.Entries);
                entries.Add(entry);
            }
            else
            {
                entries.AddRange(left.Entries);
                entries.Add(entry);
                entries.AddRange(right.Entries);
            }

            return new TraversalResult(entries, height);
        }

        private readonly record struct TraversalResult(List<TreeEntry<TKey, TValue>> Entries, int Height);
    }
    
    
    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return InOrder()
            .Select(entry => new KeyValuePair<TKey, TValue>(entry.Key, entry.Value))
            .ToList()
            .GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item)
        => TryGetValue(item.Key, out TValue? value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        if (arrayIndex < 0 || arrayIndex > array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }

        if (array.Length - arrayIndex < Count)
        {
            throw new ArgumentException("Destination array does not have enough space.");
        }

        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            array[arrayIndex++] = pair;
        }
    }
    public bool Remove(KeyValuePair<TKey, TValue> item)
        => Contains(item) && Remove(item.Key);
}
