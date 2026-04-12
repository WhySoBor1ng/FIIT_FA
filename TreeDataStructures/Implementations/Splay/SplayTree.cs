using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        Splay(newNode);
    }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
        if (parent != null)
        {
            Splay(parent);
        }
        else if (child != null)
        {
            Splay(child);
        }
    }

    public override bool ContainsKey(TKey key)
    {
        BstNode<TKey, TValue>? node = FindClosestNode(key);
        if (node != null)
        {
            Splay(node);
        }

        return node != null && Comparer.Compare(node.Key, key) == 0;
    }
    
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        BstNode<TKey, TValue>? node = FindClosestNode(key);
        if (node != null)
        {
            Splay(node);
        }

        if (node != null && Comparer.Compare(node.Key, key) == 0)
        {
            value = node.Value;
            return true;
        }
        
        value = default;
        return false;
    }

    private BstNode<TKey, TValue>? FindClosestNode(TKey key)
    {
        BstNode<TKey, TValue>? current = Root;
        BstNode<TKey, TValue>? last = null;
        while (current != null)
        {
            last = current;
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0)
            {
                return current;
            }

            current = cmp < 0 ? current.Left : current.Right;
        }

        return last;
    }

    private void Splay(BstNode<TKey, TValue> node)
    {
        while (node.Parent != null)
        {
            if (node.Parent.Parent == null)
            {
                if (node.IsLeftChild)
                {
                    RotateRight(node.Parent);
                }
                else
                {
                    RotateLeft(node.Parent);
                }
            }
            else if (node.IsLeftChild && node.Parent.IsLeftChild)
            {
                BstNode<TKey, TValue> parent = node.Parent;
                BstNode<TKey, TValue> grandParent = parent.Parent!;
                RotateRight(grandParent);
                RotateRight(parent);
            }
            else if (node.IsRightChild && node.Parent.IsRightChild)
            {
                BstNode<TKey, TValue> parent = node.Parent;
                BstNode<TKey, TValue> grandParent = parent.Parent!;
                RotateLeft(grandParent);
                RotateLeft(parent);
            }
            else if (node.IsRightChild && node.Parent.IsLeftChild)
            {
                BstNode<TKey, TValue> parent = node.Parent;
                BstNode<TKey, TValue> grandParent = parent.Parent!;
                RotateLeft(parent);
                RotateRight(grandParent);
            }
            else
            {
                BstNode<TKey, TValue> parent = node.Parent;
                BstNode<TKey, TValue> grandParent = parent.Parent!;
                RotateRight(parent);
                RotateLeft(grandParent);
            }
        }
    }
    
}
