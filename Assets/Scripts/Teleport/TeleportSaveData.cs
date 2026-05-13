
using System.Collections.Generic;
using UnityEngine;

// 이 클래스는 MonoBehaviour나 ScriptableObject가 아닙니다.
// 데이터를 담기 위한 순수 C# 클래스입니다.
[System.Serializable]
public class TeleportSaveData : ISerializationCallbackReceiver
{
    public int saveVersion;
    public string appVersion;

    // Key: TeleportData의 "pointID" (string)
    // Value: 잠금 해제 여부 (bool)
    public Dictionary<string, bool> unlockedPoints;

    // Unity의 JsonUtility는 Dictionary를 직접 (역)직렬화할 수 없으므로 리스트를 사용해 수동으로 처리합니다.
    [SerializeField] private List<string> unlockedPointKeys = new List<string>();
    [SerializeField] private List<bool> unlockedPointValues = new List<bool>();

    public TeleportSaveData()
    {
        unlockedPoints = new Dictionary<string, bool>();
    }

    public void OnBeforeSerialize()
    {
        unlockedPointKeys.Clear();
        unlockedPointValues.Clear();

        foreach (var pair in unlockedPoints)
        {
            unlockedPointKeys.Add(pair.Key);
            unlockedPointValues.Add(pair.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        unlockedPoints = new Dictionary<string, bool>();

        for (int i = 0; i < unlockedPointKeys.Count; i++)
        {
            // 키와 값의 수가 맞지 않는 경우에 대한 예외 처리
            if (i < unlockedPointValues.Count)
            {
                unlockedPoints[unlockedPointKeys[i]] = unlockedPointValues[i];
            }
        }
    }
}
