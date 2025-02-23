using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Tilemaps;
using UniRand = UnityEngine.Random;

namespace Test
{
    public sealed class MapControl : MonoBehaviour
    {
        public static MapControl? Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public int MinWidth = 10;
        public int MinHeight = 10;
        public int MaxWidth = 100;
        public int MaxHeight = 100;

        public required Tilemap Tilemap;
        public required TileBase TileBase;

        private Map m_Map = null!;

        public void CreateMap(int width, int height)
        {
            m_Map = new Map(width, height, Tilemap, TileBase);
            m_Map.RandomFillMap();
        }

        public bool FindPath(Vector3Int start, Vector3Int end, out List<Vector3Int>? path)
        {
            return AStar(start, end, out path);
        }

        public ref TileInfo GetTileInfo(Vector3Int position)
        {
            return ref m_Map.GetTileInfo(position);
        }

        public IEnumerable<Vector3Int> GetNeighbors(Vector3Int position)
        {
            var neighbors = new List<Vector3Int>
            {
                position + Vector3Int.up,
                position + Vector3Int.down,
                position + Vector3Int.left,
                position + Vector3Int.right,
            }.Where(p => p.x >= 0 && p.x < m_Map.MaxWidth && p.y >= 0 && p.y < m_Map.MaxHeight);

            return neighbors;
        }

        public bool Dijkstra(Vector3Int start, Vector3Int end, out List<Vector3Int>? path)
        {
            var visited = new HashSet<Vector3Int>();
            var priorityQueue = new PriorityQueue<Vector3Int>();
            var parent = new Dictionary<Vector3Int, Vector3Int>();
            var cost = new Dictionary<Vector3Int, float>();

            priorityQueue.Enqueue(start, 0);
            cost[start] = 0;

            while (priorityQueue.Count > 0)
            {
                var current = priorityQueue.Dequeue();
                if (current == end)
                {
                    break;
                }

                visited.Add(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (visited.Contains(neighbor)) continue;
                    
                    ref var tileInfo = ref GetTileInfo(neighbor);
                    if (tileInfo.IsWalkable is false) continue;

                    var newCost = cost[current] + tileInfo.Cost;
                    if (cost.ContainsKey(neighbor) is false || newCost < cost[neighbor])
                    {
                        cost[neighbor] = newCost;
                        priorityQueue.Enqueue(neighbor, newCost);
                        parent[neighbor] = current;
                    }
                }
            }

            if (parent.ContainsKey(end) is false)
            {
                path = null;
                return false;
            }

            path = new();
            var currentPos = end;
            while (currentPos != start)
            {
                path.Add(currentPos);
                currentPos = parent[currentPos];
            }

            path.Reverse();
            return true;
        }

        public bool AStar(Vector3Int start, Vector3Int end, out List<Vector3Int>? path)
        {
            var visited = new HashSet<Vector3Int>();
            var priorityQueue = new PriorityQueue<Vector3Int>();
            var parent = new Dictionary<Vector3Int, Vector3Int>();
            var cost = new Dictionary<Vector3Int, float>();

            priorityQueue.Enqueue(start, 0);
            cost[start] = 0;

            while (priorityQueue.Count > 0)
            {
                var current = priorityQueue.Dequeue();
                if (current == end)
                {
                    break;
                }

                visited.Add(current);
                foreach (var neighbor in GetNeighbors(current))
                {
                    if (visited.Contains(neighbor)) continue;

                    parent[neighbor] = current;
                    ref var tileInfo = ref GetTileInfo(neighbor);
                    if (tileInfo.IsWalkable is false) continue;

                    var newCost = cost[current] + tileInfo.Cost;
                    if (cost.ContainsKey(neighbor) is false || newCost < cost[neighbor])
                    {
                        cost[neighbor] = newCost;
                        var priority = newCost + Vector3Int.Distance(neighbor, end);
                        priorityQueue.Enqueue(neighbor, priority);
                        parent[neighbor] = current;
                    }
                }
            }

            if (parent.ContainsKey(end) is false)
            {
                path = null;
                return false;
            }

            path = new();
            var currentPos = end;
            while (currentPos != start)
            {
                path.Add(currentPos);
                currentPos = parent[currentPos];
            }

            path.Reverse();
            return true;
        }
    }

