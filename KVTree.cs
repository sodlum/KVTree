using System;
using System.Collections.Generic;
using System.Linq;

namespace KVTree
{
    /// <summary>
    /// A collection of Nodes sorted into a binary search tree based on Key values
    /// </summary>
    /// <typeparam name="K">System.Type for Key</typeparam>
    /// <typeparam name="V">System.Type for Value</typeparam>
    public class KVTree<K, V> : IDictionary<K, V>
        where K : IComparable
    {
        #region Properties, Fields and Constructors
        private int count;
        private Node<KeyValuePair<K, V>> root;
        private Comparer<K> comparer;

        /// <summary>
        /// Gets the total count of nodes in the tree
        /// </summary>
        public int Count { get { return count; } }

        /// <summary>
        /// Returns whether the current instance is read only
        /// </summary>
        public bool IsReadOnly { get { return false; } }

        /// <summary>
        /// Gets or sets the value of the indexed key
        /// </summary>
        /// <param name="key">Key to be matched</param>
        /// <returns>Value associated to key</returns>
        public V this[K key]
        {
            get
            {
                V val = default(V);
                if (!TryGetValue(key, out val))
                    throw new KeyNotFoundException("Key not found");

                return val;
            }
            set 
            { 
                recursiveSearch(new KeyValuePair<K, V>(key, default(V)))
                    .swapNodeData(new Node<KeyValuePair<K, V>>(new KeyValuePair<K, V>(key, value))); 
            }
        }

        /// <summary>
        /// Gets a list of sorted keys stored within the tree
        /// </summary>
        public ICollection<K> Keys
        {
            get
            {
                List<K> KeyCollection = (from entities in recursiveGetAll(root, new Stack<KeyValuePair<K, V>>()) select entities.Key).ToList();
                return KeyCollection;
            }
        }

        /// <summary>
        /// Gets a list of sorted values stored within the tree
        /// </summary>
        public ICollection<V> Values
        {
            get
            {
                List<V> ValueCollection = (from entities in recursiveGetAll(root, new Stack<KeyValuePair<K, V>>()) select entities.Value).ToList();
                return ValueCollection;
            }
        }

        /// <summary>
        /// Default constructor: Sets all fields to default values
        /// </summary>
        public KVTree()
        {
            count = 0;
            comparer = new Comparer<K>();
        }
        #endregion

        #region Add Node
        /// <summary>
        /// Adds new node with corresponding key and value
        /// </summary>
        /// <param name="inputKey"></param>
        /// <param name="inputValue"></param>
        public void Add(K inputKey, V inputValue) 
        {
            if (inputKey == null)
                throw new ArgumentNullException("Null Key cannot be added to tree");

            Add(new KeyValuePair<K, V>(inputKey, inputValue)); 
        }

        /// <summary>
        /// Adds new node with corresponding KeyValuePair
        /// </summary>
        /// <param name="newPair"></param>
        public void Add(KeyValuePair<K, V> newPair) 
        {
            if (newPair.Key == null)
                throw new ArgumentNullException("Null Key found within KeyValuePair.\nNull Key cannot be added to tree");

            Add(new Node<KeyValuePair<K, V>>(newPair)); 
        }

        /// <summary>
        /// Adds Node to tree
        /// </summary>
        /// <param name="item">Node to be added</param>
        public void Add(Node<KeyValuePair<K, V>> item)
        {
            if (root == null)
            {
                root = item;
                count++;
            }
            else recursiveAdd(item);
        }

        /// <summary>
        /// Calls recursiveAdd using the root node
        /// </summary>
        /// <param name="item">Node to be added</param>
        private void recursiveAdd(Node<KeyValuePair<K, V>> item) { recursiveAdd(root, item); }

        /// <summary>
        /// Recursively iterates through tree to find next available position to place new node
        /// Throws ArgumentException if key already exists within tree
        /// </summary>
        /// <param name="node">Current node</param>
        /// <param name="item">Node to be added</param>
        private void recursiveAdd(Node<KeyValuePair<K, V>> node, Node<KeyValuePair<K, V>> item)
        {
            if (comparer.Equals(node.Data.Key, item.Data.Key))
                throw new ArgumentException("KeyValuePair cannot be added: Key already exists.");
            else if (comparer.Compare(node.Data.Key, item.Data.Key) == -1)
            {
                if (node.Right == null)
                {
                    item.Parent = node;
                    node.Right = item;
                    count++;
                }
                else recursiveAdd(node.Right, item);
            }
            else
            {
                if (node.Left == null)
                {
                    item.Parent = node;
                    node.Left = item;
                    count++;
                }
                else recursiveAdd(node.Left, item);
            }
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes all nodes from the tree
        /// </summary>
        public void Clear()
        {
            Stack<KeyValuePair<K, V>> nodeStack = recursiveGetAll(root, new Stack<KeyValuePair<K, V>>());

            foreach (KeyValuePair<K, V> pairElement in nodeStack)
                Remove(pairElement);
        }

        /// <summary>
        /// Removes KeyValuePair from tree
        /// </summary>
        /// <param name="item">KeyValuePair to be removed</param>
        /// <returns>true if remove was successful</returns>
        public bool Remove(KeyValuePair<K, V> item) { return Remove(item.Key); }

        /// <summary>
        /// Removes node from the tree that matches the key value
        /// </summary>
        /// <param name="key">item to match</param>
        /// <returns>True if node is successfully removed or false if node is not found</returns>
        public bool Remove(K key)
        {
            Node<KeyValuePair<K, V>> nodeToRemove = recursiveSearch(new KeyValuePair<K, V>(key, default(V)));
            if (nodeToRemove == null)
                throw new ArgumentNullException("Key does not exist in tree and cannot be removed");

            if (nodeToRemove.Right == null && nodeToRemove.Left == null)
            {
                if (nodeToRemove.Parent != null)
                {
                    nodeToRemove.deleteNodeNoChildren(nodeToRemove.Parent);
                    nodeToRemove = null;
                }
                else root = null;
            }
            else
            {
                if (nodeToRemove.Right == null && nodeToRemove.Left != null)
                {
                    if (nodeToRemove.Parent != null)
                    {
                        nodeToRemove.deleteNodeLeftChild(nodeToRemove.Parent);
                        nodeToRemove = null;
                    }
                    else
                    {
                        root.deleteNodeLeftChild();
                        root = root.Left;
                    }
                }
                else if (nodeToRemove.Right != null && nodeToRemove.Left == null)
                {
                    if (nodeToRemove.Parent != null)
                    {
                        nodeToRemove.deleteNodeRightChild(nodeToRemove.Parent);
                        nodeToRemove = null;
                    }
                    else
                    {
                        root.deleteNodeRightChild();
                        root = root.Right;
                    }
                }
                else
                {
                    Node<KeyValuePair<K, V>> logicallyClosestNode = (new Random().Next(0, 2) == 1) ? 
                        logicalSuccessor(nodeToRemove) : 
                        logicalPredecessor(nodeToRemove);

                    if (nodeToRemove.Parent != null)
                        nodeToRemove.swapNodeData(logicallyClosestNode);
                    else root.swapNodeData(logicallyClosestNode);

                    if (logicallyClosestNode.Right == null && logicallyClosestNode.Left != null)
                        logicallyClosestNode.deleteNodeRightChild(logicallyClosestNode.Parent);
                    else if (logicallyClosestNode.Right != null && logicallyClosestNode.Left == null)
                        logicallyClosestNode.deleteNodeLeftChild(logicallyClosestNode.Parent);
                }
            }

            count--;
            return true;
        }


        #endregion

        #region Search
        /// <summary>
        /// Searches the tree for the specified key
        /// </summary>
        /// <param name="key">Key to match to</param>
        /// <returns>True if specified key is found</returns>
        public bool ContainsKey(K key) { return Contains(new KeyValuePair<K, V>(key, default(V))); }

        /// <summary>
        /// Searches the tree for the specified KeyValuePair
        /// </summary>
        /// <param name="item">Pair to match to</param>
        /// <returns>True if the specified KeyValuePair is found</returns>
        public bool Contains(KeyValuePair<K, V> item) { return recursiveSearch(item) != null; }

        private Node<KeyValuePair<K, V>> recursiveSearch(KeyValuePair<K, V> item)
        {
            return recursiveSearch(root, item);
        }
        /// <summary>
        /// Recursively steps through the tree to find the specified KeyValuePair
        /// </summary>
        /// <param name="node">Node to begin search from</param>
        /// <param name="item">Pair to match to</param>
        /// <returns>Node containing match to KeyValuePair or null if KeyValuePair is not matched</returns>
        private Node<KeyValuePair<K, V>> recursiveSearch(Node<KeyValuePair<K, V>> node, KeyValuePair<K, V> item)
        {
            if (comparer.Equals(node.Data.Key, item.Key))
                return node;
            else if (comparer.Compare(node.Data.Key, item.Key) == -1)
            {
                if (node.Right == null)
                    return null;
                else return recursiveSearch(node.Right, item);
            }
            else
            {
                if (node.Left == null)
                    return null;
                else return recursiveSearch(node.Left, item);
            }
        }

        /// <summary>
        /// Determines the logical successor of the current node
        /// </summary>
        /// <param name="node">Node to start search from</param>
        /// <returns>Node that is logically closest and greater in value to current node</returns>
        private Node<KeyValuePair<K, V>> logicalSuccessor(Node<KeyValuePair<K, V>> node)
        {
            Node<KeyValuePair<K, V>> successor = node.Right;

            while (successor.Left != null)
                successor = successor.Left;

            return successor;
        }

        /// <summary>
        /// Determines the logical predecessor of the current node
        /// </summary>
        /// <param name="node">Node to start search from</param>
        /// <returns>Node that is logically closest and lesser in value to current node</returns>
        private Node<KeyValuePair<K, V>> logicalPredecessor(Node<KeyValuePair<K, V>> node)
        {
            Node<KeyValuePair<K, V>> predecessor = node.Left;

            while (predecessor.Right != null)
                predecessor = predecessor.Right;

            return predecessor;
        }

        /// <summary>
        /// Attempts to get the value associated to the specified key
        /// </summary>
        /// <param name="key">Key used for matching</param>
        /// <param name="value">Value of associated key</param>
        /// <returns>true if Key is found or false otherwise</returns>
        public bool TryGetValue(K key, out V value)
        {
            Node<KeyValuePair<K, V>> node = recursiveSearch(new KeyValuePair<K, V>(key, default(V)));

            if (node == null)
            {
                value = default(V);
                return false;
            }
            else
            {
                value = node.Data.Value;
                return true;
            }
        }
        #endregion

        #region Enumerators
        /// <summary>
        /// Copies all KeyValuePairs in the tree to the given array
        /// </summary>
        /// <param name="array">Array to be copied to</param>
        /// <param name="arrayIndex">Index of array at which copying starts</param>
        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException();
            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException();
            if (array.Length - arrayIndex < count)
                throw new ArgumentException();

            foreach (KeyValuePair<K, V> pair in this)
                array[arrayIndex++] = pair;
        }

        /// <summary>
        /// Allows foreach loop to iterate through each node of the tree
        /// </summary>
        /// <returns>current iteration through the tree</returns>
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            Stack<KeyValuePair<K, V>> nodeStack = recursiveGetAll(new Stack<KeyValuePair<K, V>>());

            foreach (KeyValuePair<K, V> entity in nodeStack)
                yield return entity;
        }

