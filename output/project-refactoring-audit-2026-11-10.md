# 프로젝트 구조 및 실행 점검

검토일: 2026-11-10. 게임 코드, 프리팹, SO는 수정하지 않았다.

## 검증 범위와 한계

- Assets/Scripts와 Assets/InventoryAsset의 스킬, 전투, 적 등록, 충돌, 풀, 스탯, 인벤토리, 씬 초기화, 맵, 업그레이드 UI 및 관련 데이터 경로를 검토했다. 모든 조합을 실행 검증했다는 뜻은 아니다.
- 사용자 허가를 받아 기존 테스트 데이터로 Unity Play Mode를 확인했다. 캐릭터 선택의 Refund는 실제 선택 확정 버튼이라는 사용자 설명을 반영했다.
- 사용자가 옮겨준 던전 1F에서 이어서 확인했고, 사용자가 전투 위치로 이동한 뒤 몬스터 다수 등장, 피격, 배리어 실행 로그, 레벨업 카드 표시를 확인했다.
- 레벨업 화면 위에 메뉴를 열었다 닫아도 레벨업 화면과 정지 상태가 유지됐다. 연쇄 번개 첫 클릭은 전직 정보 표시, 두 번째 클릭은 강화 적용으로 이어졌다. 적용 로그는 피해량 13, 체인 횟수 3이었고 다음 카드 미리보기도 갱신됐다.
- 마지막 확인 시 콘솔 오류 수는 0이었다. 경고는 남아 있었다. 오류 0은 전체 시스템의 무결성 보장이 아니다.
- 배리어 실행 경로는 로그로 확인했지만, 폭발 연출의 정확한 반경과 모든 적의 넉백 거리까지 검증하지는 않았다. 모든 지속형 스킬, 융합, 전직, 던전 완주, 마을 복귀를 이번 실행에서 전부 검증하지 않았다.
- CPU 시간, GC 바이트, 메모리 최대 사용량은 Profiler로 측정하지 않았다. 아래 성능 항목은 코드상 반복 작업이 확인된 후보이며 프레임 저하 수치를 뜻하지 않는다.
- 실행은 남은 레벨업 선택 화면에 두었다. 씬 저장, 저장 데이터 초기화, 사용자 변경 취소는 하지 않았다.

## 우선 수정할 문제

### 1. [P2] 스탯 업그레이드 SO와 현재 enum/계산 코드가 맞지 않는다

분류: 데이터와 코드로 확인, 실행 화면에서도 증상 확인.

- [AddDamge.asset](G:/unity/Milestone-Survivor-main/Assets/Prefabs/Data/UpgradeScriptableObject/AddDamge.asset:45)은 upgradeType=6, bonusMode=0, statValue=0이다. 현재 enum에서 6은 Stat_Speed이며 공격력인 Stat_Damage는 4다.
- [AddMaxHealth.asset](G:/unity/Milestone-Survivor-main/Assets/Prefabs/Data/UpgradeScriptableObject/AddMaxHealth.asset:24)은 제목이 +10% Max Health지만 statValue=0이다. bonusMode도 저장되지 않아 선언 기본값인 Flat을 사용한다.
- Movement Speed.asset 역시 타입 6을 사용한다. 현재 이동 속도 타입은 Stat_MoveSpeed=3이다. Attack Speed.asset도 타입 6, 값 0이다. 공격 속도는 지원할 실제 능력치부터 결정해야 한다.
- [AddRealTimeBonus](G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/PlayerStatsCalculate.cs:182)는 지원하는 타입의 값만 누적한다. Stat_Speed는 처리되지 않는다. 선택 횟수는 증가하더라도 스탯 효과는 없는 카드가 된다.
- 실제 카드에서 공격력은 효과 - → -, 최대 체력은 0 → 0으로 표시됐다. 단순 텍스트 배치 문제가 아니다.

