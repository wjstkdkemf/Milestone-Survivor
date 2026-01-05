using UnityEngine;
using NavMeshPlus.Components;

public class MapChunkManager : MonoBehaviour
{
    // 인스펙터에서 NavMeshPlus의 Surface 컴포넌트를 연결해주세요.
    public NavMeshSurface surface; 

    // 플레이어가 이동해서 청크가 새로 생성되고 삭제된 '직후'에 이 함수를 호출하세요.
    public void OnMapChunkUpdated()
    {
        // 비동기(Async)로 빌드하여 렉(프레임 드랍)을 방지하는 것이 핵심입니다.
        surface.BuildNavMeshAsync();
    }
}
