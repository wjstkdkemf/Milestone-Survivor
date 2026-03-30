using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverPuzzleManager : MonoBehaviour
{
    public List<PuzzleLever> allLevers; // 맵에 배치된 레버 5개
    public GameObject door;
    public string ProgressID;

    // 레버가 당겨졌을 때 호출됨 (당긴 놈 인덱스, 영향받을 놈들 리스트)
    public void OnLeverPulled(int subjectIndex, List<int> targets)
    {
        // 1. 당겨진 레버 본인은 무조건 상태 변경
        ToggleLever(subjectIndex);

        // 2. 레버가 가지고 있던 '영향력 리스트'에 있는 애들도 상태 변경
        foreach (int targetIndex in targets)
        {
            // 유효한 번호인지 확인 (0 ~ 4)
            if (targetIndex >= 0 && targetIndex < allLevers.Count)
            {
                // 자기 자신은 위에서 이미 바꿨으니 제외 (중복 방지)
                if (targetIndex != subjectIndex)
                {
                    ToggleLever(targetIndex);
                }
            }
        }

        // 3. 퍼즐이 풀렸는지 확인
        CheckPuzzleSolved();
    }

    void ToggleLever(int index)
    {
        allLevers[index].Toggle();
    }

    void CheckPuzzleSolved()
    {
        bool allOn = true;
        foreach (var lever in allLevers)
        {
            if (!lever.IsOn)
            {
                allOn = false;
                GameProgressManager.Instance.Dislock(ProgressID);
                break;
            }
        }

        if (allOn)
        {
            Debug.Log("문이 열렸습니다!");
            GameProgressManager.Instance.Unlock(ProgressID);
            //door.SetActive(false);
            // 성공 사운드 재생
        }
    }
    public void Reset()
    {
        if(!GameProgressManager.Instance.IsUnlocked(ProgressID))//이미 다 푼상태라면 리셋 X
        {
            foreach (var lever in allLevers)
            {
                lever.CheakInit();
            }
        }
    }
}
