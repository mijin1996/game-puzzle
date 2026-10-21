# Quadrop 아키텍처

> 이 문서는 두 부분으로 되어 있다.
> - **1부 게임 로직** — 게임이 "어떤 규칙으로" 돌아가는가 (규칙, 진행, 상태 흐름)
> - **2부 아키텍처** — 그 로직을 "어떤 코드 구조로" 만드는가 (계층, 클래스, 의존 방향, 패턴)
>
> ROTETRA(sharkwithlasers)의 공개 정보(페이지 설명, 조작법, 플레이 후기)에서 **아이디어만** 참고했고, 세부 규칙은 Quadrop이 직접 정의한다.
> 코드·그래픽·사운드·이름은 가져오지 않는다.

---

# 1부. 게임 로직

## 1.1 한 줄 요약

**십자(+) 모양 판**의 네 변(위·오른쪽·아래·왼쪽)에서 테트로미노가 **판 중앙 쪽으로** 떨어진다.
변마다 폭 10, 깊이 10짜리 레인이 있고, 이웃한 레인은 중앙 근처 5×5 칸에서 겹친다.
처음엔 위쪽 하나만 열린 일반 스태커로 시작하고, 줄을 지울수록 **방향이 하나씩 해금**된다.
방향이 2개 이상 열리면 **판 전체를 90°씩 돌려서** 지금 화면 위쪽에 있는 변의 피스를 조작한다.

## 1.2 핵심 개념

| 개념 | 정의 | 코드 |
|---|---|---|
| **판** | N×N 격자, 기본 N=20. 원점은 왼쪽 아래 `(0,0)`. **데이터는 절대 회전하지 않는다.** | `GridModel` |
| **레인** | 변마다 폭 W(기본 10) × 깊이 N/2인 영역. 로컬 좌표로는 모든 변이 같은 직사각형 `u ∈ [(N-W)/2, (N+W)/2)`, `v ∈ [N/2, N)`. 피스는 자기 레인 밖으로 못 나간다 | `LaneLayout` |
| **변** | 피스가 생성되는 가장자리 `Top`, `Right`, `Bottom`, `Left` | `Side` |
| **바깥 방향 / 중력** | 변의 바깥 방향이 그 변 피스의 "화면 위", 반대가 중력. Top 중력 `(0,-1)`, Right `(-1,0)`, Bottom `(0,1)`, Left `(1,0)` | `Side.Outward()` |
| **로컬 좌표** | 피스는 자기 변 기준 `(u, v)` 좌표로 움직인다. u = 화면 오른쪽, v = 화면 위쪽. 낙하는 4방향 모두 `v - 1` | `SideFrame` |
| **스포너** | 활성 변마다 낙하 중인 피스가 **항상 1개** | `SpawnerSet` |
| **조작 중인 변** | 지금 화면 위쪽에 놓인 변. 입력은 이 변의 피스에만 적용 | `ControlState` |
| **판 회전** | 조작 중인 변을 바꾸는 것. 데이터는 그대로, **BoardView Transform만 돌아간다** | `BoardView` |

## 1.3 단계별 진행

STANDARD 모드 (값은 전부 `GameConfigAsset`에서 수정 가능):

| 단계 | 활성 변 | 해금 조건 (누적 줄) | 판 회전 |
|---|---|---|---|
| 1 | Top | 시작 | **잠김** — 일반 스태커와 같음 |
| 2 | + Left | 10줄 | 열림 |
| 3 | + Bottom | 25줄 | 열림 |
| 4 | + Right | 45줄 | 열림 |

- 판을 **시계 방향(D)** 으로 돌리면 왼쪽 변이 위로 온다. 그래서 해금 순서를 Top → Left → Bottom → Right로 두어 **D 한 번이면 새 방향**으로 간다.
- 해금 순간 `Unlocking` 상태로 잠깐 멈추고 "NEW SIDE OPEN!" 안내를 띄운 뒤, 새 변에서 첫 피스가 나온다.
- ALL SIDES 모드는 시작부터 4방향 전부. 새 모드는 `IProgressionRule` 구현체 추가로 만든다.

## 1.4 낙하와 고정