방향: 카드별 의도한 타입, Flat/Percent, statValue를 먼저 표로 확정하고 자산을 이관한다. 이후 enum에 명시적 숫자 값을 유지하고, 지원하지 않는 타입/의도치 않은 0값/누락 연결을 실행 전에 검사한다. enum 순서만 다시 바꾸어 고치면 다른 저장 자산을 망가뜨릴 수 있다.

### 2. [P2] 공격력 변경이 지속형 스킬의 공격 주기까지 초기화한다

분류: 호출 경로 확인, 해당 스킬의 실행 재현은 미수행.

- [AuraWeapon.RefreshDamage](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/AuraWeapon.cs:90>) → RecalculateLevelStats → SetupAura 경로가 [AuraZone의 두 타이머](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/AuraZone.cs:28>)를 0으로 만든다. 다음 Update에서 정상 틱 간격 이전에 타격 판정을 다시 할 수 있다. 실제 피해는 공통 피격 제한에도 영향을 받는다.
- [FireTrailOrbWeapon.RefreshDamage](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/Elemental/FireTrailOrbWeapon.cs:52>)는 기존 오브에 SetTrailInfo를 다시 적용한다. 이 함수는 이전 위치와 isFirstSpawn을 초기화한다. 데미지만 변경해도 새 궤적 생성 조건이 달라진다.
- [TurretFireTrailOrbWeapon.RefreshDamage](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/Elemental/Arc Elemental/TurretFireTrailOrbWeapon.cs:53>)도 SetTurretInfo를 재호출하며 [fireTimer](<G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/Mage/Fire Mage/Elemental/Arc Elemental/TurretFireTrailOrb.cs:24>)를 0으로 만든다.

방향: 최초 생성용 Initialize/Setup과 기존 객체의 수치 갱신을 분리한다. 공격력 이벤트에서는 피해량만 바꾸고, 레벨업 수치 갱신에서도 진행 중인 쿨타임·궤적 기준점을 유지한다. 새로운 오브가 추가될 때만 새 오브를 초기화한다. 공격력 변경 이벤트 자체는 유지하는 편이 맞다.

### 3. [P2] 숲의 서로 다른 이동 노드가 같은 ID로 해석된다

분류: 자산과 런타임 경고 모두 확인.

숲 1-1~1-9 자산에 독립 nodeID가 없고 targetSpawnPointID가 TeleportPoint1-1로 같다. [GetNodeID](G:/unity/Milestone-Survivor-main/Assets/Scripts/Teleport/TeleportData.cs:42)는 nodeID가 없으면 targetSpawnPointID를 사용한다. [BuildPointLookup](G:/unity/Milestone-Survivor-main/Assets/Scripts/Teleport/TeleportMapMaker.cs:144)는 중복을 경고하고 해당 중복 항목의 등록을 건너뛴다.

방향: 도착 스폰 지점과 지도 노드 ID를 구분한다. 같은 도착 지점을 사용하더라도 nodeID는 개별적으로 부여한다. 스폰 지점 ID를 임의로 바꾸어 경고만 없애지 않는다.

### 4. [P2] 풀의 생성·반환 정책이 서로 다른 곳에서 결정된다

분류: 반환 실패 경고 재현, 후속 조사에서 던전 1-1의 생성/반환 설정 불일치 확인.

전투에서 `Pool not found for 'Goblin'. Destroying.` 경고가 반복됐다. 이 분기는 실제로 객체를 파괴한다.

- [WaveSpawner](G:/unity/Milestone-Survivor-main/Assets/Scripts/SpawnSyestem/WaveSpawner.cs:404)는 Wave의 DontUseObjectPooling으로 생성 방법을 정한다.
- [Enemy.Die](G:/unity/Milestone-Survivor-main/Assets/Scripts/Enemy/Enemy.cs:573)는 별도 Enemy 필드로 반환 방법을 정한다.
- [ObjectPoolingManager](G:/unity/Milestone-Survivor-main/Assets/Scripts/SpawnSyestem/ObjectPoolingManager.cs:144)는 이름으로 풀을 찾고, 없으면 Destroy한다.

