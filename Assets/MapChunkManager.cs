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
        // 비동기로 NavMesh 굽기 시작
        var asyncOp = surface.BuildNavMeshAsync();

        // 굽기가 완료될 때까지 대기
        yield return asyncOp; 

        // 물리 엔진과 NavMesh가 동기화되도록 프레임 끝까지 대기 (중요!)
        yield return new WaitForEndOfFrame();

        // 모든 몬스터를 찾아 '새로고침' 명령 내리기
        RefreshAllEnemies();
    }

    void RefreshAllEnemies()
    {
        WaveSpawner.Instance.RefreshAllEnemies();
    }
}
