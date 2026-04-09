using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGridChunk : MonoBehaviour
{
    public Tilemap tilemap;
    
    // 자신이 글로벌 해시맵에 등록한 벽들의 절대 좌표를 기억해둡니다 (나중에 지우기 위해)
    private List<Vector2Int> registeredWallCells = new List<Vector2Int>();
    public float cellSize = 1f; // 타일 1칸의 실제 크기 (기본 1x1)

    // 매니저가 청크를 배치할 때 호출합니다.
    public void RegisterWallsToGlobalMap()
    {
        registeredWallCells.Clear();
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int localCell = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(localCell))
                {
                    // 1. 타일의 실제 월드 중심 좌표를 가져옵니다.
                    Vector3 worldPos = tilemap.GetCellCenterWorld(localCell);
                    
                    // 2. 월드 좌표를 '글로벌 셀 좌표'로 변환합니다.
                    Vector2Int globalCell = new Vector2Int(
                        Mathf.FloorToInt(worldPos.x / cellSize),
                        Mathf.FloorToInt(worldPos.y / cellSize)
                    );

                    // 3. 매니저의 글로벌 해시맵에 등록합니다. (1 = 벽)
                    InfiniteTilemapManager.Instance.globalWallMap.TryAdd(globalCell, 1);
                    
                    // 나중에 지우기 위해 명단에 적어둡니다.
                    registeredWallCells.Add(globalCell);
                }
            }
        }
    }

    // 매니저가 청크를 거둬들일 때 호출합니다.
    public void UnregisterWallsFromGlobalMap()
    {
        foreach (Vector2Int cell in registeredWallCells)
        {
            InfiniteTilemapManager.Instance.globalWallMap.Remove(cell);
        }
        registeredWallCells.Clear();
    }
}