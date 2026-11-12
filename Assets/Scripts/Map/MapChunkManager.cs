using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using NavMeshPlus.Components;
using Unity.VisualScripting;

public class MapChunkManager : MonoBehaviour
{
    // 인스펙터에서 NavMeshPlus의 Surface 컴포넌트를 연결해주세요.
    public enum NavigationState { Idle, Updating, Ready, Failed }
    public NavigationState State { get; private set; }
    public NavMeshSurface surface;
    private bool isUpdating = false;
    private bool updateRequested = false;
    private int mapVersion;
    private NavMeshData updatingData;

    public void InvalidateMap()
    {
        ++mapVersion;
        State = NavigationState.Idle;
        updateRequested = false;
    }

    // 맵이 변경되었을 때 호출되는 함수
    public void OnMapChunkUpdated()
    {
        if (!isActiveAndEnabled || surface == null || !surface.isActiveAndEnabled)
        {
            State = NavigationState.Failed;
            return;
        }
        State = NavigationState.Updating;

        if(isUpdating)
        {
            updateRequested = true;
            return;
        }

        StartCoroutine(RebakeNavMeshSequence());
    }

    private IEnumerator RebakeNavMeshSequence()
    {
        int version = mapVersion;
        isUpdating = true;
        updateRequested = false;
        bool completed = false;
        try
        {
            var targetSurface = surface;
            updatingData = targetSurface.navMeshData;

            AsyncOperation operation;
            if (updatingData == null)
            {
                operation = targetSurface.BuildNavMeshAsync();
                updatingData = targetSurface.navMeshData;
            }
            else
            {
                operation = targetSurface.UpdateNavMesh(updatingData);
            }

            if (operation == null)
            {
                Debug.LogError("[NavMesh] Update could not start.", this);
                yield break;
            }

            yield return operation;
            yield return null;

            if (version != mapVersion || updateRequested)
                yield break;

            completed = true;
            State = NavigationState.Ready;

            if (WaveSpawner.Instance != null)
                WaveSpawner.Instance.RefreshAllEnemies();
        }
        finally
        {
            updatingData = null;
            if (version == mapVersion && !updateRequested && !completed)
                State = NavigationState.Failed;

            isUpdating = false;

            if (updateRequested && isActiveAndEnabled)
            {
                updateRequested = false;
                OnMapChunkUpdated();
            }
        }
    }
    private void OnDisable() => CancelNavigationUpdate();
    private void OnDestroy() => CancelNavigationUpdate();
    private void CancelNavigationUpdate()
    {
        ++mapVersion;
        updateRequested = false;
        State = NavigationState.Failed;

        NavMeshData data = updatingData;
        StopAllCoroutines();

        if (data != null)
            UnityEngine.AI.NavMeshBuilder.Cancel(data);

        updatingData = null;
        isUpdating = false;
    }
}
