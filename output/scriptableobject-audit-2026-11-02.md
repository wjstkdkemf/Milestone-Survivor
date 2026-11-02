# ScriptableObject 검토 보고서

검토일: 2026-11-02

## 범위와 판정 기준

- 프로젝트 자체 ScriptableObject 25종과 해당 스크립트에 연결된 `.asset` 111개를 조사했다.
- 정의, C# 사용처, 씬/프리팹/에셋 GUID 참조, Resources 및 Addressables 경로를 확인했다.
- `Library`, 빌드 산출물, 패키지 내부의 Unity 제공 SO는 자체 SO 목록에서 제외했다.
- 아래의 미연결 에셋은 현재 저장된 프로젝트 기준 정리 후보이지, 향후 사용할 계획까지 없다는 뜻은 아니다.
- Unity Play Mode와 빌드는 실행하지 않았다. Inspector에서 저장하지 않은 수정은 이 결과에 포함되지 않는다.
- 소스 코드와 기존 에셋은 수정하지 않았다.

## 우선 확인할 사항

### 1. [P2] AuraDataSO.applyDamage는 동작을 제어하지 않는다

[AuraDataSO.cs:18](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/AuraDataSO.cs:18>)의 `applyDamage`를 꺼도 피해가 비활성화되지 않는다.

`AuraWeapon.Initialize()`는 `applySlow`와 `slowPercentage`는 전달하지만 `applyDamage`는 읽지 않는다. `AuraZone`도 피해 주기가 되면 피해를 처리하며 별도 피해 활성화 조건이 없다.

권장: 피해 없는 감속 전용 오라를 만들 계획이면 이 옵션을 실제 처리에 연결한다. 항상 피해를 주는 스킬이라면 필드를 제거한다. 단순 숨김은 기능이 연결되지 않은 문제를 해결하지 않는다.

### 2. [P2] 캐릭터의 DobleDamageChance는 표시와 실제 적용이 분리돼 있다

[StatEntryFactory.cs:20](G:/unity/Milestone-Survivor-main/Assets/Scripts/UI/Stats/StatEntryFactory.cs:20)은 캐릭터 SO의 값을 표시한다. 하지만 [CharacterSelection.cs:36](G:/unity/Milestone-Survivor-main/Assets/Scripts/Character/CharacterSelection.cs:36)의 초기화 호출에는 이 값이 없다.

[PlayerStatsCalculate.cs:91](G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/PlayerStatsCalculate.cs:91)의 `SetBaseStats()`에도 대응 인자가 없으며, 실제 확률은 별도 `baseDoubleDamageChance`를 사용한다. 따라서 캐릭터 SO에서 값을 올리는 것만으로는 전투 확률이 오르지 않는다.

권장: 캐릭터마다 기본 확률을 줄 계획이면 SO 값을 계산기에 전달한다. 화면에 읽는 곳이 있으므로 미사용 필드로 분류해 삭제할 항목은 아니다.

### 3. [P3] 피해량 설정이 SpreadingFlame만 이중으로 존재한다

[SpreadingFlameDataSO.cs:13](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/Elemental/Arc Elemental/SpreadingFlameDataSO.cs:13>)에는 부모의 `baseDamage`와 자신의 `damagePerTick`이 함께 있다.

실제 계산은 `SpreadingFlameWeapon.Initialize()` 및 `RecalculateLevelStats()`에서 `damagePerTick`을 사용한다. [UpgradePreviewResolver.cs:78](G:/unity/Milestone-Survivor-main/Assets/Scripts/Upgrades/UpgradePreviewResolver.cs:78)에도 이 타입만 별도로 분기하는 코드가 있다.

권장: 틱 1회의 기본 피해를 `baseDamage`로 통일할 수 있다. 이때 기존 `damagePerTick` 값을 먼저 옮기고, 무기 계산과 미리보기의 읽는 곳도 함께 변경해야 한다. 필드만 삭제하면 안 된다.

### 4. [P3] Inspector에 남아 있지만 읽지 않는 필드

| SO | 필드 | 현재 상태 / 권장 |
|---|---|---|
| CharacterScriptableObject | `CharacterPrefab` | C# 사용처 없음. 현재 캐릭터 적용은 기존 Player의 애니메이터와 비주얼 교체. 프리팹 교체 계획이 없다면 제거 후보. |
| TeleportData | `mapIcon` | C# 사용처 없음. `previewImage`와는 별개이며 후자는 실제 사용 중. |
| JobDataSO | `jobTier` | 전직 순서는 `nextAbleJobs`와 조건으로 처리. 차수 표시 계획이 없다면 제거 후보. |
| JobDataSO | `description` | 현재 전직 UI/판정에서 읽지 않음. 설명을 표시할 계획이면 유지하고 연결. |
| TurretWeaponDataSO | `fireRate` | 실제 발사 간격은 `baseCooldown` 사용. 이 필드는 제거 후보. |
| MeteorWeaponSO | `targetUpdateRate` | 현재 MeteorWeapon이 읽지 않음. 검색 주기 옵션을 지원하지 않을 것이면 제거 후보. |

