using UnityEngine;


[CreateAssetMenu]
public class WeaponMotionData : ScriptableObject
{
    public AnimationCurve positionX;
    public AnimationCurve positionY;
    public AnimationCurve rotationZ;
    public AnimationCurve scale;

    public float rotationMultiplier = 1f;
    public float duration = 0.25f;
    public float blendInTime = 0.07f;
    public float blendOutTime = 0.06f;
    public bool canInterrupt = true;
    public int priority = 0;
}