        private Stack<KeyValuePair<K, V>> recursiveGetAll(Stack<KeyValuePair<K, V>> nodeStack)
        {
            return recursiveGetAll(root, nodeStack);
        }

        /// <summary>
        /// Recursively acquires all nodes in the tree.
        /// </summary>
        /// <param name="node">node to begin collecting from</param>
        /// <param name="nodeStack">collection of indexable nodes to be returned</param>
        /// <returns></returns>
        private Stack<KeyValuePair<K, V>> recursiveGetAll(Node<KeyValuePair<K, V>> node, Stack<KeyValuePair<K, V>> nodeStack)
        {
            if (node.Right != null) recursiveGetAll(node.Right, nodeStack);
            nodeStack.Push(node.Data);
            if (node.Left != null) recursiveGetAll(node.Left, nodeStack);
            return nodeStack;
        }

        /// <summary>
        /// System.Collections.IEnumerator
        /// </summary>
        /// <returns>GetEnumerator method of current instance</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
        #endregion

        #region Helper Classes
        /// <summary>
        /// Comparator class that checks equality of T objects
        /// </summary>
        /// <typeparam name="T">System.Type of comparable objects</typeparam>
        public class Comparer<T> : IEqualityComparer<T>, IComparer<T> where T : IComparable
        {
            /// <summary>
            /// Compares to objects to check for equality
            /// </summary>
            /// <param name="x">First object</param>
            /// <param name="y">Second object</param>
            /// <returns>True if objects x and y are equivalant</returns>
            public bool Equals(T x, T y) { return Compare(x, y) == 0; }