주의: `TurretFireTrailOrbDataSO.fireRate` 및 `TurretWeaponDataSO.targetUpdateRate`는 사용 중이다. 이름이 같다고 함께 지우면 안 된다.

### 5. [P3] StageScriptableObject는 SO 전체 정리 후보에 가장 가깝다

[StageScriptableObject.cs:5](G:/unity/Milestone-Survivor-main/Assets/Scripts/StageScriptableObject.cs:5)는 기존 스테이지 선택용 데이터다.

- `SceneIndex`를 이용한 씬 이동은 `StageSelection.StartThegame()`에서 주석으로만 남아 있다.
- `TimeLimit`, `ClockSpeed`, `HPBoost`, `DamageBoost`, `MovementSpeed`, `LuckBoost`, `XPBoost`는 이 SO를 통한 실제 전투 적용 경로 없이 문자열 표시에서 읽힌다.
- Stage 에셋 4개 및 `StageSelection`/`StageButtonUi` 컴포넌트의 저장된 씬/프리팹 연결을 찾지 못했다.

권장: 기존 스테이지 선택 화면을 폐기한 것이 맞다면 관련 UI와 함께 보관 또는 삭제할 수 있다. 현재 텔레포트 시스템과 목적이 일부 겹친다는 이유만으로 필드를 자동 병합할 필요는 없다.

### 6. UpgradeScriptableObject와 WeaponDataSO는 역할이 다르지만 연결은 수동이다

[UpgradeScriptableObject.cs:40](G:/unity/Milestone-Survivor-main/Assets/Scripts/Upgrades/UpgradeScriptableObject.cs:40)의 `linkedWeaponData`는 카드에서 실제 무기로 가는 연결이다. [WeaponDataSO.cs:22](G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/skill/WeaponDataSO.cs:22)의 `upgradeData`는 무기에서 UI 정보로 가는 역방향 연결이다.

`upgradeData`는 [PlayerSkillHUD.cs:29](G:/unity/Milestone-Survivor-main/Assets/Scripts/UI/PlayerSkillHUD.cs:29)에서 아이콘을 찾을 때 실제로 사용한다. 현재 코드에서 이 참조를 자동 주입하는 경로는 발견하지 못했다. 따라서 지금 숨기기만 하면 필수 연결을 설정하기 어려워진다.

- 카드: 이름, 설명, 아이콘, 등급, 투자 횟수, 출현 확률, 일반 스탯 카드의 효과.
- 무기: 프리팹, 기본 수치, 성장 규칙, 충돌 설정, 융합 재료, 공격 모션.
- 무기 쪽 이름/설명/아이콘의 예전 선언은 이미 주석 처리돼 있다. YAML에 남아 있는 예전 값은 현재 활성 필드가 아니다.
- 현재 연결된 카드 중 `Item/U_Barrier.asset`는 Common_Skill의 무기를 가리키고, 그 무기는 Common_Skill의 카드를 가리킨다. 여러 카드가 같은 무기를 쓰는 의도일 수 있어 곧바로 오류라고 단정할 수는 없다.
- `Item/W_Barrier Data.asset`는 현재 들어오는 GUID 참조가 없다. 아이템 전용 베리어 설정을 바꾸려던 것이라면 이 파일을 수정해도 현재 연결에 반영되지 않는다.

권장: 당장은 양쪽 연결을 유지하고 검증하는 편이 작다. 자동 역참조 생성이나 공통 UI 데이터 분리는 연결 정책을 먼저 정한 다음 도입한다.

## SO 25종 개별 판정

아래의 '유지'는 구조상 유지할 근거가 있다는 뜻이며, 해당 스킬의 모든 동작을 실행 검증했다는 뜻은 아니다. 스킬별 공통 상속 필드는 다음 절의 별도 사항도 참고한다.

