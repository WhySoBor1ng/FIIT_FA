using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
{
    /// <summary>
    /// Разрезает дерево с корнем <paramref name="root"/> на два поддерева:
    /// Left: все ключи <= <paramref name="key"/>
    /// Right: все ключи > <paramref name="key"/>
    /// </summary>
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) Split(TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null)
        {
            return (null, null);
        }

        if (Comparer.Compare(root.Key, key) <= 0)
        {
            (TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right) = Split(root.Right, key);
            root.Right = left;
            if (root.Right != null)
            {
                root.Right.Parent = root;
            }

            if (right != null)
            {
                right.Parent = null;
            }

            return (root, right);
        }

        (TreapNode<TKey, TValue>? leftTree, TreapNode<TKey, TValue>? rightTree) = Split(root.Left, key);
        root.Left = rightTree;
        if (root.Left != null)
        {
            root.Left.Parent = root;
        }

        if (leftTree != null)
        {
            leftTree.Parent = null;
        }

        return (leftTree, root);
    }

    /// <summary>
    /// Сливает два дерева в одно.
    /// Важное условие: все ключи в <paramref name="left"/> должны быть меньше ключей в <paramref name="right"/>.
    /// Слияние происходит на основе Priority (куча).
    /// </summary>
    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left == null)
        {
            if (right != null)
            {
                right.Parent = null;
            }
            return right;
        }

        if (right == null)
        {
            left.Parent = null;
            return left;
        }

        if (left.Priority >= right.Priority)
        {
            left.Right = Merge(left.Right, right);
            if (left.Right != null)
            {
                left.Right.Parent = left;
            }

            left.Parent = null;
            return left;
        }

        right.Left = Merge(left, right.Left);
        if (right.Left != null)
        {
            right.Left.Parent = right;
        }

        right.Parent = null;
        return right;
    }
    

    public override void Add(TKey key, TValue value)
    {
        TreapNode<TKey, TValue>? existing = FindNode(key);
        if (existing != null)
        {
            existing.Value = value;
            return;
        }

        TreapNode<TKey, TValue> newNode = CreateNode(key, value);
        (TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right) = Split(Root, key);
        Root = Merge(Merge(left, newNode), right);
        Count++;
    }

    public override bool Remove(TKey key)
    {
        bool removed = false;
        Root = Remove(Root, key, ref removed);
        if (Root != null)
        {
            Root.Parent = null;
        }

        if (removed)
        {
            Count--;
        }

        return removed;
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new TreapNode<TKey, TValue>(key, value);
    }
    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode)
    {
        return;
    }
    
    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child)
    {
        return;
    }

    private TreapNode<TKey, TValue>? Remove(TreapNode<TKey, TValue>? node, TKey key, ref bool removed)
    {
        if (node == null)
        {
            return null;
        }

        int cmp = Comparer.Compare(key, node.Key);
        if (cmp < 0)
        {
            node.Left = Remove(node.Left, key, ref removed);
            if (node.Left != null)
            {
                node.Left.Parent = node;
            }

            return node;
        }

        if (cmp > 0)
        {
            node.Right = Remove(node.Right, key, ref removed);
            if (node.Right != null)
            {
                node.Right.Parent = node;
            }

            return node;
        }

        removed = true;
        TreapNode<TKey, TValue>? merged = Merge(node.Left, node.Right);
        if (merged != null)
        {
            merged.Parent = node.Parent;
        }

        return merged;
    }
    
}