            /// <summary>
            /// Serves as a hash function for a particular type
            /// </summary>
            /// <param name="obj">object for which hash code is generated</param>
            /// <returns>Hash Code of object</returns>
            public int GetHashCode(T obj) { return obj.GetHashCode(); }

            /// <summary>
            /// Compares two objects:
            /// </summary>
            /// <param name="x">First object</param>
            /// <param name="y">Second object</param>
            /// <returns>
            /// -1 if param x is before param y in terms of value
            /// 0 if param x falls in the same position as param y in terms of value
            /// 1 if param x is after param y in terms of value
            /// </returns>
            public int Compare(T x, T y) { return x.CompareTo(y); }
        }

        /// <summary>
        /// Nodes that define the structure of the tree
        /// </summary>
        /// <typeparam name="T">System.Type of node data</typeparam>
        public class Node<T>
        {
            private T data;
            private Node<T> parent;
            private Node<T> left;
            private Node<T> right;

            /// <summary>
            /// Gets the content stored within the node
            /// </summary>
            public T Data { get { return data; } }

            /// <summary>
            /// Gets the parent node of the current node
            /// Sets the parent node only if the parent node is null
            /// </summary>
            public Node<T> Parent
            {
                get { return parent; }
                set 
                {
                    if (parent == null) parent = value;
                    else throw new FieldAccessException("Parent node cannot be set unless Parent is null");
                }
            }

