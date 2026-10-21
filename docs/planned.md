# 구현 예정 (우선순위 순)

한 번에 하나씩, 테스트 통과 후 다음으로 넘어간다.

1. **Unity에서 컴파일·테스트 확인** — Test Runner(EditMode) 전부 통과, Play로 1~4단계 플레이 확인
2. **플레이 테스트로 밸런스 조정** — `docs/prototype.md`의 검증 항목 (판 크기·레인 폭, 해금 줄 수, 겹치는 칸 간섭 느낌)
3. **타이틀·모드 선택 화면** — STANDARD / ALL SIDES 선택, 최고 점수 표시
4. **사운드** — 이동·회전·고정·줄 지우기·해금 효과음, BGM (`GameEvents` 구독하는 `AudioView`)
5. **연출 강화** — 줄 지우기 파티클, 해금 시 새 변 깜빡임, 화면 흔들림
6. **키 재설정 화면** — Input System 리바인딩, 설정 저장
7. **추가 모드** — 예: 시간 제한 모드, 퍼즐 모드 (`IProgressionRule` 구현체 추가)
8. **PlayMode 테스트** — 부트스트랩·입력 연결 확인 (`Assets/Tests/PlayMode/`)
9. **웹 빌드와 배포 문서** — `docs/upload/itch-io.md`, `docs/upload/game-jolt.md`