    public class PriorityQueue<T>
    {
        private readonly SortedSet<(float priority, T item)> m_Queue = new(new PriorityComparer());
        public int Count => m_Queue.Count;

        public void Enqueue(T item, float priority)
        {
            m_Queue.Add((priority, item));
        }

        public T Dequeue()
        {
            var (priority, item) = m_Queue.Min;
            m_Queue.Remove((priority, item));
            return item;
        }

        private class PriorityComparer : IComparer<(float priority, T item)>
        {
            public int Compare((float priority, T item) x, (float priority, T item) y)
            {
                var result = x.priority.CompareTo(y.priority);
                if (result == 0)
                {
                    result = x.item!.GetHashCode().CompareTo(y.item!.GetHashCode());
                }
                return result;
            }
        }

        public class Enumerator : IEnumerator<T>
        {
            private readonly IEnumerator<(float priority, T item)> m_Enumerator;

            public Enumerator(IEnumerator<(float priority, T item)> enumerator)
            {
                m_Enumerator = enumerator;
            }

            public T Current => m_Enumerator.Current.item;

            public bool MoveNext()
            {
                return m_Enumerator.MoveNext();
            }

            public void Reset()
            {
                m_Enumerator.Reset();
            }

            public void Dispose()
            {
                m_Enumerator.Dispose();
            }

            object System.Collections.IEnumerator.Current => Current!;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(m_Queue.GetEnumerator());
        }
    }

    // define Map class
    public sealed class Map
    {
        public int MaxWidth { get; init; }
        public int MaxHeight { get; init; }

        private Tilemap m_Tilemap;
        private TileBase m_TileBase;

        private Dictionary<Vector3Int, TileInfo> m_TileInfos = new();

        public Map(int maxWidth, int maxHeight, Tilemap tilemap, TileBase tileBase)
        {
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            m_Tilemap = tilemap;
            m_TileBase = tileBase;
        }

        public void RandomFillMap()
        {
            for (var x = 0; x < MaxWidth; x++)
            {
                for (var y = 0; y < MaxHeight; y++)
                {
                    var position = new Vector3Int(x, y, 0);
                    var tileInfo = new TileInfo(x, y, TileTypeExt.GetRandom());
                    m_TileInfos[position] = tileInfo;

                    var tile = ScriptableObject.CreateInstance<InteractiveTile>();
                    m_Tilemap.SetTile(position, tile);
                }
            }
        }

        public ref TileInfo GetTileInfo(Vector3Int position)
        {
            return ref CollectionsMarshal.GetValueRefOrNullRef(m_TileInfos, position);
        }
    }

    public enum TileType
    {
        // Walkable with 1 cost
        Normal,
        // Not walkable
        Wall,
        // Walkable with 2 cost
        Rugged,
    }

    public static class TileTypeExt
    {
        public static TileType GetRandom()
        {
            var num = UniRand.Range(0, 100);
            return num switch
            {
                <= 30 => TileType.Normal,
                <= 80 => TileType.Rugged,
                _ => TileType.Wall,
            };
        }
    }

    // tile
    public struct TileInfo
    {
        public int X { get; init; }
        public int Y { get; init; }
        public TileType Type { get; set; }
        public int Cost { get; private set; }
        public readonly bool IsWalkable => Type != TileType.Wall;

        public TileInfo(int x, int y, TileType type)
        {
            X = x;
            Y = y;
            SetType(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetType(TileType type)
        {
            Type = type;
            Cost = type switch
            {
                TileType.Normal => 1,
                TileType.Wall => -1,
                TileType.Rugged => 2,
                _ => 1
            };
        }
    }

    public class InteractiveTile : Tile
    {
        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
            base.StartUp(position, tilemap, go);
            if (go != null)
            {
                var collider = go.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                go.AddComponent<TileInteraction>();
                return true;
            }
            return false;
        }
    }

    public class TileInteraction : MonoBehaviour
    {
        private void OnMouseDown()
        {
            Debug.Log("Tile clicked: " + transform.position);
        }
    }
}