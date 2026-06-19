using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeCircleIndicator : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int segments = 96;
    [SerializeField] private Color color = new Color(1f, 0.35f, 0.1f, 0.35f);
    [SerializeField] private float width = 0.04f;

    public void SetRadius(float radius)
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        lineRenderer.useWorldSpace = false;
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 50;

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            lineRenderer.SetPosition(i, pos);
        }
    }
}
