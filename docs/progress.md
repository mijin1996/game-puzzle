# Quadrop 진행 현황 (인수인계 문서)

> 마지막 갱신: 2026-10-21
> 다음 작업을 시작할 때 이 문서 → `CLAUDE.md` → `docs/architecture.md` → `docs/planned.md` 순서로 읽는다.

---

## 1. 프로젝트 한 줄 요약

ROTETRA(sharkwithlasers)에서 **아이디어만** 참고한 Unity 2D 낙하형 퍼즐(테트로미노 스태커)이다.
십자(+) 모양 판의 네 변에서 피스가 중앙 쪽으로 떨어진다. 줄을 지울수록 방향이 하나씩 열리고, 판을 90°씩 돌리면서 여러 방향을 동시에 관리한다.

- 경로: `C:\Users\j4m\Desktop\ALLMY_MJ_WORK\Quadrop`
- 엔진: Unity 6 (6000.3.26f1), URP 2D, Input System 1.20, Test Framework 1.6
- 코드 규칙: `CLAUDE.md` (Allman 스타일, SOLID, Result 패턴, 테스트, 문서 갱신)
- 설계 스킬: `unity-clean-logic` (Allman · Unity C# 기준)

---

## 2. 요청한 것 (대화 순서)

| # | 요청 | 결과 |
|---|---|---|
| 1 | ROTETRA 게임 파악 | 완료 (장르, 메커닉, 모드, 조작, 반응 정리) |
| 2 | 이런 게임 종류의 이름 | 완료 (낙하형 퍼즐 / 테트로미노 스태커 / 테트리스류) |
| 3 | 따라 만들면 위법인지 | 완료 (아이디어는 보호 대상이 아님. 에셋·이름·전체 표현을 베끼는 것은 위험. 변호사 조언은 아님) |
| 4 | 회전 로직을 부르는 말, 구조 설계 | 완료 (게임 로직 vs 아키텍처 구분, "데이터는 고정, 화면만 회전" 방식) |
| 5 | 게임 이름 | **Quadrop**으로 결정 (Quad + Drop) |
| 6 | 2D / 3D 선택 | **2D**로 결정 (Universal 2D 템플릿) |
| 7 | Quadrop 아키텍처 md, 규칙 md, 스킬 작성 (Allman, Java 개념을 Unity C# 기준으로) | 완료 (`CLAUDE.md`, `.editorconfig`, `docs/*`, 스킬 `unity-clean-logic`) |
| 8 | 아키텍처를 기준으로 게임 계속 만들기 | 플레이 가능한 프로토타입 코드 완료 (**Unity 컴파일·테스트는 미확인**) |
| 9 | 요청·구현·미구현·앞으로 할 것 정리 | 이 문서 |

---

## 3. 구현한 것

### 3.1 문서와 규칙

| 파일 | 내용 |
|---|---|
| `CLAUDE.md` | 작업 순서, Allman 규칙, Java → Unity C# 대응표, SOLID, 계층, Result, 테스트, 문서, 커밋 규칙 |
| `.editorconfig` | Allman 강제 (`csharp_new_line_before_open_brace = all` 등), 4칸 스페이스, private 필드 `_camelCase` |
| `docs/architecture.md` | 1부 게임 로직, 2부 코드 구조 |
| `docs/prototype.md` | 핵심 루프, 범위, 검증할 것, 실행 방법 |
| `docs/file-tree.md` | 모든 파일과 역할 |
| `docs/implemented.md` / `docs/planned.md` | 완료 목록 / 예정 목록 |
| 스킬 `unity-clean-logic` | 제안 카드로 전달됨 (계정에 저장했는지 확인 필요, 4.2 참고) |

### 3.2 게임 로직 (구현된 규칙)

- **판**: 20×20 격자 위에 변마다 폭 10, 깊이 10의 레인이 있고, 레인 4개가 십자 모양을 이룬다. 이웃한 레인은 중앙 근처 5×5 칸에서 겹친다.
- **단계 진행 (STANDARD)**: Top으로 시작해서 10줄에 Left, 25줄에 Bottom, 45줄에 Right가 열린다. 방향이 2개 이상일 때만 판을 회전할 수 있다.
- **ALL SIDES 모드**: 처음부터 4방향이 열려 있다 (메뉴가 없어서 지금은 `GameConfigAsset`에서만 고를 수 있다).
- **낙하**: 모든 활성 피스가 동시에 떨어진다.
  - 고정 블록이나 레인 경계에 닿으면 0.5초 뒤 고정된다 (위치를 고치면 대기가 다시 시작되고, 최대 15번까지).
  - 다른 낙하 피스에 막히면 고정되지 않고 기다린다.
- **조작**: 좌우 이동(DAS 0.17초, ARR 0.05초), 소프트 드롭, 하드 드롭, 피스 회전(벽 차기), 홀드(모든 변이 공유), 판 회전 A/D, 일시정지, 재시작. 키보드와 게임패드 모두 지원.
- **줄 지우기**: 활성 레인에서 꽉 찬 줄을 동시에 지운다.
  - 지운 줄보다 바깥쪽 블록을 중앙 쪽으로 당겨 메우고, 새로 꽉 찬 줄이 생기면 연쇄로 지운다.
  - 메운 블록과 겹친 낙하 피스는 바깥으로 밀어낸다.
- **점수**: 기본 점수(100/300/500/800) × 레벨 × 활성 변 배율(×1/1.5/2/3). 레벨 = 1 + 줄 수 ÷ 10.
- **게임오버**: 자기 레인의 고정 블록이 피스가 나올 자리를 막으면 끝난다.
- **최고 점수**: PlayerPrefs에 저장한다.

### 3.3 코드 구조 (89개 파일)

| 계층 | 어셈블리 | 주요 클래스 |
|---|---|---|
| Core (순수 C#) | `Quadrop.Core` | `GridModel`, `LaneLayout`, `SideFrame`, `Piece`, `PieceMover`, `LineClearer`, `ScoreCalculator`, `SevenBagSequence`, `StandardProgression`, `GameRules`, `Result<T>` |
| Game (순수 C#) | `Quadrop.Game` | `GameSession`, `GameContext`, `GameStateMachine`과 상태 6개, `SpawnerSet`, `GravityStepper`, `PieceController`, `HoldSlot`, `GameEvents`, `HighScoreTracker` |
| Infrastructure | `Quadrop.Infrastructure` | `InputReader`, `PlayerPrefsHighScoreRepository` |
| Presentation | `Quadrop.Presentation` | `GameBootstrap`, `GameLoopDriver`, `BoardView`, `PreviewView`, `HudView`, `GameConfigAsset`, `SpriteCellPool` |
| Tests | `Quadrop.Tests.Mocks`, `Quadrop.Tests.EditMode` | EditMode 테스트 61개, Mock 6개 |

사용한 패턴: State, Strategy, Observer, Object Pool, Repository, Factory, Composition Root, Result (이유는 `architecture.md` 2.5).

---

## 4. 구현하지 못한 것 / 확인하지 못한 것

### 4.1 확인이 꼭 필요한 것 (가장 먼저)

| 항목 | 이유 | 할 일 |
|---|---|---|
| **컴파일** | 작업 환경에 C# 컴파일러가 없어서 코드를 한 번도 빌드하지 못했다 | Unity를 열고 Console 오류 확인 → 오류를 복사해서 전달 |
| **EditMode 테스트 61개** | 같은 이유로 실행하지 못했다 | `Window > General > Test Runner > EditMode > Run All` |
| **실제 플레이** | 화면 배치, 회전 연출, 색, 글꼴을 눈으로 보지 못했다 | `SampleScene`에서 Play로 1~4단계 확인 |

### 4.2 환경·정리 문제

| 항목 | 상태 | 할 일 |
|---|---|---|
| git 저장소 | 없음. 커밋 규칙(`feat:` 등)을 아직 못 썼다 | `git init`, `.gitignore`(Unity용) 추가 후 첫 커밋 |
| `_to_delete/quadrop_update.tgz` | 파일 전달용 압축 파일. 원격에서 파일을 지울 권한이 없어서 남아 있다 | 직접 삭제 (Assets 밖이라 Unity에는 영향 없음) |
| 스킬 `unity-clean-logic` | 제안 카드로만 전달했다. `.claude/skills`에는 원격으로 쓸 수 없었다 | 카드에서 저장했는지 확인. 안 했다면 다시 요청 |
| `Assets/Resources/GameConfig.asset` | 만들지 않았다 (없으면 코드의 기본값을 쓴다) | 밸런스를 바꿀 때 `Create > Quadrop > Game Config`로 생성 |
| `docs/progress.md` (이 문서) | 원격 연결이 끊겨서 폴더에 직접 넣지 못하고 파일로 전달했다 | `Quadrop/docs/`에 넣기. `CLAUDE.md` 8장의 문서 목록에도 추가 |

### 4.3 요청·처음 설계와 달라진 것 (의도한 변경)

| 처음 | 지금 | 이유 |
|---|---|---|
| 정사각형 판, 반대편 벽까지 낙하 | **십자 레인, 중앙까지 낙하** | 아래 방향이 열리는 순간 피스가 나올 자리가 이미 막혀 바로 게임오버되는 버그 |
| 계층 이름 `Application` | **`Game`** | `UnityEngine.Application`과 이름 충돌 |
| 해금 순서 Top → Right → Bottom → Left | **Top → Left → Bottom → Right** | D(판 시계 회전) 한 번에 새 방향이 오도록 |
| 모드 이름 CROSS | **ALL SIDES** | ROTETRA의 모드 이름과 겹치지 않도록 |
| 회전 중에는 판 회전 입력 무시 | **모든 입력을 미뤄 두었다가 처리** | 회전 중 입력이 씹히지 않게 |
| `UnlockPromptView` 별도 클래스 | `HudView` 가운데 안내 문구로 합침 | 클래스 수를 줄이려고 |
| `TickClock` 클래스 | 피스별 `PieceTimer` | 소프트 드롭은 조작 중인 피스에만 적용되므로 |

### 4.4 알려진 제약·위험

- **조명**: 스프라이트가 URP 2D Lit 머티리얼을 쓴다. `SampleScene`에는 Global Light 2D가 있지만, 조명이 없는 씬에서 실행하면 어둡게 보일 수 있다.
- **글꼴**: 내장 `LegacyRuntime.ttf`를 쓰기 때문에 화면 문구는 영어다. 한글을 쓰려면 TextMeshPro와 한글 폰트가 필요하다.
- **미리보기 위치**: 홀드·다음 피스 위치를 시작할 때 화면 비율로 한 번만 계산한다. 창 크기를 바꾸면 위치가 어긋날 수 있다.
- **자동 생성**: `GameBootstrap`은 어느 씬에서든 Play하면 자동으로 생긴다. 메뉴 씬을 만들 때는 이 동작을 바꿔야 한다.
- **밸런스**: 판 크기, 레인 폭, 해금 줄 수, 속도는 플레이 테스트 전의 추정값이다.

---

## 5. 앞으로 구현할 것 (우선순위 순)

`docs/planned.md`와 같은 목록에 세부 작업을 덧붙였다. 한 번에 하나씩, 테스트를 통과한 뒤 다음으로 넘어간다.

1. **컴파일·테스트 통과** — 4.1 해결, 오류 수정, 61개 전부 통과
2. **git 설정과 첫 커밋** — Unity `.gitignore`, `chore: initial prototype`
3. **플레이 테스트와 밸런스** — `docs/prototype.md`의 검증 항목 6개
   - 레인 폭 8 / 10 / 12 비교, 해금 줄 수 조정
   - 겹치는 칸 간섭이 재미있는지 확인 (아니면 규칙 수정)
   - 방향별 블록 색 구분 여부 결정
4. **타이틀·모드 선택 화면** — STANDARD / ALL SIDES, 최고 점수 표시, `GameBootstrap` 자동 생성 정리
5. **사운드** — `GameEvents`를 구독하는 `AudioView` (이동·회전·고정·줄 지우기·해금·게임오버), BGM
6. **연출** — 줄 지우기 파티클, 해금 시 새 레인 깜빡임, 화면 흔들림
7. **한글 UI** — TextMeshPro와 한글 폰트로 교체
8. **키 재설정 화면** — Input System 리바인딩, 설정 저장 (Repository + Result)
9. **추가 모드** — `IProgressionRule` 구현체 추가 (예: 시간 제한, 퍼즐)
10. **PlayMode 테스트** — 부트스트랩·입력 연결 (`Assets/Tests/PlayMode/`, asmdef 추가)
11. **웹 빌드와 배포** — itch.io / Game Jolt, `docs/upload/itch-io.md`, `docs/upload/game-jolt.md` (공식 문서 확인 날짜 기록)

---

## 6. 다음 세션 시작 방법

1. Quadrop 폴더를 연결한다 (Claude 데스크톱 앱 `+` → 폴더 추가).
2. 이렇게 요청한다:
   > `docs/progress.md`와 `CLAUDE.md`를 읽고, 5번 목록의 다음 항목부터 이어서 진행해줘. (Unity Console 오류나 테스트 결과가 있으면 같이 붙여넣기)
3. 작업이 끝날 때마다 이 문서의 2~5장, `implemented.md`, `planned.md`, `file-tree.md`를 갱신한다.