- 모든 활성 피스는 같은 간격으로 자기 중력 방향으로 한 칸씩 떨어진다. 조작 중이 아닌 피스도 계속 떨어진다.
- 처리 순서는 enum 순서(Top → Right → Bottom → Left)로 고정 → 결과가 항상 같다.
- 이동하려는 칸이:
  - **비어 있음** → 이동
  - **레인 밖(판 중앙선 포함)이거나 고정 블록** → 고정 대기(0.5초) 시작, 끝나면 고정. 바닥에 닿은 채 움직이거나 돌리면 대기를 다시 시작 (최대 15번)
  - **다른 낙하 피스** → 기다린다. 고정되지 않는다 ("공중에서 멈춤")
- 소프트 드롭: 조작 중인 피스만 0.03초 간격으로 떨어진다.
- 하드 드롭: 막힐 때까지 즉시 이동 후 **바로 고정**. 다른 낙하 피스에 막히면 그 자리(공중)에 고정된다.
- 낙하 간격 = `max(0.05, 0.8 × 0.85^(레벨-1))`초, 레벨 = 1 + 줄 수 ÷ 10.

## 1.5 조작

| 키보드 | 게임패드 | 동작 |
|---|---|---|
| ← → | D-pad 좌우 | 화면 기준 좌우 이동 (누르고 있으면 0.17초 뒤 0.05초 간격 반복) |
| ↓ | D-pad 아래 | 소프트 드롭 (누르는 동안) |
| Space | D-pad 위 | 하드 드롭 |
| Z / ↑ | South / East | 피스 시계 방향 회전 (벽 차기 포함) |
| C | L / R 숄더 | 홀드 (모든 변 공유, 고정 1회당 1번) |
| A | 왼쪽 트리거 / West | 판 반시계 회전 |
| D | 오른쪽 트리거 / North | 판 시계 회전 |
| Esc / Enter | Start | 일시정지 |
| R | Select | 재시작 (일시정지·게임오버 중) |

화면 기준 입력은 로컬 좌표에서 그대로 `u ± 1`이 되므로, 변이 바뀌어도 변환 코드가 따로 없다.

## 1.6 줄 지우기

"줄"은 **레인 안의 한 줄** (폭 W칸)이다. 위·아래 레인의 줄은 가로, 왼쪽·오른쪽 레인의 줄은 세로다.

1. 피스가 고정되면 **활성 변 레인**에서 꽉 찬 줄을 모두 찾는다.
2. 찾은 줄을 흰색으로 깜빡이고 `Clearing` 상태로 0.3초 멈춘다 (모든 낙하 정지, 입력은 버림).
3. 표시된 줄을 **동시에** 지운다 (겹치는 칸 때문에 한쪽이 덜 세어지지 않게).
4. 그 레인에서 지운 줄보다 **바깥쪽** 블록을 한 칸씩 **중앙 쪽으로** 당겨 메운다 (같은 레인은 바깥 줄부터 처리).
5. 메운 뒤 새로 꽉 찬 줄이 생기면 연쇄로 계속 지운다 (모두 점수에 포함).
6. 메운 블록이 낙하 중인 피스와 겹치면, 그 피스를 중력 반대 방향으로 밀어낸다. 레인 밖까지 밀려나면 게임오버.

겹치는 칸(이웃 레인 공유)은 두 레인의 줄에 모두 속한다. 한 레인을 정리하면 이웃 레인의 블록도 같이 움직일 수 있는데, 이게 이 게임의 "변끼리 영향을 주는" 부분이다.

## 1.7 피스 생성과 게임오버

- 다음 피스는 **공유 7-bag** (7종을 섞어서 하나씩)에서 꺼낸다.
- 생성 위치: 변의 로컬 좌표 `u = (N - 박스) / 2`, `v = N - 박스` (레인 바깥 끝, 가운데).
- 레인 양옆 여백 `(N - W) / 2`가 피스 박스(4)보다 넓어서, 생성 위치는 **다른 레인과 겹치지 않는다**. 그래서 새 변이 열리는 순간 바로 막히는 일이 없다 (`GameRules.Validate`가 강제).
- 생성 위치를 다른 낙하 피스가 지나가는 중이면 다음 틱에 다시 시도한다.
- 생성 위치를 **자기 레인의 고정 블록**이 막으면 (= 레인 끝까지 쌓이면) 게임오버.

### 왜 십자 모양인가

