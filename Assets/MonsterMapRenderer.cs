using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(LineRenderer))]
public class MonsterMapRenderer : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private LineRenderer lineRenderer;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        lineRenderer = GetComponent<LineRenderer>();

        SetupLine();
    }

    void SetupLine()
    {
        // BoxCollider2D의 로컬 크기와 오프셋을 기반으로 꼭짓점 계산
        Vector2 size = boxCollider.size / 2; // 중심에서의 거리이므로 절반 크기 사용
        Vector2 offset = boxCollider.offset;

        Vector3[] positions = new Vector3[5];
        positions[0] = new Vector3(-size.x, -size.y, 0) + (Vector3)offset; // 좌하단
        positions[1] = new Vector3(size.x, -size.y, 0) + (Vector3)offset; // 우하단
        positions[2] = new Vector3(size.x, size.y, 0) + (Vector3)offset; // 우상단
        positions[3] = new Vector3(-size.x, size.y, 0) + (Vector3)offset; // 좌상단
        positions[4] = positions[0]; // 루프를 닫기 위해 첫 점으로 돌아옴

        lineRenderer.positionCount = 5;
        lineRenderer.SetPositions(positions);
    }
}
