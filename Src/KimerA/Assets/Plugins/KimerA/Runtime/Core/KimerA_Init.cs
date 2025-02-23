using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace KimerA
{
    public sealed class KimerA_Init : INode
    {
        public KimerA_Init()
        {
            m_rootProvider = m_NodeSystem.GetOrInitRoot(this);
        }

        private NodeProvider<KimerA_Init> m_rootProvider;

        private NodeSystem m_NodeSystem = new();

        private Action<IImmutableNodeProvider> m_NodeProviderInsertAction = _ => {};

        /// <summary>
        /// Add a node as a child of root
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public KimerA_Init AddNode<TNode>(TNode node) where TNode : INode
        {
            m_NodeSystem.AddNode(node, m_rootProvider);
            return this;
        }

        /// <summary>
        /// A builder helper for adding children nodes
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        public class ChildrenBuilder<TNode> where TNode : INode
        {
            private readonly NodeProvider<TNode> m_parentProvider;

            private readonly NodeSystem m_NodeSystem;

            internal ChildrenBuilder(NodeProvider<TNode> parentProvider, NodeSystem nodeSystem)
            {
                m_parentProvider = parentProvider;
                m_NodeSystem = nodeSystem;
            }

            /// <summary>
            /// Add a node as a child of the parent node
            /// </summary>
            /// <typeparam name="RNode"></typeparam>
            /// <param name="node"></param>
            /// <returns></returns>
            public ChildrenBuilder<TNode> AddNode<RNode>(RNode node) where RNode : INode
            {
                m_NodeSystem.AddNode(node, m_parentProvider);
                return this;
            }

            /// <summary>
            /// Add a node as a child of the parent node with children
            /// </summary>
            /// <typeparam name="RNode"></typeparam>
            /// <param name="node"></param>
            /// <param name="childrenBuilder"></param>
            /// <returns></returns>
            public ChildrenBuilder<TNode> AddWithChildren<RNode>(RNode node, Action<ChildrenBuilder<RNode>> childrenBuilder) where RNode : INode
            {
                var provider = m_NodeSystem.AddNode(node, m_parentProvider);
                childrenBuilder(new ChildrenBuilder<RNode>(provider, m_NodeSystem));
                return this;
            }
        }

        /// <summary>
        /// Add a node as a child of root with children
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="childrenBuilder"></param>
        /// <returns></returns>
        public KimerA_Init AddWithChildren<TNode>(TNode node, Action<ChildrenBuilder<TNode>> childrenBuilder) where TNode : INode
        {
            var provider = m_NodeSystem.AddNode(node, m_rootProvider);
            childrenBuilder(new ChildrenBuilder<TNode>(provider, m_NodeSystem));
            return this;
        }

        /// <summary>
        /// Insert a node by a predicate.
        /// <para>The inserted node will replace the original parent node of the target node, 
        /// and then the parent node will become the parent node of the inserted node.</para>
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="insertNode"></param>
        /// <returns></returns>
        public KimerA_Init InsertBy<TNode, RNode>(TNode insertNode, Func<ImmutableNodeProvider<RNode>, bool> predicate, Func<IImmutableNodeProvider, ImmutableNodeProvider<RNode>?> transform)
            where TNode : INode
            where RNode : INode
        {
            m_NodeProviderInsertAction += (immutableNodeProvider) =>
            {
                if (immutableNodeProvider is ImmutableNodeProvider<RNode> nodeProvider)
                {
                    if (predicate(nodeProvider) && transform(immutableNodeProvider) is ImmutableNodeProvider<RNode> transformed)
                    {
                        var provider = m_NodeSystem.GetNodeProvider(transformed);
                        m_NodeSystem.InsertNode(provider, insertNode);
                    }
                }
            };
            return this;
        }

        /// <summary>
        /// Apply all changes
        /// </summary>
        public void Load()
        {
            var nodes = m_NodeSystem.GetAllNodes();
            // Insert nodes
            foreach (var node in nodes)
            {
                m_NodeProviderInsertAction(node.ToImmutable());
            }
        }

        public void OnPreStartup(ref NodeActionState state)
        {
            foreach (var node in m_NodeSystem.GetAllNodes())
            {
                node.OnPreStartup(ref state);
            }
        }

        public void OnStartup(ref NodeActionState state)
        {
            foreach (var node in m_NodeSystem.GetAllNodes())
            {
                node.OnStartup(ref state);
            }
        }

        public void OnPostStartup(ref NodeActionState state)
        {
            foreach (var node in m_NodeSystem.GetAllNodes())
            {
                node.OnPostStartup(ref state);
            }
        }

        public void OnUpdate(ref NodeActionState state)
        {
            foreach (var node in m_NodeSystem.GetAllNodes())
            {
                node.OnUpdate(ref state);
            }
        }

        public void OnPostUpdate(ref NodeActionState state)
        {
            foreach (var node in m_NodeSystem.GetAllNodes())
            {
                node.OnPostUpdate(ref state);
            }
        }

        public void OnFinish(ref NodeActionState state)
        {
            foreach (var node in m_NodeSystem.GetAllNodes())
            {
                node.OnFinish(ref state);
            }
        }

        public void Run()
        {
            UniTask.RunOnThreadPool(RunAsync, true, TokenSource.Token).Forget();
        }

        public CancellationTokenSource TokenSource { get; } = new();

        private async UniTask RunAsync()
        {
            var state = new NodeActionState();
            OnPreStartup(ref state);
            await UniTask.Yield();
            OnStartup(ref state);
            await UniTask.Yield();
            OnPostStartup(ref state);
            await UniTask.Yield();

            while (TokenSource.Token.IsCancellationRequested is false)
            {
                OnUpdate(ref state);
                await UniTask.Yield();
                OnPostUpdate(ref state);
                await UniTask.Delay(1000 / 60);
            }

            OnFinish(ref state);
        }
    }
}