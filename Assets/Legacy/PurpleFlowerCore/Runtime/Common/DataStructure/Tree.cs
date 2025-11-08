using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

namespace PurpleFlowerCore
{
    [Serializable]
    public class Tree<T> : IEnumerable<TreeNode<T>>
    {
        public TreeNode<T> Root;
        
        public TreeNode<T> this[string[] path] => GetNode(path);
        
        public TreeNode<T> this[string path] => GetNode(path);
        
        public Tree(string rootName = "Root")
        {
            Root = new TreeNode<T>(new[] {rootName});
        }


        public void CreateNodeByPath(string path, T data)
        {
            path = Root.name + '/' + path.Trim('/');
            CreateNodeByPath(path.Split('/'), data);
        }
        
        public void CreateNodeByPath(string[] path, T data)
        {
            Root.CreateNodeByPath(path, data,2);
        }
        
        public TreeNode<T> GetNode(string path)
        {
            path = Root.name+ '/' + path.Trim('/');
            return GetNode(path.Split('/'));
        }
        
        public TreeNode<T> GetNode(string[] path)
        {
            var node = Root;
            foreach (var name in path)
            {
                node = node.GetChild(name);
                if (node == null)
                {
                    return null;
                }
            }
            return node;
        }
        
        public void RemoveNode(string[] path)
        {
            var node = GetNode(path);
            if (node != null)
            {
                node.Parent.RemoveChild(node);
            }
        }

        public List<TreeNode<T>> GetNodes()
        {
            var nodes = new List<TreeNode<T>>();
            var queue = new Queue<TreeNode<T>>();
            queue.Enqueue(Root);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                nodes.Add(node);
                foreach (var child in node.Children)
                {
                    queue.Enqueue(child);
                }
            }
            return nodes;
        }
        
        public List<TreeNode<T>> GetLeaves()
        {
            var leaves = new List<TreeNode<T>>();
            var queue = new Queue<TreeNode<T>>();
            queue.Enqueue(Root);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node.IsLeaf)
                {
                    leaves.Add(node);
                }
                else
                {
                    foreach (var child in node.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
            return leaves;
        }

        public IEnumerator<TreeNode<T>> GetEnumerator()
        {
            var nodes = GetNodes();
            foreach (var node in nodes)
            {
                yield return node;
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    [Serializable]
    public class TreeNode<T>
    {
        public string name;
        private string[] path;
        public string[] Path => path[1..];
        public T Value;
        public TreeNode<T> Parent;
        public List<TreeNode<T>> Children;
        public bool IsLeaf => Children.Count == 0;
        public bool IsRoot => Parent == null;
        /// <summary>
        /// Depth of the node in the tree, root node has depth 1
        /// </summary>
        public int Depth => path.Length;

        public TreeNode<T> LeftChild
        {
            get
            {
                if(Children.Count > 2) PFCLog.Warning("LeftChild() called on a node with more than 2 children");
                return Children[0];
            }
        }
        
        public TreeNode<T> RightChild
        {
            get
            {
                if(Children.Count > 2) PFCLog.Warning("RightChild() called on a node with more than 2 children");
                return Children.Last();
            }
        }
        
        public TreeNode<T> this[string name] => GetChild(name);
        
        public TreeNode<T> CreateNodeByPath(string[] path, T data, int depth)
        {
            string[] childPath = path[..depth];
            var child = GetChild(childPath[depth-1]);
            if(child == null)
            {
                child = new TreeNode<T>(childPath);
                AddChild(child);
            }

            if (path.Length == depth)
            {
                child.Value = data;
                return child;
            }
            depth++;
            return child.CreateNodeByPath(path, data, depth);
        }
        
        public TreeNode(string[] path, T value = default)
        {
            name = path.Last();
            this.Value = value;
            this.path = path.Clone() as string[];
            Children = new List<TreeNode<T>>();
        }
        
        public void AddChild(TreeNode<T> child)
        {
            Children.Add(child);
            child.Parent = this;
        }
        
        public void RemoveChild(TreeNode<T> child)
        {
            Children.Remove(child);
            child.Parent = null;
        }
        
        // public TreeNode<T> LeftChild()
        // {
        //     return Children[0];
        // }
        //
        // public TreeNode<T> RightChild()
        // {
        //     return Children[1];
        // }
        
        public TreeNode<T> GetChild(string name)
        {
            return Children.Find(child => child.name == name);
        }
        
        public TreeNode<T> GetChild(TreeNode<T> child)
        {
            return Children.Find(c => c == child);
        }
    }
}