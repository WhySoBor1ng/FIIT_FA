using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    public override void Add(TKey key, TValue value)
    {
        bool added = false;
        Root = Insert(Root, key, value, null, ref added);
        if (Root != null)
        {
            Root.Parent = null;
        }

        if (added)
        {
            Count++;
        }
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

    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        return;
    }

    private AvlNode<TKey, TValue> Insert(AvlNode<TKey, TValue>? node, TKey key, TValue value, AvlNode<TKey, TValue>? parent, ref bool added)
    {
        if (node == null)
        {
            added = true;
            AvlNode<TKey, TValue> newNode = CreateNode(key, value);
            newNode.Parent = parent;
            return newNode;
        }

        int cmp = Comparer.Compare(key, node.Key);
        if (cmp < 0)
        {
            node.Left = Insert(node.Left, key, value, node, ref added);
        }
        else if (cmp > 0)
        {
            node.Right = Insert(node.Right, key, value, node, ref added);
        }
        else
        {
            node.Value = value;
            return node;
        }

        UpdateHeight(node);
        return Balance(node);
    }

    private AvlNode<TKey, TValue>? Remove(AvlNode<TKey, TValue>? node, TKey key, ref bool removed)
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
        }
        else if (cmp > 0)
        {
            node.Right = Remove(node.Right, key, ref removed);
            if (node.Right != null)
            {
                node.Right.Parent = node;
            }
        }
        else
        {
            removed = true;
            if (node.Left == null)
            {
                if (node.Right != null)
                {
                    node.Right.Parent = node.Parent;
                }

                return node.Right;
            }

            if (node.Right == null)
            {
                node.Left.Parent = node.Parent;
                return node.Left;
            }

            AvlNode<TKey, TValue> successor = FindMin(node.Right);
            node.Key = successor.Key;
            node.Value = successor.Value;
            node.Right = Remove(node.Right, successor.Key, ref removed);
            if (node.Right != null)
            {
                node.Right.Parent = node;
            }
        }

        UpdateHeight(node);
        return Balance(node);
    }

    private AvlNode<TKey, TValue> Balance(AvlNode<TKey, TValue> node)
    {
        int balanceFactor = GetHeight(node.Left) - GetHeight(node.Right);
        if (balanceFactor > 1)
        {
            if (GetHeight(node.Left!.Left) < GetHeight(node.Left.Right))
            {
                node.Left = RotateLeftLocal(node.Left);
                if (node.Left != null)
                {
                    node.Left.Parent = node;
                }
            }

            return RotateRightLocal(node);
        }

        if (balanceFactor < -1)
        {
            if (GetHeight(node.Right!.Right) < GetHeight(node.Right.Left))
            {
                node.Right = RotateRightLocal(node.Right);
                if (node.Right != null)
                {
                    node.Right.Parent = node;
                }
            }

            return RotateLeftLocal(node);
        }

        return node;
    }

    private static int GetHeight(AvlNode<TKey, TValue>? node) => node?.Height ?? 0;

    private static void UpdateHeight(AvlNode<TKey, TValue> node)
        => node.Height = Math.Max(GetHeight(node.Left), GetHeight(node.Right)) + 1;

    private static AvlNode<TKey, TValue> FindMin(AvlNode<TKey, TValue> node)
    {
        AvlNode<TKey, TValue> current = node;
        while (current.Left != null)
        {
            current = current.Left;
        }

        return current;
    }

    private AvlNode<TKey, TValue> RotateLeftLocal(AvlNode<TKey, TValue> node)
    {
        AvlNode<TKey, TValue> pivot = node.Right!;
        node.Right = pivot.Left;
        if (node.Right != null)
        {
            node.Right.Parent = node;
        }

        pivot.Left = node;
        pivot.Parent = node.Parent;
        node.Parent = pivot;

        UpdateHeight(node);
        UpdateHeight(pivot);
        return pivot;
    }

    private AvlNode<TKey, TValue> RotateRightLocal(AvlNode<TKey, TValue> node)
    {
        AvlNode<TKey, TValue> pivot = node.Left!;
        node.Left = pivot.Right;
        if (node.Left != null)
        {
            node.Left.Parent = node;
        }

        pivot.Right = node;
        pivot.Parent = node.Parent;
        node.Parent = pivot;

        UpdateHeight(node);
        UpdateHeight(pivot);
        return pivot;
    }

    
}
