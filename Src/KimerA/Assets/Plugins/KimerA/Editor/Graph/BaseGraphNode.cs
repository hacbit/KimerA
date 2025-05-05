#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KimerA.Editor.Graph
{
    public class BaseGraphNode
    {
        public string Title;

        public Rect Rect;

        private readonly HashSet<BaseGraphNode> Connections;

        public bool IsDragged;

        /// <summary>
        /// Default size of the node. This is used while creating a new node
        /// without specifying width and height.
        /// </summary>
        public virtual (float width, float height) DefaultSize => (100, 100);

        #region Constructors
        protected BaseGraphNode()
            : this(string.Empty, Vector2.zero)
        {
        }

        public BaseGraphNode(string title)
            : this(title, Vector2.zero)
        {
        }

        /// <summary>
        /// Creates a new node with default size.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="position"></param>
        public BaseGraphNode(string title, Vector2 position)
        {
            Title = title;
            Rect = new Rect(position, new Vector2(DefaultSize.width, DefaultSize.height));
            Connections = new();
        }

        public BaseGraphNode(string title, Vector2 position, float width, float height)
            : this(title, position, new Vector2(width, height))
        {
        }

        public BaseGraphNode(string title, Vector2 position, (float width, float height) size)
            : this(title, position, new Vector2(size.width, size.height))
        {
        }

        public BaseGraphNode(string title, Vector2 position, Vector2 size)
            : this(title, new Rect(position, size))
        {
        }

        public BaseGraphNode(string title, Rect rect)
        {
            Title = title;
            Rect = rect;
            Connections = new();
        }
        #endregion

        #region Virtual Methods
        public virtual void OnDraw(float zoom)
        {
            var scaledRect = new Rect(Rect.position * zoom, Rect.size * zoom);
            GUI.Box(scaledRect, Title);
        }

        public virtual void OnDrag(Vector2 delta)
        {
            Rect = new Rect(Rect.position + delta, Rect.size);
        }

        public virtual void MakeConnection(BaseGraphNode to)
        {
            Connections.Add(to);
            to.Connections.Add(this);
        }
        #endregion

        public IEnumerable<TGraphNode> GetConnections<TGraphNode>() where TGraphNode : BaseGraphNode
        {
            return Connections.Select(node => (TGraphNode)node);
        }
    }
}

#endif