# JungHo 작업 기록

담당 영역: UI / 대화 시스템(Yarn 연동) / NPC / 주점 운영 / 퀘스트·이벤트 / 인벤토리·상점

작업 브랜치: `JungHo` (from `develop`)

이 문서는 다른 팀원이 커밋/PR을 훑어보지 않아도 "무엇을 왜 만들었는지"를 파악할 수 있도록,
기능 단위로 작업 내용을 정리합니다. 새 기능을 완료할 때마다 아래 표에 항목을 추가합니다.

## 진행 현황

| 날짜 | 작업 내용 | 관련 파일 | 상태 |
|---|---|---|---|
| 2026-08-22 | 노션 기획서를 리포에 반입, `JungHo` 브랜치 생성(from develop) | `Docs/기획서.md`, `Docs/images/` | 완료 |
| 2026-08-23 | Bar 씬 퀘스트/이벤트 · 인벤토리/상점 프로토타입 골격 구현 | `Scripts/JungHo/*`, `Scripts/Core/GameManager.cs`, `Scripts/Core/SaveData.cs`, `Yarn/TestYarnScript.yarn` | 완료 |

## 작업 내용 상세

### 2026-08-22 — 기획서 반입 & 브랜치 세팅

- 팀 노션 기획서("기획 정리")를 내보내기(Export)한 마크다운을 `Docs/기획서.md`로 정리해서 반입.
  참고 이미지 4장은 `Docs/images/`에 저장 (대화뷰, 별자리 탑뷰, 요리뷰 2종).
- `develop` 기준으로 `JungHo` 개발 브랜치를 생성. 이후 UI/대화/NPC/주점운영 관련 작업은 이 브랜치에서 진행.
- 다음 목표: 1차 프로토타입 범위(챕터1 · 겨울 · 오리온자리)에 맞춰 대화 시스템(Yarn) 확장 및 NPC 응대 흐름부터 설계.

### 2026-08-23 — 퀘스트/이벤트 · 인벤토리/상점 프로토타입 골격

Bar 씬에서 실제 구현 시작. "흔히 게임에서 보는 인벤토리/퀘스트" 수준의 뼈대를 목표로, 세부 밸런싱은 나중으로 미루고 아래를 구현:

- **데이터/저장**: `SaveData`에 `QuestState` enum, `InventoryEntry`/`QuestEntry`, `inventory`/`quests`/`currency` 필드 추가. 저장 버전 1→2로 상향(기존 save.json은 자연스럽게 무효화됨, 프로토타입 단계라 허용).
- **매니저** (`Scripts/JungHo/`): `InventoryManager`(아이템 추가/제거/조회), `QuestManager`(NotStarted→InProgress→Complete 3단계), `ShopCatalog`+`ShopManager`(재화 기반 구매), `GameEvents`(상태 변경 브로드캐스트). `GameManager`가 이들을 소유하도록 확장(`Inventory`/`Quests`/`Shop`/`Currency` 프로퍼티, `SaveGame()`/`AddCurrency()`/`SpendCurrency()`).
- **Yarn 연동**: `QuestInventoryYarnCommands`로 대화 중 `<<give_item>>`/`<<take_item>>`/`<<start_quest>>`/`<<complete_quest>>`/`<<add_currency>>` 커맨드와 `has_item()`/`is_quest_complete()`/`get_item_count()`/`get_currency()` 함수를 대화 텍스트/조건문에서 바로 사용 가능하게 함. `TestYarnScript.yarn`에서 실제로 검증(아이템 지급, 퀘스트 시작→완료, 대사에 `{get_currency()}` 인라인 표시).
- **UI**: `UITheme`/`UIFactory`로 기존 Bar UI 톤(네이비 `#292E4C`, 코랄 `#FF9191`, 골드 `#F4D58D` 포인트 추가, Pretendard)에 맞춘 패널을 코드로 런타임 생성 — 인벤토리/퀘스트/상점 패널 3개, `Bar.unity` 씬 파일은 직접 건드리지 않음(다른 팀원과 씬 병합 충돌 방지). 토글 버튼은 좌측 상단 "대화재생" 버튼 아래에 배치.
- **검증**: Play 모드에서 대화로 아이템 지급/퀘스트 진행/재화 지급 → 상점 구매(재화 차감+인벤토리 반영, 부족 시 버튼 비활성화) → Play 종료 후 "이어하기"로 재진입해 상태(아이템/퀘스트/재화)가 그대로 복원되는 것까지 실제 확인 완료.
- 유니티 에디터에서 수동으로 한 일: `Bar.unity`의 `UI` GameObject에 `QuestInventoryYarnCommands`/`UITheme`/`InventoryPanelUI`/`QuestLogPanelUI`/`ShopPanelUI` 컴포넌트 추가 + `UITheme.Font`에 Pretendard 폰트 에셋 연결.
- 다음 단계 후보: 실제 챕터1 대화(오리온자리) 작성 시 `give_item`/`start_quest` 등을 진짜 아이템/퀘스트 id로 연결, Kitchen 씬 요리 시스템과 인벤토리 연동, NPC별 상호작용(현재는 버튼 하나로 대화 시작 — NPC 클릭/식별 시스템 없음).

### Scripts/JungHo/ 파일별 기능 요약

**씬에 컴포넌트로 붙인 것**
- `QuestInventoryYarnCommands` — 대화(Yarn) 중 `<<give_item>>`/`<<start_quest>>` 등 명령어 처리
- `UITheme` — 색상·폰트 값 보관
- `InventoryPanelUI` — 인벤토리 패널 생성/표시
- `QuestLogPanelUI` — 퀘스트 패널 생성/표시
- `ShopPanelUI` — 상점 패널 생성/표시 + 구매 처리

**코드에서만 쓰이는 것**
- `GameEvents` — 상태 변경 알림(이벤트) 방송
- `InventoryManager` — 아이템 추가/제거/조회 로직
- `QuestManager` — 퀘스트 상태(시작/진행/완료) 로직
- `ShopManager` — 재화 확인 후 구매 처리
- `ShopCatalog` — 판매 아이템 목록 데이터
- `UIFactory` — 버튼/라벨/패널 생성 공통 함수

**기존 파일 수정**
- `GameManager.cs` — 인벤토리/퀘스트/상점/재화 접근 창구 추가
- `SaveData.cs` — 인벤토리/퀘스트/재화 저장 필드 추가

기존 스크립트(`GameManager`, `SaveData` 외 5개)와 이름 겹치는 클래스 없음, 위 2개 파일도 기존 코드는 삭제 없이 추가만 함.
