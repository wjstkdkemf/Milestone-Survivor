using System.Collections;

// 모든 씬 초기화 스크립트는 이 인터페이스를 구현해야 합니다.
public interface ISceneInitializer
{
    // 초기화 작업을 수행하는 코루틴
    IEnumerator Initialize();
}