처음 설계(정사각형 판에서 반대편 벽까지 떨어짐)에서는 위쪽 피스가 아래 벽까지 쌓이므로, 아래쪽 변이 열리는 순간 그 생성 위치가 이미 막혀 바로 게임오버가 됐다.
레인을 중앙까지만 두면 변마다 자기 공간이 있고, 겹치는 중앙 칸에서만 서로 간섭한다.

## 1.8 점수

`기본 점수 × 레벨 × 변 배율`

| 동시에 지운 줄 | 기본 점수 | | 활성 변 | 배율 |
|---|---|---|---|---|
| 1 | 100 | | 1 | ×1 |
| 2 | 300 | | 2 | ×1.5 |
| 3 | 500 | | 3 | ×2 |
| 4 이상 | 800 | | 4 | ×3 |

최고 점수는 게임오버 때 PlayerPrefs에 저장된다.

## 1.9 게임 상태 흐름

```
 Playing ──(피스 고정, 꽉 찬 줄)──▶ Clearing ──▶ (새 변 해금?) ──예──▶ Unlocking ──▶ Playing
   │  ▲                               │ 아니오                                  
   │  └───────────────────────────────┘                                          
   ├──(A/D, 활성 변 2개 이상)──▶ Rotating ──(0.15초)──▶ Playing
   ├──(Esc)──▶ Paused ──(Esc)──▶ Playing      (R → 재시작)
   └──(생성 위치가 고정 블록)──▶ GameOver ──(R)──▶ 재시작
```

| 상태 | 낙하 | 입력 처리 |
|---|---|---|
| Playing | 진행 | 바로 처리 |
| Rotating | 진행 | **미뤄 둠** (큐에 남겨 두었다가 연출 끝나면 처리 — 회전 중 입력이 씹히지 않게) |
| Clearing | 정지 | 버림 |
| Unlocking | 정지 | 버림 |
| Paused | 정지 | Esc(재개), R(재시작)만 |
| GameOver | 정지 | R(재시작)만 |

---

# 2부. 아키텍처

## 2.1 계층과 의존 방향

```
Quadrop.Presentation  (MonoBehaviour: Bootstrap, View, HUD, ScriptableObject 설정)
        │
        ├──────────────▶ Quadrop.Infrastructure (Input System 입력, PlayerPrefs 저장)
        ▼                       │
Quadrop.Game  (게임 흐름: 세션, 상태 머신, 스포너)   ◀─┘
        ▼
Quadrop.Core  (규칙: 판, 피스, 줄 지우기, 점수, 진행 — 순수 C#)
```

- 화살표는 "참조한다" 방향. **Core는 아무것도 참조하지 않는다.**
- Core와 Game은 `noEngineReferences: true` → UnityEngine 없이 EditMode 테스트로 전부 검증된다.
- Infrastructure는 Game의 인터페이스(`IGameCommandSink`, `IHighScoreRepository`)를 구현한다 (DIP).
- 게임 흐름 계층 이름은 `Application` 대신 `Game` — `UnityEngine.Application`과 이름 충돌을 피하려고.
- 객체 조립은 `GameBootstrap` 한 곳에서만 한다.

## 2.2 어셈블리 (.asmdef)

| 어셈블리 | 경로 | 참조 | 엔진 |
|---|---|---|---|
| `Quadrop.Core` | `Assets/Scripts/Core/` | 없음 | 없음 |
| `Quadrop.Game` | `Assets/Scripts/Game/` | Core | 없음 |
| `Quadrop.Infrastructure` | `Assets/Scripts/Infrastructure/` | Core, Game, Unity.InputSystem | 있음 |
| `Quadrop.Presentation` | `Assets/Scripts/Presentation/` | Core, Game, Infrastructure, UnityEngine.UI | 있음 |
| `Quadrop.Tests.Mocks` | `Assets/Tests/Mocks/` | Core, Game | 에디터 전용 |
| `Quadrop.Tests.EditMode` | `Assets/Tests/EditMode/` | Core, Game, Mocks, NUnit | 에디터 전용 |

Game은 `InternalsVisibleTo`로 테스트 어셈블리에만 내부 클래스를 보여 준다.

## 2.3 클래스 구성

### Core (규칙 — 순수 C#)

