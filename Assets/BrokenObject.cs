using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenObject : ConditionalBase
{
    public List<string> CheckProgressID;
    private bool Change = true;

    // Start is called before the first frame update
    void Start()
    {
        CheckProgess();
    }
    public override void CheckProgess()
    {
        for (int i = CheckProgressID.Count - 1; i >= 0; i--)//역순체크 -> 추후 추가되는 업적이 더 상위 업적일것으로 예상.
        {
            string link = CheckProgressID[i];

            if (!GameProgressManager.Instance.IsUnlocked(link))
            {
                Change = false;
                break;
            }
            else
            {
                Change = true;
            }
        }

        if(Change)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}
