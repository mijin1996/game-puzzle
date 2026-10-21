# 파일 구조

> 파일을 추가·삭제·이동하면 바로 갱신한다. Unity 기본 생성 파일(Library, ProjectSettings, .meta 등)은 적지 않는다.

## 문서·설정

```
CLAUDE.md                                      — 프로젝트 작업 규칙 (Claude Code가 매번 읽음)
.editorconfig                                  — 코드 스타일 강제 (Allman, 4칸 스페이스, 이름 규칙)
docs/file-tree.md                              — 전체 파일 구조와 역할
docs/architecture.md                           — Quadrop 아키텍처 (1부 게임 로직, 2부 코드 구조)
docs/prototype.md                              — 핵심 루프, 프로토타입 범위, 검증할 것
docs/planned.md                                — 구현 예정 기능 (우선순위 순)
docs/implemented.md                            — 구현 완료 기능
Assets/Scenes/SampleScene.unity                — 기본 씬 (Play하면 GameBootstrap이 자동 생성됨)
Assets/Settings/                               — URP 2D 렌더러, Input System 기본 액션 (Unity 템플릿)
```

## Core — 규칙 (순수 C#, `Quadrop.Core`)

```
Assets/Scripts/Core/Quadrop.Core.asmdef                — Core 어셈블리 (엔진 참조 없음)
Assets/Scripts/Core/Common/Result.cs                   — 성공/실패 결과값 (프로젝트에 하나)
Assets/Scripts/Core/Common/IRandomSource.cs            — 랜덤 추상화
Assets/Scripts/Core/Common/SystemRandomSource.cs       — System.Random 구현
Assets/Scripts/Core/Grid/GridPos.cs                    — 판 좌표 struct
Assets/Scripts/Core/Grid/Side.cs                       — 변 enum (Top, Right, Bottom, Left)
Assets/Scripts/Core/Grid/SideExtensions.cs             — 바깥 방향·중력·회전 순서·판 각도
Assets/Scripts/Core/Grid/SideSet.cs                    — 활성 변 집합 (비트마스크)
Assets/Scripts/Core/Grid/SideFrame.cs                  — 변 로컬 좌표 → 판 좌표 변환
Assets/Scripts/Core/Grid/BoardFrames.cs                — 4개 SideFrame, 피스 셀 판 좌표 계산
Assets/Scripts/Core/Grid/LaneLayout.cs                 — 십자 모양 레인 범위, 로컬 ↔ 판 좌표
Assets/Scripts/Core/Grid/IReadOnlyGrid.cs              — 화면용 읽기 전용 판
Assets/Scripts/Core/Grid/GridModel.cs                  — 고정 블록 저장, 줄 검사·복사·삭제
Assets/Scripts/Core/Pieces/TetrominoType.cs            — 피스 7종 enum
Assets/Scripts/Core/Pieces/TetrominoShapes.cs          — 종류·회전별 셀 오프셋 표
Assets/Scripts/Core/Pieces/Piece.cs                    — 낙하 중인 피스 (로컬 좌표)
Assets/Scripts/Core/Pieces/CellState.cs                — 칸 상태 (Empty, Piece, Solid)
Assets/Scripts/Core/Pieces/MoveResult.cs               — 이동 결과
Assets/Scripts/Core/Pieces/ICellQuery.cs               — 칸 상태 질의 인터페이스
Assets/Scripts/Core/Pieces/PieceMover.cs               — 이동·회전(벽 차기)·드롭 거리·밀어내기
Assets/Scripts/Core/Rules/LaneLine.cs                  — 레인 한 줄 (변, 깊이)
Assets/Scripts/Core/Rules/LineClearer.cs               — 레인 줄 표시·삭제·중앙 쪽 메우기·연쇄
Assets/Scripts/Core/Rules/ScoreCalculator.cs           — 점수 계산
Assets/Scripts/Core/Rules/GravitySchedule.cs           — 레벨별 낙하 간격
Assets/Scripts/Core/Rules/IPieceSequence.cs            — 피스 순서 인터페이스
Assets/Scripts/Core/Rules/SevenBagSequence.cs          — 7-bag 피스 순서
Assets/Scripts/Core/Progression/IProgressionRule.cs    — 변 해금 규칙 인터페이스
Assets/Scripts/Core/Progression/StandardProgression.cs — STANDARD: 줄 수마다 변 해금
Assets/Scripts/Core/Progression/AllSidesProgression.cs — ALL SIDES: 처음부터 4방향
Assets/Scripts/Core/Config/GameRules.cs                — 규칙 값 묶음 + 검증
```

## Game — 게임 흐름 (`Quadrop.Game`)

