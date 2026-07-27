using InventorySystem;
using UnityEngine;

public class InventoryEquipmentSkillSync : MonoBehaviour
{
    private void OnDisable()
    {
        if (!Application.isPlaying)
            return;

        if (InventoryController.instance != null)
        {
            InventoryController.instance
                .ReapplyAllEquipmentSkills();
        }
    }
}