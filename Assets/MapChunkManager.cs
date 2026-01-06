using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using NavMeshPlus.Components;

public class MapChunkManager : MonoBehaviour
{
    // 인스펙터에서 NavMeshPlus의 Surface 컴포넌트를 연결해주세요.
    public NavMeshSurface surface; 

    // 맵이 변경되었을 때 호출되는 함수
    public void OnMapChunkUpdated()
    {
        StartCoroutine(RebakeNavMeshSequence());
    }

    IEnumerator RebakeNavMeshSequence()
    {
        // 1. 비동기로 NavMesh 굽기 시작
        var asyncOp = surface.BuildNavMeshAsync();

        // 2. 굽기가 완료될 때까지 대기
        yield return asyncOp; 

        // 3. 물리 엔진과 NavMesh가 동기화되도록 프레임 끝까지 대기 (중요!)
        yield return new WaitForEndOfFrame();

        // 4. [핵심] 모든 몬스터를 찾아 '새로고침' 명령 내리기
        RefreshAllEnemies();
    }

    void RefreshAllEnemies()
    {
        // 현재 씬에 있는 모든 몬스터를 찾습니다.
        // (최적화를 위해 ObjectPoolingManager에서 활성화된 몬스터 리스트를 가져오는 게 더 좋지만,
        // 일단은 FindObjectsOfType으로 해결 가능합니다.)
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (var enemy in enemies)
        {
            if (enemy.gameObject.activeSelf)
            {
                enemy.OnNavMeshUpdated();
            }
        }
    }
}
