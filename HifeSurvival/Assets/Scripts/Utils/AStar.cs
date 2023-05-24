using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Heap;

public class AStar
{
    public struct Path : IComparable<Path>
    {
        public Vector3Int coord;
        public int f;
        public int g;

        public static bool operator <(Path a, Path b)
            => a.f < b.f;

        public static bool operator >(Path a, Path b)
            => a.f > b.f;

        public static bool operator <=(Path a, Path b)
            => a.f <= b.f;

        public static bool operator >=(Path a, Path b)
            => a.f >= b.f;

        public static int GetH(Vector3Int a, Vector3Int b)
            => 10 * ((int)Mathf.Abs(b.y - a.y) + (int)Mathf.Abs(b.x - a.x));

        public int CompareTo(Path other) 
            => f.CompareTo(other.f);

    }


    Vector2[] dirArr = new Vector2[]
    {
            new Vector2(0, 1),  // UP
            new Vector2(-1, 0), // LEFT
            new Vector2(0, -1), // DOWN
            new Vector2(1,  0), // RIGHT
            new Vector2(-1, 1), // UP_LEFT
            new Vector2(-1,-1), // DOWN_LEFT
            new Vector2(1, -1), // DOWN_RIGHT
            new Vector2(1,  1)  // UP_RIGHT
    };


    int[] cost = new int[]
    {
            10,
            10,
            10,
            10,
            14,
            14,
            14,
            14,
    };


    private Dictionary<Vector3Int, int> best = new Dictionary<Vector3Int, int>();

    private Dictionary<Vector3Int, Vector3Int> parent = new Dictionary<Vector3Int, Vector3Int>();

    private PriorityQueue<Path> openList = new PriorityQueue<Path>();

    private HashSet<Vector3Int> closedList = new HashSet<Vector3Int>();

    private Func<Vector3Int, bool> canGoFunc = null;


    public AStar(Func<Vector3Int, bool> inCanGoFunc)
    {
        canGoFunc = inCanGoFunc;
    }

    public List<Vector3Int> Run(Vector3Int inStart, Vector3Int inEnd)
    {
        best.Clear();
        parent.Clear();
        openList.Clear();
        closedList.Clear();

        // 현 위치 출발점 세팅
        openList.Enqueue(new Path
        {
            coord = inStart,
            f = Path.GetH(inEnd, inStart),
        });

        best[inStart] = Path.GetH(inEnd, inStart);
        parent[inStart] = inStart;

        while (openList.Count > 0)
        {
            var path = openList.Dequeue();

            if (best.ContainsKey(path.coord) && best[path.coord] < path.f)
                continue;

            if (closedList.Contains(path.coord) == false)
                closedList.Add(path.coord);

            if (path.coord == inEnd)
                break;

            for (int i = 0; i < dirArr.Length; i++)
            {
                Vector3Int nextCoord = new Vector3Int()
                {
                    x = (int)(path.coord.x + dirArr[i].x),
                    y = (int)(path.coord.y + dirArr[i].y),
                };    

                if (canGoFunc?.Invoke(nextCoord) == false)
                    continue;

                if (closedList.Contains(nextCoord))
                    continue;

                int G = path.g + cost[i];
                int H = Path.GetH(inEnd, nextCoord);

                if (best.ContainsKey(nextCoord) && best[nextCoord] <= G + H)
                    continue;

                if (best.ContainsKey(nextCoord) == true)
                    best[nextCoord] = G + H;
                else
                    best.Add(nextCoord, G + H);

                openList.Enqueue(new Path
                {
                    coord = nextCoord,
                    f = G + H,
                    g = G
                });

                if (parent.ContainsKey(nextCoord) == true)
                    parent[nextCoord] = path.coord;
                else
                    parent.Add(nextCoord, path.coord);
            }
        }

        List<Vector3Int> result = new List<Vector3Int>();
        Vector3Int pos = inEnd;

        while (true)
        {
            result.Add(pos);

            if (parent.ContainsKey(pos) == false || pos == parent[pos])
                break;

            pos = parent[pos];
        }

        result.Reverse();

        return result;
    }

}
