using UnityEngine;
using Cinemachine;

public class ArenaManager : MonoBehaviour
{
    public static ArenaManager Instance;

    [Header("카메라 세팅")]
    public CinemachineVirtualCamera vcam;
    public PolygonCollider2D explorationBounds;
    private PolygonCollider2D combatBounds;
    private CinemachineConfiner confiner;

    void Start()
    {
        // 1. 카메라에서 컨파이너 부품을 찾아옵니다. (버전에 따라 CinemachineConfiner2D 일 수 있음)
        confiner = vcam.GetComponent<CinemachineConfiner>();

        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void ChangeMap(PolygonCollider2D spawnedArenaBounds)
    {
        explorationBounds = spawnedArenaBounds;
        confiner.m_BoundingShape2D = explorationBounds;
        
        confiner.InvalidatePathCache(); 
    }

    public void ExitMap()
    {
        confiner.InvalidatePathCache(); 
    }

    public void StartCombat(PolygonCollider2D spawnedArenaBounds)
    {
        combatBounds = spawnedArenaBounds;
        confiner.m_BoundingShape2D = combatBounds;
        
        confiner.InvalidatePathCache(); 
    }

    public void EndCombat()
    {
        confiner.m_BoundingShape2D = explorationBounds;
        
        confiner.InvalidatePathCache(); 
    }
}