using UnityEngine;

// 🚨 범인 색출용 스파이 스크립트 (원인 파악 후 삭제하세요!)
public class PhysicsSpy : MonoBehaviour
{
    // 유니티 엔진이 '단단한 물리 충돌(SolveDiscrete)'을 할 때만 무조건 호출되는 함수입니다.
    // (트리거 통과일 때는 절대 호출되지 않습니다!)
    private void OnCollisionStay2D(Collision2D collision)
    {
        // 렉이 걸리는 순간 콘솔창을 붉게 물들이며 범인의 이름을 토해낼 것입니다.
        Debug.LogError($"[물리 렉 진범 체포!] '{gameObject.name}'가 '{collision.gameObject.name}'(Layer: {LayerMask.LayerToName(collision.gameObject.layer)})와 단단하게 부딪히고 있습니다!!");
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log($"[유령 렉 진범 체포!] '{gameObject.name}'의 트리거가 '{other.gameObject.name}'(Layer: {LayerMask.LayerToName(other.gameObject.layer)})와 겹치며 장부를 쓰고 있습니다!!");
    }
}