| 클래스 | 역할 |
|---|---|
| `Result<T>` | 실패할 수 있는 작업의 결과 (프로젝트에 하나) |
| `IRandomSource`, `SystemRandomSource` | 랜덤 추상화와 실제 구현 |
| `GridPos` | 판 좌표 `readonly struct` (`IEquatable` 구현) |
| `Side`, `SideExtensions` | 변 enum과 바깥 방향·중력·판 각도 계산 |
| `SideSet` | 활성 변 집합 (비트마스크 struct, GC 없음) |
| `SideFrame` | 변 하나의 로컬 좌표 → 판 좌표 변환 |
| `BoardFrames` | 4개 SideFrame 보관, 피스 셀의 판 좌표 계산 |
| `LaneLayout` | 십자 모양 레인 범위, 로컬 ↔ 판 좌표 변환 |
| `GridModel`, `IReadOnlyGrid` | 고정 블록 저장 (`byte[]`, 0 = 빈칸, 1~7 = 피스 종류) |
| `TetrominoType`, `TetrominoShapes` | 피스 7종과 회전별 셀 표 (시작 시 한 번 생성) |
| `Piece` | 낙하 중인 피스 (변, 로컬 위치, 회전) — 변마다 하나를 재사용 |
| `ICellQuery`, `CellState`, `MoveResult` | 칸 상태 질의 계약 |
| `PieceMover` | 레인 범위·충돌 검사 후 이동·회전(벽 차기)·드롭 거리·밀어내기 |
| `LaneLine`, `LineClearer` | 레인 줄, 꽉 찬 줄 표시·동시 삭제·중앙 쪽으로 메우기·연쇄 |
| `ScoreCalculator` | 줄 수 · 레벨 · 활성 변 수 → 점수 |
| `GravitySchedule` | 레벨 → 낙하 간격 |
| `IPieceSequence`, `SevenBagSequence` | 다음 피스 순서 (Strategy) |
| `IProgressionRule`, `StandardProgression`, `AllSidesProgression` | 모드별 변 해금 규칙 (Strategy) |
| `GameRules` | 규칙 값 묶음 + `Validate()` |

### Game (게임 흐름)

| 클래스 | 역할 |
|---|---|
| `GameSession` | 바깥에 보이는 창구: 명령 큐, 틱, 읽기 전용 정보(판, 피스 셀, 고스트, 홀드, 다음 피스) |
| `GameContext` | 한 판에 필요한 객체 묶음 + 상태들이 같이 쓰는 짧은 흐름 함수 |
| `GameStateMachine`, `IGameState`, `GameStateBase` | 상태 전환 (State) |
| `PlayingState` · `RotatingState` · `ClearingState` · `UnlockingState` · `PausedState` · `GameOverState` | 상태별 동작 |
| `CommandPolicy` | 상태별 입력 처리 방식 (처리 / 미룸 / 버림) |
| `SpawnerSet` | 변별 활성 여부, 낙하 피스, 생성 (Ready / Pending / Blocked) |
| `BoardCollision` | `ICellQuery` 구현: 고정 블록 + 다른 낙하 피스 |
| `GravityStepper` | 모든 피스 낙하와 고정 대기 |
| `PieceController` | 조작 중인 피스에 이동·회전·드롭·홀드 적용 |
| `PieceLocker` | 피스를 판에 고정 |
| `PieceTimer` | 피스별 낙하·고정 대기 시간 |
| `ControlState` | 조작 중인 변과 판 회전 |
| `HoldSlot` | 공유 홀드 칸 |
| `GameStats` | 점수, 줄 수, 레벨 |
| `GameEvents` | View에 알리는 이벤트 (Observer) |
| `GameCommand`, `IGameCommandSink` | 입력 명령과 그 통로 |
| `GameMode`, `ProgressionFactory` | 모드 → 해금 규칙 생성 (Factory) |
| `IHighScoreRepository`, `HighScoreTracker` | 최고 점수 저장 추상화와 게임오버 시 갱신 |

### Infrastructure

| 클래스 | 역할 |
|---|---|
| `InputReader` | 코드로 만든 Input System 액션 맵 → `GameCommand`, DAS/ARR 자동 반복 |
| `InputTuning` | DAS/ARR 값 |
| `PlayerPrefsHighScoreRepository` | 최고 점수 저장 (Result 반환) |