후속 조사: [던전 1-1.prefab](<G:/unity/Milestone-Survivor-main/Assets/Prefabs/Map/RealMap/탑(던전)/던전 1-1.prefab:34674>)에 `waves.Array.data[0].DontUseObjectPooling`의 프리팹 오버라이드가 `value: 1`로 저장되어 있다. 반면 [Goblin.prefab](G:/unity/Milestone-Survivor-main/Assets/Prefabs/Enemy/Normal/1.Forest/Goblin.prefab:210)은 `DontUseObjectPooling: 0`이다. 첫 웨이브는 일반 Instantiate로 생성하지만 Goblin은 사망 시 풀에 반환하려 한다. Goblin 풀이 없는 상태라면 관측한 경고 후 Destroy 경로와 일치한다. 이전 검색은 직접 직렬화된 필드만 검사해 propertyPath/value 형태의 오버라이드를 놓쳤다. 런타임 Inspector에서 별도 값을 바꾸지 않았다면 이 저장 설정이 원인이다.

방향: 생성 시 실제 풀 소유자를 객체에 기록하고, 반환은 그 소유자에게 요청한다. 풀 밖 생성 객체는 명시적으로 Destroy한다. 이름 대신 프리팹/풀 식별자를 사용하고 중복 반환 방어를 함께 둔다. 우선 경고 객체의 생성 경로부터 확인하고 무조건 풀링을 켜는 식으로 처리하지 않는다.

## 반복 비용을 줄일 후보

### 5. 경험치 한 번 획득할 때마다 전체 스탯을 다시 계산한다

[PlayerStats.AddXP](G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/PlayerStats.cs:237)는 레벨이 오르지 않아도 LevelUpBonus를 호출한다. [LevelUpBonus](G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/PlayerStatsCalculate.cs:106)는 보정을 다시 만들고 UpdatePlayerStats를 실행한다.

[UpdatePlayerStats](G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/PlayerStatsCalculate.cs:305)는 장비 보너스 13종을 각각 질의한다. [GetStatBonus](G:/unity/Milestone-Survivor-main/Assets/Scripts/Manager/EquipmentEffectManager.cs:106)는 질의할 때마다 장착 효과 목록을 순회한다. 태그 검색 및 플레이어 컴포넌트 조회도 함께 발생한다. ApplyPowerUps 역시 항목마다 스탯 갱신을 일으킨다.

작은 첫 단계: AddXP 이전 레벨을 보관하고 실제 레벨 변화가 있을 때만 레벨 보정을 갱신한다. 여러 레벨이 한 번에 올라도 최종 레벨로 한 번 계산한다. 그다음 초기화/파워업 적용을 일괄 처리하고 최종 갱신을 한 번만 한다. 장비 합계는 장착 변경 시 캐싱한다. 중앙 씬 초기화에서 참조를 모으는 기존 방향은 유지한다.

### 6. 풀에서 한 개를 꺼낼 때마다 리스트 전체 검사와 이동이 발생한다

[spawnGameObject](G:/unity/Milestone-Survivor-main/Assets/Scripts/SpawnSyestem/ObjectPoolingManager.cs:111)는 매 호출 RemoveAll을 실행하고 첫 객체를 가져와 Remove한다. 첫 요소 제거 시 뒤쪽 요소가 당겨진다. 같은 풀에서 많은 객체를 연속 생성하면 누적 비용이 커진다.

방향: 반환된 객체 저장소를 Stack 또는 Queue로 바꾸고, 꺼낸 객체가 파괴되어 있으면 그 항목만 건너뛴다. HashSet 또는 객체 상태로 중복 반환을 막는다. 활성화 전에 호출자가 초기화할 수 있는 계약은 별도 단계로 정리한다. 풀 소비자 전체를 한 번에 갈아엎을 필요는 없다.

