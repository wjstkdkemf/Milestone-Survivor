using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;
using InventorySystem;

public class ItemAcquisitionRegressionTests
{
    private readonly List<Object> owned = new List<Object>();
    private readonly Dictionary<string, Inventory> inventories = new Dictionary<string, Inventory>();
    private readonly Dictionary<string, InventoryItem> templates = new Dictionary<string, InventoryItem>();
    private InventoryController controller;
    private InventoryController previousController;
    private PlayerStats previousStats;
    private InventoryManager previousStorage;
    private InventoryManager storage;

    [SetUp]
    public void SetUp()
    {
        previousController = InventoryController.instance;
        previousStats = PlayerStats.Instance;
        previousStorage = InventoryManager.Instance;
        controller = Root("Acquisition controller").AddComponent<InventoryController>();
        InventoryController.instance = controller;
        SetField(controller, "inventoryManager", inventories);
        SetField(controller, "itemManager", templates);
        PlayerStats.Instance = Root("Acquisition stats").AddComponent<PlayerStats>();
        storage = Root("Pending items").AddComponent<InventoryManager>();
        SetStorage(storage);
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = owned.Count - 1; i >= 0; i--)
            if (owned[i] != null) Object.DestroyImmediate(owned[i]);
        owned.Clear();
        inventories.Clear();
        templates.Clear();
        InventoryController.instance = previousController;
        PlayerStats.Instance = previousStats;
        SetStorage(previousStorage);
    }

    [Test]
    public void RawAdd_ReportsOnlyWhatFitsInSlots()
    {
        Inventory inventory = Inventory("Inventory", 2);
        Assert.That(inventory.AddItemAuto(Item("Potion", 5), 12), Is.EqualTo(10));
        Assert.That(inventory.Count("Potion"), Is.EqualTo(10));
    }

    [Test]
    public void RawAdd_ReportsPartialMergeWithoutFreeSlots()
    {
        Inventory inventory = Inventory("Inventory", 1);
        InventoryItem item = Item("Potion", 5);
        inventory.AddItemAuto(item, 4);
        Assert.That(inventory.AddItemAuto(item, 4), Is.EqualTo(1));
        Assert.That(inventory.Count("Potion"), Is.EqualTo(5));
    }

    [Test]
    public void FullInventory_PreservesRemainderWithoutConvertingToGold()
    {
        Inventory inventory = Inventory("Inventory", 1);
        inventory.AddItemAuto(Item("Blocker", 1), 1);
        Item("Potion", 5);
        AssertResult(controller.AddItemWithResult("Inventory", "Potion", 3), 0, 0, 3);
        Assert.That(controller.AddItem("Inventory", "Potion"), Is.False);
        Assert.That(PlayerStats.Instance.GoldAmount, Is.Zero);
    }

    [Test]
    public void OwnershipCap_AddsAvailableQuantityAndConvertsOnlyExcess()
    {
        Inventory inventory = Inventory("Inventory", 1);
        inventory.AddItemAuto(Item("Potion", 5), 4);
        AssertResult(controller.AddItemWithResult("Inventory", "Potion", 3), 1, 2, 0);
        Assert.That(inventory.Count("Potion"), Is.EqualTo(5));
        Assert.That(PlayerStats.Instance.GoldAmount, Is.EqualTo(20));
    }

    [Test]
    public void CapacityAndOwnershipLimits_ReportSeparateRemainders()
    {
        Inventory inventory = Inventory("Inventory", 1);
        inventory.AddItemAuto(Item("Blocker", 1), 1);
        Item("Potion", 5);
        AssertResult(controller.AddItemWithResult("Inventory", "Potion", 7), 0, 2, 5);
        Assert.That(PlayerStats.Instance.GoldAmount, Is.EqualTo(20));
    }

    [Test]
    public void MissingStats_DoesNotConsumeItemsThatNeedGoldConversion()
    {
        Inventory inventory = Inventory("Inventory", 1);
        inventory.AddItemAuto(Item("Potion", 1), 1);
        PlayerStats.Instance = null;
        AssertResult(controller.AddItemWithResult("Inventory", "Potion", 1), 0, 0, 1);
    }

    [Test]
    public void GoldNearLimit_LeavesUnconvertibleItemsPending()
    {
        Inventory inventory = Inventory("Inventory", 1);
        inventory.AddItemAuto(Item("Potion", 1), 1);
        PlayerStats.Instance.SetGold(long.MaxValue - 15);
        AssertResult(controller.AddItemWithResult("Inventory", "Potion", 2), 0, 1, 1);
        Assert.That(PlayerStats.Instance.GoldAmount, Is.EqualTo(long.MaxValue - 5));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void InvalidQuantity_DoesNotCreateItemsOrGold(int amount)
    {
        Inventory inventory = Inventory("Inventory", 1);
        Item("Potion", 5);
        Assert.That(controller.AddItem("Inventory", "Potion", amount), Is.False);
        Assert.That(inventory.Count("Potion"), Is.Zero);
        Assert.That(PlayerStats.Instance.GoldAmount, Is.Zero);
    }

    [Test]
    public void Restore_RetainsBlockedItemsAndRetryDoesNotDuplicate()
    {
        Inventory inventory = Inventory("Inventory", 1);
        inventory.AddItemAuto(Item("Blocker", 1), 1);
        Item("Potion", 5);
        storage.AddItem("Potion", 3);
        storage.RestoreInventoryTo("Inventory");
        Assert.That(storage.GetInventoryData()["Potion"], Is.EqualTo(3));
        inventory.RemoveItemInPosition(0, 1);
        storage.RestoreInventoryTo("Inventory");
        storage.RestoreInventoryTo("Inventory");
        Assert.That(storage.GetInventoryData(), Is.Empty);
        Assert.That(inventory.Count("Potion"), Is.EqualTo(3));
        Assert.That(PlayerStats.Instance.GoldAmount, Is.Zero);
    }

    [Test]
    public void Restore_PartialConversionRetiresOnlyProcessedUnits()
    {
        Inventory inventory = Inventory("Inventory", 1);
        inventory.AddItemAuto(Item("Blocker", 1), 1);
        Item("Potion", 5);
        storage.AddItem("Potion", 7);
        storage.RestoreInventoryTo("Inventory");
        storage.RestoreInventoryTo("Inventory");
        Assert.That(storage.GetInventoryData()["Potion"], Is.EqualTo(5));
        Assert.That(PlayerStats.Instance.GoldAmount, Is.EqualTo(20));
    }

    [Test]
    public void StoreRepeatedly_DoesNotDuplicateSourceItems()
    {
        Inventory inventory = Inventory("ClearInventory", 1);
        inventory.AddItemAuto(Item("Potion", 5), 3);
        storage.StoreInventoryFrom("ClearInventory");
        storage.StoreInventoryFrom("ClearInventory");
        Assert.That(inventory.Count("Potion"), Is.Zero);
        Assert.That(storage.GetInventoryData()["Potion"], Is.EqualTo(3));
    }

    [Test]
    public void StoreOverflow_ReportsFailureAndLeavesSourceUntouched()
    {
        Inventory inventory = Inventory("ClearInventory", 1);
        inventory.AddItemAuto(Item("Potion", 5), 3);
        storage.AddItem("Potion", int.MaxValue);
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error,
            "[InventoryManager] Pending quantity overflow for 'Potion'.");
        Assert.That(storage.StoreInventoryFrom("ClearInventory"), Is.False);
        Assert.That(inventory.Count("Potion"), Is.EqualTo(3));
        Assert.That(storage.GetInventoryData()["Potion"], Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void PendingItems_SurviveSaveJsonRoundTrip()
    {
        storage.AddItem("Potion", 3);
        storage.AddItem("Sword", 1);
        var data = new GameSaveData("{}", null, null) { pendingInventoryItems = storage.GetPendingSaveData() };
        string json = JsonUtility.ToJson(data);
        storage.ClearStoredInventory();
        storage.LoadPendingSaveData(JsonUtility.FromJson<GameSaveData>(json).pendingInventoryItems);
        Assert.That(storage.GetInventoryData()["Potion"], Is.EqualTo(3));
        Assert.That(storage.GetInventoryData()["Sword"], Is.EqualTo(1));
    }

    [Test]
    public void LegacySaveWithoutPendingItems_ClearsPreviousSlotPendingItems()
    {
        storage.AddItem("Potion", 3);
        storage.LoadPendingSaveData(JsonUtility.FromJson<GameSaveData>("{\"inventoryJson\":\"{}\"}").pendingInventoryItems);
        Assert.That(storage.GetInventoryData(), Is.Empty);
    }

    [Test]
    public void TransferBlocked_KeepsSourceAndSameInventoryIsNoOp()
    {
        Inventory source = Inventory("Source", 1);
        Inventory target = Inventory("Target", 1);
        source.AddItemAuto(Item("Potion", 5), 3);
        target.AddItemAuto(Item("Blocker", 1), 1);
        InventoryItem item = source.InventoryGetItem(0);
        AssertResult(controller.TransferItem("Source", "Target", item), 0, 0, 3);
        AssertResult(controller.TransferItem("Source", "Source", item), 0, 0, 3);
        Assert.That(source.Count("Potion"), Is.EqualTo(3));
    }

    [Test]
    public void TransferSuccessful_PreservesEnhancementAndRemovesSourceOnce()
    {
        Inventory source = Inventory("Source", 1);
        Inventory target = Inventory("Target", 1);
        source.AddItemAuto(Item("Sword", 1), 1);
        InventoryItem item = source.InventoryGetItem(0);
        item.SetEnhancementLevel(4);
        AssertResult(controller.TransferItem("Source", "Target", item), 1, 0, 0);
        Assert.That(source.Count("Sword"), Is.Zero);
        Assert.That(target.InventoryGetItem(0).GetEnhancementLevel(), Is.EqualTo(4));
        controller.TransferItem("Source", "Target", item);
        Assert.That(PlayerStats.Instance.GoldAmount, Is.Zero);
    }

    [Test]
    public void FailedPickup_KeepsObjectAndReenablesCollider()
    {
        Inventory inventory = Inventory("ClearInventory", 1);
        inventory.AddItemAuto(Item("Blocker", 1), 1);
        Item("Potion", 5);
        var pickup = Root("Pickup").AddComponent<ItemObject>();
        pickup.itemData = (ItemData)owned.Find(value => value is ItemData data && data.itemName == "Potion");
        var collider = pickup.gameObject.AddComponent<BoxCollider2D>();
        SetField(pickup, "isCollecting", true);
        var routine = (System.Collections.IEnumerator)typeof(ItemObject)
            .GetMethod("MoveAndCollect", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(pickup, new object[] { Root("Player").transform });
        Assert.That(routine.MoveNext(), Is.False);
        Assert.That(pickup != null, Is.True);
        Assert.That(collider.enabled, Is.True);
        Assert.That(typeof(ItemObject).GetField("isCollecting", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(pickup), Is.False);
    }

    private GameObject Root(string name)
    {
        var root = new GameObject(name);
        root.SetActive(false);
        owned.Add(root);
        return root;
    }

    private Inventory Inventory(string name, int capacity)
    {
        var ui = Root(name).AddComponent<InventoryUIManager>();
        var inventory = new Inventory(ui.gameObject, name, capacity);
        inventory.Init();
        inventory.SetupItemAcceptance(true, false, new List<string>());
        inventories.Add(name, inventory);
        return inventory;
    }

    private InventoryItem Item(string name, int maxStack)
    {
        var data = ScriptableObject.CreateInstance<ItemData>();
        data.itemName = name;
        data.itemType = ItemType.Consumable;
        data.maxStackAmount = maxStack;
        data.price = 10;
        owned.Add(data);
        var item = new InventoryItem(data);
        templates.Add(name, item);
        return item;
    }

    private static void AssertResult(ItemAddResult result, int added, int converted, int remaining)
    {
        Assert.That(result.Added, Is.EqualTo(added));
        Assert.That(result.Converted, Is.EqualTo(converted));
        Assert.That(result.Remaining, Is.EqualTo(remaining));
        Assert.That(result.Requested, Is.EqualTo(added + converted + remaining));
    }

    private static void SetField(object target, string name, object value)
        => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

    private static void SetStorage(InventoryManager value)
        => typeof(InventoryManager).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, value);
}

public class PowerUpButtonVisualRegressionTests
{
    private readonly List<GameObject> owned = new List<GameObject>();
    private float previousEstimatedDelta;
    private float previousActualDelta;

    [SetUp]
    public void SetUp()
    {
        previousEstimatedDelta = LeanTween.dtEstimated;
        previousActualDelta = LeanTween.dtActual;
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = owned.Count - 1; i >= 0; i--)
        {
            if (owned[i] == null) continue;
            LeanTween.cancel(owned[i]);
            Object.DestroyImmediate(owned[i]);
        }
        owned.Clear();
        LeanTween.dtEstimated = previousEstimatedDelta;
        LeanTween.dtActual = previousActualDelta;
    }

    [Test]
    public void InactiveFirstDeselect_PreservesAuthoredScaleAndDisablesDuplicateEffect()
    {
        var root = Root("Inactive power-up");
        var expected = new Vector3(0.8f, 0.9f, 1f);
        root.transform.localScale = expected;
        var genericEffect = root.AddComponent<CardUIEffects>();
        var button = root.AddComponent<PowerUpButton>();
        button.DeSelected(true);
        Assert.That(root.transform.localScale, Is.EqualTo(expected));
        Assert.That(genericEffect.enabled, Is.False);
        Assert.That(TweenId(button), Is.EqualTo(-1));
    }

    [Test]
    public void RebindDuringHover_RestoresScaleWithoutAccumulation()
    {
        var button = Button();
        var expected = button.transform.localScale;
        for (int i = 0; i < 100; i++)
        {
            button.OnPointerEnter(null);
            Step(button);
            button.Initialize(null);
            Assert.That(button.transform.localScale, Is.EqualTo(expected));
            Assert.That(button.IsSelected, Is.False);
            Assert.That(TweenId(button), Is.EqualTo(-1));
        }
    }

    [Test]
    public void RepeatedHover_KeepsOneTweenAndDeselectNeverOvershoots()
    {
        var button = Button();
        float normal = button.transform.localScale.x;
        button.OnPointerEnter(null);
        int id = TweenId(button);
        button.OnPointerEnter(null);
        Assert.That(TweenId(button), Is.EqualTo(id));
        Step(button);
        float previous = button.transform.localScale.x;
        Assert.That(previous, Is.InRange(normal, normal * button.hoverScaleFactor));
        button.DeSelected();
        var tween = LeanTween.descr(TweenId(button));
        Assert.That(tween.useEstimatedTime, Is.True);
        LeanTween.dtActual = 0f;
        LeanTween.dtEstimated = 0.02f;
        bool complete = false;
        for (int i = 0; i < 30 && !complete; i++)
        {
            complete = tween.updateInternal();
            float current = button.transform.localScale.x;
            Assert.That(current, Is.InRange(normal - 0.00001f, previous + 0.00001f));
            previous = current;
        }
        Assert.That(complete, Is.True);
        Assert.That(previous, Is.EqualTo(normal).Within(0.00001f));
    }

    [Test]
    public void SelectingAnotherButton_DoesNotAnimateUnselectedPeers()
    {
        var manager = Root("Power-up manager").AddComponent<PowerUpManager>();
        var a = Button();
        var b = Button();
        var c = Button();
        foreach (var button in new[] { a, b, c }) button.Initialize(manager);
        typeof(PowerUpManager).GetField("buttonPool", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(manager, new List<PowerUpButton> { a, b, c });
        a.Selected();
        Assert.That(TweenId(b), Is.EqualTo(-1));
        Assert.That(TweenId(c), Is.EqualTo(-1));
        Step(a);
        b.Selected();
        Assert.That(a.IsSelected, Is.False);
        Assert.That(b.IsSelected, Is.True);
        Assert.That(TweenId(c), Is.EqualTo(-1));
    }

    [Test]
    public void Disable_ClearsSelectionColorAndRunningTween()
    {
        var root = Root("Disable power-up");
        var graphic = root.AddComponent<UnityEngine.UI.Image>();
        graphic.color = Color.white;
        var button = root.AddComponent<PowerUpButton>();
        button.Initialize(null);
        root.SetActive(true);
        button.OnPointerEnter(null);
        Step(button);
        graphic.color = Color.yellow;
        button.IsSelected = true;
        typeof(PowerUpButton).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(button, null);
        Assert.That(button.IsSelected, Is.False);
        Assert.That(graphic.color, Is.EqualTo(Color.white));
        Assert.That(root.transform.localScale, Is.EqualTo(Vector3.one));
        Assert.That(TweenId(button), Is.EqualTo(-1));
    }

    private GameObject Root(string name)
    {
        var root = new GameObject(name, typeof(RectTransform));
        root.SetActive(false);
        owned.Add(root);
        return root;
    }

    private PowerUpButton Button()
    {
        var root = Root("Power-up button");
        root.transform.localScale = new Vector3(0.8f, 0.9f, 1f);
        var button = root.AddComponent<PowerUpButton>();
        button.Initialize(null);
        root.SetActive(true);
        return button;
    }

    private static int TweenId(PowerUpButton button)
        => (int)typeof(PowerUpButton).GetField("scaleTweenId", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(button);

    private static void Step(PowerUpButton button)
    {
        LeanTween.dtEstimated = 0.02f;
        LeanTween.dtActual = 0f;
        LeanTween.descr(TweenId(button)).updateInternal();
    }
}

public class ReviewFixRegressionTests
{
    private readonly List<Object> owned = new List<Object>();
    private PlayerStats previousStats;
    private EnemySwarmSystem previousSwarm;
    private PlayerStats stats;

    [SetUp]
    public void SetUp()
    {
        previousStats = PlayerStats.Instance;
        previousSwarm = EnemySwarmSystem.Instance;
        var root = Own(new GameObject("Stats regression"));
        root.SetActive(false);
        stats = root.AddComponent<PlayerStats>();
        PlayerStats.Instance = stats;
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = owned.Count - 1; i >= 0; i--)
            if (owned[i] != null) Object.DestroyImmediate(owned[i]);
        owned.Clear();
        PlayerStats.Instance = previousStats;
        EnemySwarmSystem.Instance = previousSwarm;
    }

    [Test]
    public void Gold_PreservesIntegerBeyondFloatPrecision()
    {
        stats.SetGold(16777217L);
        Assert.That(stats.GoldAmount, Is.EqualTo(16777217L));
        stats.SetGold(long.MaxValue);
        Assert.That(stats.GoldAmount, Is.EqualTo(long.MaxValue));
        stats.SetGold(-1);
        Assert.That(stats.GoldAmount, Is.Zero);
    }

    [Test]
    public void Experience_Level45CanAdvanceBeyondIntRange()
    {
        stats.level = 45;
        stats.requiredXP = 2798888000d;
        stats.currentXP = 2798887999L;
        stats.AddXP(1);
        Assert.That(stats.level, Is.EqualTo(46));
        Assert.That(stats.currentXP, Is.Zero);
        Assert.That(stats.requiredXP, Is.EqualTo(4198332000d));
    }

    [Test]
    public void Experience_IntegerThresholdAndRemainderAgree()
    {
        stats.requiredXP = 112.5d;
        stats.currentXP = 111;
        stats.AddXP(1);
        Assert.That(stats.level, Is.EqualTo(1));
        stats.AddXP(1);
        Assert.That(stats.level, Is.EqualTo(2));
        Assert.That(stats.currentXP, Is.Zero);
    }

    [Test]
    public void Experience_SaturatesWithoutNegativeValuesOrInfiniteGrowth()
    {
        stats.level = 100;
        stats.requiredXP = double.PositiveInfinity;
        stats.currentXP = long.MaxValue - 1;
        stats.AddXP(int.MaxValue);
        Assert.That(stats.level, Is.EqualTo(101));
        Assert.That(stats.currentXP, Is.EqualTo(PlayerStats.MaxRequiredXP - 1));
        Assert.That(stats.requiredXP, Is.EqualTo((double)PlayerStats.MaxRequiredXP));
        Assert.That(PlayerStats.NormalizeRequiredXP(double.NaN), Is.EqualTo(50d));
    }

    [Test]
    public void Save_ReadsLegacyNumbersAndRoundTripsLargeExperience()
    {
        var legacy = JsonUtility.FromJson<PlayerStatsData>("{\"currentXP\":123,\"requiredXP\":112.5}");
        Assert.That(legacy.currentXP, Is.EqualTo(123L));
        Assert.That(legacy.requiredXP, Is.EqualTo(112.5d));
        var data = new PlayerStatsData { currentXP = 9007199254740993L, requiredXP = 1e16 };
        var copy = JsonUtility.FromJson<PlayerStatsData>(JsonUtility.ToJson(data));
        Assert.That(copy.currentXP, Is.EqualTo(data.currentXP));
        Assert.That(copy.requiredXP, Is.EqualTo(data.requiredXP));
    }

    [TestCase(0f, 10f)]
    [TestCase(10f, 9f)]
    [TestCase(50f, 5f)]
    [TestCase(100f, 0.01f)]
    public void Cooldown_UsesPercentagePoints(float reduction, float expected)
    {
        stats.cooldownReduction = reduction;
        var weapon = Own(new GameObject("Cooldown regression")).AddComponent<MeteorWeapon>();
        var method = typeof(WeaponBase).GetMethod("GetCooldown", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That((float)method.Invoke(weapon, new object[] { 10f }), Is.EqualTo(expected).Within(0.0001f));
    }

    [Test]
    public void Barrier_RegistersOnAcquireAndReenableAndUnregistersOnDisable()
    {
        var root = Own(new GameObject("Barrier owner"));
        var health = root.AddComponent<PlayerHealth>();
        var child = Own(new GameObject("Barrier"));
        child.transform.SetParent(root.transform);
        var barrier = child.AddComponent<BarrierWeapon>();
        var data = Own(ScriptableObject.CreateInstance<BarrierDataSO>());
        barrier.Initialize(data);
        Assert.That(Field(health, "myBarrierSkill").GetValue(health), Is.SameAs(barrier));
        child.SetActive(false);
        Invoke(barrier, "OnDisable");
        Assert.That(Field(health, "myBarrierSkill").GetValue(health), Is.Null);
        child.SetActive(true);
        Invoke(barrier, "OnEnable");
        Assert.That(Field(health, "myBarrierSkill").GetValue(health), Is.SameAs(barrier));
    }

    [Test]
    public void Flame_OldSpawnCannotDamageOrRemoveNewSpawnsBurn()
    {
        var enemy = Own(new GameObject("Burn target")).AddComponent<MeleeEnemy>();
        enemy.SpawnVersion = 7;
        enemy.currentNormalState = Enemy.EnemyState.Chasing;
        var oldFlame = Own(new GameObject("Old flame")).AddComponent<SpreadingFlameEffect>();
        oldFlame.Attach(enemy, 1f, 10f, 1f, 2f, 0f, "test");
        Assert.That(SpreadingFlameEffect.IsBurning(enemy), Is.True);
        enemy.SpawnVersion++;
        Assert.That(SpreadingFlameEffect.IsBurning(enemy), Is.False);
        var valid = typeof(SpreadingFlameEffect).GetProperty("HasValidTarget", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That((bool)valid.GetValue(oldFlame), Is.False);
        var newFlame = Own(new GameObject("New flame")).AddComponent<SpreadingFlameEffect>();
        newFlame.Attach(enemy, 1f, 10f, 1f, 2f, 0f, "test");
        Invoke(oldFlame, "OnDisable");
        Assert.That(SpreadingFlameEffect.IsBurning(enemy), Is.True);
        Invoke(newFlame, "OnDisable");
        Assert.That(SpreadingFlameEffect.IsBurning(enemy), Is.False);
    }

    [Test]
    public void Meteor_IgnoresInvalidCandidatesAndSnapshotsActualPositions()
    {
        EnemySwarmSystem.Instance = null;
        var swarm = Own(new GameObject("Meteor swarm")).AddComponent<EnemySwarmSystem>();
        if (!swarm.grid.IsCreated) Invoke(swarm, "Awake");
        var enemy = Own(new GameObject("Meteor target")).AddComponent<MeleeEnemy>();
        enemy.currentNormalState = Enemy.EnemyState.Chasing;
        enemy.SpawnVersion = 4;
        enemy.transform.position = new Vector3(3f, 2f, 0f);
        swarm.activeEnemies.Add(enemy);
        swarm.activeEnemies.Add(null);
        ((uint[])Field(swarm, "indexedSpawnVersions").GetValue(swarm))[0] = 4;
        var weapon = Own(new GameObject("Meteor weapon")).AddComponent<MeteorWeapon>();
        Field(weapon, "currentMeteorNumber").SetValue(weapon, 5);
        var results = (List<int>)Field(weapon, "searchResults").GetValue(weapon);
        results.AddRange(new[] { -1, 0, 1, 99 });
        Invoke(weapon, "FindTargetPoints");
        var targets = (List<Vector3>)Field(weapon, "currentTargets").GetValue(weapon);
        Assert.That(targets.Count, Is.EqualTo(5));
        Assert.That(targets, Has.All.EqualTo(enemy.transform.position));
        enemy.SpawnVersion++;
        Invoke(weapon, "FindTargetPoints");
        Assert.That(targets, Is.Empty);
        Invoke(swarm, "OnDestroy");
    }

    [TestCase(0, 2, true)]
    [TestCase(1, 2, true)]
    [TestCase(2, 2, false)]
    [TestCase(-1, 2, false)]
    [TestCase(1, 1, false)]
    public void PowerUp_PurchaseRequiresNextLevelAndValidCost(int level, int costs, bool expected)
    {
        var data = Own(ScriptableObject.CreateInstance<PowerUpScriptableObject>());
        data.upgradeValues = new[] { 0f, 5f, 10f };
        data.costPerLevel = new long[costs];
        data.CurrentLevel = level;
        var method = typeof(PowerUpManager).GetMethod("TryGetPurchaseCost", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That((bool)method.Invoke(null, new object[] { data, 0L }), Is.EqualTo(expected));
    }

    [Test]
    public void Teleport_NodeClickDoesNotReplaceRegionSelection()
    {
        var root = Own(new GameObject("Teleport regression"));
        root.SetActive(false);
        var ui = root.AddComponent<TeleportUI>();
        var region = Own(new GameObject("Region", typeof(RectTransform))).AddComponent<UnityEngine.UI.Button>();
        var node = Own(new GameObject("Node", typeof(RectTransform))).AddComponent<UnityEngine.UI.Button>();
        Field(ui, "SelectbigButton").SetValue(ui, region);
        ui.OnTeleportButtonClick(Own(ScriptableObject.CreateInstance<TeleportData>()), node);
        Assert.That(Field(ui, "SelectbigButton").GetValue(ui), Is.SameAs(region));
    }

    private T Own<T>(T value) where T : Object { owned.Add(value); return value; }
    private static FieldInfo Field(object target, string name)
        => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
    private static void Invoke(object target, string name)
        => target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
}

public class EquipmentSkillRegressionTests
{
    private readonly List<Object> ownedObjects = new List<Object>();
    private PlayerWeaponController controller;

    [SetUp]
    public void SetUp()
    {
        var root = Own(new GameObject("Equipment regression test"));
        controller = root.AddComponent<PlayerWeaponController>();
        Field(typeof(PlayerWeaponController), "weaponHolder").SetValue(controller, root.transform);
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = ownedObjects.Count - 1; i >= 0; i--)
            if (ownedObjects[i] != null) Object.DestroyImmediate(ownedObjects[i]);
        ownedObjects.Clear();
    }

    [Test]
    public void ReapplyAndUnequip_PreserveLearnedLevelsAndCooldown()
    {
        ChainLightningDataSO data = CreateLightning();
        controller.AddWeapon(data);
        controller.AddWeapon(data);
        controller.AddWeapon(data);
        WeaponBase weapon = controller.GetWeapon(data);
        Field(typeof(ChainLightningWeapon), "cooldownTimer").SetValue(weapon, 7f);

        for (int i = 0; i < 20; i++)
            controller.SetEquipmentSkills(new WeaponDataSO[] { data, data });

        Assert.That(weapon.CurrentLevel, Is.EqualTo(5));
        Assert.That(controller.GetLearnedLevel(data), Is.EqualTo(3));
        Assert.That(controller.GetEquipmentLevel(data), Is.EqualTo(2));
        Assert.That(controller.GetWeapon(data), Is.SameAs(weapon));
        Assert.That(controller.activeWeapons.Count, Is.EqualTo(1));
        Assert.That(controller.WeaponsFromEquipment, Is.Empty);
        Assert.That(Read<float>(weapon, "currentBaseDamage"), Is.EqualTo(22f));

        controller.ClearEquipmentSkills();
        Assert.That(weapon.CurrentLevel, Is.EqualTo(3));
        Assert.That(Read<float>(weapon, "currentBaseDamage"), Is.EqualTo(16f));
        Assert.That(Read<int>(weapon, "currentChainCount"), Is.EqualTo(4));
        Assert.That(Read<float>(weapon, "cooldownTimer"), Is.EqualTo(7f));
    }

    [Test]
    public void LearningBorrowedSkill_MergesInstanceAndSurvivesUnequip()
    {
        ChainLightningDataSO data = CreateLightning();
        controller.SetEquipmentSkills(new WeaponDataSO[] { data, data });
        WeaponBase borrowed = controller.WeaponsFromEquipment[0];
        controller.AddWeapon(data);
        controller.AddWeapon(data);

        Assert.That(controller.GetWeapon(data), Is.SameAs(borrowed));
        Assert.That(borrowed.CurrentLevel, Is.EqualTo(4));
        Assert.That(controller.WeaponsFromEquipment, Is.Empty);
        controller.ClearEquipmentSkills();
        Assert.That(borrowed.CurrentLevel, Is.EqualTo(2));
        Assert.That(Read<float>(borrowed, "currentBaseDamage"), Is.EqualTo(13f));
    }

    [Test]
    public void EquipmentFusion_DoesNotConsumeLearnedIngredients()
    {
        ChainLightningDataSO ingredient = CreateLightning();
        ChainLightningDataSO fusion = CreateLightning();
        fusion.fusionWeaponData = new List<WeaponDataSO> { ingredient };
        controller.AddWeapon(ingredient);
        WeaponBase original = controller.GetWeapon(ingredient);
        controller.SetEquipmentSkills(new WeaponDataSO[] { fusion });
        Assert.That(controller.GetLearnedLevel(ingredient), Is.EqualTo(1));
        Assert.That(controller.GetWeapon(ingredient), Is.SameAs(original));
    }

    [Test]
    public void LearnedFusion_PreservesEquipmentContributionToIngredient()
    {
        ChainLightningDataSO ingredient = CreateLightning();
        ChainLightningDataSO fusion = CreateLightning();
        fusion.fusionWeaponData = new List<WeaponDataSO> { ingredient };
        controller.AddWeapon(ingredient);
        controller.SetEquipmentSkills(new WeaponDataSO[] { ingredient });
        WeaponBase original = controller.GetWeapon(ingredient);
        controller.AddWeapon(fusion);
        Assert.That(controller.GetLearnedLevel(ingredient), Is.Zero);
        Assert.That(controller.GetEquipmentLevel(ingredient), Is.EqualTo(1));
        Assert.That(controller.WeaponsFromEquipment[0], Is.SameAs(original));
        Assert.That(original.CurrentLevel, Is.EqualTo(1));
    }

    [Test]
    public void Exploration_NewEquipmentWeaponRemainsInactive()
    {
        controller.ToggleCombatMode(false);
        controller.SetEquipmentSkills(new WeaponDataSO[] { CreateLightning() });
        Assert.That(controller.WeaponsFromEquipment[0].gameObject.activeSelf, Is.False);
        controller.ToggleCombatMode(true);
        Assert.That(controller.WeaponsFromEquipment[0].gameObject.activeSelf, Is.True);
    }

    [Test]
    public void Barrier_RestoresUncappedStatsWithoutResettingRuntimeState()
    {
        var data = Own(ScriptableObject.CreateInstance<BarrierDataSO>());
        data.chargeTime = 4f;
        data.knockbackForce = 10f;
        data.weaponPrefab = Own(new GameObject("Barrier prototype"));
        data.weaponPrefab.AddComponent<BarrierWeapon>();
        controller.AddWeapon(data);
        var weapon = (BarrierWeapon)controller.GetWeapon(data);
        weapon.isBarrierActive = false;
        Field(typeof(BarrierWeapon), "chargeTimer").SetValue(weapon, 0.75f);

        weapon.SetLevel(20);
        Assert.That(Read<float>(weapon, "currentChargeTimeLimit"), Is.EqualTo(1f));
        weapon.SetLevel(1);
        Assert.That(Read<float>(weapon, "currentChargeTimeLimit"), Is.EqualTo(4f));
        Assert.That(Read<float>(weapon, "currentKnockbackForce"), Is.EqualTo(10f));
        Assert.That(Read<float>(weapon, "chargeTimer"), Is.EqualTo(0.75f));
        Assert.That(weapon.isBarrierActive, Is.False);
    }

    private ChainLightningDataSO CreateLightning()
    {
        var data = Own(ScriptableObject.CreateInstance<ChainLightningDataSO>());
        data.baseDamage = 10;
        data.chainCount = 2;
        data.weaponPrefab = Own(new GameObject("Lightning prototype"));
        data.weaponPrefab.AddComponent<ChainLightningWeapon>();
        return data;
    }

    private T Own<T>(T value) where T : Object
    {
        ownedObjects.Add(value);
        return value;
    }

    private static FieldInfo Field(System.Type type, string name)
        => type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);

    private static T Read<T>(object target, string name)
        => (T)Field(target.GetType(), name).GetValue(target);
}

public class SkillCollisionCapacityRegressionTests
{
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(int.MaxValue)]
    public void FirstFrame_GrowsForCurrentSkillsAndReusesCapacity(int maxHits)
    {
        const int enemyCount = 4000;
        SkillCollisionManager previousManager = SkillCollisionManager.Instance;
        EnemySwarmSystem previousSwarm = EnemySwarmSystem.Instance;
        var root = new GameObject("Collision capacity regression test");
        SkillCollisionManager manager = null;
        EnemySwarmSystem swarm = null;
        try
        {
            SkillCollisionManager.Instance = null;
            EnemySwarmSystem.Instance = null;
            manager = root.AddComponent<SkillCollisionManager>();
            if (!Results(manager).IsCreated) Invoke(manager, "Awake");
            swarm = root.AddComponent<EnemySwarmSystem>();
            EnemySwarmSystem.Instance = swarm;
            if (swarm.grid.IsCreated) swarm.grid.Dispose();
            if (swarm.nextHitTimes.IsCreated) swarm.nextHitTimes.Dispose();
            swarm.positions = new NativeArray<float2>(enemyCount, Allocator.Persistent);
            swarm.enemyRadii = new NativeArray<float>(enemyCount, Allocator.Persistent);
            swarm.nextHitTimes = new NativeArray<float>(enemyCount, Allocator.Persistent);
            swarm.grid = new NativeParallelMultiHashMap<long, int>(enemyCount, Allocator.Persistent);
            swarm.cellSize = 1f;
            // Empty grid isolates allocation from damage callbacks.
            for (int i = 0; i < enemyCount; i++) swarm.activeEnemies.Add(null);
            var skill = root.AddComponent<Orb>();
            skill.maxHits = maxHits;
            manager.RegisterSkill(skill);

            Invoke(manager, "LateUpdate");
            int capacity = Results(manager).Capacity;
            Assert.That(capacity, Is.GreaterThanOrEqualTo(enemyCount));
            Assert.That(capacity, Is.LessThanOrEqualTo(enemyCount * 2));
            skill.maxHits = 1;
            Invoke(manager, "LateUpdate");
            Assert.That(Results(manager).Capacity, Is.EqualTo(capacity));
        }
        finally
        {
            if (manager != null) Invoke(manager, "OnDestroy");
            if (swarm != null) Invoke(swarm, "OnDestroy");
            Object.DestroyImmediate(root);
            SkillCollisionManager.Instance = previousManager;
            EnemySwarmSystem.Instance = previousSwarm;
        }
    }

    [Test]
    public void MixedLimitedAndUnlimitedSkills_FitTheCalculatedUpperBound()
    {
        const int enemyCount = 100;
        using var limits = new NativeArray<int>(new[] { -1, 0, 1, 3, int.MaxValue }, Allocator.TempJob);
        using var skillPositions = new NativeArray<float2>(limits.Length, Allocator.TempJob);
        using var skillRadii = new NativeArray<float>(new[] { 1f, 1f, 1f, 1f, 1f }, Allocator.TempJob);
        using var enemyPositions = new NativeArray<float2>(enemyCount, Allocator.TempJob);
        using var enemyRadii = new NativeArray<float>(enemyCount, Allocator.TempJob);
        using var nextHitTimes = new NativeArray<float>(enemyCount, Allocator.TempJob);
        using var grid = new NativeParallelMultiHashMap<long, int>(enemyCount, Allocator.TempJob);
        // 100 + 100 + 1 + 3 + 100, not a sum of raw maxHits.
        using var results = new NativeParallelMultiHashMap<int, int>(304, Allocator.TempJob);
        for (int i = 0; i < enemyCount; i++) grid.Add(0L, i);
        var job = new SkillCollisionManager.SkillCollisionJob
        {
            skillPositions = skillPositions, skillRadii = skillRadii, skillMaxHits = limits,
            enemyPositions = enemyPositions, enemyRadii = enemyRadii, nextHitTimes = nextHitTimes,
            enemyGrid = grid, cellSize = 1f, currentTime = 1f, hitResults = results.AsParallelWriter()
        };
        job.Schedule(limits.Length, 1).Complete();
        Assert.That(results.Count(), Is.EqualTo(304));
    }

    private static NativeParallelMultiHashMap<int, int> Results(SkillCollisionManager manager)
        => (NativeParallelMultiHashMap<int, int>)typeof(SkillCollisionManager)
            .GetField("hitResults", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager);

    private static void Invoke(object target, string method)
        => target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
}
