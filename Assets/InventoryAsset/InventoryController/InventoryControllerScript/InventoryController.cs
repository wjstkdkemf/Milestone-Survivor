using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InventorySystem
{
    //Author: Jaxon Schauer
    //Modified by Gemini to integrate with a central SaveLoadManager
    public class InventoryController : MonoBehaviour
    {
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
            // Automatic loading on start is removed. Loading will be handled by SaveLoadManager.
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
            InventoryItem item = new InventoryItem(itemManager[itemType], amount);
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
                inventoryManager.Add(inventoryInstance.GetInventoryName(), inventoryInstance.GetInventory());
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
    }
}