### 7. 적 투사체 충돌 처리에 반복 메모리 할당이 있다

[EnemyProjectileManager](G:/unity/Milestone-Survivor-main/Assets/Scripts/Manager/EnemyProjectileManager.cs:99)는 활성 투사체가 있는 처리 프레임마다 TempJob NativeList 두 개를 생성/해제한다. [ProcessCollisionResults](G:/unity/Milestone-Survivor-main/Assets/Scripts/Manager/EnemyProjectileManager.cs:136)는 Dictionary와 정렬용 List를 새로 만든다. 네이티브 할당과 관리 메모리 GC는 구분해서 측정해야 한다.

방향: 컬렉션을 필드로 재사용하고 필요할 때만 용량을 늘린다. 충돌 결과가 없으면 결과용 관리 컬렉션 작업을 생략한다. 현재 역순 처리는 반환 중 인덱스 변경에 대응하므로 무작정 정렬을 지우면 안 된다. 플레이어 참조도 씬 초기화 때 바인딩할 수 있다.

### 8. 적 등록 해제와 일시정지 중 Swarm 작업을 줄일 여지가 있다

[EnemySwarmSystem](G:/unity/Milestone-Survivor-main/Assets/Scripts/SpawnSyestem/EnemySwarmSystem.cs:128)은 등록 해제 시 IndexOf, WaveSpawner는 Contains/Remove, EnemyProjectileManager도 IndexOf를 사용한다. 다수가 동시에 사라지는 상황에서 선형 검색이 반복된다.

방향: enemy → index 사전을 추가하고 swap-back 시 이동한 객체의 인덱스도 갱신한다. SpawnVersion, 예약 반영 시점, 같은 객체 재사용 방어는 유지해야 한다. 적의 존재 목록을 누가 소유할지도 정리하되 AI와 스폰을 한 클래스로 합칠 필요는 없다.

[Swarm.Update](G:/unity/Milestone-Survivor-main/Assets/Scripts/SpawnSyestem/EnemySwarmSystem.cs:149)는 일시정지 여부와 별개로 배열 복사, grid 구축, Job 실행을 진행하며 grid.Clear도 두 번 호출한다. 멈춘 화면에서도 이 작업은 남는다. 단순 조기 return보다는 예약 반영과 인덱스 유효성을 유지하면서 변경이 없을 때 이동/재구축을 생략하는 방향이 안전하다. Schedule 직후 Complete의 실제 대기 비용은 Profiler 확인 후 조정한다.

### 9. 청크를 반환하기 전에 새 청크부터 가져온다

[UpdateChunks](G:/unity/Milestone-Survivor-main/Assets/Scripts/Map/InfiniteTilemapManager.cs:240)는 새 필요 영역을 Load한 뒤 이전 영역을 Unload한다. 빈 풀에서 경계를 넘으면 같은 갱신에서 곧 반환할 객체가 있는데도 Instantiate할 수 있다. 계속 무한히 증가한다는 뜻은 아니며, 불필요한 최초 확장과 이후 유지 용량이 문제다.

방향: 필요 좌표 계산 → 불필요 청크 반환 → 없는 청크 로드 순으로 바꾼다. 벽 좌표 등록/해제와 최종 맵 상태를 같이 검증해야 한다. 이미 있는 갱신 주기와 경계 히스테리시스는 유지한다.

### 10. 아이템 탐색과 대규모 인벤토리 UI

[PlayerXpPickup.Update](G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/PlayerXpPickup.cs:22)는 OverlapCircleAll 배열을 매번 생성한다. 일괄 회수는 타입별 전역 검색을 수행한다. 결과 컬렉션 재사용과 활성 드롭 등록 목록을 고려할 수 있다. 고정 크기 버퍼를 쓰면서 초과 결과를 버리면 누락 문제가 생기므로 크기 증가 또는 재조회 정책이 필요하다. 이미 있는 중복 수령 방어는 유지한다.

