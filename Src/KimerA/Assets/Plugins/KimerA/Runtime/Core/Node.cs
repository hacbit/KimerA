using System;
using System.Collections.Generic;
using System.Linq;

namespace KimerA
{
    public readonly struct NodeId
    {
        public int Id { get; init; }

        public NodeId(int id)
        {
            Id = id;
        }
    }

    public class NodeBuilder
    {
        private static int s_Id;

        public NodeBuilder()
        {
            s_Id = new Random().Next();
        }

        public NodeId MakeId()
        {
            var id = new NodeId(s_Id);
            s_Id += 1;
            return id;
        }
    }

    public interface INodeAction
    {
        void OnPreStartup(ref NodeActionState state) { }
        void OnStartup(ref NodeActionState state) { }
        void OnPostStartup(ref NodeActionState state) { }
        void OnUpdate(ref NodeActionState state) { }
        void OnPostUpdate(ref NodeActionState state) { }
        void OnFinish(ref NodeActionState state) { }
    }

    public delegate void NodeAction(ref NodeActionState state);

    public struct NodeActionState
    {
        internal Func<INode, NodeAction, NodeAction> Interceptors;

        public void Refresh()
        {
            Interceptors = (_, action) => action;
        }

        public void ApplyInterceptor(Func<INode, bool> predicate, Func<NodeAction, NodeAction> transform)
        {
            var interceptor = (INode node, NodeAction action) =>
            {
                if (predicate(node))
                {
                    return transform(action);
                }
                return action;
            };
            Interceptors += interceptor;
        }
    }

    public interface INode : INodeAction
    {

    }

#region Node Provider
    public interface IImmutableNodeProvider
    {
        NodeId Id { get; }

        IImmutableNodeProvider? Parent { get; }

        IEnumerable<IImmutableNodeProvider> Children { get; }
    }

    public class ImmutableNodeProvider<TNode> : IImmutableNodeProvider where TNode : INode
    {
        public readonly TNode Instance;

        private readonly IImmutableNodeProvider? m_Parent;

        private readonly List<IImmutableNodeProvider> m_Children;

        public ImmutableNodeProvider(TNode instance, NodeId id, IImmutableNodeProvider? parent)
        {
            Instance = instance;
            Id = id;
            m_Parent = parent;
            m_Children = new List<IImmutableNodeProvider>();
        }

        public NodeId Id { get; init; }

        public IImmutableNodeProvider? Parent => m_Parent;

        public IEnumerable<IImmutableNodeProvider> Children => m_Children;
    }

    internal interface INodeProvider : INodeAction
    {
        INode Instance { get; }

        NodeId Id { get; }

        INodeProvider? Parent { get; }

        IEnumerable<INodeProvider> Children { get; }

        void AddChild(INodeProvider child);

        void AddChildren(IEnumerable<INodeProvider> children);

        void RemoveChild(INodeProvider child);

        void ReplaceChild(INodeProvider origin, INodeProvider target);

        void ReplaceChildInherit(INodeProvider origin, INodeProvider target);

        IImmutableNodeProvider ToImmutable();
    }

    internal class NodeProvider<TNode> : INodeProvider where TNode : INode
    {
        private readonly TNode m_Instance;

        public INode Instance => Instance;

        private readonly INodeProvider? m_Parent;

        private readonly List<INodeProvider> m_Children;

        public NodeProvider(TNode instance, NodeId id, INodeProvider? parent)
        {
            m_Instance = instance;
            Id = id;
            m_Parent = parent;
            m_Children = new List<INodeProvider>();
        }

        /// <summary>
        /// Node id
        /// </summary>
        public NodeId Id { get; init; }

        /// <summary>
        /// Parent node
        /// </summary>
        public INodeProvider? Parent => m_Parent;

        /// <summary>
        /// Children nodes
        /// </summary>
        public IEnumerable<INodeProvider> Children => m_Children;

        /// <summary>
        /// Add child node
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(INodeProvider child)
        {
            m_Children.Add(child);
        }

        /// <summary>
        /// Add children nodes
        /// </summary>
        /// <param name="children"></param>
        public void AddChildren(IEnumerable<INodeProvider> children)
        {
            m_Children.AddRange(children);
        }

        /// <summary>
        /// Remove child node
        /// </summary>
        /// <param name="child"></param>
        public void RemoveChild(INodeProvider child)
        {
            m_Children.Remove(child);
        }

        /// <summary>
        /// Replace child node with another node, and the children of the original node will be removed
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        public void ReplaceChild(INodeProvider origin, INodeProvider target)
        {
            var index = m_Children.IndexOf(origin);
            m_Children[index] = target;
        }

        /// <summary>
        /// Replace child node with another node, and the replaced node will inherit the children of the original node
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        public void ReplaceChildInherit(INodeProvider origin, INodeProvider target)
        {
            var index = m_Children.IndexOf(origin);
            m_Children[index] = target;
            target.AddChildren(origin.Children);
        }

        public IImmutableNodeProvider ToImmutable()
        {
            return new ImmutableNodeProvider<TNode>(m_Instance, Id, m_Parent?.ToImmutable());
        }

        public void OnPreStartup(ref NodeActionState state) => m_Instance.OnPreStartup(ref state);

        public void OnStartup(ref NodeActionState state) => m_Instance.OnStartup(ref state);

        public void OnPostStartup(ref NodeActionState state) => m_Instance.OnPostStartup(ref state);

        public void OnUpdate(ref NodeActionState state) => m_Instance.OnUpdate(ref state);

