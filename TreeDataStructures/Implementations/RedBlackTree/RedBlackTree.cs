using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
{
    public override void Add(TKey key, TValue value)
    {
        if (Root == null)
        {
            Root = CreateNode(key, value);
            Root.Color = RbColor.Black;
            Count = 1;
            return;
        }

        RbNode<TKey, TValue>? current = Root;
        RbNode<TKey, TValue>? parent = null;
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

        RbNode<TKey, TValue> newNode = CreateNode(key, value);
        newNode.Parent = parent;
        if (Comparer.Compare(key, parent!.Key) < 0)
        {
            parent.Left = newNode;
        }
        else
        {
            parent.Right = newNode;
        }

        FixInsert(newNode);
        Root!.Color = RbColor.Black;
        Count++;
    }

    public override bool Remove(TKey key)
    {
        RbNode<TKey, TValue>? node = FindNode(key);
        if (node == null)
        {
            return false;
        }

        RbNode<TKey, TValue> y = node;
        RbColor originalColor = y.Color;
        RbNode<TKey, TValue>? x;
        RbNode<TKey, TValue>? xParent;

        if (node.Left == null)
        {
            x = node.Right;
            xParent = node.Parent;
            Transplant(node, node.Right);
        }
        else if (node.Right == null)
        {
            x = node.Left;
            xParent = node.Parent;
            Transplant(node, node.Left);
        }
        else
        {
            y = Minimum(node.Right);
            originalColor = y.Color;
            x = y.Right;
            if (ReferenceEquals(y.Parent, node))
            {
                xParent = y;
                if (x != null)
                {
                    x.Parent = y;
                }
            }
            else
            {
                xParent = y.Parent;
                Transplant(y, y.Right);
                y.Right = node.Right;
                if (y.Right != null)
                {
                    y.Right.Parent = y;
                }
            }

            Transplant(node, y);
            y.Left = node.Left;
            if (y.Left != null)
            {
                y.Left.Parent = y;
            }

            y.Color = node.Color;
        }

        if (originalColor == RbColor.Black)
        {
            FixDelete(x, xParent);
        }

        if (Root != null)
        {
            Root.Color = RbColor.Black;
        }

        Count--;
        return true;
    }

    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new RbNode<TKey, TValue>(key, value);
    }
    
    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        if (Root != null)
        {
            Root.Color = RbColor.Black;
        }
    }
    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        if (Root != null)
        {
            Root.Color = RbColor.Black;
        }
    }

    private void FixInsert(RbNode<TKey, TValue> node)
    {
        while (node.Parent?.Color == RbColor.Red)
        {
            RbNode<TKey, TValue> parent = node.Parent;
            RbNode<TKey, TValue> grandParent = parent.Parent!;

            if (parent.IsLeftChild)
            {
                RbNode<TKey, TValue>? uncle = grandParent.Right;
                if (ColorOf(uncle) == RbColor.Red)
                {
                    parent.Color = RbColor.Black;
                    uncle!.Color = RbColor.Black;
                    grandParent.Color = RbColor.Red;
                    node = grandParent;
                    continue;
                }

                if (node.IsRightChild)
                {
                    node = parent;
                    RotateLeft(node);
                    parent = node.Parent!;
                    grandParent = parent.Parent!;
                }

                parent.Color = RbColor.Black;
                grandParent.Color = RbColor.Red;
                RotateRight(grandParent);
            }
            else
            {
                RbNode<TKey, TValue>? uncle = grandParent.Left;
                if (ColorOf(uncle) == RbColor.Red)
                {
                    parent.Color = RbColor.Black;
                    uncle!.Color = RbColor.Black;
                    grandParent.Color = RbColor.Red;
                    node = grandParent;
                    continue;
                }

                if (node.IsLeftChild)
                {
                    node = parent;
                    RotateRight(node);
                    parent = node.Parent!;
                    grandParent = parent.Parent!;
                }

                parent.Color = RbColor.Black;
                grandParent.Color = RbColor.Red;
                RotateLeft(grandParent);
            }
        }
    }

    private void FixDelete(RbNode<TKey, TValue>? node, RbNode<TKey, TValue>? parent)
    {
        while (!ReferenceEquals(node, Root) && ColorOf(node) == RbColor.Black)
        {
            if (ReferenceEquals(node, parent?.Left))
            {
                RbNode<TKey, TValue>? sibling = parent.Right;
                if (ColorOf(sibling) == RbColor.Red)
                {
                    sibling!.Color = RbColor.Black;
                    parent.Color = RbColor.Red;
                    RotateLeft(parent);
                    sibling = parent.Right;
                }

                if (ColorOf(sibling?.Left) == RbColor.Black && ColorOf(sibling?.Right) == RbColor.Black)
                {
                    if (sibling != null)
                    {
                        sibling.Color = RbColor.Red;
                    }

                    node = parent;
                    parent = node?.Parent;
                }
                else
                {
                    if (ColorOf(sibling?.Right) == RbColor.Black)
                    {
                        if (sibling?.Left != null)
                        {
                            sibling.Left.Color = RbColor.Black;
                        }

                        if (sibling != null)
                        {
                            sibling.Color = RbColor.Red;
                            RotateRight(sibling);
                        }

                        sibling = parent.Right;
                    }

                    if (sibling != null)
                    {
                        sibling.Color = parent.Color;
                    }

                    parent.Color = RbColor.Black;
                    if (sibling?.Right != null)
                    {
                        sibling.Right.Color = RbColor.Black;
                    }

                    RotateLeft(parent);
                    node = Root;
                    parent = null;
                }
            }
            else
            {
                RbNode<TKey, TValue>? sibling = parent?.Left;
                if (ColorOf(sibling) == RbColor.Red)
                {
                    sibling!.Color = RbColor.Black;
                    parent!.Color = RbColor.Red;
                    RotateRight(parent);
                    sibling = parent.Left;
                }

                if (ColorOf(sibling?.Left) == RbColor.Black && ColorOf(sibling?.Right) == RbColor.Black)
                {
                    if (sibling != null)
                    {
                        sibling.Color = RbColor.Red;
                    }

                    node = parent;
                    parent = node?.Parent;
                }
                else
                {
                    if (ColorOf(sibling?.Left) == RbColor.Black)
                    {
                        if (sibling?.Right != null)
                        {
                            sibling.Right.Color = RbColor.Black;
                        }

                        if (sibling != null)
                        {
                            sibling.Color = RbColor.Red;
                            RotateLeft(sibling);
                        }

                        sibling = parent?.Left;
                    }

                    if (sibling != null)
                    {
                        sibling.Color = parent!.Color;
                    }

                    parent!.Color = RbColor.Black;
                    if (sibling?.Left != null)
                    {
                        sibling.Left.Color = RbColor.Black;
                    }

                    RotateRight(parent);
                    node = Root;
                    parent = null;
                }
            }
        }

        if (node != null)
        {
            node.Color = RbColor.Black;
        }
    }

    private static RbColor ColorOf(RbNode<TKey, TValue>? node) => node?.Color ?? RbColor.Black;
}