```
Assets/Scripts/Game/Quadrop.Game.asmdef                — Game 어셈블리 (엔진 참조 없음)
Assets/Scripts/Game/AssemblyInfo.cs                    — 테스트 어셈블리에 internal 공개
Assets/Scripts/Game/GameSession.cs                     — 바깥 창구: 명령 큐, 틱, 읽기 전용 정보
Assets/Scripts/Game/GameContext.cs                     — 한 판의 객체 묶음 + 공통 흐름 함수
Assets/Scripts/Game/GameCommand.cs                     — 입력 명령 enum
Assets/Scripts/Game/IGameCommandSink.cs                — 입력 → 게임 통로
Assets/Scripts/Game/GameEvents.cs                      — View용 이벤트 모음
Assets/Scripts/Game/GameStats.cs                       — 점수·줄 수·레벨
Assets/Scripts/Game/GameStateId.cs                     — 상태 enum
Assets/Scripts/Game/GameMode.cs                        — 모드 enum
Assets/Scripts/Game/ProgressionFactory.cs              — 모드 → 해금 규칙 생성
Assets/Scripts/Game/States/CommandPolicy.cs            — 상태별 입력 처리 방식
Assets/Scripts/Game/States/IGameState.cs               — 상태 인터페이스
Assets/Scripts/Game/States/GameStateBase.cs            — 상태 기본 구현
Assets/Scripts/Game/States/GameStateMachine.cs         — 상태 전환
Assets/Scripts/Game/States/PlayingState.cs             — 기본 플레이
Assets/Scripts/Game/States/RotatingState.cs            — 판 회전 연출 (입력 미룸)
Assets/Scripts/Game/States/ClearingState.cs            — 줄 지우기 + 점수 + 해금 판단
Assets/Scripts/Game/States/UnlockingState.cs           — 새 변 해금 연출
Assets/Scripts/Game/States/PausedState.cs              — 일시정지
Assets/Scripts/Game/States/GameOverState.cs            — 게임오버
Assets/Scripts/Game/Pieces/SpawnerSet.cs               — 변별 활성·낙하 피스·생성
Assets/Scripts/Game/Pieces/SpawnOutcome.cs             — 생성 결과 (Ready, Pending, Blocked)
Assets/Scripts/Game/Pieces/BoardCollision.cs           — 고정 블록 + 다른 피스 충돌 질의
Assets/Scripts/Game/Pieces/GravityStepper.cs           — 낙하와 고정 대기
Assets/Scripts/Game/Pieces/PieceController.cs          — 조작 중인 피스 이동·회전·드롭·홀드
Assets/Scripts/Game/Pieces/PieceLocker.cs              — 피스를 판에 고정
Assets/Scripts/Game/Pieces/PieceTimer.cs               — 피스별 타이머
Assets/Scripts/Game/Pieces/ControlState.cs             — 조작 중인 변, 판 회전
Assets/Scripts/Game/Pieces/HoldSlot.cs                 — 공유 홀드 칸
Assets/Scripts/Game/HighScore/IHighScoreRepository.cs  — 최고 점수 저장 인터페이스
Assets/Scripts/Game/HighScore/HighScoreTracker.cs      — 게임오버 시 최고 점수 갱신
```

## Infrastructure (`Quadrop.Infrastructure`)

```
Assets/Scripts/Infrastructure/Quadrop.Infrastructure.asmdef      — Infrastructure 어셈블리
Assets/Scripts/Infrastructure/Input/InputReader.cs               — 키보드·게임패드 → GameCommand, DAS/ARR
Assets/Scripts/Infrastructure/Input/InputTuning.cs               — DAS/ARR 값
Assets/Scripts/Infrastructure/Save/PlayerPrefsHighScoreRepository.cs — PlayerPrefs 최고 점수 저장
```

## Presentation (`Quadrop.Presentation`)

```
Assets/Scripts/Presentation/Quadrop.Presentation.asmdef  — Presentation 어셈블리
Assets/Scripts/Presentation/GameBootstrap.cs             — 조립 (Composition Root), 카메라 설정, 자동 생성
Assets/Scripts/Presentation/GameLoopDriver.cs            — Update → 입력·세션 틱
Assets/Scripts/Presentation/Config/GameConfigAsset.cs    — 밸런스 ScriptableObject
Assets/Scripts/Presentation/Board/BoardView.cs           — 판 그리기와 회전 연출
Assets/Scripts/Presentation/Board/PreviewView.cs         — 홀드·다음 피스 미리보기
Assets/Scripts/Presentation/Hud/HudView.cs               — 점수·안내 UI
Assets/Scripts/Presentation/Rendering/SpriteCellPool.cs  — 셀 렌더러 풀
Assets/Scripts/Presentation/Rendering/SpriteFactory.cs   — 셀 스프라이트 생성
Assets/Scripts/Presentation/Rendering/BlockPalette.cs    — 색상 모음
```

## Tests

```
Assets/Tests/Mocks/Quadrop.Tests.Mocks.asmdef      — Mock 어셈블리 (에디터 전용)
Assets/Tests/Mocks/FixedPieceSequence.cs           — 항상 같은 피스
Assets/Tests/Mocks/FixedRandomSource.cs            — 고정 랜덤
Assets/Tests/Mocks/MockCellQuery.cs                — 판 + 가짜 낙하 피스 칸
Assets/Tests/Mocks/MockHighScoreRepository.cs      — 메모리 최고 점수 저장소
Assets/Tests/Mocks/MockGrids.cs                    — 테스트용 판 채우기
Assets/Tests/Mocks/TestRules.cs                    — 작은 판·짧은 연출 규칙
Assets/Tests/EditMode/Quadrop.Tests.EditMode.asmdef — EditMode 테스트 어셈블리
Assets/Tests/EditMode/GridTests.cs                 — 좌표·판·변 좌표계
Assets/Tests/EditMode/PieceTests.cs                — 모양·이동·레인 경계·충돌·벽 차기·밀어내기
Assets/Tests/EditMode/LineClearerTests.cs          — 레인 줄 지우기·메우기, 레인 좌표 변환
Assets/Tests/EditMode/RulesTests.cs                — 점수·속도·7-bag·해금·규칙 검증
Assets/Tests/EditMode/GameFlowTests.cs             — 판 회전·홀드·세션 흐름·게임오버·최고 점수
```
