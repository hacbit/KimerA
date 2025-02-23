using System.Linq;
using UnityEngine;

namespace Test
{
    public sealed class MapTest : MonoBehaviour
    {
        public void Start()
        {
            MapControl.Instance?.CreateMap(10, 10);

            var start = new Vector3Int(2, 2, 0);
            var end = new Vector3Int(6, 6, 0);

            ref var tile = ref MapControl.Instance!.GetTileInfo(start);
            tile.SetType(TileType.Normal);

            tile = ref MapControl.Instance.GetTileInfo(end);
            tile.SetType(TileType.Normal);

            // debug map
            var line = string.Empty;
            for (var y = 0; y < 10; y++)
            {
                for (var x = 0; x < 10; x++)
                {
                    var tileInfo = MapControl.Instance?.GetTileInfo(new Vector3Int(x, y, 0));
                    line += tileInfo?.Type switch
                    {
                        TileType.Wall => "E",
                        TileType.Normal => "N",
                        TileType.Rugged => "S",
                        _ => "?",
                    };
                }
                line += "\n";
            }
            Debug.Log(line);

            if (MapControl.Instance?.FindPath(start, end, out var path) is true)
            {
                var p = $"({start.x}, {start.y})";
                foreach (var point in path!)
                {
                    p += $" -> ({point.x}, {point.y})";
                }
                Debug.Log(p);

                var last = start;
                var solution = string.Empty;
                foreach (var point in path)
                {
                    if (point.x > last.x)
                    {
                        solution += "R";
                    }
                    else if (point.x < last.x)
                    {
                        solution += "L";
                    }
                    else if (point.y > last.y)
                    {
                        solution += "U";
                    }
                    else if (point.y < last.y)
                    {
                        solution += "D";
                    }
                    last = point;
                }
                Debug.Log(solution);
            }
            else
            {
                Debug.Log("No path found");
            }
        }
    }
}