#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace KimerA.Editor.Graph
{
    public readonly struct GraphMenuTree : IEnumerable
    {
        public readonly string Name;
        private readonly List<GraphMenuItem> Items;

        public GraphMenuTree()
            : this(string.Empty)
        {
        }

        public GraphMenuTree(string name)
        {
            Name = name;
            Items = new();
        }

        /// <summary>
        /// Builds the context menu for the graph editor.
        /// </summary>
        /// <param name="editor"></param>
        /// <returns></returns>
        internal GenericMenu CreateMenu<TNode>(BaseGraphEditor<TNode> editor) where TNode : BaseGraphNode
        {
            var menu = new GenericMenu();
            foreach (var item in Items)
            {
                menu.AddItem(new GUIContent(item.Path), false,
                    (editor) =>
                    {
                        var node = item.Callback.Invoke();
                        node.Rect.position = Event.current.mousePosition;
                        if (editor is IGraphNodeChanger nodeChanger)
                        {
                            nodeChanger.AddNode(node);
                        }
                        else
                        {
                            Debug.LogError($"Graph editor is not of type {typeof(BaseGraphEditor<TNode>).Name}.");
                        }
                    }, editor);
            }
            return menu;
        }

        public void Add(string path, GraphMenuItemCallback callback)
        {
            if (string.IsNullOrEmpty(Name))
            {
                Items.Add(new GraphMenuItem(path, callback));
            }
            else
            {
                var newPath = $"{Name}/{path}";
                Items.Add(new GraphMenuItem(newPath, callback));
            }
        }

        public void Add(GraphMenuItem item)
        {
            if (string.IsNullOrEmpty(Name))
            {
                Items.Add(item);
            }
            else
            {
                var newPath = $"{Name}/{item.Path}";
                Items.Add(new GraphMenuItem(newPath, item.Callback));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(GraphMenuTree menu)
        {
            foreach (var item in menu.Items)
            {
                Add(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }

    public delegate BaseGraphNode GraphMenuItemCallback();

    public readonly struct GraphMenuItem
    {
        public string Path { get; }
        public GraphMenuItemCallback Callback { get; }

        public GraphMenuItem(string path, GraphMenuItemCallback callback)
        {
            Path = path;
            Callback = callback;
        }

        public GraphMenuItem(string path, GraphMenuItem item)
            : this(path, item.Callback)
        {
        }
    }
}

#endif