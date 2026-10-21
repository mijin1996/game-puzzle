# Quadrop 프로젝트 작업 규칙

> Unity 6 (6000.3) · C# · URP 2D · Input System · Unity Test Framework
> Claude Code는 이 프로젝트에서 작업할 때마다 이 파일을 먼저 읽는다.
> 코드 설계 방법은 `unity-clean-logic` 스킬(Allman · Unity C# 기준)을 따른다. 코드를 쓰기 전에 이 스킬을 불러온다.

---

## 0. 작업 순서 (매번 지킬 것)

1. 작업 전 `docs/file-tree.md`, `docs/architecture.md`, `docs/implemented.md`, `docs/planned.md`를 읽고 현재 구조를 파악한다.
2. 새 파일을 만들기 전에 비슷한 역할의 파일이 이미 있는지 확인한다. 있으면 확장한다.
3. 구현 → 테스트 작성 → 테스트·빌드 통과 확인 → 문서 갱신 → 커밋 순서로 진행한다.
4. 테스트가 실패하면 다음 단계로 넘어가지 않는다.
5. `docs/planned.md`의 우선순위 순서대로 한 번에 기능 하나씩 진행한다.

---

## 1. 코드 스타일: Allman (예외 없음)

- 여는 중괄호 `{`는 **항상 다음 줄**에 단독으로 둔다. (클래스, 메서드, 프로퍼티, if, for, foreach, while, switch, try, 람다 블록, 이니셜라이저 전부)
- `else`, `else if`, `catch`, `finally`는 닫는 중괄호 `}` **다음 줄**에서 시작한다.
- 한 줄짜리 `if`/`for`도 중괄호를 생략하지 않는다.
- 들여쓰기는 스페이스 4칸, 탭 금지.
- 식 본문 멤버(`=>`)는 **한 줄로 끝나는 읽기 전용 프로퍼티·간단한 반환**에만 허용한다.
- 다른 스타일(K&R 등)로 된 코드나 예제를 가져오면 **반드시 Allman으로 고쳐서** 넣는다.

```csharp
public sealed class JumpController
{
    private bool _isGrounded;
    private bool _bufferedJump;

    public int Width => _width;   // 한 줄 읽기 전용 프로퍼티는 => 허용

    public void Jump()
    {
        if (_isGrounded)
        {
            ApplyJumpForce();
        }
        else
        {
            _bufferedJump = true;
        }
    }

    public Result<SaveData> Load(string path)
    {
        try
        {
            return ReadFile(path);
        }
        catch (IOException e)
        {
            return Result<SaveData>.Fail(e.Message);
        }
        finally
        {
            CloseHandles();
        }
    }
}
```

루트의 `.editorconfig`가 이 규칙을 강제한다 (`csharp_new_line_before_open_brace = all` 등). 수정하지 않는다.

### 이름 규칙
| 대상 | 규칙 | 예 |
|---|---|---|
| 클래스·구조체·enum·메서드·프로퍼티·이벤트 | PascalCase | `GridModel`, `TryMove()`, `IsFull` |
| 인터페이스 | `I` + PascalCase | `IRandomSource` |
| private 필드 | `_camelCase` | `_cells` |
| Inspector 노출 필드 | `[SerializeField] private` + `_camelCase` | `[SerializeField] private float _tickSeconds;` |
| 지역 변수·매개변수 | camelCase | `lineCount` |
| 상수 | PascalCase | `const int MaxDirections = 4;` |

`public` 필드는 쓰지 않는다. Inspector에 보여야 하면 `[SerializeField] private`를 쓴다.

---

## 2. Java 개념 → Unity C# 기준

설계 원칙(OOP, SOLID, 패턴)은 Java와 같다. 문법과 Unity 특성만 아래처럼 바꿔서 적용한다.

| Java | Unity C# | 주의 |
|---|---|---|
| `getX()` / `setX()` | 프로퍼티 `public int X { get; private set; }` | setter에서 값 검증 |
| `final` 필드 | `readonly` 필드, 상수는 `const` | |
| `interface` | `interface` (이름 앞에 `I`) | |
| `abstract class` | `abstract class` | |
| `record` / 불변 객체 | `readonly struct` 또는 `sealed class` + `readonly` 필드 | 자주 생성되는 작은 값은 struct (GC 없음) |
| `Optional<T>` | nullable `T?` 또는 `TryGet(out T)` | |
| `ArrayList` / `HashMap` / `HashSet` / `ArrayDeque` | `List` / `Dictionary` / `HashSet` / `Queue`·`Stack` | 미리 용량 지정, 재사용 |
| `equals()` / `hashCode()` | `IEquatable<T>.Equals` + `GetHashCode` 같이 구현 | struct는 박싱 방지 위해 꼭 구현 |
| `enum` | `enum` (값 기반, 메서드는 확장 메서드로) | |
| 생성자 주입 | 일반 C# 클래스는 생성자 주입. MonoBehaviour는 생성자 불가 → `Construct(...)` / `Init(...)` 메서드로 주입 | |
| `main()` 진입점 | 씬의 `GameBootstrap`(Composition Root) 하나 | 객체 조립은 여기서만 |
| Thread / CompletableFuture | `async` + Unity `Awaitable` (Unity 6) | 메인 스레드에서만 Unity API 호출 |
| 리스너 인터페이스 | C# `event Action<T>` | 구독한 쪽이 `OnDisable`에서 반드시 해제 |
| Stream API | LINQ | **Update·틱 같은 매 프레임 경로에서 LINQ 금지** (GC 발생) |
| JUnit | Unity Test Framework (NUnit) EditMode / PlayMode | |

### Unity만의 규칙
- **MonoBehaviour는 View 역할만** 한다. 게임 규칙은 순수 C# 클래스(Core)에 둔다.
- `Update()` 안에서 `GetComponent`, `Find`, `new`(참조 타입), LINQ, 문자열 이어 붙이기 금지.
- 컴포넌트 참조는 `Awake`에서 한 번 캐싱하거나 Inspector로 연결한다.
- 자주 생기고 사라지는 오브젝트(블록 셀, 이펙트)는 Object Pool (`UnityEngine.Pool.ObjectPool<T>`).
- 밸런스 값(그리드 크기, 낙하 속도, 방향 해금 조건)은 ScriptableObject에 둔다. 매직 넘버 금지.
- Core 어셈블리는 `UnityEngine`을 참조하지 않는다 (`noEngineReferences: true`).

---

## 3. 설계 원칙

### SOLID
| 원칙 | 의미 | Quadrop 적용 |
|---|---|---|
| SRP | 클래스는 변경 이유가 하나뿐 | 낙하·줄 지우기·회전·진행·점수를 한 클래스에 넣지 않는다 |
| OCP | 확장에 열리고 수정에 닫힘 | 새 게임 모드는 `IProgressionRule` 구현체 추가로 만든다 |
| LSP | 하위 타입이 상위 타입을 대체 | 인터페이스 구현체가 예외를 던지거나 기능을 빼먹지 않는다 |
| ISP | 쓰지 않는 메서드에 의존 안 함 | 입력·저장·랜덤을 작은 인터페이스로 나눈다 |
| DIP | 추상에 의존 | 랜덤, 저장소, 입력은 인터페이스로 받고 구현은 주입한다 |

### 기타 원칙
| 원칙 | 의미 | 적용 방법 |
|---|---|---|
| DRY | 같은 지식을 두 번 쓰지 않는다 | 4방향 처리는 방향별 복붙 대신 `Direction` 벡터 하나로 공통 처리 |
| PIE | 의도를 드러내게 작성 | 영리한 한 줄보다 읽히는 세 줄 |
| SDP | 안정적인 쪽으로 의존 | 자주 바뀌는 연출·UI가 Core에 의존. 반대 금지 |
| ADP | 순환 의존 금지 | 순환이 생기면 인터페이스를 Core로 올려 끊는다 |

### 계층과 의존 방향

```
Presentation (MonoBehaviour View, 연출, HUD) ─┐
                                               ├──▶ Game (게임 흐름, 상태 머신) ──▶ Core (규칙, 순수 C#)
Infrastructure (입력, 저장, 플랫폼) ──────────┘        (Core의 인터페이스를 Infrastructure가 구현)
```

- 계층은 **Assembly Definition(.asmdef)** 으로 나눠서 잘못된 방향의 참조를 컴파일 에러로 막는다.
- 게임 흐름 계층의 이름은 `Application`이 아니라 **`Game`** 이다 (`UnityEngine.Application`과 이름이 부딪히지 않게).
- 자세한 구조는 `docs/architecture.md` 참고.

---

## 4. 패턴 사용 규칙

- 패턴은 **문제가 있을 때만** 쓴다.
- 패턴을 쓴 클래스 맨 위에 **패턴 이름과 이유를 1줄 주석**으로 적는다.

```csharp
// 패턴: State — 생성·낙하·줄 지우기·회전·게임오버마다 허용 입력이 달라서
public sealed class GameStateMachine
{
}
```

### 금지
- `GameManager` 하나에 모든 기능 몰아넣기 (God Object)
- 싱글톤 남발 — 정말 전역이어야 하는 것만, 이유를 주석으로
- 매 프레임 GC 할당
- 반복 코드, 같은 역할의 파일 두 개

---

## 5. Result 패턴

실패할 수 있는 작업(저장·불러오기, 파싱, 플랫폼 SDK)은 예외를 던지지 않고 **Result**를 반환한다.
예외는 프로그래머 실수(null 인자, 잘못된 상태)에만 쓴다. 호출한 쪽은 실패를 반드시 처리한다.

```csharp
// 패턴: Result — 실패를 예외 대신 값으로 돌려줘서 호출부가 처리를 빠뜨리지 않게
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }

    private Result(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value)
    {
        return new Result<T>(true, value, null);
    }

    public static Result<T> Fail(string error)
    {
        return new Result<T>(false, default, error);
    }
}
```

`Result`는 `Assets/Scripts/Core/Common/Result.cs` **한 곳에만** 정의한다.
게임 규칙의 "실패"(블록이 못 움직임 등)는 Result가 아니라 `bool TryMove(...)`처럼 가볍게 처리한다 (매 틱 호출되므로).

---

## 6. 테스트와 Mock

- 기능을 구현하면 테스트를 함께 작성한다.
- Core 로직은 **EditMode** 테스트 (`Assets/Tests/EditMode/`), 씬·입력이 필요한 것은 **PlayMode** 테스트 (`Assets/Tests/PlayMode/`).
- 테스트 이름: `메서드_조건_기대결과` (예: `TryMove_BlockedByLockedCell_ReturnsFalse`)
- 정상 경로와 실패·경계 경로(0, 음수, 판 가장자리, 빈 그리드, 가득 찬 그리드)를 모두 테스트한다.
- Mock은 `Assets/Tests/Mocks/`에 모으고 팩토리 함수로 만든다 (예: `MockGrids.CreateWithFullRow(row: 0)`).
- 랜덤은 `IRandomSource`의 고정 시드 Mock을 주입해서 결과가 매번 같게 만든다.
- 테스트 실행(명령줄): `Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml`

---

## 7. 중복 금지

- 같은 역할의 파일이나 코드를 두 개 만들지 않는다. 새 파일 전에 `docs/file-tree.md`를 확인한다.
- 같은 로직이 두 번째로 나오면 공통화한다.
- 상수는 한 곳에 정의한다. 밸런스 값은 ScriptableObject로.
- 안 쓰는 파일·코드·주석 처리된 옛 코드는 지운다. 기록은 git이 한다.

---

## 8. 문서 구조 (`docs/`)

```
docs/
├── file-tree.md      # 전체 파일 구조 + 각 파일 역할 1줄
├── architecture.md   # Quadrop 아키텍처: 게임 로직, 계층, 의존 방향, 패턴과 이유
├── prototype.md      # 핵심 플레이 루프, 프로토타입 범위, 검증할 것
├── implemented.md    # 구현 완료된 기능 (완료일 포함)
├── planned.md        # 구현 예정 기능 (우선순위 순)
└── upload/           # 배포할 때 작성 (itch-io.md, game-jolt.md 등)
```

### 문서 갱신 규칙
- 파일을 추가·삭제·이동하면 `file-tree.md`를 바로 갱신한다.
- 기능이 끝나면 `planned.md`에서 지우고 `implemented.md`로 옮긴다.
- 구조나 패턴이 바뀌면 `architecture.md`를 갱신한다.
- 문서는 위 목록의 파일에만 쓴다. 비슷한 문서를 새로 만들지 않는다.
- 업로드 문서는 작성·수정할 때 반드시 공식 문서를 확인하고 확인 날짜를 적는다.

### `file-tree.md` 형식
```
Assets/Scripts/Core/Grid/GridModel.cs   — 셀 점유 상태를 담는 2D 그리드 (순수 C#)
```

---

## 9. 커밋

- 기능 하나가 끝나고 테스트·빌드가 통과하면 커밋한다.
- 형식: `feat(범위): 요약`, `fix(범위): 요약`, `refactor(범위): 요약`, `test(범위): 요약`, `docs: 요약`
- 범위 예: `core`, `grid`, `piece`, `progression`, `view`, `input`
- 커밋 후 무엇을 했는지 짧게 보고하고 다음 작업으로 넘어간다.

---

## 10. 완료 전 체크리스트

- [ ] Allman 스타일을 지켰다 (중괄호 전부 다음 줄)
- [ ] 클래스 하나가 한 가지 일만 한다 (SRP)
- [ ] Core가 UnityEngine을 참조하지 않는다
- [ ] 의존 방향이 규칙대로이고 순환이 없다 (SDP, ADP)
- [ ] 패턴을 쓴 곳에 이유 1줄 주석이 있다
- [ ] 실패 가능한 작업은 Result를 반환하고, 호출부에서 처리한다
- [ ] 매 프레임 경로에 GC 할당이 없다
- [ ] 테스트를 작성했고 모두 통과한다 (Mock 사용)
- [ ] 중복 파일·중복 코드·매직 넘버가 없다
- [ ] `docs/` 문서를 갱신했다
- [ ] 커밋했다
