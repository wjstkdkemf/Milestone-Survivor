# Milestone Survivor: AI-Assisted Rogue-lite

## 프로젝트 소개
> Milestone Survivor는 뱀서라이크(Vampire Survivors-like) 장르의 핵심 재미인 대규모 물량전과 무한한 성장을 구현한 로그라이트 액션 게임입니다.
> 1인 개발의 한계를 극복하기 위해 **Gemini API/CLI**를 개발 파이프라인에 적극 도입하여 생산성을 극대화했으며, 오브젝트 풀링과 공간 분할을 통해 최적화된 성능을 구현했습니다.

* **개발 기간:** 2025.06 ~ 진행 중
* **개발 인원:** 1인 (All-round)
* **핵심 목표:** 대규모 객체 렌더링 최적화 & AI 기반 개발 프로세스 정립

## 기술적 구현 상세

### 1. 데이터 주도적 스킬 시스템 
빈번한 콘텐츠 업데이트가 요구되는 장르 특성을 고려하여, OCP(개방-폐쇄 원칙)를 준수하는 확장형 구조를 설계했습니다.
* 구조: 공통 로직은 `WeaponBase` 추상 클래스로 정의.
* 데이터: `ScriptableObject`를 통해 스킬 수치와 프리팹을 코드로 부터 분리.
* 성과: 기존 코드를 수정하지 않고도, 데이터 에셋 생성만으로 신규 스킬 구현 가능.
* Code: [WeaponBase.cs](https://github.com/wjstkdkemf/Milestone-Survivor/blob/main/Assets/Scripts/Player/skill/WeaponBase.cs)
  및 다양한 SO 파일.

### 2. 고성능 렌더링 최적화
수백 마리의 몬스터와 투사체가 난무하는 상황에서 안정적인 프레임레이트를 확보하기 위해 두 가지 핵심 기술을 적용했습니다.
* Object Pooling: 생성/파괴 비용과 GC(Garbage Collection) 스파이크를 방지하기 위해 `PoolManager`를 통해 객체를 재사용.
* Spatial Partitioning (Chunking): 맵을 3x3 청크로 분할하고, 플레이어 시야 밖의 객체는 렌더링 및 연산을 중지(Culling)하여 CPU 부하 최소화.
* Code: [ObjectPoolingManager.cs](https://github.com/wjstkdkemf/Milestone-Survivor/blob/main/Assets/Scripts/ObjectPoolingManager.cs)
        [InfiniteTilemapManager.cs](https://github.com/wjstkdkemf/Milestone-Survivor/blob/main/Assets/Scripts/InfiniteTilemapManager.cs#L144)

### 3. AI 기반 개발 파이프라인
1인 개발의 리소스 및 코딩 병목을 해결하기 위해 생성형 AI(Gemini)를 도구로 활용했습니다.
* Code: Gemini CLI와 프롬프트 템플릿을 활용하여 반복적인 스킬 보일러플레이트 코드 자동 생성.
* Asset: 캐릭터 및 이펙트 스프라이트 시트를 AI로 생성하여 아트 리소스 비용 절감.

### 4. 데이터 무결성 보장 (Data Integrity)
* 문제: 씬(Scene) 전환 시 인벤토리 데이터가 초기화되거나 유실되는 현상 발생.
* 해결: `GlobalDataManager`를 두어 씬 로드/언로드 시점에 명시적으로 데이터를 직렬화(Serialization)하여 저장 및 검증.
* Code: [SaveLoadManager.cs](https://github.com/wjstkdkemf/Milestone-Survivor/blob/main/Assets/Scripts/SaveLoadManager.cs)

## 프로젝트 시연 영상

각 시스템의 작동 방식 및 게임 플레이 영상을 통해 구현 내용을 확인하실 수 있습니다.

* [게임 시작 씬과 게임 씬의 전환 영상](https://www.youtube.com/watch?v=h9m_7P1c9AE)
* [씬내 이동 반경 제한 및 물리 법칙(가속도) 구현 영상](https://www.youtube.com/watch?v=qmt11QpNQd0)
* [스킬 인벤토리 및 스킬 데미지 구현 영상](https://www.youtube.com/watch?v=1P8lNQFo7Vo)
* [스킬 인벤토리와 캐릭터 스킬 시스템의 연동 영상](https://www.youtube.com/watch?v=--5En7LuhW8)
* [2.5D 환경의 점프 구현 영상](https://www.youtube.com/watch?v=Gir6DMIhsvc)
* [포탈 구현](https://www.youtube.com/watch?v=oRgj7u0u7VI)
* [포탈 후 스킬 기능 작동 확인 영상](https://www.youtube.com/watch?v=QO8wDUo0Pp8)

##  트러블 슈팅 (Dev Log)
개발 과정에서 마주친 기술적 문제와 해결 과정을 기록했습니다.
