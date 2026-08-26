using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public float cellSize = 1.5f;

    private Dictionary<Vector2Int, List<Enemy>> grid = new();
    private HashSet<Enemy> registeredEnemies = new();

    void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
        if (e == null || registeredEnemies.Contains(e))
            return;

        Vector2Int cell = GetCell(e.transform.position);

        if (!grid.TryGetValue(cell, out var list))
        {
            list = new List<Enemy>();
            grid[cell] = list;
        }

        list.Add(e);
        registeredEnemies.Add(e);
        e.currentCell = cell;
    }

    public void UpdateCell(Enemy e)
    {
        if (e == null)
            return;

        if (!registeredEnemies.Contains(e))
        {
            Register(e);
            return;
        }

        Vector2Int newCell = GetCell(e.transform.position);

        if (newCell == e.currentCell) return;

        if (grid.TryGetValue(e.currentCell, out var oldList))
        {
            oldList.Remove(e);
            if (oldList.Count == 0)
                grid.Remove(e.currentCell);
        }

        if (!grid.TryGetValue(newCell, out var list))
        {
            list = new List<Enemy>();
            grid[newCell] = list;
        }

        if (!list.Contains(e))
            list.Add(e);

        e.currentCell = newCell;
    }

    public void Unregister(Enemy e)
    {
        if (e == null || !registeredEnemies.Remove(e))
            return;

        if (grid.TryGetValue(e.currentCell, out var list))
        {
            list.Remove(e);
            if (list.Count == 0)
                grid.Remove(e.currentCell);
        }
    }

    public void GetNearby(Vector3 pos, List<Enemy> result)
    {
        if (result == null)
            return;

        result.Clear();

        Vector2Int center = GetCell(pos);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int cell = center + new Vector2Int(x, y);

                if (!grid.TryGetValue(cell, out var list))
                    continue;

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    Enemy enemy = list[i];
                    if (enemy == null)
                    {
                        list.RemoveAt(i);
                        continue;
                    }

                    result.Add(enemy);
                }

                if (list.Count == 0)
                    grid.Remove(cell);
            }
        }
    }
}
