using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InventorySystem
{
    //Author: Jaxon Schauer
    //Modified by Gemini to integrate with a central SaveLoadManager
    public class InventoryController : MonoBehaviour
    {
        public const string HotBarInventoryName = "HotBar";
        public const string InventoryName = "Inventory";
        public const string ClearInventoryName = "ClearInventory";

        [Header("============[ Setup Confirmation ]============")]
        [Header("**********************************************")]
        [Header("Click the \"I Understand The Setup\" to show you understand")]
        [Header("there should only ever be one InventoryController and that")]
        [Header("the InventoryController prefab must be unpacked in the scene")]
        [Header("**********************************************")]
        [Tooltip("Toggle to confirm you understand the setup requirements.")]
        [SerializeField]
        private bool iUnderstandTheSetup = false;

        [Space(10)]

        [Header("============[ Inventory Controller Setup ]============")]
        [Space(20)]
        [Tooltip("Assign the main UI canvas.")]
        [SerializeField]
        private Transform UI;

        [Space(10)]

        [Header("========[ Items Setup ]========")]
        [Header("NOTE: All changes to items must be made here")]
        [Tooltip("Add templates for each allowable inventory item.")]
        [SerializeField]
        public List<ItemData> items;

        [Space(10)]

        [Header("========[ Inventory Setup ]========")]
        [Header("NOTE: After initialization, changes here won't take effect.")]
        [Header("Modify the inventory under the UI component.")]
        [Tooltip("Add templates for each inventory to be initialized.")]
        [SerializeField]
        public List<InventoryInitializer> initializeInventory = new List<InventoryInitializer>();

        [SerializeField, HideInInspector]
        private List<InventoryInitializer> prevInventoryTracker;

        [Tooltip("Prefab for the inventory manager that controls each of the inventory UI's.")]
        [SerializeField]
        private GameObject inventoryManagerObj;

        [SerializeField, HideInInspector]
        private List<GameObject> allInventoryUI = new List<GameObject>();
        private Dictionary<string, Inventory> inventoryManager = new Dictionary<string, Inventory>();
        private Dictionary<string, GameObject> inventoryUIDict = new Dictionary<string, GameObject>();
        private Dictionary<string, InventoryItem> itemManager = new Dictionary<string, InventoryItem>();
        private Dictionary<string, List<GameObject>> EnableDisableDict = new Dictionary<string, List<GameObject>>();

        [SerializeField, HideInInspector]
        public static InventoryController instance;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                Debug.LogError("There should only be one inventory controller in the scene");
                return;
            }
            if (iUnderstandTheSetup)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (!TestInstance()) return;
            if (!TestSetup()) return;
            TestChildObject();
            AllignDictionaries();
            InitializeItems();

            checkStartMode();
        }

        private void checkStartMode()
        {
            if(SaveLoadManager.Instance != null)
            {
                switch (SaveLoadManager.Instance.startMode)
                {
                    case SaveLoadManager.GameStartMode.NewGame:
                        Debug.Log("체크포인트 1");
                        SaveLoadManager.Instance.ClearAllDataForNewGame();
                        SaveLoadManager.Instance.SettingMode(3);
                        break;
                    case SaveLoadManager.GameStartMode.LoadGame:
                        SaveLoadManager.Instance.LoadGame(SaveLoadManager.Instance.selectedIndex);
                        SaveLoadManager.Instance.SettingMode(3);
                        break;
                    case SaveLoadManager.GameStartMode.Running:
                        if (GameObject.FindGameObjectWithTag("Village") != null)
                        {
                            InventoryManager.Instance.LoadAllInventories("Current.json");
                            InventoryManager.Instance.RestoreInventoryTo(InventoryName);
                        }
                        else if(GameObject.FindGameObjectWithTag("GameScene") != null)
                        {
                            InventoryManager.Instance.LoadAllInventories("Current.json");
                            CopyEquippedItemsToInventory(HotBarInventoryName, ClearInventoryName);
                        }
                        break;
                }
            }
        }

        private void Update()
        {
            ToggleOnKeyInput();
        }

        #region Save/Load Integration
        
        /// <summary>
        /// Clears all items from all managed inventories. Used before loading new data.
        /// </summary>
        public void ClearAllInventories()
        {
            foreach (var pair in inventoryManager)
            {
                pair.Value.Clear();
            }
            Debug.Log("<color=yellow>[InventoryController]</color> All inventories cleared.");
        }

        /// <summary>
        /// Generates a JSON string representing the state of all save-enabled inventories.
        /// </summary>
        /// <returns>A JSON string of the inventory data.</returns>
        public string GetSaveData()
        {
            if (inventoryManager == null)
            {
                Debug.LogError("[InventoryController] GetSaveData failed: inventoryManager is null.");
                return "";
            }
            InventoryData data = new InventoryData(inventoryManager);
            return JsonUtility.ToJson(data, true);
        }

        /// <summary>
        /// Loads inventory states from a JSON string.
        /// </summary>
        /// <param name="jsonData">The JSON data to load from.</param>
        public void LoadFromData(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogWarning("[InventoryController] LoadFromData received null or empty JSON data. Inventories will be empty.");
                ClearAllInventories();
                return;
            }

            InventoryData itemData = JsonUtility.FromJson<InventoryData>(jsonData);
            if (itemData == null)
            {
                Debug.LogError("[InventoryController] Failed to deserialize JSON data. Inventories will not be loaded.");
                return;
            }

            ClearAllInventories();

            Dictionary<string, List<ItemSaveData>> loadedInventories = itemData.ToDictionary();

            foreach (var pair in loadedInventories)
            {
                string invName = pair.Key;
                if (!inventoryManager.ContainsKey(invName))
                {
                    Debug.LogWarning($"[InventoryController] Save data contains inventory '{invName}' which does not exist in the current scene. Skipping.");
                    continue;
                }

                List<ItemSaveData> itemsToLoad = pair.Value;
                foreach (ItemSaveData item in itemsToLoad)
                {
                    if (item.name != null && itemManager.ContainsKey(item.name))
                    {
                        InventoryItem copyItem = itemManager[item.name];
                        InventoryItem newItem = new InventoryItem(copyItem, item.amount);
                        AddItemPos(invName, newItem, item.position);
                    }
                    else
                    {
                        Debug.LogWarning("[InventoryController] Could not find item type '" + item.name + "' in item manager. It will not be loaded.");
                    }
                }
            }
            Debug.Log("<color=green>[InventoryController]</color> Successfully loaded inventories from data.");
        }

        #endregion

        public void InitializeInventories()
        {
            if (!TestSetup()) return;
            instance = this;
            AllignDictionaries();
            RemoveDeletedInventories();
            InitializeNewInventories();
            UpdateInventoryTracker();
            InitializeItems();
        }

        private void UpdateInventoryTracker()
        {
            prevInventoryTracker.Clear();
            for (int i = 0; i < initializeInventory.Count; i++)
            {
                InventoryInitializer InitilizerCopy = new InventoryInitializer();
                InitilizerCopy.Copy(initializeInventory[i]);
                prevInventoryTracker.Add(InitilizerCopy);
            }
        }

        private void InitializeNewInventories()
        {
            foreach (InventoryInitializer initializer in initializeInventory)
            {
                if (!prevInventoryTracker.Contains(initializer))
                {
                    initializer.SetInitialized(true);
                    GameObject tempinventoryUI = Instantiate(inventoryManagerObj, transform.position, Quaternion.identity, UI);
                    RectTransform UIRect = UI.GetComponent<RectTransform>();
                    tempinventoryUI.transform.position = new Vector3(Random.Range(0.0f, UIRect.sizeDelta.x), Random.Range(0.0f, UIRect.sizeDelta.y), 0);
                    tempinventoryUI.SetActive(true);
                    tempinventoryUI.name = initializer.GetInventoryName();
                    allInventoryUI.Add(tempinventoryUI);

                    string inventoryName = initializer.GetInventoryName();
                    int InventorySize = initializer.GetRows() * initializer.GetCols();
                    Inventory curInventory = new Inventory(tempinventoryUI, inventoryName, InventorySize);

                    inventoryManager.Add(inventoryName, curInventory);

                    InventoryUIManager inventoryUI = tempinventoryUI.GetComponent<InventoryUIManager>();
                    inventoryUI.SetVarsOnInit();
                    inventoryUI.SetInventory(ref curInventory);
                    inventoryUI.SetRowCol(initializer.GetRows(), initializer.GetCols());
                    inventoryUI.SetInventoryName(initializer.GetInventoryName());
                    inventoryUI.UpdateInventoryUI();
                }
            }
            foreach (GameObject inObjects in allInventoryUI)
            {
                inObjects.GetComponent<InventoryUIManager>().UpdateInventoryUI();
            }
        }

        private void RemoveDeletedInventories()
        {
            List<GameObject> toremove = new List<GameObject>();
            foreach (InventoryInitializer initializer in prevInventoryTracker)
            {
                if (!initializeInventory.Contains(initializer))
                {
                    foreach (GameObject UI in allInventoryUI)
                    {
                        InventoryUIManager UIInstance = UI.GetComponent<InventoryUIManager>();
                        if (UIInstance.GetInventoryName() == initializer.GetInventoryName())
                        {
                            toremove.Add(UI);
                            inventoryManager.Remove(UIInstance.GetInventoryName());
                        }
                    }
                }
            }

            foreach (GameObject remove in toremove)
            {
                allInventoryUI.Remove(remove);
                DestroyImmediate(remove);
            }
        }

        private void InitializeItems()
        {
            itemManager.Clear();
            foreach (ItemData itemData in items)
            {
                if (itemData == null) continue;
                InventoryItem newItem = new InventoryItem(itemData);
                if(!itemManager.ContainsKey(newItem.GetItemType()))
                {
                    itemManager.Add(itemData.itemName, newItem);
                }
                else
                {
                    Debug.LogError("There can only be one of each ItemType: " + itemData.itemName);
                }
            }
        }

        public void AddItemPos(string inventoryName, InventoryItem itemType, int position)
        {
            if (!(TestInventoryDict(inventoryName)))
            {
                return;
            }
            if (itemType.GetIsNull())
            {
                Debug.LogError("Cannot add null item");
                return;
            }
            Inventory inventory = inventoryManager[inventoryName];
            inventory.AddItemPos(position, itemType);
        }

        public void AddItemPos(string inventoryName, string itemType, int position, int amount = 1)
        {
            if (!(TestInventoryDict(inventoryName) && TestItemDict(itemType)))
            {
                return;
            }
            Inventory inventory = inventoryManager[inventoryName];
            InventoryItem item = new InventoryItem(itemManager[itemType], amount);
            inventory.AddItemPos(position, item);
        }

        public void AddItem(string inventoryName, string itemType, int amount = 1)
        {
            if (!(TestInventoryDict(inventoryName) && TestItemDict(itemType)))
            {
                return;
            }

            Inventory inventory = inventoryManager[inventoryName];
            InventoryItem itemTemplate = itemManager[itemType];
            int maxStack = itemTemplate.GetItemStackAmount();
            int currentAmount = inventory.Count(itemType);

            if (currentAmount + amount > maxStack)
            {
                Debug.Log($"Cannot add item. Total amount for '{itemType}' would exceed max stack of {maxStack}.");
                return;
            }

            InventoryItem item = new InventoryItem(itemTemplate, amount);
            inventory.AddItemAuto(item, amount);
        }

        public void RemoveItemPos(string inventoryName, int position, int amount)
        {
            if (!TestInventoryDict(inventoryName))
            {
                return;
            }
            Inventory inventory = inventoryManager[inventoryName];
            inventory.RemoveItemInPosition(position, amount);
        }

        public void RemoveItem(string inventoryName,string itemType, int amount)
        {
            if (!(TestInventoryDict(inventoryName) && TestItemDict(itemType)))
            {
                return;
            }
            Inventory inventory = inventoryManager[inventoryName];
            inventory.RemoveItemAuto(itemType, amount);
        }

        public void RemoveItem(string inventoryName, InventoryItem item, int amount = 1)
        {
            if (!(TestInventoryDict(inventoryName)))
            {
                return;
            }
            if (item.GetIsNull())
            {
                Debug.LogError("Cannot remove null item");
                return;
            }

            Inventory inventory = inventoryManager[inventoryName];
            inventory.RemoveItemInPosition(item, amount);
        }

        public bool InventoryFull(string inventoryName, string itemType)
        {
            if (!TestInventoryDict(inventoryName))
            {
                return true;
            }
            Inventory inventory = inventoryManager[inventoryName];
            return inventory.Full(itemType);
        }

        public void InventoryClear(string inventoryName)
        {
            inventoryManager[inventoryName].Clear();
        }

        private bool TestInventoryDict(string inventoryName)
        {
            if (inventoryName == null)
            {
                Debug.LogError("Inventory name is null");
                return false;
            }
            if (inventoryManager.ContainsKey(inventoryName))
            {
                return true;
            }
            else
            {
                Debug.LogError("No existing inventory with name: " + inventoryName);
                return false;
            }
        }

        private bool TestItemDict(string itemType)
        {
            if (itemType == null)
            {
                Debug.LogError("Itemtype is null");
                return false;
            }
            if (itemManager.ContainsKey(itemType))
            {
                return true;
            }
            else
            {
                Debug.LogError("No existing item with name: " + itemType);
                return false;
            }
        }

        public void ResetInventory()
        {
            inventoryManager.Clear();
            itemManager.Clear();
            prevInventoryTracker.Clear();
            foreach (ItemData item in items)
            {
                itemManager.Add(item.itemName, new InventoryItem(item));
            }
            foreach (GameObject obj in allInventoryUI)
            {
                DestroyImmediate(obj);
            }
            allInventoryUI.Clear();
            inventoryUIDict.Clear();
        }

        public void AllignDictionaries()
        {
            inventoryManager.Clear();
            foreach (GameObject InventoryUI in allInventoryUI)
            {
                bool setActive = false;
                if (!InventoryUI.activeSelf)
                {
                    setActive = true;
                    InventoryUI.SetActive(true);
                }
                InventoryUIManager inventoryInstance = InventoryUI.GetComponent<InventoryUIManager>();
                if (!inventoryUIDict.ContainsKey(inventoryInstance.GetInventoryName()))
                {
                    inventoryUIDict.Add(inventoryInstance.GetInventoryName(), InventoryUI);
                }
                inventoryInstance.GetInventory().InitList();
                if (!inventoryManager.ContainsKey(inventoryInstance.GetInventoryName()))
                {
                    //Debug.Log(inventoryInstance.GetInventoryName() + " " + inventoryInstance.gameObject.name);
                    inventoryManager.Add(inventoryInstance.GetInventoryName(), inventoryInstance.GetInventory());
                }
                foreach (char character in inventoryInstance.GetEnableDisable())
                {
                    if (EnableDisableDict.ContainsKey(character.ToString().ToLower()))
                    {
                        EnableDisableDict[character.ToString().ToLower()].Add(InventoryUI);
                    }
                    else
                    {
                        EnableDisableDict.Add(character.ToString().ToLower(), new List<GameObject>());
                        EnableDisableDict[character.ToString().ToLower()].Add(InventoryUI);
                    }
                }
                if (setActive)
                {
                    InventoryUI.SetActive(false);
                }
            }
        }

        public int CountItems(string inventoryName, string itemType)
        {
            if (!(TestInventoryDict(inventoryName) && TestItemDict(itemType)))
            {
                return 0;
            }
            Inventory inventory = inventoryManager[inventoryName];
            return inventory.Count(itemType);
        }

        public void AddToggleKey(string InventoryName, char character)
        {
            if (EnableDisableDict.ContainsKey(character.ToString().ToLower()))
            {
                if (EnableDisableDict[character.ToString().ToLower()].Contains(inventoryUIDict[InventoryName]))
                {
                    return;
                }
                EnableDisableDict[character.ToString().ToLower()].Add(inventoryUIDict[InventoryName]);
            }
            else
            {
                EnableDisableDict.Add(character.ToString().ToLower(), new List<GameObject>());
                EnableDisableDict[character.ToString().ToLower()].Add(inventoryUIDict[InventoryName]);
            }
        }

        public void RemoveToggleKey(string InventoryName, char character)
        {
            if (EnableDisableDict.ContainsKey(character.ToString().ToLower()))
            {
                if (!EnableDisableDict[character.ToString().ToLower()].Contains(inventoryUIDict[InventoryName]))
                {
                    return;
                }
                EnableDisableDict[character.ToString().ToLower()].Remove(inventoryUIDict[InventoryName]);
            }
        }

        private void ToggleOnKeyInput()
        {
            if (Input.anyKeyDown)
            {
                string input = Input.inputString;
                input = input.ToLower();
                if (EnableDisableDict.ContainsKey(input))
                {
                    List<GameObject> inventoryUIs = EnableDisableDict[input];
                    foreach (GameObject inventoryUI in inventoryUIs)
                    {
                        if (inventoryUI.activeSelf)
                        {
                            inventoryUI.SetActive(false);
                        }
                        else
                        {
                            inventoryUI.SetActive(true);
                        }
                    }
                }
            }
        }

        public void CreateInventory(Transform instantiaterPos, string inventoryName, int row, int col,
            bool highlightable = false, bool draggable = false, bool saveInventory = false, bool isActive = true)
        {
            if (!TestSetup()) return;
            GameObject tempinventoryUI = Instantiate(inventoryManagerObj, instantiaterPos.position, Quaternion.identity, UI);
            tempinventoryUI.SetActive(isActive);

            tempinventoryUI.transform.position = instantiaterPos.position;

            Inventory curInventory = new Inventory(tempinventoryUI, inventoryName, col * row);
            inventoryManager.Add(inventoryName, curInventory);

            InventoryUIManager inventoryUI = tempinventoryUI.GetComponent<InventoryUIManager>();
            inventoryUI.SetVarsOnInit();
            inventoryUI.SetSave(saveInventory);
            inventoryUI.SetInventory(ref curInventory);
            inventoryUI.SetHighlightable(highlightable);
            inventoryUI.SetDraggable(draggable);
            inventoryUI.SetRowCol(row, col);
            inventoryUI.SetInventoryName(inventoryName);
            inventoryUI.UpdateInventoryUI();
        }
        
        private bool TestSetup()
        {
            return TestIunderstandTheSetup()
                && TestinventoryManagerObjSetup()
                && TestInventoryUI()
                && TestInveInitializerListSetup()
                && TestUISetup();
        }

        private bool TestIunderstandTheSetup()
        {
            if(!iUnderstandTheSetup)
            {
                Debug.LogError("Read instructions and click The I understand The setup bool.");
                return false;
            }
            return true;
        }

        private bool TestInventoryUI()
        {
            if (allInventoryUI.Count == 0 && Application.isPlaying)
            {
                Debug.LogWarning("No InventoryUIManagers detected. Ensure to initialize all inventories in editor mode. If unexpected try unpacking InventoryController");
                return false;
            }
            for (int i = 0; i < allInventoryUI.Count; i++)
            {
                if (allInventoryUI[i] == null)
                {
                    Debug.LogError("Inventories in allInventoryUI are null at index " + i + ". Total items in list: " + allInventoryUI.Count + ". This can be caused by a serialization issue or a broken reference from the editor. Try re-running the setup steps.");
                    return false;
                }
            }
            return true;
        }

        public bool TestinventoryManagerObjSetup()
        {
            if (inventoryManagerObj == null)
            {
                Debug.LogError("Inventory manager object is null");
                return false;
            }
            return true;
        }

        private bool TestInveInitializerListSetup()
        {
            for (int i = 0; i < initializeInventory.Count; i++)
            {
                int countInstance = 0;
                for (int j = 0; j < initializeInventory.Count; j++)
                {
                    if (initializeInventory[i].GetInventoryName().Equals(initializeInventory[j].GetInventoryName()))
                    {
                        countInstance++;
                    }
                    if (countInstance > 1)
                    {
                        Debug.LogError("There can only be one of each Inventory");
                        return false;
                    }
                }
            }
            return true;
        }

        private bool TestUISetup()
        {
            if (UI == null)
            {
                Debug.LogError("UI Canvas Not Set Correctly");
                return false;
            }
            return true;
        }

        private bool TestInstance()
        {
            if (instance == null)
            {
                Debug.LogError("Read instructions and click The I understand The setup bool. InventoryController destroyed");
                return false;
            }
            return true;
        }

        private void TestChildObject()
        {
            InventoryUIManager manager = transform.GetComponentInChildren<InventoryUIManager>();

            if (manager != null)
            {
                if (manager.gameObject.activeSelf)
                {
                    Debug.LogWarning("The Child of Inventory Controller, InventoryUIManager is active. Disabling it now.");
                    manager.gameObject.SetActive(false);
                }
            }
            else
            {
                if (transform.childCount == 0)
                    Debug.LogWarning("Inventory Controller Does Not Have Child Object with InventoryUIManager");
            }
        }

        public bool checkEnabled(string inventoryName)
        {
            if (inventoryUIDict[inventoryName].activeSelf)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool checkUI(GameObject obj)
        {
            if (inventoryUIDict.ContainsValue(obj))
            {
                return true;
            }
            return false;
        }

        public GameObject GetInventoryManagerPrefab()
        {
            return inventoryManagerObj;
        }

        public Inventory GetInventory(string inventoryName)
        {
            
            return inventoryManager[inventoryName];
        }

        public Transform GetUI()
        {
            return UI;
        }

        public InventoryItem GetItem(string inventoryName, int index)
        {
            return inventoryManager[inventoryName].InventoryGetItem(index);
        }

        public List<ItemData> GetItems()
        {
            return items;
        }

        public void RegisterExternalUI(GameObject uiObject)
        {
            if (allInventoryUI == null)
            {
                allInventoryUI = new List<GameObject>();
            }
            if (uiObject != null && !allInventoryUI.Contains(uiObject))
            {
                allInventoryUI.Add(uiObject);
            }
        }

        public void ClearRegisteredUI()
        {
            if (allInventoryUI != null)
            {
                allInventoryUI.Clear();
            }
        }

        // ===================[ 장비 장착/교체 로직 추가 ]===================

        /// <summary>
        /// 아이템 이름으로 Resources 폴더에서 EquipmentData를 로드하여 반환합니다.
        /// </summary>
        private EquipmentData LoadEquipmentData(string itemName)
        {
            // 아이템 데이터가 "Resources/Items" 폴더에 있다고 가정합니다.
            // 실제 경로에 맞게 수정해야 할 수 있습니다.
            var equipmentData = Resources.Load<EquipmentData>($"Items/{itemName}");

            if (equipmentData == null)
            {
                Debug.LogWarning($"[InventoryController] LoadEquipmentData: 'Resources/Items/{itemName}' 경로에서 EquipmentData를 찾을 수 없습니다.");
            }
            return equipmentData;
        }

        /// <summary>
        /// 인벤토리의 이름으로 해당하는 InventoryUIManager를 찾아서 반환합니다.
        /// </summary>
        public InventoryUIManager GetInventoryUIByName(string name)
        {
            if (inventoryUIDict.ContainsKey(name))
            {
                return inventoryUIDict[name].GetComponent<InventoryUIManager>();
            }
            Debug.LogError($"[InventoryController] '{name}' 이라는 이름의 인벤토리를 찾을 수 없습니다.");
            return null;
        }

        /// <summary>
        /// 두 인벤토리의 특정 슬롯에 있는 아이템을 서로 교환합니다.
        /// </summary>
        public void SwapItems(string sourceInvName, int sourceIndex, string targetInvName, int targetIndex)
        {
            if (!inventoryManager.ContainsKey(sourceInvName) || !inventoryManager.ContainsKey(targetInvName))
            {
                Debug.LogError("[InventoryController] SwapItems 실패: 유효하지 않은 인벤토리 이름입니다.");
                return;
            }

            Inventory sourceInv = inventoryManager[sourceInvName];
            Inventory targetInv = inventoryManager[targetInvName];

            InventoryItem sourceItem = sourceInv.InventoryGetItem(sourceIndex);
            InventoryItem targetItem = targetInv.InventoryGetItem(targetIndex);

            // 1. 장비 효과 해제 (Unequip)
            // 소스 슬롯이 장비창 슬롯이고, 아이템이 있었다면 Unequip
            if (sourceInvName == HotBarInventoryName && !sourceItem.GetIsNull())
            {
                EquipmentEffectManager.Instance.Unequip(LoadEquipmentData(sourceItem.GetItemType()));
            }
            // 타겟 슬롯이 장비창 슬롯이고, 아이템이 있었다면 Unequip
            if (targetInvName == HotBarInventoryName && !targetItem.GetIsNull())
            {
                EquipmentEffectManager.Instance.Unequip(LoadEquipmentData(targetItem.GetItemType()));
            }

            // 2. 아이템 교환
            InventoryItem sourceItemCopy = new InventoryItem(sourceItem);
            InventoryItem targetItemCopy = new InventoryItem(targetItem);

            sourceInv.RemoveItemHelper(sourceItem, sourceIndex, false);
            targetInv.RemoveItemHelper(targetItem, targetIndex, false);

            sourceInv.AddItemHelper(targetItemCopy, sourceIndex, true);
            targetInv.AddItemHelper(sourceItemCopy, targetIndex, true);

            // 3. 장비 효과 적용 (Equip)
            // 소스 슬롯이 장비창 슬롯이고, 새로 들어온 아이템이 있다면 Equip
            if (sourceInvName == HotBarInventoryName && !targetItem.GetIsNull())
            {
                EquipmentEffectManager.Instance.Equip(LoadEquipmentData(targetItem.GetItemType()));
            }
            // 타겟 슬롯이 장비창 슬롯이고, 새로 들어온 아이템이 있다면 Equip
            if (targetInvName == HotBarInventoryName && !sourceItem.GetIsNull())
            {
                EquipmentEffectManager.Instance.Equip(LoadEquipmentData(sourceItem.GetItemType()));
            }

            Debug.Log($"'{sourceInvName}'의 {sourceIndex}번 슬롯과 '{targetInvName}'의 {targetIndex}번 슬롯 아이템 교환 완료");
        }

        /// <summary>
        /// 아이템을 장비창에 장착합니다. 아이템을 복사하며, 기존 아이템이 있다면 덮어씁니다.
        /// </summary>
        public void EquipItem(InventoryItem itemToEquip, Vector3 slotPosition)
        {
            // 1. 장착할 아이템이 EquipmentData 타입인지 확인
            if (itemToEquip.GetEquipmentType() == EquipmentType.None)
            {
                Debug.Log($"'{itemToEquip.GetItemType()}' 아이템은 장비가 아닙니다.");
                return;
            }

            // 2. 아이템이 'Accessory' 타입인지 확인
            if (itemToEquip.GetEquipmentType() == EquipmentType.Ring)
            {
                // 3. 사용 가능한 링 슬롯 목록을 가져옴
                // List<string> availableSlots = GetAvailableRingSlots();

                // 4. 사용 가능한 슬롯이 없으면 메시지 출력 후 종료
                // if (availableSlots.Count == 0)
                // {
                //     Debug.Log("장착할 수 있는 링 슬롯이 없습니다.");
                //     return;
                // }

                // 5. 링 선택 UI 표시
                RingSelectionUI.Instance.ShowSelection(itemToEquip, slotPosition);
            }
            else // 링이 아닌 다른 장비는 기존 로직대로 처리
            {
                EquipItemToSlot(itemToEquip, itemToEquip.GetEquipmentType().ToString());
            }
        }

        /// <summary>
        /// 특정 슬롯 타입에 아이템을 장착하는 내부 로직
        /// </summary>
        private void EquipItemToSlot(InventoryItem itemToEquip, string slotType)
        {
            InventoryUIManager equipmentUI = GetInventoryUIByName(HotBarInventoryName);
            if (equipmentUI == null || itemToEquip.GetEquit() == true) return;
            
            Inventory targetInv = GetInventory(HotBarInventoryName);
            if (targetInv == null) return;
            // 아이템 타입과 슬롯 타입이 맞는 곳을 찾음
            foreach (GameObject slotObj in equipmentUI.GetSlot())
            {
                Slot targetSlot = slotObj.GetComponent<Slot>();
                if (targetSlot.slotType == slotType)
                {
                    InventoryItem oldItem = targetInv.InventoryGetItem(targetSlot.GetPosition());

                    if (oldItem != null && !oldItem.GetIsNull() && oldItem.GetItemType() == itemToEquip.GetItemType())
                    {
                        Debug.Log("이미 같은 아이템을 장착하고 있습니다.");
                        return;
                    }
                    //아이템이 있다면 효과를 제거
                    if (oldItem != null && !oldItem.GetIsNull())
                    {
                        EquipmentEffectManager.Instance.Unequip(LoadEquipmentData(oldItem.GetItemType()));

                        // itemToEquip이 있던 인벤토리에서 장착 해제된 아이템(oldItem)을 찾아 Equit = false로 설정
                        string sourceInventoryName = itemToEquip.GetInventory();
                        if (!string.IsNullOrEmpty(sourceInventoryName))
                        {
                            Inventory sourceInventory = GetInventory(sourceInventoryName);
                            if (sourceInventory != null)
                            {
                                // 인벤토리 리스트에서 oldItem과 이름이 같은 아이템 찾기
                                InventoryItem itemToUpdate = sourceInventory.GetList().Find(item => item != null && !item.GetIsNull() && item.GetItemType() == oldItem.GetItemType());

                                if (itemToUpdate != null)
                                {
                                    itemToUpdate.SetEquit(false);

                                    // Get the UIManager for the source inventory and update the slot
                                    if (inventoryUIDict.ContainsKey(sourceInventoryName))
                                    {
                                        InventoryUIManager sourceUIManager = inventoryUIDict[sourceInventoryName].GetComponent<InventoryUIManager>();
                                        if (sourceUIManager != null)
                                        {
                                            sourceUIManager.UpdateSlot(itemToUpdate.GetPosition());
                                        }
                                    }

                                    Debug.Log($"'{sourceInventoryName}' 인벤토리의 '{itemToUpdate.GetItemType()}' 아이템을 장착 해제 상태로 변경했습니다.");
                                }
                            }
                        }
                        RemoveItemPos(targetInv.GetName(), targetSlot.GetPosition() , 1);
                    }
                    //새 아이템 효과 추가
                    AddItemPos(targetInv.GetName(), new InventoryItem(itemToEquip), targetSlot.GetPosition());
                    EquipmentEffectManager.Instance.Equip(LoadEquipmentData(itemToEquip.GetItemType()));

                    // itemToEquip이 있던 인벤토리에서 장착한 아이템을 찾아 Equit = true로 설정
                    string equippedItemSourceInv = itemToEquip.GetInventory();
                    if (!string.IsNullOrEmpty(equippedItemSourceInv))
                    {
                        Inventory sourceInv = GetInventory(equippedItemSourceInv);
                        if (sourceInv != null)
                        {
                            InventoryItem itemInSource = sourceInv.GetList().Find(item => item != null && !item.GetIsNull() && item.GetItemType() == itemToEquip.GetItemType());
                            if (itemInSource != null)
                            {
                                itemInSource.SetEquit(true);
                                Debug.Log($"'{equippedItemSourceInv}' 인벤토리의 '{itemInSource.GetItemType()}' 아이템을 장착 상태로 변경했습니다.");
                            }
                        }
                    }
                    
                    Debug.Log($"'{itemToEquip.GetItemType()}' 아이템을 '{targetSlot.slotType}' 슬롯에 장착했습니다.");
                    return; 
                }
            }
            Debug.LogWarning($"'{itemToEquip.GetEquipmentType()}' 타입을 장착할 수 있는 '{slotType}' 슬롯이 'HotBar' 인벤토리에 없습니다.");
        }

        /// <summary>
        /// RingSelectionUI에서 호출되어 특정 링 슬롯에 아이템을 장착합니다.
        /// </summary>
        public void EquipRingInSlot(InventoryItem ringItem, string slotType)
        {
            Debug.Log($"EquipRingInSlot 호출됨: item={ringItem.GetItemType()}, slot={slotType}");
            EquipItemToSlot(ringItem, slotType);
        }

        /// <summary>
        /// 사용 가능한 링 슬롯("Ring1", "Ring2" 등)의 목록을 반환합니다.
        /// </summary>
        private List<string> GetAvailableRingSlots()
        {
            List<string> availableSlots = new List<string>();
            InventoryUIManager equipmentUI = GetInventoryUIByName(HotBarInventoryName);
            if (equipmentUI == null) return availableSlots;

            Inventory targetInv = GetInventory(HotBarInventoryName);
            if (targetInv == null) return availableSlots;

            foreach (GameObject slotObj in equipmentUI.GetSlot())
            {
                Slot targetSlot = slotObj.GetComponent<Slot>();
                // 슬롯 타입이 "Ring"으로 시작하는지 확인 (예: "Ring1", "Ring2")
                if (targetSlot.slotType.StartsWith("Ring"))
                {
                    // 해당 슬롯이 비어있는지 확인
                    InventoryItem itemInSlot = targetInv.InventoryGetItem(targetSlot.GetPosition());
                    if (itemInSlot == null || itemInSlot.GetIsNull())
                    {
                        availableSlots.Add(targetSlot.slotType);
                    }
                }
            }
            return availableSlots;
        }

        /// <summary>
        /// 장비창의 모든 아이템을 순회하며 EquipmentEffectManager에 효과를 다시 적용합니다.
        /// </summary>
        public void ReapplyAllEquipmentEffects()
        {
            // 1. 기존 효과 모두 초기화
            EquipmentEffectManager.Instance.ClearAllEffects();

            // 2. "HotBar" 인벤토리를 가져옴
            Inventory hotbarInv = GetInventory(HotBarInventoryName);
            if (hotbarInv == null)
            {
                Debug.LogWarning("[InventoryController] ReapplyAllEquipmentEffects: 'HotBar' inventory not found.");
                return;
            }

            // 3. 장비창의 모든 아이템을 순회
            foreach (InventoryItem item in hotbarInv.GetList())
            {
                if (item != null && !item.GetIsNull())
                {
                    // 4. 아이템 데이터를 로드하고 장비인 경우 효과 적용
                    EquipmentData equipmentData = LoadEquipmentData(item.GetItemType());
                    if (equipmentData != null)
                    {
                        EquipmentEffectManager.Instance.Equip(equipmentData);
                    }
                }
            }
            Debug.Log("<color=cyan>[InventoryController]</color> All equipment effects from 'HotBar' have been reapplied.");
        }
        public void CopyEquippedItemsToInventory(string equipmentInvName, string mainInvName)
        {
            if (!inventoryManager.ContainsKey(equipmentInvName) || !inventoryManager.ContainsKey(mainInvName))
            {
                Debug.LogError($"[CopyEquippedItemsToInventory] Invalid inventory name provided. Source: {equipmentInvName}, Target: {mainInvName}");
                return;
            }

            Inventory equipmentInventory = GetInventory(equipmentInvName);
            Inventory mainInventory = GetInventory(mainInvName);

            if (equipmentInventory == null || mainInventory == null) return;

            foreach (InventoryItem equippedItem in equipmentInventory.GetList())
            {
                if (equippedItem != null && !equippedItem.GetIsNull())
                {
                    int existingAmount = mainInventory.Count(equippedItem.GetItemType());
                    if (existingAmount > 0)
                    {
                        Debug.Log($"Item '{equippedItem.GetItemType()}' already exists in '{mainInvName}'. Skipping copy.");
                        continue;
                    }

                    InventoryItem newItemForMain = new InventoryItem(equippedItem);
                    newItemForMain.SetEquit(true);

                    mainInventory.AddItemAuto(newItemForMain, newItemForMain.GetAmount());
                    
                    Debug.Log($"Copied equipped item '{newItemForMain.GetItemType()}' to '{mainInvName}'.");
                }
            }
        }

        public void SyncEquippedStatus(string sourceInventoryName, string targetInventoryName)
        {
            if (!inventoryManager.ContainsKey(sourceInventoryName) || !inventoryManager.ContainsKey(targetInventoryName))
            {
                Debug.LogError($"[SyncEquippedStatus] Invalid inventory name provided. Source: {sourceInventoryName}, Target: {targetInventoryName}");
                return;
            }

            Inventory sourceInventory = GetInventory(sourceInventoryName);
            Inventory targetInventory = GetInventory(targetInventoryName);

            if (sourceInventory == null || targetInventory == null) return;

            foreach (InventoryItem sourceItem in sourceInventory.GetList())
            {
                if (sourceItem != null && !sourceItem.GetIsNull())
                {
                    InventoryItem targetItem = targetInventory.GetList().Find(item => item != null && !item.GetIsNull() && item.GetItemType() == sourceItem.GetItemType());

                    if (targetItem != null)
                    {
                        targetItem.SetEquit(true);

                        if (inventoryUIDict.ContainsKey(targetInventoryName))
                        {
                            InventoryUIManager targetUIManager = inventoryUIDict[targetInventoryName].GetComponent<InventoryUIManager>();
                            if (targetUIManager != null)
                            {
                                targetUIManager.UpdateSlot(targetItem.GetPosition());
                            }
                        }
                        Debug.Log($"Synced Equit status for '{targetItem.GetItemType()}' in '{targetInventoryName}'.");
                    }
                }
            }
        }
    }
}
