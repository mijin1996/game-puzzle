# Git 업로드 가이드 (Quadrop)

> 작성일: 2026-10-22 · Unity 6000.3.26f1 기준
> 다른 컴퓨터에서도 이어서 작업할 수 있게 GitHub에 올리고 받는 방법

---

## 1. 저장소 공개 범위: private 추천

- 아직 개발 중인 게임이라 남이 코드를 복사해 가지 않도록 **private**를 추천한다. 법적인 이유 때문은 아니다.
  - 저장소에는 직접 짠 코드와 문서만 있고, ROTETRA의 코드·그래픽·사운드·이름은 없다.
  - 법적 문제는 게임을 **공개 배포할 때** 겉모습과 구성이 원작과 얼마나 비슷한지로 따진다 (변호사 조언 아님).
- GitHub private 저장소는 무료이고 개수 제한이 없다.
- 나중에 `Settings > General > Danger Zone > Change visibility`에서 언제든 public으로 바꿀 수 있다.

---

## 2. 올리기 전에 Unity 설정 확인

`Edit > Project Settings > Editor`에서 확인한다 (보통 기본값이 이미 이렇게 되어 있다).

| 항목 | 값 | 이유 |
|---|---|---|
| Version Control Mode | **Visible Meta Files** | `.meta` 파일이 보여야 git에 올라간다 |
| Asset Serialization Mode | **Force Text** | 씬·에셋이 텍스트로 저장되어 변경 내역을 비교할 수 있다 |

---

## 3. 올릴 것과 뺄 것

| 올림 ✅ | 뺌 ❌ (Unity가 다시 만들어 줌) |
|---|---|
| `Assets/` (**.meta 파일 포함**) | `Library/`, `Temp/`, `Logs/`, `obj/` |
| `Packages/` (`manifest.json`, `packages-lock.json`) | `UserSettings/` (개인 에디터 설정) |
| `ProjectSettings/` (`ProjectVersion.txt` 포함) | `*.csproj`, `*.sln`, `*.slnx` (자동 생성) |
| `CLAUDE.md`, `.editorconfig`, `docs/` | `Build/`, `_to_delete/` |

> ⚠️ `.meta` 파일을 빼먹으면 다른 컴퓨터에서 씬과 스크립트 연결이 전부 끊긴다. 가장 흔한 실수다.

---

## 4. 프로젝트 루트에 둘 `.gitignore`

```
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Uu]ser[Ss]ettings/
/[Mm]emoryCaptures/
*.csproj
*.sln
*.slnx
*.suo
*.user
.vs/
.idea/
_to_delete/
.DS_Store
```

---

## 5. 처음 올리기

명령어 기준이다. GitHub Desktop을 써도 순서는 같다.

1. **Git**과 **Git LFS**를 설치한다. LFS는 나중에 이미지·사운드 같은 큰 파일을 넣을 때 필요하다.
2. GitHub에서 **private** 저장소 `Quadrop`을 만든다. README·.gitignore는 만들지 않는다 (빈 저장소로 둔다).
3. `Quadrop` 폴더에서 아래 명령을 실행한다.

```
git init
git lfs install
git lfs track "*.png" "*.psd" "*.wav" "*.mp3" "*.ogg" "*.fbx" "*.ttf"
git add .gitattributes .gitignore
git status
```

4. `git status` 결과에 `Library/`, `Temp/`가 **없는지** 확인한다. 있으면 `.gitignore` 위치(프로젝트 루트)를 다시 확인한다.

```
git add .
git commit -m "chore: initial prototype"
git branch -M main
git remote add origin https://github.com/<아이디>/Quadrop.git
git push -u origin main
```

---

## 6. 다른 컴퓨터에서 받기

1. Unity Hub에서 **같은 버전(6000.3.26f1)** 을 설치한다. 정확한 버전은 `ProjectSettings/ProjectVersion.txt`에 있다.
2. 저장소를 받는다.

```
git clone https://github.com/<아이디>/Quadrop.git
cd Quadrop
git lfs pull
```

3. Unity Hub → `Add > Add project from disk` → `Quadrop` 폴더를 연다.
4. 처음 열 때는 `Library`를 다시 만드느라 몇 분 걸린다.

---

## 7. 평소 작업 순서

| 시점 | 명령 |
|---|---|
| 작업 시작 | `git pull` |
| 기능 하나 완료 (테스트 통과) | `git add .` → `git commit -m "feat(범위): 요약"` → `git push` |
| 다른 컴퓨터로 옮기기 전 | 반드시 `git push`까지 했는지 확인 |

커밋 메시지 형식은 `CLAUDE.md` 9장을 따른다: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`.

---

## 8. 자주 나는 문제

| 증상 | 원인 | 해결 |
|---|---|---|
| 다른 컴퓨터에서 스크립트가 "Missing"으로 뜸 | `.meta` 파일이 안 올라감 | Visible Meta Files 확인 후 `.meta` 파일 커밋 |
| push가 너무 오래 걸리거나 용량 초과 | `Library/`가 올라감 | `.gitignore` 확인, `git rm -r --cached Library` 후 다시 커밋 |
| 프로젝트를 열자 버전 변환 창이 뜸 | 다른 Unity 버전으로 엶 | `ProjectVersion.txt`와 같은 버전 설치 |
| 이미지·사운드가 깨짐 | LFS 파일을 안 받음 | `git lfs pull` |
