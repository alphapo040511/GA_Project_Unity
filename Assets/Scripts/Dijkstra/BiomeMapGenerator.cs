using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class BiomeMapGenerator : MonoBehaviour
{
    public TileWeighted tileWeigth;
    public int waitCount = 1000;

    int[,] map;

    bool[,] visited;    // 방문기록
    protected Vector2Int[] dirs =
{
        new Vector2Int(0,1),
        new Vector2Int(0,-1),
        new Vector2Int(1,0),
        new Vector2Int(-1,0)
    };                                  // 탐색 순서

    public async Task<int[,]> GenerateRandomMap(int width, int depth, Vector2Int start, Vector2Int goal)
    {
        map = new int[width, depth];
        int tryCount = 0;
        while (tryCount < 100000)
        {
            tryCount++;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < depth; y++)
                {
                    if (x == 0 || y == 0 || x == width - 1 || y == depth - 1)
                        map[x, y] = 0;          // 벽 위치면 벽 생성
                    else if (new Vector2Int(x, y) == start || new Vector2Int(width, depth) == goal)
                        map[x, y] = 1;  // 시작이나 끝 위치면 일반 바닥 생성
                    else
                        map[x, y] = tileWeigth.GetRandomTile();         // 일반 좌표의 경우 가중치 따라 생성
                }

            visited = new bool[width, depth];
            bool ok = SearchMaze(start.x, start.y, goal);
            if (ok)
            {
                // 탈출 가능
                return map;
            }

            if(tryCount % waitCount == 0)
            {
                Debug.Log($"{tryCount}회 만큼 시도중..");
                await Task.Yield();   // 한 프레임 쉬기
            }
        }

        Debug.LogWarning("100000 시도 했는데 안생겨요 ㅠㅠ");
        return null;
    }

    bool SearchMaze(int x, int y, Vector2Int goal)
    {
        // 범위/벽/재방문 체크
        if (x < 0 || y < 0 || x >= map.GetLength(0) || y >= map.GetLength(1)) return false;
        if (map[x, y] == 0 || visited[x, y]) return false;

        // 방문 표시
        visited[x, y] = true;

        // 목표 도달
        if (x == goal.x && y == goal.y) return true;

        // 4방향 재귀 탐색
        foreach (var d in dirs)
            if (SearchMaze(x + d.x, y + d.y, goal)) return true;

        return false;
    }

    bool IsBounded(int[,] map, int x, int y)
    {
        return x >= 0 && y >= 0 &&
            x < map.GetLength(0) &&
            y < map.GetLength(1);
    }
}

[System.Serializable]
public class TileWeighted
{
    public int wall = 3;
    public int ground = 10;
    public int forest = 5;
    public int mud = 4;

    [Header("일반 타일에 적이 등장 할 확률")]
    public float enemyRate = 0;


    public int GetRandomTile()
    {
        int total = wall + ground + forest + mud;

        int ran = Random.Range(0, total);

        if (ran < ground)
        {
            if (Random.value < enemyRate)
                return 4;                           // 4 = 적이 있는 바닥 타일
            else
                return 1;
        }
        else if (ran < ground + forest)
            return 2;
        else if (ran < ground + forest + mud)
            return 3;
        else
            return 0;                       // 항상 예외처리는 벽으로
    }
}