| SO / 정의 파일 | 에셋 수 | 역할과 판정 |
|---|---:|---|
| [CharacterScriptableObject](G:/unity/Milestone-Survivor-main/Assets/Scripts/Character/CharacterScriptableObject.cs:8) | 5 | 유지. 기본 능력치, 레벨 보정, 시작 무기/덱, 비주얼/구매 정보를 제공. `CharacterPrefab` 미사용, 기본 더블데미지 연결 누락은 위 내용 참고. |
| [WeaponMotionData](G:/unity/Milestone-Survivor-main/Assets/Scripts/Character/WeaponMotionData.cs:5) | 1 | 유지. 모션 커브, 길이, 블렌드, 우선순위/중단 설정을 WeaponAnimationController가 소비. |
| [ItemData](G:/unity/Milestone-Survivor-main/Assets/Scripts/Item/ItemData.cs:25) | 1 | 기반 타입 유지. InventoryItem이 표시/스택/조작/가격 정보를 복사. 단독 샘플 에셋은 미연결 후보. |
| [EquipmentData](G:/unity/Milestone-Survivor-main/Assets/Scripts/Item/EquipmentData.cs:24) | 5 | 유지. ItemData에 장비 부위, 스탯 보정, 장착 무기 목록을 추가. 상속은 중복 선언이 아님. |
| [PowerUpScriptableObject](G:/unity/Milestone-Survivor-main/Assets/Scripts/PowerUp/PowerUpScriptableObject.cs:5) | 16 | 유지. 영구 강화 비용/효과/표시. `CurrentLevel`은 런타임 상태 분리 후보이나 미사용 값은 아님. |
| [StageScriptableObject](G:/unity/Milestone-Survivor-main/Assets/Scripts/StageScriptableObject.cs:5) | 4 | 기존 화면을 폐기했다면 타입 전체 정리 후보. 현재 전투 적용/에셋 연결이 없음. |
| [TeleportData](G:/unity/Milestone-Survivor-main/Assets/Scripts/Teleport/TeleportData.cs:6) | 21 | 유지. 노드, 목적지, 연결선, 미리보기. `mapIcon`만 현행 코드 사용처 없음. |
| [TeleportDatabase](G:/unity/Milestone-Survivor-main/Assets/Scripts/Teleport/TeleportDatabase.cs:6) | 1 | 유지. 지역별 노드를 묶는 카탈로그이며 개별 TeleportData와 역할이 다름. |
| [QuestDataSO](G:/unity/Milestone-Survivor-main/Assets/Scripts/Quest/QuestDataSO.cs:29) | 4 | 유지. 조건/보상/지역/확률/다음 퀘스트를 정의. 명확한 미사용 필드는 발견하지 못함. |
| [JobDataSO](G:/unity/Milestone-Survivor-main/Assets/Scripts/Upgrades/JobDataSO.cs:5) | 11 | 유지. 조건/전직 분기/덱 변경. `description`, `jobTier` 정리 또는 표시 연결 후보. |
| [UpgradeScriptableObject](G:/unity/Milestone-Survivor-main/Assets/Scripts/Upgrades/UpgradeScriptableObject.cs:9) | 26 | 유지. 카드 정보와 투자/덱 정보. 무기 SO와 통합할 필요 없음. 역참조와 런타임 상태는 별도 정리 후보. |
| [StatIconDatabase](G:/unity/Milestone-Survivor-main/Assets/Scripts/UI/Stats/StatIconDatabase.cs:5) | 1 | 유지. 스탯 키별 아이콘 원본 목록과 조회용 사전은 데이터/캐시 관계이지 중복 설정이 아님. |
| [WeaponDataSO](G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/skill/WeaponDataSO.cs:8) | 1 | 기반 타입 유지. 단독 NewWeapon 에셋은 미연결. 상속된 공통 필드는 스킬별 필요성 차이가 있음. |
| [TurretWeaponDataSO](G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/TurretWeaponDataSO.cs:4) | 1 | 유지. `fireRate`는 제거 후보, 실제 쿨타임은 부모 `baseCooldown`. 나머지 고유 설정은 사용됨. |
| [ChainLightningDataSO](G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/ChainLightningDataSO.cs:4) | 1 | 유지. 최초 검색 범위/연쇄 범위/횟수/피해 감쇠는 서로 다른 역할. |
| [AuraDataSO](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/AuraDataSO.cs:4>) | 1 | 유지. 반경/틱 간격/감속이 고유 설정. `applyDamage`는 실제 연결 또는 제거 필요. |
| [EvolvedFireballDataSO](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/EvolvedFireballDataSO.cs:4>) | 1 | 유지. 발사와 잔류 불길 설정 사용. 다른 화염구 타입과 선언 구조를 공유할 여지는 있음. |
| [MeteorWeaponSO](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/MeteorWeaponSO.cs:4>) | 1 | 유지. 밀집 탐색/경고/연속 발사 설정이 고유. `targetUpdateRate` 미사용. |
| [OrbWeaponDataSO](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/OrbWeaponDataSO.cs:4>) | 1 | 유지. 회전 반경/속도/개수/프리팹 사용. 불길 오브의 부모로도 필요. |
| [BarrierDataSO](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/Elemental/BarrierDataSO.cs:4>) | 2 | 유지. 재충전/밀치기/시각효과는 고유 정보. Item 폴더의 별도 에셋은 미연결 후보. |
| [FireTrailOrbDataSO](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/Elemental/FireTrailOrbDataSO.cs:5>) | 1 | 유지. 오브 정보에 불길 생성 주기/수명/계수를 추가하는 정상적인 확장. |
| [ElementalFireballSO](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/Elemental/ElementalFireballSO.cs:6>) | 2 | 유지. 폭발 크기/프리팹을 포함. Elemental/Arc 두 에셋은 같은 타입의 별도 설정으로 활용. |
| [SpreadingFlameDataSO](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/Elemental/Arc Elemental/SpreadingFlameDataSO.cs:4>) | 1 | 유지하되 `damagePerTick`과 상속 `baseDamage` 통합 후보. 탐색/전이 반경은 별개. |
| [TurretFireTrailOrbDataSO](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/Elemental/Arc Elemental/TurretFireTrailOrbDataSO.cs:5>) | 1 | 유지. 오브 불길 피해와 추가 발사체 피해는 별개라 각각의 계수가 필요. `fireRate` 사용 중. |
| [CelestialFireballSO](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/Elemental/Arc Elemental/Celestial/CelestialFireballSO.cs:6>) | 1 | 유지. 화염구 공통 설정에 연쇄 조건 추가. 직격과 폭발의 복합 피해 의도를 중복 오류로 취급하지 않음. |