            /// <summary>
            /// Gets the left child node of the current node
            /// Sets the left child node only if the left child node is null
            /// </summary>
            public Node<T> Left
            {
                get { return left; }
                set 
                {
                    if (left == null) left = value;
                    else throw new FieldAccessException("Left node cannot be set unless Left is null");
                }
            }

            /// <summary>
            /// Gets the right child node of the current node
            /// Sets the right child node only if the right child node is null
            /// </summary>
            public Node<T> Right
            {
                get { return right; }
                set 
                {
                    if (right == null) right = value;
                    else throw new FieldAccessException("Right node cannot be set unless Left is null");
                }
            }

            /// <summary>
            /// Creates a new, isolated node with no connection to the tree
            /// </summary>
            /// <param name="item">Instantiates the content of the new node</param>
            public Node(T item) { data = item; }

            /// <summary>
            /// Sets the value of the data field in the current node to the value of the data field in the source node
            /// </summary>
            /// <param name="source">Node to be copied from</param>
            public void swapNodeData(Node<T> source) { this.data = source.data; }

            /// <summary>
            /// Clears parent pointer of a node that has no child nodes
            /// </summary>
            /// <param name="parentNode">parent of current node</param>
            public void deleteNodeNoChildren(Node<T> parentNode)
            {
                if (parentNode.right == this)
                    parentNode.right = null;
                else parentNode.left = null;
            }

            /// <summary>
            /// Clears parent and right pointers of a node with only one right child
            /// </summary>
            /// <param name="parentNode">parent of current node</param>
            public void deleteNodeRightChild(Node<T> parentNode = null)
            {
                if (parentNode != null)
                {
                    if (parentNode.right == this)
                        parentNode.right = this.right;
                    else parentNode.left = this.right;

                    this.right.parent = parentNode;
                }
                else this.right.parent = null;
            }

            /// <summary>
            /// Clears parent and left pointers of a node with only one left child
            /// </summary>
            /// <param name="parentNode">parent of current node</param>
            public void deleteNodeLeftChild(Node<T> parentNode = null)
            {
                if (parentNode != null)
                {
                    if (parentNode.right == this)
                        parentNode.right = this.left;
                    else parentNode.left = this.left;

                    this.left.parent = parentNode;
                }
                else this.left.parent = null;
            }
        }
        #endregion
    }
}