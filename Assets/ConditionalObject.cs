using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalObject : MonoBehaviour
{
    public List<string> CheckProgressID;
    public Sprite ChangeSprite;
    public Sprite BeforeSprite;
    private bool Change = true;

    // Start is called before the first frame update
    void Start()
    {
        CheckProgess();
    }
    public void CheckProgess()
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
            gameObject.GetComponent<SpriteRenderer>().sprite = ChangeSprite;
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = BeforeSprite;
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
        }
    }
}