## 중복처럼 보여도 바로 제거하면 안 되는 정보

- `UpgradeScriptableObject.Points`와 무기의 `CurrentLevel`: 카드 투자 횟수와 장비 등을 포함한 실제 무기 레벨로 역할이 다르다.
- `Chance`와 `InitialChance`: 현재 덱 가중치와 런 시작 초기값이다. `ResetRunData()`가 초기값으로 복구한다.
- `PowerUp.isPercentage`와 `bonusMode`: 표시 대상 수치의 단위와 실제 계산 방식이다. 하나로 합치면 퍼센트 값에 대한 Flat 보정을 구분하기 어려워진다.
- `CharacterName`, `powerUpName`, `itemName`과 번역 이름: 일부 문자열은 저장/조회 식별자로도 사용한다. 표시 문자열로 치환하거나 제거하면 저장 호환성 문제가 생길 수 있다.
- `TeleportData.nodeID`, `pointID`, `targetSpawnPointID`: 노드 식별, 목적지 맵, 목적지 스폰 지점이다.
- `LocalizedString`, 문자열 번역 키, 원문 문자열: Upgrade/Quest에서는 순서가 있는 대체값 경로다. 단일 방식으로 이관할 수 있지만 현재 사용 데이터를 옮기기 전 일괄 삭제하면 안 된다.
- `UpgradeLevelInfo`의 설명과 `GrowthRules`: 설명 문구와 실제 수치 규칙이다. 같은 수치를 문구에 직접 적으면 불일치 위험은 있지만, 별도의 기획 문구가 필요한 용도까지 없어지는 것은 아니다.
- `ItemData`를 `InventoryItem`에 복사하는 것: 장비별 강화/수량 등 인스턴스 상태와 원본 정의를 구분하기 위한 복사다. SO에 역으로 상태를 몰아넣지 않는다.

## 구조 정리 후보

### 스킬별로 의미 없는 부모 필드

모든 스킬이 `WeaponDataSO`를 상속하므로 Barrier에도 일반 피해/발사체 관련 항목이 보인다. 이것은 각 스킬에 의미 없는 Inspector 항목이 노출되는 문제이지, 부모 필드가 프로젝트 전체에서 미사용이라는 의미는 아니다.

당장은 타입에 맞는 Inspector 표시로 혼란을 줄이는 편이 작다. 이후 공격형/회전형/방어형의 공통 설정을 나눌 수 있으나, 지금 모든 스킬의 상속 구조를 바꿀 필요는 없다.

### 화염구 3종의 공통 선언

`EvolvedFireballDataSO`, `ElementalFireballSO`, `CelestialFireballSO`에는 `bulletNumber`, `range`, `trailDamageScaling`, `trailDuration`, `trailSpawnDistance`, `fireballPrefab`, `trailPrefab` 등 비슷한 필드가 반복된다. 폭발형 두 타입은 폭발 설정도 겹친다.

