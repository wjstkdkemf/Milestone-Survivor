using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class InstantExplosion : MonoBehaviour
{
    
    [Header("시각 효과 설정")]
    [Tooltip("애니메이션이 끝나는 데 걸리는 시간 (초)")]
    public float lifetime = 1.0f;

    private void OnEnable()
    {
        Invoke(nameof(ReturnToPool), lifetime);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ReturnToPool));
    }

    public void ReturnToPool()
    {
        if (ObjectPoolingManager.Instance != null && gameObject.activeInHierarchy)
        {
            ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject);
        }
    }
}