[InventoryUIManager.createSlots](G:/unity/Milestone-Survivor-main/Assets/InventoryAsset/InventoryUI/InventoryUIScripts/InventoryUIManager.cs:368)는 rows×cols 전체 슬롯을 Instantiate한다. 보관 용량을 매우 크게 할 계획이라면 데이터 용량과 표시 슬롯 수를 분리해야 한다. 화면에 보이는 행만 재사용하는 방식이 적절하다. 현재 작은 용량에서 당장 병목이라고 단정하지 않는다.

## 구조 리팩토링 순서

### 11. 스킬 구조는 전면 교체보다 계산과 적용을 분리한다

현재 WeaponBase + 개별 Weapon, 학습 레벨/장비 레벨 분리, 공통 CalculateCount/CalculateLevelStat는 유지할 가치가 있다. 모든 스킬을 거대한 switch나 범용 딕셔너리 엔진으로 통합하는 것을 권하지 않는다.

실제 중복은 최초 Initialize, RecalculateLevelStats, 보유 무기 GetUpgradePreview, 미보유 무기 [UpgradePreviewFactory](G:/unity/Milestone-Survivor-main/Assets/Scripts/Upgrades/UpgradePreviewResolver.cs:63) 사이에 있다. [SyncWeapon](G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/PlayerWeaponController.cs:186)은 레벨을 1로 설정하고 Initialize 후 SetLevel을 부르므로, 1레벨은 SetLevel의 동일 레벨 방어에 의해 재계산되지 않는다. 최초 계산 규칙을 자식마다 지켜야 하는 구조다.

배리어는 현재 별도 GetUpgradePreview가 없어 신규 획득 시 보이던 충전시간·넉백 정보가 보유 이후에는 부모의 레벨 줄만으로 바뀔 수 있다.

권장 책임:

1. SO: 기본값과 성장 규칙.
2. 스킬별 수치 계산: SO와 레벨을 받아 실행 객체 변경 없이 결과 산출.
3. Weapon: 계산 결과 적용, 발사/쿨타임/생성 객체 수명 관리.
4. UI: 현재·다음 계산 결과를 표시 형식으로 변환.

처음에는 지속형 스킬 한 계열에만 도입해 타이머 유지 문제와 함께 해결한다. 복잡한 새 프레임워크부터 만들지 않는다.

### 12. 씬의 준비 완료와 일시정지를 구분한다

[CanProcessCombat](G:/unity/Milestone-Survivor-main/Assets/Scripts/OverAll/GameManager.cs:13)은 !Pause만 확인한다. [LoadingManager](G:/unity/Milestone-Survivor-main/Assets/Scripts/SceneLoder/LoadingManager.cs:131)는 씬 활성화 이후 초기화 대상을 찾는다. 참조 주입/스탯 복구/체력 초기화 완료 전에도 Pause가 아니면 전투 가능 상태가 될 수 있는 계약이다. 이번 실행에서 초기화 NullReference는 재현되지 않았다.

방향: 중앙 초기화에서 참조 연결 → 데이터 복구 → 최종 스탯 계산 → 전투 준비 완료를 명시한다. CanProcessCombat에 준비 상태를 반영한다. 초기화 실패를 성공처럼 취급하지 않는다. 이 과정에서 Pause를 활용한다면 기존 WaitForSeconds가 정지에 막히지 않도록 연출 대기는 unscaled 기준으로 검토한다. 모든 컴포넌트의 Awake에 자기 참조 검색을 분산시키는 방향은 피한다.

### 13. 인벤토리와 UI는 경계별로 분리한다

[InventoryController.HandleSceneTransition](G:/unity/Milestone-Survivor-main/Assets/InventoryAsset/InventoryController/InventoryControllerScript/InventoryController.cs:153)의 마을/던전 반입·반출 정책을 첫 분리 대상으로 삼을 만하다. 일반 슬롯 처리와 게임 규칙이 한 파일에 모여 있다. 장비 효과/스킬 재적용은 별도 단계로 정리한다. 현재 SetEquipmentSkills 기반 동기화를 무시하고 중복 레벨업 버그라고 다시 분류하지 않는다.