이는 선언 구조를 공통화할 후보이지, 개별 스킬의 밸런스 값을 하나로 강제 공유할 이유는 아니다. 공통 기반 타입이나 직렬화 설정 구조체를 도입할 수 있지만, 구조체로 중첩 경로를 바꾸면 기존 에셋 값의 이관이 필요하다.

### 런타임 상태 분리

`Upgrade.Points/Chance`, `PowerUp.CurrentLevel`, `Character.purchased`는 읽고 쓰는 상태다. 미사용 필드가 아니다. 장기적으로 원본 설정과 플레이 상태를 분리하면 관리하기 편하지만, 지금 숨기거나 삭제하는 것만으로 대체할 수 없다.

## 미연결 및 복제 에셋 후보

현재 씬/프리팹/에셋에서 들어오는 GUID 참조를 찾지 못한 목록이다. 아래 후보들은 Resources 장비 에셋과 구분했다. 외부 콘텐츠나 향후 기획상 필요 여부는 삭제 전에 확인해야 한다.

| 위치 | 후보 |
|---|---|
| `Assets/Prefabs/Data/Character` | `Leaf Ranger 1.asset`. `Leaf Ranger.asset`와 파일 내부 이름을 제외한 설정이 동일하고, 원본만 씬에서 참조됨. 가장 명확한 복제 정리 후보. |
| `Assets/Prefabs/Skill` | `NewWeapon.asset` |
| `Assets/Scripts/Item` | `New Item Data.asset` |
| `Assets/Prefabs/Skill/Item` | `W_Barrier Data.asset`. `U_Barrier.asset`와 ItemBarrierWeapon 프리팹은 Common_Skill 쪽 데이터를 참조하고 있음. |
| `Assets/Prefabs/Skill/1.Mage/Lightning Mage` | `J_Lightning Mage.asset` |
| `Assets/Scripts/Player/Mage/Lightning Mage` | `LightSpark.asset` |
| `Assets` | `LightningBolt.asset`, `KnifeThrowing.asset`, `SowrdSlash.asset` |
| `Assets/Prefabs/Data/Stage` | `Level 1.asset`부터 `Level 4.asset` |
| `Assets/Prefabs/Map/Teleport` | 루트의 `TeleportPoint1.asset`, `TeleportPoint2.asset` |
| `Assets/Prefabs/Data/PowerUps` | `Xp Boost`, `Regeneration`, `Move Speed`, `Luck`, `Knock Back`, `DoubleDamage`, `defense_boost`, `Damage`, `CoolDown` 에셋 |

삭제하면 안 되는 반례:

- `J_Fire Mage.asset`와 `J_Fire Mage 1.asset`는 둘 다 씬/전직 경로에서 참조한다. 이름/보상은 같지만 후자는 요구 스킬이 하나 더 많다. 불필요한 테스트 분기인지 확인할 수는 있으나 단순 복제라고 단정할 수 없다.
- PowerUps의 `Level 0` 및 하위 `Level 1`의 4개 에셋은 Village/Main Menu 씬에서 참조한다. 폴더 이름이 오래돼 보인다고 제거할 수 없다.
- `Resources/Items`의 장비는 이름 기반 로딩 경로가 있다. 직접 참조 수만으로 사용 여부를 판단하지 않는다.
- `.asset`에 `weaponName`, `weaponDescription`, `enemyLayerMask` 등 예전 필드가 남아 있어도 현재 클래스에 해당 선언이 없다면 활성 설정이 아니라 직렬화 잔재다. 새 중복 필드로 세지 않았으며, 이를 위해 YAML을 일괄 편집할 필요도 없다.

## 권장 진행 순서

1. 오라 피해 옵션과 캐릭터 기본 더블데미지처럼 설정이 실제로 반영되지 않는 두 경로를 먼저 결정한다.
2. 미사용 필드 6개를 향후 사용 계획과 대조해 제거하거나 연결한다.
3. SpreadingFlame의 피해량 원본을 통일하고 기존 에셋 값을 이관한다.
4. 베리어가 아이템 전용 무기를 가져야 하는지, 같은 무기를 공유해야 하는지 결정한 뒤 참조를 정리한다.
5. Leaf Ranger 복제본 및 미연결 에셋을 별도로 정리한다. Stage 시스템 폐기도 이 단계에서 판단한다.
6. 마지막으로 Inspector 표시 개선과 화염구 공통 설정 추출을 검토한다. 이를 위해 모든 SO를 한 번에 재설계할 필요는 없다.
