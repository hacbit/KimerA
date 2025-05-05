#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace KimerA.Editor.Graph
{
    public abstract class BaseGraphEditor<TGraphNode> : EditorWindow, IGraphNodeChanger
        where TGraphNode : BaseGraphNode
    {
        private HashSet<TGraphNode> m_Nodes = new();
        private TGraphNode? m_SelectedNode;
        private Vector2 m_Offset;
        private Vector2 m_Drag;

        protected virtual void OnEnable()
        {
            InitContextMenu_Internal();
        }

        #region Display Graph
        private void OnGUI()
        {
            DrawGrid(20 * m_Zoom, 0.2f, Color.gray);
            DrawGrid(100 * m_Zoom, 0.4f, Color.gray);

            DrawNodes();
            DrawConnections();
            DrawConnecting();

            ProcessEvents(Event.current);

            if (GUI.changed) Repaint();
        }

        /// <summary>
        /// <para>
        /// Draws a grid as a background for the graph editor.
        /// </para>
        /// </summary>
        /// <param name="gridSpacing"></param>
        /// <param name="gridOpacity"></param>
        /// <param name="gridColor"></param>
        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, 0, 0), new Vector3(gridSpacing * i, position.height, 0f));
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(0, gridSpacing * j, 0), new Vector3(position.width, gridSpacing * j, 0f));
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void DrawNodes()
        {
            if (m_Nodes == null) return;

            foreach (var node in m_Nodes)
            {
                node.OnDraw(m_Zoom);
            }
        }

        /// <summary>
        /// <para>
        /// Draws the connections between nodes in the graph editor.
        /// </para>
        /// </summary>
        private void DrawConnections()
        {
            if (m_Nodes == null) return;

            foreach (var node in m_Nodes)
            {
                foreach (var connectedNode in node.GetConnections<TGraphNode>())
                {
                    DrawConnection(node.Rect.center, connectedNode.Rect.center);
                }
            }
        }

        private void DrawConnection(Vector2 start, Vector2 end)
        {
            Handles.DrawBezier(
                start,
                end,
                start + Vector2.left * 50f,
                end - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );
        }
        #endregion

        #region Process Events
        /// <summary>
        /// <para>
        /// Processes the mouse events for the graph editor.
        /// </para>
        /// </summary>
        /// <param name="e"></param>
        private void ProcessEvents(Event e)
        {
            m_Drag = Vector2.zero;

            switch (e.type)
            {
                case EventType.MouseDown:
                    // right mouse button down
                    if (e.button is 1)
                    {
                    }
                    // left mouse button down
                    else if (e.button is 0)
                    {
                        // `OnSelectNode()` should after this method.
                        // `OnConnectionEnd()` depends the `m_ConnectionStartNode` value,
                        // however, that property is represented by `m_SelectedNode`.
                        // If call `OnSelectNode()` first, the `m_ConnectionStartNode` will be changed.
                        if (m_IsConnecting)
                        {
                            OnConnectionEnd();
                        }

                        // select node
                        OnSelectNode(e.mousePosition / m_Zoom);
                    }
                    else if (e.button is 2)
                    {

                    }
                    break;

                case EventType.MouseDrag:
                    // left mouse button drag
                    if (e.button is 0)
                    {
                        // move selected node
                        OnDragSelectedNode(e.delta / m_Zoom);
                    }
                    // right mouse button drag
                    else if (e.button is 1)
                    {
                    }
                    // middle mouse button drag
                    else if (e.button is 2)
                    {
                        // move the whole graph
                        OnWholeGraphDrag(e.delta / m_Zoom);
                    }
                    break;

                case EventType.MouseUp:
                    // left mouse button up
                    if (e.button is 0)
                    {
                        // release selected node (not unselect)
                        OnReleaseSelectedNode();
                    }
                    // right mouse button up
                    else if (e.button is 1)
                    {
                        // pop up context menu
                        ShowContextMenu(e.mousePosition);
                    }
                    break;

                case EventType.ScrollWheel:
                    // scroll wheel event
                    // zoom in/out the graph
                    OnZooming(e.delta);
                    break;
            }
        }

        /// <summary>
        /// <para>
        /// This method checks if the mouse position is within any node's rectangle
        /// and sets the selected node accordingly.
        /// </para>
        /// </summary>
        /// <param name="mousePosition"></param>
        private void OnSelectNode(Vector2 mousePosition)
        {
            m_SelectedNode = default;
            foreach (var node in m_Nodes)
            {
                if (node.Rect.Contains(mousePosition))
                {
                    m_SelectedNode = node;
                    node.IsDragged = true;
                    break;
                }
            }
        }

        /// <summary>
        /// <para>
        /// This method checks if a node is selected and drags it according to the mouse delta.
        /// </para>
        /// </summary>
        /// <param name="delta"></param>
        private void OnDragSelectedNode(Vector2 delta)
        {
            if (m_SelectedNode != null && m_SelectedNode.IsDragged)
            {
                m_SelectedNode.OnDrag(delta);
                GUI.changed = true;
            }
        }

        /// <summary>
        /// <para>
        /// This method resets the IsDragged property of the selected node.
        /// **The node will not be unselected.**
        /// </para>
        /// </summary>
        private void OnReleaseSelectedNode()
        {
            // *DO NOT* modify to `m_SelectedNode?.IsDragged = false;`
            // operator `?.xxx` returns a right value, so it will not be set to `false`.
            if (m_SelectedNode != null)
            {
                m_SelectedNode.IsDragged = false;
            }
        }

        /// <summary>
        /// <para>
        /// This method drags all nodes in the graph according to the mouse delta.
        /// </para>
        /// </summary>
        /// <param name="delta"></param>
        private void OnWholeGraphDrag(Vector2 delta)
        {
            m_Drag = delta;

            if (m_Nodes != null)
            {
                foreach (var node in m_Nodes)
                {
                    node.OnDrag(delta);
                }
            }

            GUI.changed = true;
        }
        #endregion

        #region Context Menu
        private GenericMenu m_ContextMenu;

        /// <summary>
        /// <para>
        /// Initializes the context menu by `BuildContextMenu()` method.
        /// </para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitContextMenu_Internal()
        {
            m_ContextMenu = BuildContextMenu().CreateMenu(this);
        }

        /// <summary>
        /// <para>
        /// Builds the context menu for the graph editor.
        /// </para>
        /// </summary>
        /// <returns></returns>
        public abstract GraphMenuTree BuildContextMenu();

        /// <summary>
        /// <para>
        /// Pop up a context menu on the graph editor.
        /// </para>
        /// </summary>
        /// <param name="mousePosition"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ShowContextMenu(Vector2 mousePosition)
        {
            m_ContextMenu.ShowAsContext();
        }
        #endregion

        #region Scale and Zoom

        private float m_Zoom = 1.0f;
        private const float k_ZoomMin = 0.5f;
        private const float k_ZoomMax = 2.0f;
        private const float k_ZoomSpeed = 0.1f;

        /// <summary>
        /// <para>
        /// Zooms the graph editor in and out based on the mouse scroll wheel delta.
        /// </para>
        /// </summary>
        /// <param name="delta"></param>
        private void OnZooming(Vector2 delta)
        {
            var zoomDelta = -delta.y * k_ZoomSpeed;
            m_Zoom = Mathf.Clamp(m_Zoom + zoomDelta, k_ZoomMin, k_ZoomMax);
            GUI.changed = true;
        }
        #endregion

        #region Connections
        private bool m_IsConnecting = false;

        /// <summary>
        /// Represents the start node of the connection.
        /// </summary>
        private TGraphNode? m_ConnectionStartNode
        {
            get => m_SelectedNode;
            set => m_SelectedNode = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DrawConnecting()
        {
            if (m_IsConnecting is false || m_ConnectionStartNode is null) return;

            DrawConnection(m_ConnectionStartNode.Rect.center, Event.current.mousePosition);
        }

        /// <summary>
        /// <para>
        /// Called when the user starts a connection from a node.
        /// </para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnConnectionStart()
        {
            m_IsConnecting = true;
        }

        /// <summary>
        /// <para>
        /// Called when the user choose a connection target node.
        /// </para>
        /// </summary>
        private void OnConnectionEnd()
        {
            if (m_ConnectionStartNode is null) return;

            // check if the target node is valid
            foreach (var node in m_Nodes)
            {
                if (node == m_ConnectionStartNode) continue;

                if (node.Rect.Contains(Event.current.mousePosition))
                {
                    // make the connection
                    m_ConnectionStartNode.MakeConnection(node);

                    // reset the connection state
                    m_IsConnecting = false;
                    m_ConnectionStartNode = null;
                    break;
                }
            }
        }

        #endregion

        #region Add Node

        void IGraphNodeChanger.AddNode(BaseGraphNode node)
        {
            if (node is TGraphNode graphNode)
            {
                node.Rect.position = Event.current.mousePosition / m_Zoom;
                m_Nodes.Add(graphNode);
            }
            else
            {
                Debug.LogError($"Node is not of type {typeof(TGraphNode).Name}.");
            }
        }

        void IGraphNodeChanger.RemoveNode(BaseGraphNode node)
        {
            if (node is TGraphNode graphNode)
            {
                m_Nodes.Remove(graphNode);
            }
            else
            {
                Debug.LogError($"Node is not of type {typeof(TGraphNode).Name}.");
            }
        }

        #endregion
    }

    public interface IGraphNodeChanger
    {
        void AddNode(BaseGraphNode node);
        void RemoveNode(BaseGraphNode node);
    }

    [Flags]
    public enum GraphMenuFeature
    {
        None = 0,
        AddNode = 1 << 0,
        RemoveNode = 1 << 1,
        ConnectNode = 1 << 2,
        DisconnectNode = 1 << 3,
        All = AddNode | RemoveNode | ConnectNode | DisconnectNode,
    }
}

#endif