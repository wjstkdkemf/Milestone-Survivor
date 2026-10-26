using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageClearController : MonoBehaviour
{
    private Coroutine clearRoutine;
    private GameManager clearingGameManager;
    private bool ownsClearFlags;

    public void BeginClear()
    {
        if (!isActiveAndEnabled || clearRoutine != null)
          return;

        var manager = GameManager.Instance;
        if (manager == null)
            return;

        clearingGameManager = manager;
        ownsClearFlags = true;

        manager.AllKill = true;
        manager.Heal = true;

        clearRoutine = StartCoroutine(Clearing());
    }
    public void CancelAndReset()
    {
        if (clearRoutine != null)
        {
            StopCoroutine(clearRoutine);
            clearRoutine = null;
        }

        if (ownsClearFlags && clearingGameManager != null)
        {
            clearingGameManager.AllKill = false;
            clearingGameManager.Heal = false;
        }

        ownsClearFlags = false;
        clearingGameManager = null;
    }
    private void OnDisable()
    {
        CancelAndReset();
    }
    private IEnumerator Clearing()
    {
        PlayerXpPickup playerPickup = null;
        GameObject playerObj = GameObject.FindWithTag("Player");

        var retryDelay = new WaitForSeconds(0.5f);

        if (playerObj != null)
        {
            playerPickup = playerObj.GetComponent<PlayerXpPickup>();
        }

        yield return retryDelay;

        while (true)
        {
            if (playerPickup == null || !playerPickup.isActiveAndEnabled)
            {
                playerObj = GameObject.FindWithTag("Player");
                playerPickup = playerObj != null
                    ? playerObj.GetComponent<PlayerXpPickup>()
                    : null;
            }

            if (playerPickup != null &&
                playerPickup.isActiveAndEnabled &&
                playerPickup.CollectEverything())
            {
                break;
            }

            yield return retryDelay;
        }
        if (clearingGameManager != null)
            clearingGameManager.AllKill = false;

        yield return new WaitForSeconds(1f);

        clearRoutine = null;
        ownsClearFlags = false;
        clearingGameManager = null;

        if (GameOver.Instance != null)
            GameOver.Instance.stageClear(true);
    }
}