        public void OnPostUpdate(ref NodeActionState state) => m_Instance.OnPostUpdate(ref state);

        public void OnFinish(ref NodeActionState state) => m_Instance.OnFinish(ref state);
    }
#endregion

#region  NodeSystem
    public class NodeSystem
    {
        private readonly NodeBuilder m_NodeBuilder = new();

        /// <summary>
        /// Mark node by nodeId
        /// </summary>
        private readonly Dictionary<NodeId, INodeProvider> m_NodeMap = new();

        /// <summary>
        /// Use Type to speed up the search
        /// </summary>
        private readonly Dictionary<Type, List<INodeProvider>> m_NodeTypeMap = new();

        /// <summary>
        /// Find nodes by type
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <returns></returns>
        internal IEnumerable<NodeProvider<TNode>> FindNodes<TNode>() where TNode : INode
        {
            if (m_NodeTypeMap.TryGetValue(typeof(TNode), out var nodes))
            {
                return nodes.Select(node => (NodeProvider<TNode>)node);
            }
            else
            {
                return Array.Empty<NodeProvider<TNode>>();
            }
        }

        /// <summary>
        /// Get All nodes
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<INodeProvider> GetAllNodes()
        {
            return m_NodeMap.Values;
        }

        internal void AddWithSystem<TNode>(NodeProvider<TNode> node) where TNode : INode
        {
            m_NodeMap.Add(node.Id, node);
            if (m_NodeTypeMap.TryGetValue(typeof(TNode), out var nodes))
            {
                nodes.Add(node);
            }
            else
            {
                m_NodeTypeMap.Add(typeof(TNode), new() { node });
            }
        }

        /// <summary>
        /// Add node as a child node of specified parent node
        /// </summary>
        /// <typeparam name="PNode"></typeparam>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="parent"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        internal NodeProvider<TNode> AddNode<TNode, PNode>(TNode node, NodeProvider<PNode> parent)
            where TNode : INode
            where PNode : INode
        {
            var nodeProvider = new NodeProvider<TNode>(node, m_NodeBuilder.MakeId(), parent);
            parent.AddChild(nodeProvider);
            AddWithSystem(nodeProvider);
            return nodeProvider;
        }

        /// <summary>
        /// Get or initialize the root node
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        internal NodeProvider<TNode> GetOrInitRoot<TNode>(TNode node) where TNode : INode
        {
            if (m_NodeMap.Count == 0)
            {
                var nodeProvider = new NodeProvider<TNode>(node, m_NodeBuilder.MakeId(), null);
                AddWithSystem(nodeProvider);
                return nodeProvider;
            }
            return (NodeProvider<TNode>)m_NodeMap.First().Value;
        }

        /// <summary>
        /// Inserts a node between the specified original node and its parent node
        /// </summary>
        /// <typeparam name="ONode"></typeparam>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="originNode"></param>
        /// <param name="insertNode"></param>
        internal void InsertNode<ONode, TNode>(NodeProvider<ONode> originNode, TNode insertNode)
            where ONode : INode
            where TNode : INode
        {
            var nodeProvider = new NodeProvider<TNode>(insertNode, m_NodeBuilder.MakeId(), originNode.Parent);
            AddWithSystem(nodeProvider);
            originNode.Parent?.RemoveChild(originNode);
            originNode.Parent?.AddChild(nodeProvider);
            nodeProvider.AddChild(originNode);
        }

        /// <summary>
        /// Replace node with another node, and the replaced node will inherit the children of the original node
        /// </summary>
        /// <typeparam name="ONode"></typeparam>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="originNode"></param>
        /// <param name="targetNode"></param>
        internal void ReplaceNodeInherit<ONode, TNode>(NodeProvider<ONode> originNode, TNode targetNode)
            where ONode : INode
            where TNode : INode
        {
            var nodeProvider = new NodeProvider<TNode>(targetNode, m_NodeBuilder.MakeId(), originNode.Parent);
            AddWithSystem(nodeProvider);
            originNode.Parent?.ReplaceChildInherit(originNode, nodeProvider);
        }

        /// <summary>
        /// Replace node with another node, and then will remove the original node
        /// </summary>
        /// <typeparam name="ONode"></typeparam>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="originNode"></param>
        /// <param name="targetNode"></param>
        internal void ReplaceNode<ONode, TNode>(NodeProvider<ONode> originNode, TNode targetNode)
            where ONode : INode
            where TNode : INode
        {
            var nodeProvider = new NodeProvider<TNode>(targetNode, m_NodeBuilder.MakeId(), originNode.Parent);
            AddWithSystem(nodeProvider);
            originNode.Parent?.ReplaceChild(originNode, nodeProvider);
        }

        internal void RemoveWithSystem<TNode>(NodeProvider<TNode> node) where TNode : INode
        {
            m_NodeMap.Remove(node.Id);
            if (m_NodeTypeMap.TryGetValue(typeof(TNode), out var nodes))
            {
                nodes.Remove(node);
            }
        }

        /// <summary>
        /// Remove node
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        internal void RemoveNode<TNode>(NodeProvider<TNode> node) where TNode : INode
        {
            RemoveWithSystem(node);
            node.Parent?.RemoveChild(node);
        }

        /// <summary>
        /// Get node provider by immutable node provider
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="immutableNodeProvider"></param>
        /// <returns></returns>
        internal NodeProvider<TNode> GetNodeProvider<TNode>(ImmutableNodeProvider<TNode> immutableNodeProvider) where TNode : INode
        {
            return (NodeProvider<TNode>)m_NodeMap[immutableNodeProvider.Id];
        }
    }
#endregion

}