[PlayerSkillHUD](G:/unity/Milestone-Survivor-main/Assets/Scripts/UI/PlayerSkillHUD.cs:16)는 갱신 때 전체 아이콘을 파괴/재생성한다. 융합처럼 여러 무기가 한 번에 바뀔 때는 변경 묶음이 끝난 뒤 한 번 갱신하고, 이후 필요하면 아이콘을 재사용한다. [UpgradeDescriptionPanel](G:/unity/Milestone-Survivor-main/Assets/Scripts/Upgrades/UpgradeDescriptionPanel.cs:14)도 줄 재사용과 레이아웃 갱신 묶음을 고려할 수 있다. 전투 핫패스보다 우선순위는 낮다.

## 측정 후 결정할 항목

- SkillCollisionManager의 [충돌 결과 용량](G:/unity/Milestone-Survivor-main/Assets/Scripts/Player/skill/SkillCollisionManager.cs:130)은 무제한 타격 스킬 수×적 수에 비례할 수 있다. 현재 방식은 용량 초과를 막는 안전장치다. 단순히 작은 고정 용량으로 되돌리지 않는다. 실제 capacity와 hit 수의 차이를 측정한 뒤 배치 처리 또는 2단계 집계를 검토한다.
- 근접 적의 [공격당 Debug.Log](G:/unity/Milestone-Survivor-main/Assets/Scripts/Enemy/MeleeEnemy.cs:82)가 실제 전투에서 반복됐다. 필요 시 개발 전용 로그 또는 집계 로그로 제한한다. 로그를 켠 Editor 수치를 실제 빌드 성능으로 일반화하지 않는다.
- NavEnemy의 경로 요청 주기, Job Complete 대기, 많은 수집 코루틴의 비용은 적 수·드롭 수를 고정한 측정 이후 판단한다.
- 구형 공격 컴포넌트는 일부 프리팹에 참조가 남아 있다. 이름만 보고 미사용으로 삭제하지 않는다. 부모 활성 상태, UnityEvent 연결, 실제 생성 경로 확인이 필요하다.
- 보스/네임드 상태 구조 재설계는 사용자가 별도 계획 중이므로 이번 우선 수정 묶음에 넣지 않았다.

## 권장 진행 순서와 수동 검증

1. 잘못된 스탯 카드 자산부터 바로잡는다. 선택 전후 realTime 보너스, 실제 능력치, 표시값, 투자 횟수를 대조한다.
2. 지속형 스킬의 최초 초기화와 수치 갱신을 분리한다. 공격력 변경 전후 틱 간격, 궤적 중복 생성, 포탑 발사 타이머가 유지되는지 플레이로 확인한다.
3. AddXP의 불필요한 전체 계산을 제거한다. 경험치만 획득/한 레벨/여러 레벨 상승을 나눠 확인한다.
4. 풀 소유권을 정리하고 Stack/Queue 저장소를 도입한다. 풀 생성·반환, 풀 밖 생성, 중복 반환, 씬 전환을 각각 확인한다.
5. 충돌 결과 컬렉션 재사용과 적 인덱스 사전을 적용한다. 다수 동시 사망/같은 프레임 재생성/일시정지 중 UI 조작을 확인한다.
6. 청크 반환 순서와 인벤토리 표시 슬롯 수를 조정한다. 실제 사용 규모에서 메모리·프레임 시간을 비교한다.
7. 계산-적용 분리와 씬 준비 상태를 점진적으로 확대한다.

전면 재작성보다, 상태를 초기화하는 함수와 값을 갱신하는 함수의 구분 및 데이터 검증을 먼저 확실히 하는 편이 위험과 작업량을 함께 줄인다.