### Presentation

| 클래스 | 역할 |
|---|---|
| `GameBootstrap` | 설정 로드, 모든 객체 생성·주입, 카메라 설정. 씬에 없으면 자동 생성 |
| `GameLoopDriver` | `Update` → `input.Tick`, `session.Tick` |
| `GameConfigAsset` | 밸런스 값 ScriptableObject → `GameRules` |
| `BoardView` | 십자 레인 바닥·고정 블록·낙하 피스·고스트·변 표시 막대 그리기, 회전 연출 |
| `PreviewView` | 홀드·다음 피스 미리보기 (회전 안 함) |
| `HudView` | 점수·레벨·안내 문구 (UGUI) |
| `SpriteCellPool` | 셀 SpriteRenderer 미리 만들어 재사용 (Object Pool) |
| `SpriteFactory`, `BlockPalette` | 셀 스프라이트 생성, 색상 모음 |

## 2.4 한 프레임의 흐름

```
GameLoopDriver.Update(dt)
 ├─ InputReader.Tick(dt)            좌우 자동 반복 → Enqueue(MoveLeft/Right)
 │                                   (다른 키는 Input System 콜백에서 바로 Enqueue)
 └─ GameSession.Tick(dt)             dt는 최대 0.1초로 자름
     ├─ 1. 명령 큐 처리 (현재 상태의 CommandPolicy에 따라)
     └─ 2. 현재 상태.Tick(dt)
           Playing/Rotating → GameContext.AdvancePieces
              ├─ SpawnerSet.FillEmpty   빈 활성 변에 피스 생성 (Blocked → GameOver)
              └─ GravityStepper.Step    낙하 · 고정 → 고정됐으면 CheckLines → Clearing
           Clearing  → 대기 후 LineClearer → 점수 → 밀어내기 → 해금 확인 → Unlocking / Playing
           Unlocking → 대기 후 Playing
BoardView.LateUpdate()               회전 트윈 + 낙하 피스·고스트 그리기
```

## 2.5 사용하는 패턴과 이유

| 패턴 | 위치 | 이유 |
|---|---|---|
| State | `GameStateMachine` + 상태 6개 | 상태마다 낙하·입력 허용이 달라서 큰 if/switch를 피하려고 |
| Strategy | `IProgressionRule`, `IPieceSequence` | 모드와 피스 순서를 기존 코드 수정 없이 교체 (OCP) |
| Observer | `GameEvents` | Game이 View를 모르게 (SDP) |
| Object Pool | `SpriteCellPool` | 셀을 매번 생성·파괴하면 GC 스파이크 |
| Repository | `IHighScoreRepository` | 저장 위치가 바뀌어도 게임 흐름을 고치지 않게 |
| Factory | `ProgressionFactory` | 모드 → 규칙 선택을 한 곳에 |
| Composition Root | `GameBootstrap` | `new`와 연결을 한 곳에 모아서 나머지는 인터페이스만 알게 |
| Result | `Result<T>` | 저장·검증 실패를 값으로 돌려서 처리를 빠뜨리지 않게 |

## 2.6 왜 "데이터는 고정, 화면만 회전"인가

- 판 데이터를 실제로 돌리면 회전할 때마다 모든 셀과 여러 낙하 피스 좌표를 다시 계산해야 해서 버그가 나기 쉽다.
- 데이터를 고정하고 피스를 **변의 로컬 좌표**로 다루면, 낙하·이동·회전·충돌이 4방향 모두 **같은 코드**다 (DRY).
- 판 회전은 `ControlState` 값 하나와 `BoardView` Transform의 Z축 회전뿐이다. 각도 = 변 번호 × 90°.

## 2.7 GC·성능 규칙

- 피스는 변마다 하나를 미리 만들어 `Reset`으로 재사용, 셀 계산은 미리 할당한 `GridPos[4]` 버퍼에 채운다.
- 셀 렌더러는 시작할 때 전부 만들어 두고 켜고 끈다 (레인 바닥 N² + 고정 블록 N² + 16 + 4 + 4개).
- 문자열(HUD)은 값이 바뀔 때만 `StringBuilder`로 만든다.
- `Update` 경로에 LINQ, `GetComponent`, `Find`, 참조 타입 `new` 없음.
