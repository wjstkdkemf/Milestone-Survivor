using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public float cellSize = 1.5f;

    private Dictionary<Vector2Int, List<Enemy>> grid = new();

    void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    Vector2Int GetCell(Vector3 pos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(pos.x / cellSize),
            Mathf.FloorToInt(pos.y / cellSize)
        );
    }

    public void Register(Enemy e)
    {
        Vector2Int cell = GetCell(e.transform.position);

        if (!grid.TryGetValue(cell, out var list))
        {
            list = new List<Enemy>();
            grid[cell] = list;
        }

        list.Add(e);
        e.currentCell = cell;
    }

    public void UpdateCell(Enemy e)
    {
        Vector2Int newCell = GetCell(e.transform.position);

        if (newCell == e.currentCell) return;

        grid[e.currentCell].Remove(e);

        if (!grid.TryGetValue(newCell, out var list))
        {
            list = new List<Enemy>();
            grid[newCell] = list;
        }

        list.Add(e);
        e.currentCell = newCell;
    }

    public void Unregister(Enemy e)
    {
        if (grid.TryGetValue(e.currentCell, out var list))
            list.Remove(e);
    }

    public void GetNearby(Vector3 pos, List<Enemy> result)
    {
        result.Clear();

        Vector2Int center = GetCell(pos);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int cell = center + new Vector2Int(x, y);

                if (grid.TryGetValue(cell, out var list))
                    result.AddRange(list);
            }
        }
    }
}