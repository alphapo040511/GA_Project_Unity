using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;


[RequireComponent(typeof(BiomeMapGenerator))]
public class DijkstraMazeGenerator : MonoBehaviour
{
    public BiomeMapGenerator mapGenerator;

    [Header("Maze Settings")]
    public int width = 15;
    public int depth = 15;

    [Header("Color Settings")]
    public Color wallColor;
    public Color groundColor;
    public Color forestColor;
    public Color mudColor;

    int[,] map;

    Vector2Int start;
    Vector2Int goal;

    bool showed = false;
    List<Vector2Int> path = new List<Vector2Int>();

    // Start is called before the first frame update
    void Start()
    {
        mapGenerator = GetComponent<BiomeMapGenerator>();
        GenerateMaze();
    }

    public void ShowPath()
    {
        if (showed) return;

        StartCoroutine(ShowPath(path));
        showed = true;
    }

    public async void GenerateMaze()
    {
        StopAllCoroutines();
        showed = false;

        start = new Vector2Int(1, 1);
        goal = new Vector2Int(width - 2, depth - 2);
        map = await mapGenerator.GenerateRandomMap(width, depth, start, goal);

        if(map == null)
        {
            Debug.LogWarning("맵 생성 실패. 재시도 바람.");
            return;
        }

        ShowMap();

        path = new List<Vector2Int>();
        path =  await Dijkstra(map, start, goal);

        if (path == null)
            Debug.Log("경로 없음");
        else
        {
            Debug.Log($"경로 길이: {path.Count}");
            foreach (var p in path)
                Debug.Log(p);
        }
    }

    async Task<List<Vector2Int>> Dijkstra(int[,] map, Vector2Int start, Vector2Int goal)
    {
        await Task.Yield();   // 한 프레임 쉬기

        int w = map.GetLength(0);
        int h = map.GetLength(1);

        int[,] dist = new int[w, h];                        // 지금까지 온 최소 비용
        bool[,] visited = new bool[w, h];                   // 확정 여부
        Vector2Int?[,] parent = new Vector2Int?[w, h];      // 경로 복원용

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                dist[x, y] = int.MaxValue;
            }
        }

        dist[start.x, start.y] = 0;

        Vector2Int[] dirs =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        SimplePriorityQueue<Vector2Int> open = new SimplePriorityQueue<Vector2Int>();
        open.Enqueue(start, dist[start.x, start.y]);

        while (open.Count > 0)
        {
            Vector2Int cur = open.Dequeue();

            if (visited[cur.x, cur.y]) continue;        // 이미 방문(확정) 했다면 넘기기
            visited[cur.x, cur.y] = true;

            if (cur == goal)
                return ReconstructPaht(parent, start, goal);

            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;

                if (!InBounds(map, nx, ny)) continue;
                if (map[nx, ny] == 0) continue;         // 벽

                int moveCost = TileCost(map[nx, ny]);   // cur -> (nx, ny) 비용
                if (moveCost == int.MaxValue) continue;

                int newDist = dist[cur.x, cur.y] + moveCost;

                // 더 싼 길 발견
                if (newDist < dist[nx, ny])
                {
                    dist[nx, ny] = newDist;
                    parent[nx, ny] = cur;

                    if (!visited[nx, ny] && !open.Contains(new Vector2Int(nx, ny)))
                        open.Enqueue(new Vector2Int(nx, ny), dist[nx, ny]);
                }
            }
        }
        return null;
    }

    List<Vector2Int> ReconstructPaht(Vector2Int?[,] parent, Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int? cur = goal;

        while (cur.HasValue)
        {
            path.Add(cur.Value);
            if (cur.Value == start) break;
            cur = parent[cur.Value.x, cur.Value.y];
        }

        path.Reverse();
        return path;
    }

    void ShowMap()
    {
        ClearMap();

        for(int x = 0; x < map.GetLength(0); x++)
            for(int y = 0; y < map.GetLength(1); y++)
            {
                if (new Vector2Int(x, y) == start)
                {
                    CreateCube(x, y, new Vector3(1, 0.1f, 1), Color.blue);
                    continue;
                }
                else if (new Vector2Int(x, y) == start)
                {
                    CreateCube(x, y, new Vector3(1, 0.1f, 1), Color.red);
                    continue;
                }


                switch (map[x,y])
                {
                    case 1:
                        CreateCube(x, y, new Vector3(1, 0.1f, 1), groundColor);    // 땅 생성
                        break;
                    case 2:
                        CreateCube(x, y, new Vector3(1, 0.1f, 1), forestColor);    // 숲 생성
                        break;
                    case 3:
                        CreateCube(x, y, new Vector3(1, 0.1f, 1), mudColor);    // 진흙 생성
                        break;
                    default:
                        CreateCube(x, y, new Vector3(1, 1, 1), wallColor);    // 벽 생성
                        break;
                }
            }
    }

    IEnumerator ShowPath(List<Vector2Int> path)
    {
        for(int i = 0; i < path.Count; i++)
        {
            Vector2Int pos = path[i];
            if ( i == 0)
                CreateCube(pos.x, pos.y, Vector3.one * 0.5f, Color.blue);
            else if( i == path.Count - 1)
                CreateCube(pos.x, pos.y, Vector3.one * 0.5f, Color.red);
            else
                CreateCube(pos.x, pos.y, Vector3.one * 0.5f, Color.yellow);

            yield return new WaitForSeconds(0.1f);
        }
    }

    void CreateCube(int x, int y, Vector3 size, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(x, size.y / 2f, y);
        go.transform.localRotation = quaternion.identity;
        go.transform.localScale = size;
        go.GetComponent<MeshRenderer>().material.color = color;
        go.name = $"{x},{y}_block";
    }

    void ClearMap()
    {
        foreach(Transform c in transform)
        {
            Destroy(c.gameObject);
        }
    }

    int TileCost(int tile)
    {
        switch (tile)
        {
            case 1: return 1;   // 평지
            case 2: return 3;   // 숲
            case 3: return 5;   // 진흙
            default: return int.MaxValue;   // 0=벽 포함
        }
    }

    bool InBounds(int[,] map, int x, int y)
    {
        return x >= 0 && y >= 0 &&
               x < map.GetLength(0) &&
               y < map.GetLength(1);
    }
}
