using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventorySystem
{
    //Author Jaxon Schauer
    /// <summary>
    /// This class creates a slot gameObject that displays an image of the item when notified by the assigned inventory
    /// </summary>
    public class Slot : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("슬롯 타입을 지정합니다. (예: Helmet, Armor, Inventory)")]
        public string slotType = "Inventory";

        [SerializeField]
        private int position;//The position if the inventories items list
        [SerializeField]
        private GameObject slotChildPrefab;//This holds the prefab for the image allowing it to be instantiated when the child object is dragged to a new location
        [SerializeField]
        private GameObject SlotItemHolder;//This is a child object that is used to display an image of the object

        private InventoryItem item;//This is the current item in the inventory, there is always an item however item.GetIsNull() determines if the object contains a real item
        private UnityEngine.Color color;//This is the color of the slot
        private Image slotImage;//This is the image of the slot
        private InventoryUIManager inventoryUIManager;
        private Vector3 initialChildScale;//holds the scale for the slot child to allow for it to be instantiated with the correct size
        private Vector3 initialSlotChildPosition;//This holds the position of the slot child so it can be instantiated with the correct location
        private float textSize;
        private Vector2 SlotItemHolderSize;
        private bool returnOnMiss = false;//checks whether or not item should return to inventory when the user misses
        public System.Action OnSlotClickedForTutorial;



        /// <summary>
        /// Sets essential variables for the inventory slot
        /// </summary>
        private void Awake()
        {
            slotImage = GetComponent<Image>();
            color = slotImage.color;

            inventoryUIManager = transform.parent.GetComponent<InventoryUIManager>();

            initialChildScale = SlotItemHolder.transform.localScale;


        }
        /// <summary>
        /// Initializes slot child, calling <see cref="UpdateSlot"/>
        /// </summary>
        private void Start()
        {
            SlotItemHolder.SetActive(true);

            item = inventoryUIManager.GetInventoryItem(position);
            initialSlotChildPosition = SlotItemHolder.transform.position;

            UpdateSlot();

        }
        /// <summary>
        /// Updates the slot to display the item in the slots associated position
        /// </summary>
        public void UpdateSlot()
        {
            item = inventoryUIManager.GetInventoryItem(position);
            if (item != null)
            {
                if (!item.GetIsNull())
                {
                    DragItem dragItem = SlotItemHolder.GetComponent<DragItem>();
                    dragItem.SetItem(item);
                    dragItem.SetText();
                    dragItem.SetEquitText();
                    dragItem.SetEnchantText();

                    // Find and assign the ScrollRect from parents
                    ScrollRect parentScrollRect = GetComponentInParent<ScrollRect>();
                    if (parentScrollRect != null)
                    {
                        dragItem.scrollRect = parentScrollRect;
                    }

                    SlotItemHolder.GetComponent<Image>().sprite = item.GetItemImage();
                    SlotItemHolder.SetActive(true);
                    SlotItemHolder.GetComponent<RectTransform>().localPosition = Vector3.zero;
                }
                else
                {
                    SlotItemHolder.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("Item is null");
            }

        }
        /// <summary>
        /// Adds a new slotchild when slot child is dragged away, and resets the slot to empty
        /// </summary>
        public void ResetSlot()
        {
            GameObject newInstance = Instantiate(slotChildPrefab, initialSlotChildPosition, Quaternion.identity);
            newInstance.transform.SetParent(transform);
            newInstance.transform.localScale = initialChildScale;
            Vector2 prevTextPos = SlotItemHolder.GetComponent<DragItem>().GetTextPosition();

            SlotItemHolder = newInstance;
            DragItem slotDragItem= SlotItemHolder.GetComponent<DragItem>();
            slotDragItem.Initiailize();
            slotDragItem.SetTextPosition(prevTextPos);
            inventoryUIManager.GetInventory().EraseItemInPosition(position);
            SetChildImageSize(SlotItemHolderSize);
            SetTextSize(textSize);
            slotDragItem.SetReturnOnMiss(returnOnMiss);
            SlotItemHolder.SetActive(false);
        }
        /// <summary>
        /// Adds a new slotchild when slot child is dragged away, and resets the slot to empty
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // --- 이벤트 발생 코드 추가 ---
            // 아이템이 null이 아닐 때만 이벤트를 발생시킵니다.
            //if (item != null && !item.GetIsNull())
            //{
                //InventoryEventSystem.RaiseSlotClicked(item, slotType);
            //}
            // --------------------------
            OnSlotClickedForTutorial?.Invoke();
            InventoryEventSystem.RaiseSlotClicked(item, slotType);
            // 더블클릭 감지
            if (!Application.isMobilePlatform &&
                eventData.clickCount == 2)
            {
                if (InventoryController.instance == null) return;

                // 현재 슬롯의 아이템 가져오기
                InventoryItem currentItem = GetItem();

                // 아이템이 없거나, 장착 불가능한 슬롯(예: 장비창)에서 더블클릭한 경우 무시
                if (currentItem.GetIsNull())
                {
                    return;
                }

                // HotBar에 있는 아이템을 더블클릭하면 장착 해제
                if (inventoryUIManager.GetInventoryName() == InventoryController.HotBarInventoryName)
                {
                    InventoryController.instance.UnequipItemFromHotbar(currentItem);
                }
                else if(currentItem.GetEquit())
                {
                    InventoryController.instance.UnequipItemFromInventory(currentItem);
                }
                else// 그 외 인벤토리 아이템은 장착
                {
                    InventoryController.instance.EquipItem(currentItem, eventData.position);
                }
                UpdateSlot();
            }
            else // 싱글클릭
            {
                inventoryUIManager.SetPressed(gameObject);
                inventoryUIManager.MoveOnPress(gameObject);
            }
        }
        public void SetTextSize(float size)
        {
            textSize = size;
            if (SlotItemHolder != null)
            {
                SlotItemHolder.GetComponent<DragItem>().SetTextSize(size);

            }
            else
            {
                Debug.LogError("Slot Child Null");
            }
        }
        public void SetTextOffset(Vector3 offset)
        {
            if (SlotItemHolder != null)
            {
                SlotItemHolder.GetComponent<DragItem>().SetTextPositionOffset(offset);

            }
            else
            {
                Debug.LogError("Slot Child Null");

            }
        }
        public void SetImageOffSet(Vector3 offset)
        {
            if (SlotItemHolder != null)
            {
                SlotItemHolder.GetComponent<DragItem>().SetImagePositionOffset(offset);

            }
            else
            {
                Debug.LogError("Slot Child Null");

            }
        }
        public void SetChildImageSize(Vector2 size)
        {
            SlotItemHolder.GetComponent<DragItem>().SetImageSize(size);
            SlotItemHolderSize = size;
        }
        public float GetTextSize()
        {
            return SlotItemHolder.GetComponent<DragItem>().GetTextSize();
        }
        public Image GetSlotImage()
        {
            return slotImage;
        }
        public void SetSlotImage(Image newImage)
        {
            slotImage = newImage;
        }
        public GameObject GetItemHolder()
        {
            return SlotItemHolder;
        }
        public InventoryUIManager GetInventoryUI()
        {
            return inventoryUIManager;
        }
        public UnityEngine.Color GetColor()
        {
            return color;
        }
        public InventoryItem GetItem()
        {
            return item;
        }
        public void SetPosition(int position)
        {
            this.position = position;
        }
        public int GetPosition()
        {
            return position;
        }
        public void SetReturnOnMiss(bool destroyOnMiss)
        {
            if (destroyOnMiss)
            {
                returnOnMiss = false;
                SlotItemHolder.GetComponent<DragItem>().SetReturnOnMiss(returnOnMiss);
            }
            else
            {
                returnOnMiss = true;
                SlotItemHolder.GetComponent<DragItem>().SetReturnOnMiss(returnOnMiss);
            }
        }
    }
}
